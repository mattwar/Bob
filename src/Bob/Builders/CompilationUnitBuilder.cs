using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Builders
{
    public class CompilationUnitBuilder : SyntaxBuilder
    {
        internal CompilationUnitBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal CompilationUnitBuilder(BuilderContext context)
            : base(context)
        {
        }

        public IEnumerable<NamespaceImportBuilder> Imports => Members.OfType<NamespaceImportBuilder>();
        public IEnumerable<TypeBuilder> Types => Members.OfType<TypeBuilder>();
        public IEnumerable<NamespaceBuilder> Namespaces => Members.OfType<NamespaceBuilder>();

        public NamespaceBuilder AddNamespace(string name)
        {
            return (NamespaceBuilder)AddMember(Generator.NamespaceDeclaration(name));
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