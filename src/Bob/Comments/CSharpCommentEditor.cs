using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Editing;

namespace Builders
{
    internal class CSharpCommentEditor : CommentEditor
    {
        internal static readonly CSharpCommentEditor Singleton = new CSharpCommentEditor();

        private struct CommentSpan
        {
            public readonly int Start;
            public readonly int Length;

            public CommentSpan(int start, int length)
            {
                this.Start = start;
                this.Length = length;
            }

            public int End => Start + Length;
        }

        public override int GetCommentCount(SyntaxNode node)
        {
            return GetCommentSpans(node.GetLeadingTrivia()).Count();
        }

        private IEnumerable<CommentSpan> GetCommentSpans(SyntaxTriviaList list)
        {
            var spanList = new List<CommentSpan>();

            int offset = 0;
            while (offset < list.Count)
            {
                var start = offset;
                if (ScanCommentGroup(list, ref offset, SyntaxKind.SingleLineCommentTrivia)
                    || ScanCommentGroup(list, ref offset, SyntaxKind.MultiLineCommentTrivia)
                    || ScanComment(list, ref offset, SyntaxKind.SingleLineDocumentationCommentTrivia)
                    || ScanComment(list, ref offset, SyntaxKind.MultiLineDocumentationCommentTrivia))
                {
                    yield return new CommentSpan(start, offset - start);
                }
                else
                {
                    offset++;
                }
            }
        }

        private static bool ScanCommentGroup(SyntaxTriviaList list, ref int offset, SyntaxKind commentKind)
        {
            if (ScanComment(list, ref offset, commentKind))
            {
                // gather additional comment lines
                while (ScanComment(list, ref offset, commentKind))
                {
                    // do nothing...
                }

                return true;
            }

            return false;
        }

        private static bool ScanComment(SyntaxTriviaList list, ref int offset, SyntaxKind commentKind)
        {
            var index = offset;
            while (index < list.Count && list[index].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                index++;
            }

            if (index < list.Count && list[index].IsKind(commentKind))
            {
                index++;

                if (index < list.Count && list[index].IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    index++;
                }

                offset = index;
                return true;
            }

            return false;
        }

        private static bool IsEndOfLine(SyntaxTriviaList list, int position)
        {
            return (position >= 0 && position < list.Count && list[position].IsKind(SyntaxKind.EndOfLineTrivia));
        }

        private static bool IsEmptyLine(SyntaxTriviaList list, int position)
        {
            // an empty line is one that has two adjacent EOL's.
            return (position > 0 && position < list.Count && IsEndOfLine(list, position) && IsEndOfLine(list, position - 1));
        }

        private string GetCommentFullText(SyntaxNode node, CommentSpan span)
        {
            var list = node.GetLeadingTrivia();
            var builder = new StringBuilder();

            for (int i = span.Start; i < span.Start + span.Length; i++)
            {
                builder.Append(list[i].ToFullString());
            }

            return builder.ToString();
        }

        private CommentSpan GetCommentSpan(SyntaxNode node, int index)
        {
            return GetCommentSpans(node.GetLeadingTrivia()).ElementAt(index);
        }

        public override string GetCommentText(SyntaxNode node, int index)
        {
            var span = GetCommentSpan(node, index);
            var comment = CreateComment(node, span);
            return comment.Text;
        }

        public override CommentStyle GetCommentStyle(SyntaxNode node, int index)
        {
            var span = GetCommentSpan(node, index);
            var trivia = node.GetLeadingTrivia();
            switch (trivia[span.Start].Kind())
            {
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                    return CommentStyle.SingleLineBlock;
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                    return CommentStyle.Documentation;
                default:
                    throw new InvalidOperationException();
            }
        }

        private Comment CreateComment(SyntaxNode node, CommentSpan span)
        {
            var list = node.GetLeadingTrivia();
            var fullText = GetCommentFullText(node, span);

            var first = list.First(IsCommentTrivia);
            switch (first.Kind())
            {
                case SyntaxKind.SingleLineCommentTrivia:
                    return ParseComment(fullText, s_SingleLineCommentBlockPattern);
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                    return ParseComment(fullText, s_SingleLineDocumenationCommentBlockPattern);
                case SyntaxKind.MultiLineCommentTrivia:
                    return ParseComment(fullText, s_MultiLineCommentPattern);
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                    return ParseComment(fullText, s_MultiLineDocumentationCommentPattern);
                default:
                    throw new InvalidOperationException();
            }
        }

        private static bool IsCommentTrivia(SyntaxTrivia trivia)
        {
            switch (trivia.Kind())
            {
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                    return true;
                default:
                    return false;
            }
        }

        private static readonly CommentPattern s_SingleLineCommentBlockPattern
            = new CommentPattern("//", " ");

        private static readonly CommentPattern s_SingleLineDocumenationCommentBlockPattern
            = new CommentPattern("///", " ");

        private static readonly CommentPattern s_MultiLineCommentPattern
            = new CommentPattern(firstLineLeadingToken: "/*", firstLineGap: " ", secondLineLeadingToken: "  ", lastLineTrailingToken: "*/");

        private static readonly CommentPattern s_MultiLineDocumentationCommentPattern
            = new CommentPattern(firstLineLeadingToken: "/**", firstLineGap: " ", secondLineLeadingToken: " **", lastLineTrailingToken: "*/");

        private static Comment CreateComment(string text, CommentStyle style)
        {
            switch (style)
            {
                case CommentStyle.SingleLineBlock:
                    return Comment.From(s_SingleLineCommentBlockPattern, text);
                case CommentStyle.Documentation:
                    return Comment.From(s_SingleLineDocumenationCommentBlockPattern, text);
                case CommentStyle.MultiLineBlock:
                    return Comment.From(s_MultiLineCommentPattern, text);
                default:
                    throw new InvalidOperationException();
            }
        }

        public override SyntaxNode WithCommentText(SyntaxNode node, int index, string comment)
        {
            var span = GetCommentSpan(node, index);
            var newComment = CreateComment(node, span).WithText(comment);
            var newTrivia = SyntaxFactory.ParseLeadingTrivia(newComment.FullText);

            var currentList = node.GetLeadingTrivia();
            var newList = RemoveRange(currentList, span.Start, span.Length).InsertRange(span.Start, newTrivia);

            return node.WithLeadingTrivia(newList);
        }

        public override SyntaxNode WithCommentStyle(SyntaxNode node, int index, CommentStyle style)
        {
            var span = GetCommentSpan(node, index);
            var newComment = CreateComment(GetCommentText(node, index), style);
            var newTrivia = SyntaxFactory.ParseLeadingTrivia(newComment.FullText);

            var currentList = node.GetLeadingTrivia();
            var newList = RemoveRange(currentList, span.Start, span.Length).InsertRange(span.Start, newTrivia);

            return node.WithLeadingTrivia(newList);
        }

        private static SyntaxTriviaList RemoveRange(SyntaxTriviaList list, int start, int length)
        {
            for (; length > 0; length--)
            {
                list = list.RemoveAt(start);
            }

            return list;
        }

        public override SyntaxNode AddComment(SyntaxNode node, string comment, CommentStyle style)
        {
            var newComment = CreateComment(comment, style);
            var newTrivia = SyntaxFactory.ParseLeadingTrivia(newComment.FullText);

            var newList = node.GetLeadingTrivia();

#if false
            if (newList.Count > 0 && !IsEmptyLine(newList, newList.Count - 1))
            {
                newList = newList.Add(SyntaxFactory.ElasticCarriageReturnLineFeed);
            }
#endif

            newList = newList.AddRange(newTrivia);

            if (newList.Count > 0 && !IsEndOfLine(newList, newList.Count - 1))
            {
                newList = newList.Add(SyntaxFactory.ElasticCarriageReturnLineFeed);
            }

            return node.WithLeadingTrivia(newList);
        }

        public override SyntaxNode InsertComment(SyntaxNode node, int index, string comment, CommentStyle style = CommentStyle.SingleLineBlock)
        {
            throw new NotImplementedException();
        }

        public override SyntaxNode RemoveComment(SyntaxNode node, int index)
        {
            throw new NotImplementedException();
        }

        #region Comment Parsing

        private static Comment ParseComment(string text, CommentPattern pattern)
        {
            var lines = new List<CommentLine>();

            int offset = 0;
            while (offset < text.Length)
            {
                var line = ParseCommentLine(ref offset, text, pattern, isFirstLine: lines.Count == 0);
                lines.Add(line);
            }

            return new Comment(pattern, lines.ToImmutableArray());
        }

        private static CommentLine ParseCommentLine(ref int offset, string text, CommentPattern pattern, bool isFirstLine)
        {
            var leadingToken = isFirstLine ? pattern.FirstLineLeadingToken : pattern.SecondLineLeadingToken;
            var gapText = isFirstLine ? pattern.FirstLineGap : pattern.SecondLineGap; // what about last line gap?
            var trailingToken = pattern.LastLineTrailingToken;

            // parse line prefix
            var prefixStart = offset;

            // keep going while have whitespace, but have not found the token.
            while (offset < text.Length)
            {
                if (char.IsWhiteSpace(text[offset]))
                {
                    offset++;
                }
                else if (Matches(offset, text, leadingToken))
                {
                    offset += leadingToken.Length;

                    // include next whitespace after token in prefix text
                    if (offset < text.Length && Matches(offset, text, gapText))
                    {
                        offset += gapText.Length;
                    }
                    break;
                }
                else
                {
                    // not whitespace, but not expected prefix
                    break;
                }
            }

            var prefixSpan = TextSpan.FromBounds(prefixStart, offset);

            // parse text (read until EOL or no more text)
            var textStart = offset;
            for (; offset < text.Length; offset++)
            {
                var ch = text[offset];
                if (ch == '\r' || ch == '\n')
                {
                    break;
                }
            }

            var textSpan = TextSpan.FromBounds(textStart, offset);

            // default postfix as zero-width here and fix up later if this is last line
            var postfixSpan = TextSpan.FromBounds(offset, offset);

            // parse EOL
            var eolStart = offset;
            if (offset < text.Length)
            {
                if (text[offset] == '\r')
                {
                    offset++;
                    if (offset < text.Length && text[offset] == '\n')
                    {
                        offset++;
                    }
                }
                else if (text[offset] == '\n')
                {
                    offset++;
                }
            }

            var eolSpan = TextSpan.FromBounds(eolStart, offset);

            var isBanner = isFirstLine && IsBanner(text, textSpan);

            // we are at the end .. check for actual postfix text
            if (offset == text.Length && trailingToken != null)
            {
                int postfixStart = textSpan.End - trailingToken.Length;
                if (textSpan.Length >= trailingToken.Length && Matches(postfixStart, text, trailingToken))
                {
                    // include most immediate whitespace in postfix
                    if (gapText.Length > 0 && postfixStart > textSpan.Start && Matches(postfixStart - gapText.Length, text, gapText))
                    {
                        postfixStart--;
                    }

                    textSpan = TextSpan.FromBounds(textSpan.Start, postfixStart);
                    postfixSpan = TextSpan.FromBounds(postfixStart, eolSpan.Start);
                    isBanner = IsBanner(text, textSpan);
                }
            }

            return new CommentLine(
                prefix: text.Substring(prefixSpan.Start, prefixSpan.Length),
                text: !isBanner ? text.Substring(textSpan.Start, textSpan.Length) : "",
                banner: isBanner ? text.Substring(textSpan.Start, textSpan.Length) : null,
                postfix: postfixSpan.Length > 0 ? text.Substring(postfixSpan.Start, postfixSpan.Length) : null,
                endOfLine: eolSpan.Length > 0 ? text.Substring(eolSpan.Start, eolSpan.Length) : null);
        }

        private static bool IsBanner(string text, TextSpan span)
        {
            // any unbroken sequence of characters that is not letters/digits is considered a banner and not part of the comment
            for (int i = span.Start; i < span.End; i++)
            {
                var ch = text[i];
                if (char.IsWhiteSpace(ch) || char.IsLetterOrDigit(ch))
                {
                    return false;
                }
            }

            return false;
        }

        private static bool Matches(int start, string text, string match)
        {
            if (start + match.Length > text.Length)
            {
                return false;
            }

            for (int i = 0; i < match.Length; i++)
            {
                if (match[i] != text[start + i])
                {
                    return false;
                }
            }

            return true;
        }
        #endregion


        public override IReadOnlyList<string> GetTypeParameterNames(SyntaxNode node)
        {
            switch (node)
            {
                case TypeDeclarationSyntax td:
                    return GetTypeParameterNames(td.TypeParameterList);

                case MethodDeclarationSyntax md:
                    return GetTypeParameterNames(md.TypeParameterList);

                case DelegateDeclarationSyntax dd:
                    return GetTypeParameterNames(dd.TypeParameterList);

                default:
                    return Array.Empty<string>();
            }
        }

        private IReadOnlyList<string> GetTypeParameterNames(TypeParameterListSyntax list)
        {
            return list?.Parameters.Select(p => p.Identifier.Text).ToArray() ?? Array.Empty<string>();
        }

        public override IReadOnlyList<SyntaxNode> GetTypeConstraints(SyntaxNode node, string typeParameterName)
        {
            switch (node)
            {
                case TypeDeclarationSyntax td:
                    return GetTypeConstraints(td.ConstraintClauses, typeParameterName);

                case MethodDeclarationSyntax md:
                    return GetTypeConstraints(md.ConstraintClauses, typeParameterName);

                case DelegateDeclarationSyntax dd:
                    return GetTypeConstraints(dd.ConstraintClauses, typeParameterName);

                default:
                    return Array.Empty<SyntaxNode>();
            }
        }

        private IReadOnlyList<SyntaxNode> GetTypeConstraints(SyntaxList<TypeParameterConstraintClauseSyntax> clauses, string name)
        {
            return clauses.Where(cc => cc.Name.Identifier.Text == name).SelectMany(c => c.Constraints).OfType<TypeConstraintSyntax>().Select(c => c.Type).ToArray() ?? Array.Empty<SyntaxNode>();
        }

        public override SpecialTypeConstraintKind GetSpecialTypeConstraints(SyntaxNode node, string typeParameterName)
        {
            switch (node)
            {
                case TypeDeclarationSyntax td:
                    return GetSpecialTypeConstraints(td.ConstraintClauses, typeParameterName);

                case MethodDeclarationSyntax md:
                    return GetSpecialTypeConstraints(md.ConstraintClauses, typeParameterName);

                case DelegateDeclarationSyntax dd:
                    return GetSpecialTypeConstraints(dd.ConstraintClauses, typeParameterName);

                default:
                    return SpecialTypeConstraintKind.None;
            }
        }

        private SpecialTypeConstraintKind GetSpecialTypeConstraints(SyntaxList<TypeParameterConstraintClauseSyntax> clauses, string name)
        {
            var kind = SpecialTypeConstraintKind.None;
            foreach (var constraint in clauses.Where(cc => cc.Name.Identifier.Text == name).SelectMany(c => c.Constraints))
            {
                switch (constraint.Kind())
                {
                    case SyntaxKind.ClassConstraint:
                        kind |= SpecialTypeConstraintKind.ReferenceType;
                        break;
                    case SyntaxKind.StructConstraint:
                        kind |= SpecialTypeConstraintKind.ValueType;
                        break;
                    case SyntaxKind.ConstructorConstraint:
                        kind |= SpecialTypeConstraintKind.Constructor;
                        break;
                }
            }

            return kind;
        }

        public override SyntaxNode WithTypeParameterNameChanged(SyntaxNode node, string typeParameterName, string newTypeParameterName)
        {
            var nodesToChange = GetTypeParameterDeclarationNodes(node, typeParameterName);
            return node.ReplaceNodes(nodesToChange, (o, r) => RenameNode(r, newTypeParameterName));
        }

        private IEnumerable<SyntaxNode> GetTypeParameterDeclarationNodes(SyntaxNode node, string typeParameterName)
        {
            switch (node)
            {
                case TypeDeclarationSyntax td:
                    return td.TypeParameterList.Parameters.Where(p => p.Identifier.Text == typeParameterName).Cast<SyntaxNode>()
                        .Concat(td.ConstraintClauses.Where(cc => cc.Name.Identifier.Text == typeParameterName));

                case MethodDeclarationSyntax md:
                    return md.TypeParameterList.Parameters.Where(p => p.Identifier.Text == typeParameterName).Cast<SyntaxNode>()
                        .Concat(md.ConstraintClauses.Where(cc => cc.Name.Identifier.Text == typeParameterName));

                case DelegateDeclarationSyntax dd:
                    return dd.TypeParameterList.Parameters.Where(p => p.Identifier.Text == typeParameterName).Cast<SyntaxNode>()
                        .Concat(dd.ConstraintClauses.Where(cc => cc.Name.Identifier.Text == typeParameterName));

                default:
                    return Array.Empty<SyntaxNode>();
            }
        }

        private static SyntaxNode RenameNode(SyntaxNode node, string newName)
        {
            switch (node)
            {
                case TypeParameterSyntax tp:
                    return tp.WithIdentifier(SyntaxFactory.Identifier(newName).WithTriviaFrom(tp.Identifier));

                case TypeParameterConstraintClauseSyntax cc:
                    return cc.WithName(SyntaxFactory.IdentifierName(newName).WithTriviaFrom(cc.Name));

                default:
                    return node;
            }
        }
    }
}
