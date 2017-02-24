using Microsoft.CodeAnalysis;

namespace Builders
{
    public class AttributeArgumentBuilder : SyntaxBuilder
    {
        internal AttributeArgumentBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        public string Name
        {
            get { return Generator.GetName(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithName(CurrentNode, value)); }
        }

        public SyntaxNode Expression
        {
            get { return Generator.GetExpression(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithExpression(CurrentNode, value)); }
        }
    }
}