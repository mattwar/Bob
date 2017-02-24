using Microsoft.CodeAnalysis;

namespace Builders
{
    public abstract class NamespaceMemberBuilder : SyntaxBuilder
    {
        internal NamespaceMemberBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal NamespaceMemberBuilder(BuilderContext context)
            : base(context)
        {
        }
    }
}