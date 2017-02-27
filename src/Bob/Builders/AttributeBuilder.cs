using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Builders
{
    public class AttributeBuilder : SyntaxBuilder
    {
        internal AttributeBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal AttributeBuilder(BuilderContext context)
            : base(context)
        {
        }

        public string Name
        {
            get { return Generator.GetName(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithName(CurrentNode, value)); }
        }

        private SyntaxBuilderList<AttributeArgumentBuilder> _arguments;
        public SyntaxBuilderList<AttributeArgumentBuilder> Arguments
        {
            get
            {
                if (_arguments == null)
                {
                    var argumentNodes = Generator.GetAttributeArguments(CurrentNode);
                    TrackNodes(argumentNodes);
                    _arguments = new SyntaxBuilderList<AttributeArgumentBuilder>(
                        this,
                        argumentNodes.Select(n => new AttributeArgumentBuilder(this, n)),
                        (g, r, n) => g.AddAttributeArguments(r, new[] { n }),
                        (b, n) => new AttributeArgumentBuilder(b, n));
                }

                return _arguments;
            }
        }

        public AttributeArgumentBuilder AddArgument(Expression expression)
        {
            return AddArgumentNode(Generator.AttributeArgument(expression.ToSyntaxNode(Context)));
        }

        public AttributeArgumentBuilder AddArgument(string name, Expression expression)
        {
            return AddArgumentNode(Generator.AttributeArgument(name, expression.ToSyntaxNode(Context)));
        }

        private AttributeArgumentBuilder AddArgumentNode(SyntaxNode argumentNode)
        {
            return this.Arguments.Add(argumentNode);
        }

        internal override IEnumerable<SyntaxBuilder> Children => Arguments;
    }
}