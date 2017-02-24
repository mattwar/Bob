using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Builders
{
    public class NamespaceBuilder : SyntaxBuilder
    {
        internal NamespaceBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal NamespaceBuilder(BuilderContext context)
            : base(context)
        {
        }

        public string Name
        {
            get { return Generator.GetName(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithName(CurrentNode, value)); }
        }

        public IEnumerable<NamespaceImportBuilder> Imports => Members.OfType<NamespaceImportBuilder>();
        public IEnumerable<TypeBuilder> Types => Members.OfType<TypeBuilder>();
        public IEnumerable<NamespaceBuilder> Namespaces => Members.OfType<NamespaceBuilder>();

        public NamespaceBuilder AddNamespace(string name)
        {
            return (NamespaceBuilder)AddMember(Generator.NamespaceDeclaration(name));
        }

        public NamespaceImportBuilder AddNamespaceImport(SyntaxNode type)
        {
            return (NamespaceImportBuilder)AddMember(Generator.NamespaceImportDeclaration(type));
        }

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
    }
}