using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Builders
{
    [ExportLanguageService(typeof(SyntaxParser), LanguageNames.CSharp)]
    public class CSharpSyntaxParser : SyntaxParser
    {
        public override SyntaxNode ParseExpression(string expression)
        {
            return SyntaxFactory.ParseExpression(expression);
        }
    }
}