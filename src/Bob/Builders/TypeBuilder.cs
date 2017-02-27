using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Builders
{
    public class TypeBuilder : MemberBuilder
    {
        internal TypeBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal TypeBuilder(BuilderContext context)
            : base(context)
        {
        }

        public IEnumerable<MethodBuilder> Methods => Members.OfType<MethodBuilder>();
        public IEnumerable<FieldBuilder> Fields => Members.OfType<FieldBuilder>();

        public TypeBuilder AddClass(string name)
        {
            return (TypeBuilder)AddMember(Generator.ClassDeclaration(name));
        }

        public TypeBuilder AddInterface(string name)
        {
            return (TypeBuilder)AddMember(Generator.InterfaceDeclaration(name));
        }

        public TypeBuilder AddStruct(string name)
        {
            return (TypeBuilder)AddMember(Generator.StructDeclaration(name));
        }

        public TypeBuilder AddEnum(string name)
        {
            return (TypeBuilder)AddMember(Generator.EnumDeclaration(name));
        }

        public DelegateBuilder AddDelegate(string name)
        {
            return (DelegateBuilder)AddMember(Generator.DelegateDeclaration(name));
        }

        public MethodBuilder AddMethod(string name)
        {
            return (MethodBuilder)AddMember(Generator.MethodDeclaration(name));
        }

        public PropertyBuilder AddProperty(string name, TypeExpression type)
        {
            return (PropertyBuilder)AddMember(Generator.PropertyDeclaration(name, type.ToSyntaxNode(Context)));
        }

        public PropertyBuilder AddIndexer(SyntaxNode type)
        {
            return (PropertyBuilder)AddMember(Generator.IndexerDeclaration(Array.Empty<SyntaxNode>(), type));
        }

        public FieldBuilder AddEvent(string name, TypeExpression type)
        {
            return (FieldBuilder)AddMember(Generator.EventDeclaration(name, type.ToSyntaxNode(Context)));
        }

        public PropertyBuilder AddCustomEvent(string name, TypeExpression type)
        {
            return (PropertyBuilder)AddMember(Generator.CustomEventDeclaration(name, type.ToSyntaxNode(Context)));
        }

        public FieldBuilder AddField(string name, TypeExpression type)
        {
            return (FieldBuilder)AddMember(Generator.FieldDeclaration(name, type.ToSyntaxNode(Context)));
        }

        public FieldBuilder AddField(string name)
        {
            return AddField(name, Generator.TypeExpression(SpecialType.System_Int32));
        }

        public FieldBuilder AddEnumMember(string name, Expression value = default(Expression))
        {
            return (FieldBuilder)AddMember(Generator.EnumMember(name, value.ToSyntaxNode(Context)));
        }
    }
}