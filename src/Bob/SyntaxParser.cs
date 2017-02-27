using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;

namespace Builders
{
    public abstract class SyntaxParser : ILanguageService
    {
        public static SyntaxParser GetParser(Workspace workspace, string language)
        {
            return workspace.Services.GetLanguageServices(language).GetService<SyntaxParser>();
        }

        public abstract SyntaxNode ParseExpression(string expression);
    }
}