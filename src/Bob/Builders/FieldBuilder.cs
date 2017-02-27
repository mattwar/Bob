using Microsoft.CodeAnalysis;

namespace Builders
{
    public class FieldBuilder : MemberBuilder
    {
        internal FieldBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal FieldBuilder(BuilderContext context)
            : base(context)
        {
        }

        public TypeExpression Type
        {
            get { return Generator.GetType(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithType(CurrentNode, value.ToSyntaxNode(Context))); }
        }

        public Expression Initializer
        {
            get { return Generator.GetExpression(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithExpression(CurrentNode, value.ToSyntaxNode(Context))); }
        }
    }
}