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

        public TypeExpression Type
        {
            get { return Generator.GetType(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithType(CurrentNode, value.ToSyntaxNode(Context))); }
        }

        public Expression Default
        {
            get { return Generator.GetExpression(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithExpression(CurrentNode, value.ToSyntaxNode(Context))); }
        }
    }
}