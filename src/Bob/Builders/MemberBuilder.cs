using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Builders
{
    public abstract class MemberBuilder : SyntaxBuilder
    {
        internal MemberBuilder(SyntaxBuilder parent, SyntaxNode node)
            : base(parent, node)
        {
        }

        internal MemberBuilder(BuilderContext context)
            : base(context)
        {
        }

        private SyntaxBuilderList<AttributeBuilder> _attributes;
        public SyntaxBuilderList<AttributeBuilder> Attributes
        {
            get
            {
                if (_attributes == null)
                {
                    var attributeNodes = Generator.GetAttributes(CurrentNode);
                    TrackNodes(attributeNodes);
                    _attributes = new SyntaxBuilderList<AttributeBuilder>(
                        this,
                        attributeNodes.Select(n => (AttributeBuilder)CreateChild(this, n)),
                        (g, r, n) => g.AddAttributes(r, n));
                }

                return _attributes;
            }
        }

        public AttributeBuilder AddAttribute(string name)
        {
            return AddAttribute(Generator.Attribute(name));
        }

        public AttributeBuilder AddAttribute(SyntaxNode attributeNode)
        {
            return this.Attributes.Add(attributeNode);
        }

        public Accessibility Accessibility
        {
            get { return Generator.GetAccessibility(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithAccessibility(CurrentNode, value)); }
        }

        public DeclarationModifiers Modifiers
        {
            get { return Generator.GetModifiers(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithModifiers(CurrentNode, value)); }
        }

        public string Name
        {
            get { return Generator.GetName(CurrentNode); }
            set { UpdateCurrentNode(Generator.WithName(CurrentNode, value)); }
        }
    }
}