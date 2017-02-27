using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Builders
{
    public class MethodBuilder : MethodBaseBuilder
    {
        internal MethodBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal MethodBuilder(BuilderContext context)
            : base(context)
        {
        }

        public TypeExpression ReturnType
        {
            get { return Generator.GetType(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithType(CurrentNode, value.ToSyntaxNode(Context))); }
        }

        public IReadOnlyList<SyntaxNode> Statements
        {
            get { return Generator.GetStatements(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithStatements(CurrentNode, value)); }
        }

        public Expression Expression
        {
            get { return Generator.GetExpression(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithExpression(CurrentNode, value.ToSyntaxNode(Context))); }
        }
    }
}