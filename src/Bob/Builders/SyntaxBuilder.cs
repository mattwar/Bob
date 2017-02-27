using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System.Threading;

namespace Builders
{
    /// <summary>
    /// A base class for building/editing syntax nodes.
    /// 
    /// Changes made to a <see cref="SyntaxBuilder"/> modify the underlying <see cref="SyntaxNode"/> tree,
    /// keeping all related <see cref="SyntaxBuilder"/> instances up to date with the changes.
    /// </summary>
    public abstract class SyntaxBuilder
    {
        private SyntaxNode _originalNode;
        private SyntaxBuilder _parent;
        private BuilderContext _context;

        internal SyntaxBuilder(SyntaxBuilder parent, SyntaxNode node)
        {
            Debug.Assert(parent != null);
            Debug.Assert(node != null);
            _parent = parent;
            _originalNode = node;
        }

        internal SyntaxBuilder(BuilderContext context)
        {
            Debug.Assert(context != null);
            _parent = null;
            _context = context;
            _originalNode = context.OriginalRoot;
        }

        internal void DetachFromParent()
        {
            if (Parent != null)
            {
                var context = Context;
                var current = CurrentNode;
                Context.RemoveNode(current);
                _parent = null;
                _context = new BuilderContext(context.Workspace, current);
            }
        }

        /// <summary>
        /// The builder of this syntax element's parent node.
        /// </summary>
        public SyntaxBuilder Parent => _parent;

        internal BuilderContext Context
        {
            get
            {
                var builder = this;
                while (builder._parent != null)
                {
                    builder = builder.Parent;
                }

                System.Diagnostics.Debug.Assert(builder._context != null);
                return builder._context;
            }
        }

        /// <summary>
        /// The current <see cref="SyntaxNode"/> that corresponds to this builder.
        /// </summary>
        public SyntaxNode CurrentNode
        {
            get
            {
                var context = Context;
                if (context.OriginalRoot == _originalNode)
                {
                    return context.CurrentRoot;
                }
                else
                {
                    return context.CurrentRoot.GetCurrentNode(_originalNode);
                }
            }
        }

        internal Workspace Workspace => Context.Workspace;

        /// <summary>
        /// The <see cref="SyntaxGenerator"/> this builder uses.
        /// </summary>
        public SyntaxGenerator Generator => Context.Generator;

        /// <summary>
        /// The kind of declaration this builder's <see cref="SyntaxNode"/> corresponds to.
        /// </summary>
        public DeclarationKind Kind => Generator.GetDeclarationKind(CurrentNode);

        internal void TrackNodes(IEnumerable<SyntaxNode> currentNodes)
        {
            Context.TrackNodes(currentNodes);
        }

        private static readonly string TrackingAnnotationId = "Id";

        internal static SyntaxNode ClearTracking(SyntaxNode node)
        {
            // remove Id annotation from node
            node = node.WithoutAnnotations(TrackingAnnotationId);

            // remove Id annotations from all sub nodes
            var annotatedNodes = node.GetAnnotatedNodes(TrackingAnnotationId);
            if (annotatedNodes.Any())
            {
                node = node.ReplaceNodes(annotatedNodes, (o, r) => r.WithoutAnnotations(TrackingAnnotationId));
            }

            return node;
        }

        internal void UpdateCurrentNode(SyntaxNode newNode)
        {
            Context.Replace(CurrentNode, newNode);
        }

        private SyntaxBuilderList<SyntaxBuilder> _members;

        /// <summary>
        /// The declared members of this syntax element.
        /// </summary>
        public SyntaxBuilderList<SyntaxBuilder> Members
        {
            get
            {
                if (_members == null)
                {
                    var memberNodes = Generator.GetMembers(CurrentNode);
                    TrackNodes(memberNodes);
                    _members = new SyntaxBuilderList<SyntaxBuilder>(
                        this,
                        memberNodes.Select(n => CreateChild(this, n)),
                        (g, r, n) => g.AddMembers(r, new[] { n }));
                }

                return _members;
            }
        }

        public SyntaxBuilder AddMember(SyntaxNode memberNode)
        {
            return this.Members.Add(memberNode);
        }

        internal delegate SyntaxNode NodeAdder(SyntaxGenerator generator, SyntaxNode root, SyntaxNode subNodeToAdd);

        internal SyntaxNode AddNode(SyntaxNode node, NodeAdder adder)
        {
            var annotation = new SyntaxAnnotation();

            node = node.WithAdditionalAnnotations(annotation);
            UpdateCurrentNode(adder(Generator, CurrentNode, node));

            var insertedNode = CurrentNode.GetAnnotatedNodes(annotation).First();
            TrackNodes(new[] { insertedNode });

            // prove that we can find the node again
            System.Diagnostics.Debug.Assert(Context.CurrentRoot.GetCurrentNode(insertedNode) != null);

            return insertedNode;
        }

        internal virtual IEnumerable<SyntaxBuilder> Children => this.Members;


        private CommentBuilderList _leadingComments;

        /// <summary>
        /// Comments preceding this syntax element.
        /// </summary>
        public CommentBuilderList LeadingComments
        {
            get
            {
                if (_leadingComments == null)
                {
                    _leadingComments = new CommentBuilderList(this);
                }

                return _leadingComments;
            }
        }

        /// <summary>
        /// Get all the builders for the specified <see cref="DeclarationKind"/> and name.
        /// </summary>
        public IEnumerable<SyntaxBuilder> GetBuilders(DeclarationKind kind, string name = null)
        {
            return from n in CurrentNode.DescendantNodesAndSelf()
                   where Generator.GetDeclarationKind(n) == kind && (name == null || string.Equals(name, Generator.GetName(n)))
                   select GetBuilder(n.Span);
        }

        /// <summary>
        /// Get all the builders for the specific builder type and name.
        /// </summary>
        public IEnumerable<TBuilder> GetBuilders<TBuilder>(string name = null) where TBuilder : SyntaxBuilder
        {
            return from n in CurrentNode.DescendantNodesAndSelf()
                   where GetBuilderType(Generator.GetDeclarationKind(n)) == typeof(TBuilder) && (name == null || string.Equals(name, Generator.GetName(n)))
                   select (TBuilder)GetBuilder(n.Span);
        }

        /// <summary>
        /// Gets the builder containing the <see cref="SyntaxNode"/>
        /// </summary>
        public SyntaxBuilder GetBuilder(SyntaxNode node)
        {
            if (CurrentNode.Contains(node))
            {
                return GetBuilder(node.Span);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the builder that contains the text span.
        /// </summary>
        public SyntaxBuilder GetBuilder(TextSpan span)
        {
            var builder = this;

            top:
            foreach (var child in builder.Children)
            {
                if (child.CurrentNode.FullSpan.Contains(span))
                {
                    builder = child;
                    goto top;
                }
            }

            return builder;
        }

        /// <summary>
        /// Get the builder that contains the position.
        /// </summary>
        public SyntaxBuilder GetBuilder(int position)
        {
            return GetBuilder(new TextSpan(position, 0));
        }

        /// <summary>
        /// Formats the syntax nodes.
        /// </summary>
        public void Format(CancellationToken cancellationToken = default(CancellationToken))
        {
            UpdateCurrentNode(Formatter.Format(CurrentNode, Context.Workspace, cancellationToken: cancellationToken));
        }

        #region Factories
        public static CompilationUnitBuilder CreateCompilationUnit(Workspace workspace, string language)
        {
            var generator = SyntaxGenerator.GetGenerator(workspace, language);
            var cu = generator.CompilationUnit();
            return new CompilationUnitBuilder(new BuilderContext(workspace, cu));
        }

        public static CompilationUnitBuilder CreateCompilationUnit(Workspace workspace, SyntaxNode compilationUnitNode)
        {
            var generator = SyntaxGenerator.GetGenerator(workspace, compilationUnitNode.Language);
            return new CompilationUnitBuilder(new BuilderContext(workspace, compilationUnitNode));
        }

        public CompilationUnitBuilder CreateCompilationUnit()
        {
            return new CompilationUnitBuilder(new BuilderContext(Context.Workspace, Generator.CompilationUnit()));
        }

        public NamespaceBuilder CreateNamespace(string name)
        {
            return new NamespaceBuilder(new BuilderContext(Context.Workspace, Generator.NamespaceDeclaration(name)));
        }

        public NamespaceImportBuilder CreateNamespaceImport(string name)
        {
            return new NamespaceImportBuilder(new BuilderContext(Context.Workspace, Generator.NamespaceImportDeclaration(name)));
        }

        public NamespaceImportBuilder CreateNamespaceImport(SyntaxNode nameNode)
        {
            return new NamespaceImportBuilder(new BuilderContext(Context.Workspace, Generator.NamespaceImportDeclaration(nameNode)));
        }

        public TypeBuilder CreateClass(string name)
        {
            return new TypeBuilder(new BuilderContext(Context.Workspace, Generator.ClassDeclaration(name)));
        }

        public TypeBuilder CreateStruct(string name)
        {
            return new TypeBuilder(new BuilderContext(Context.Workspace, Generator.StructDeclaration(name)));
        }

        public TypeBuilder CreateInterface(string name)
        {
            return new TypeBuilder(new BuilderContext(Context.Workspace, Generator.InterfaceDeclaration(name)));
        }

        public TypeBuilder CreateEnum(string name)
        {
            return new TypeBuilder(new BuilderContext(Context.Workspace, Generator.EnumDeclaration(name)));
        }

        public DelegateBuilder CreateDelegate(string name)
        {
            return new DelegateBuilder(new BuilderContext(Context.Workspace, Generator.DelegateDeclaration(name)));
        }

        public MethodBuilder CreateMethod(string name)
        {
            return new MethodBuilder(new BuilderContext(Context.Workspace, Generator.MethodDeclaration(name)));
        }

        public ParameterBuilder CreateParameter(string name, TypeExpression type = default(TypeExpression))
        {
            return new ParameterBuilder(new BuilderContext(Context.Workspace, Generator.ParameterDeclaration(name, type.ToSyntaxNode(Context))));
        }

        public FieldBuilder CreateField(string name, TypeExpression type)
        {
            return new FieldBuilder(new BuilderContext(Context.Workspace, Generator.FieldDeclaration(name, type.ToSyntaxNode(Context))));
        }

        public PropertyBuilder CreateProperty(string name, TypeExpression type)
        {
            return new PropertyBuilder(new BuilderContext(Context.Workspace, Generator.PropertyDeclaration(name, type.ToSyntaxNode(Context))));
        }

        public PropertyBuilder CreateIndexer(TypeExpression type)
        {
            return new PropertyBuilder(new BuilderContext(Context.Workspace, Generator.IndexerDeclaration(null, type.ToSyntaxNode(Context))));
        }

        public FieldBuilder CreateEvent(string name, TypeExpression type)
        {
            return new FieldBuilder(new BuilderContext(Context.Workspace, Generator.EventDeclaration(name, type.ToSyntaxNode(Context))));
        }

        public PropertyBuilder CreateCustomEvent(string name, TypeExpression type)
        {
            return new PropertyBuilder(new BuilderContext(Context.Workspace, Generator.CustomEventDeclaration(name, type.ToSyntaxNode(Context))));
        }

        internal static SyntaxBuilder CreateChild(SyntaxBuilder parent, SyntaxNode node)
        {
            switch (parent.Generator.GetDeclarationKind(node))
            {
                case DeclarationKind.CompilationUnit:
                    return new CompilationUnitBuilder(parent, node);
                case DeclarationKind.Namespace:
                    return new NamespaceBuilder(parent, node);
                case DeclarationKind.NamespaceImport:
                    return new NamespaceImportBuilder(parent, node);
                case DeclarationKind.Class:
                case DeclarationKind.Interface:
                case DeclarationKind.Struct:
                case DeclarationKind.Enum:
                    return new TypeBuilder(parent, node);
                case DeclarationKind.Method:
                case DeclarationKind.Constructor:
                case DeclarationKind.Destructor:
                case DeclarationKind.Operator:
                case DeclarationKind.ConversionOperator:
                    return new MethodBuilder(parent, node);
                case DeclarationKind.Field:
                case DeclarationKind.EnumMember:
                case DeclarationKind.Event:
                    return new FieldBuilder(parent, node);
                case DeclarationKind.Attribute:
                    return new AttributeBuilder(parent, node);
                case DeclarationKind.Parameter:
                    return new ParameterBuilder(parent, node);
                case DeclarationKind.Property:
                case DeclarationKind.Indexer:
                case DeclarationKind.CustomEvent:
                    return new PropertyBuilder(parent, node);
                case DeclarationKind.AddAccessor:
                case DeclarationKind.RaiseAccessor:
                case DeclarationKind.RemoveAccessor:
                case DeclarationKind.SetAccessor:
                case DeclarationKind.GetAccessor:
                    return new AccessorBuilder(parent, node);
                case DeclarationKind.Delegate:
                    return new DelegateBuilder(parent, node);
                case DeclarationKind.LambdaExpression:
                case DeclarationKind.Variable:
                default:
                    throw new NotImplementedException();
            }
        }

        public static SyntaxBuilder Create(Workspace workspace, SyntaxNode node)
        {
            var generator = SyntaxGenerator.GetGenerator(workspace, node.Language);
            var context = new BuilderContext(workspace, node);

            switch (generator.GetDeclarationKind(node))
            {
                case DeclarationKind.CompilationUnit:
                    return new CompilationUnitBuilder(context);
                case DeclarationKind.Namespace:
                    return new NamespaceBuilder(context);
                case DeclarationKind.NamespaceImport:
                    return new NamespaceImportBuilder(context);
                case DeclarationKind.Class:
                case DeclarationKind.Interface:
                case DeclarationKind.Struct:
                case DeclarationKind.Enum:
                    return new TypeBuilder(context);
                case DeclarationKind.Method:
                case DeclarationKind.Constructor:
                case DeclarationKind.Destructor:
                case DeclarationKind.Operator:
                case DeclarationKind.ConversionOperator:
                    return new MethodBuilder(context);
                case DeclarationKind.Field:
                case DeclarationKind.EnumMember:
                case DeclarationKind.Event:
                    return new FieldBuilder(context);
                case DeclarationKind.Attribute:
                    return new AttributeBuilder(context);
                case DeclarationKind.Parameter:
                    return new ParameterBuilder(context);
                case DeclarationKind.Property:
                case DeclarationKind.Indexer:
                case DeclarationKind.CustomEvent:
                    return new PropertyBuilder(context);
                case DeclarationKind.AddAccessor:
                case DeclarationKind.RaiseAccessor:
                case DeclarationKind.RemoveAccessor:
                case DeclarationKind.SetAccessor:
                case DeclarationKind.GetAccessor:
                    return new AccessorBuilder(context);
                case DeclarationKind.Delegate:
                    return new DelegateBuilder(context);
                case DeclarationKind.LambdaExpression:
                case DeclarationKind.Variable:
                default:
                    throw new NotImplementedException();
            }
        }

        private static Type GetBuilderType(DeclarationKind kind)
        {
            switch (kind)
            {
                case DeclarationKind.CompilationUnit:
                    return typeof(CompilationUnitBuilder);
                case DeclarationKind.Namespace:
                    return typeof(NamespaceBuilder);
                case DeclarationKind.NamespaceImport:
                    return typeof(NamespaceImportBuilder);
                case DeclarationKind.Class:
                case DeclarationKind.Interface:
                case DeclarationKind.Struct:
                case DeclarationKind.Enum:
                    return typeof(TypeBuilder);
                case DeclarationKind.Method:
                case DeclarationKind.Constructor:
                case DeclarationKind.Destructor:
                case DeclarationKind.Operator:
                case DeclarationKind.ConversionOperator:
                    return typeof(MethodBuilder);
                case DeclarationKind.Field:
                case DeclarationKind.EnumMember:
                case DeclarationKind.Event:
                    return typeof(FieldBuilder);
                case DeclarationKind.Attribute:
                    return typeof(AttributeBuilder);
                case DeclarationKind.Parameter:
                    return typeof(ParameterBuilder);
                case DeclarationKind.Property:
                case DeclarationKind.Indexer:
                case DeclarationKind.CustomEvent:
                    return typeof(PropertyBuilder);
                case DeclarationKind.AddAccessor:
                case DeclarationKind.RaiseAccessor:
                case DeclarationKind.RemoveAccessor:
                case DeclarationKind.SetAccessor:
                case DeclarationKind.GetAccessor:
                    return typeof(AccessorBuilder);
                case DeclarationKind.Delegate:
                    return typeof(DelegateBuilder);
                case DeclarationKind.LambdaExpression:
                case DeclarationKind.Variable:
                default:
                    return null;
            }
        }
        #endregion
    }
}