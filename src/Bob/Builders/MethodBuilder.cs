using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Linq;

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

        public TypeParameterList TypeParameters => TypeParameterList.GetList(this);

        public TypeExpression ReturnType
        {
            get { return Generator.GetType(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithType(CurrentNode, value.ToSyntaxNode(Context))); }
        }

        public StatementList Statements => StatementList.GetList(this);

        public Expression Expression
        {
            get { return Generator.GetExpression(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithExpression(CurrentNode, value.ToSyntaxNode(Context))); }
        }
    }
}