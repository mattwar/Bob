using Microsoft.CodeAnalysis;

namespace Builders
{
    public class DelegateBuilder : MethodBaseBuilder
    {
        internal DelegateBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal DelegateBuilder(BuilderContext context)
            : base(context)
        {
        }

        public SyntaxNode ReturnType
        {
            get { return Generator.GetType(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithType(CurrentNode, value)); }
        }
    }
}