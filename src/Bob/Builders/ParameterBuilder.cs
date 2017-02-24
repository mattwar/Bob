using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Builders
{
    public class ParameterBuilder : MemberBuilder
    {
        internal ParameterBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal ParameterBuilder(BuilderContext context)
            : base(context)
        {
        }

        public SyntaxNode Type
        {
            get { return Generator.GetType(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithType(CurrentNode, value)); }
        }

        public string Default
        {
            get { return Generator.GetExpression(CurrentNode)?.ToString() ?? ""; }
            set { UpdateCurrentNode(Generator.WithExpression(CurrentNode, SyntaxFactory.ParseExpression(value, consumeFullText: true))); }
        }
    }
}