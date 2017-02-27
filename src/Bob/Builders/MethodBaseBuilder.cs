using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Builders
{
    public abstract class MethodBaseBuilder : MemberBuilder
    {
        internal MethodBaseBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal MethodBaseBuilder(BuilderContext context)
            : base(context)
        {
        }

        private SyntaxBuilderList<ParameterBuilder> _parameters;
        public SyntaxBuilderList<ParameterBuilder> Parameters
        {
            get
            {
                if (_parameters == null)
                {
                    var parameterNodes = Generator.GetParameters(CurrentNode);
                    TrackNodes(parameterNodes);
                    _parameters = new SyntaxBuilderList<ParameterBuilder>(
                        this,
                        parameterNodes.Select(an => (ParameterBuilder)CreateChild(this, an)),
                        (g, r, n) => g.AddParameters(r, new[] { n }));
                }

                return _parameters;
            }
        }

        public ParameterBuilder AddParameter(string name, TypeExpression type = default(TypeExpression))
        {
            return AddParameter(Generator.ParameterDeclaration(name, type.ToSyntaxNode(Context)));
        }

        public ParameterBuilder AddParameter(SyntaxNode parameterNode)
        {
            return this.Parameters.Add(parameterNode);
        }

        internal override IEnumerable<SyntaxBuilder> Children => Parameters;
    }
}