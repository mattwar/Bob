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

        public PropertyBuilder AddProperty(string name, SyntaxNode type)
        {
            return (PropertyBuilder)AddMember(Generator.PropertyDeclaration(name, type));
        }

        public PropertyBuilder AddIndexer(SyntaxNode type)
        {
            return (PropertyBuilder)AddMember(Generator.IndexerDeclaration(Array.Empty<SyntaxNode>(), type));
        }

        public FieldBuilder AddEvent(string name, SyntaxNode type)
        {
            return (FieldBuilder)AddMember(Generator.EventDeclaration(name, type));
        }

        public PropertyBuilder AddCustomEvent(string name, SyntaxNode type)
        {
            return (PropertyBuilder)AddMember(Generator.CustomEventDeclaration(name, type));
        }

        public FieldBuilder AddField(string name, SyntaxNode type)
        {
            return (FieldBuilder)AddMember(Generator.FieldDeclaration(name, type));
        }

        public FieldBuilder AddField(string name)
        {
            return AddField(name, Generator.TypeExpression(SpecialType.System_Int32));
        }

        public FieldBuilder AddEnumMember(string name, SyntaxNode value = null)
        {
            return (FieldBuilder)AddMember(Generator.EnumMember(name, value));
        }
    }
}