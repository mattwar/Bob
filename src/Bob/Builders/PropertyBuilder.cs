using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Builders
{
    public class PropertyBuilder : MemberBuilder
    {
        internal PropertyBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal PropertyBuilder(BuilderContext context)
            : base(context)
        {
        }

        public TypeExpression Type
        {
            get { return Generator.GetType(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithType(CurrentNode, value.ToSyntaxNode(Context))); }
        }

        public Expression Expression
        {
            get { return Generator.GetExpression(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithExpression(CurrentNode, value.ToSyntaxNode(Context))); }
        }

        private SyntaxBuilderList<AccessorBuilder> _accessors;
        public SyntaxBuilderList<AccessorBuilder> Accessors
        {
            get
            {
                if (_accessors == null)
                {
                    var accessorNodes = Generator.GetAccessors(CurrentNode);
                    TrackNodes(accessorNodes);
                    _accessors = new SyntaxBuilderList<AccessorBuilder>(
                        this,
                        accessorNodes.Select(an => (AccessorBuilder)CreateChild(this, an)),
                        (g, r, n) => g.AddAccessors(r, new[] { n }));
                }

                return _accessors;
            }
        }

        private AccessorBuilder AddAccessor(SyntaxNode accessorNode)
        {
            return this.Accessors.Add(accessorNode);
        }

        internal override IEnumerable<SyntaxBuilder> Children => Accessors;
    }
}