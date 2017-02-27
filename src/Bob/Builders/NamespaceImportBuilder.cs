using Microsoft.CodeAnalysis;

namespace Builders
{
    public class NamespaceImportBuilder : SyntaxBuilder
    {
        internal NamespaceImportBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal NamespaceImportBuilder(BuilderContext context)
            : base(context)
        {
        }

        public string Name
        {
            get { return Generator.GetName(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithName(CurrentNode, value)); }
        }

        public TypeExpression Type
        {
            get { return Generator.GetType(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithType(CurrentNode, value.ToSyntaxNode(Context))); }
        }
    }
}