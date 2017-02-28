using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Builders
{
    public class AccessorBuilder : SyntaxBuilder
    {
        internal AccessorBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal AccessorBuilder(BuilderContext context)
            : base(context)
        {
        }

        public DeclarationModifiers Modifiers
        {
            get { return Generator.GetModifiers(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithModifiers(CurrentNode, value)); }
        }

        public StatementList Statements => StatementList.GetList(this);
    }
}