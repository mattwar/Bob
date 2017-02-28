using System;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Editing;

namespace Builders
{
    public abstract partial class CommentEditor
    {
        public static CommentEditor GetEditor(Workspace workspace, string language)
        {
            if (language == LanguageNames.CSharp)
            {
                return CSharpCommentEditor.Singleton;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public abstract int GetCommentCount(SyntaxNode node);
        public abstract string GetCommentText(SyntaxNode node, int index);
        public abstract CommentStyle GetCommentStyle(SyntaxNode node, int index);
        public abstract SyntaxNode WithCommentText(SyntaxNode node, int index, string comment);
        public abstract SyntaxNode WithCommentStyle(SyntaxNode node, int index, CommentStyle style);
        public abstract SyntaxNode AddComment(SyntaxNode node, string comment, CommentStyle style = CommentStyle.SingleLineBlock);
        public abstract SyntaxNode InsertComment(SyntaxNode node, int index, string comment, CommentStyle style = CommentStyle.SingleLineBlock);
        public abstract SyntaxNode RemoveComment(SyntaxNode node, int index);

        // code missing from SyntaxGenerator
        public abstract IReadOnlyList<string> GetTypeParameterNames(SyntaxNode node);
        public abstract IReadOnlyList<SyntaxNode> GetTypeConstraints(SyntaxNode node, string typeParameterName);
        public abstract SpecialTypeConstraintKind GetSpecialTypeConstraints(SyntaxNode node, string typeParameterName);
        public abstract SyntaxNode WithTypeParameterNameChanged(SyntaxNode node, string typeParameterName, string newTypeParameterName);
    }
}