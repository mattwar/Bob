using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

        public TypeParameterList TypeParameters => TypeParameterList.GetList(this);

        public TypeExpression ReturnType
        {
            get { return Generator.GetType(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithType(CurrentNode, value.ToSyntaxNode(Context))); }
        }
    }
}