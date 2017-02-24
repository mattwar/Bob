using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Builders
{
    internal class CommentPattern
    {
        /// <summary>
        /// The initial token used on the first line.  
        /// </summary>
        public readonly string FirstLineLeadingToken;

        /// <summary>
        /// Additional whitespace (if any) after the first line's prefix, but not part of the comment text.
        /// </summary>
        public readonly string FirstLineGap;

        /// <summary>
        /// The initial token used on subsequence lines...
        /// </summary>
        public readonly string SecondLineLeadingToken;

        /// <summary>
        /// Additional whitespace (if any) after any secondary line's leading token, but not part of the comment text.
        /// </summary>
        public readonly string SecondLineGap;

        /// <summary>
        /// The last token used on the last line, before EOL. Possibly null if there is no special trailing token.
        /// </summary>
        public readonly string LastLineTrailingToken;

        /// <summary>
        /// Additional whitespace (if any) before the trailing token, but not part of the comment text.
        /// </summary>
        public readonly string LastLineGap;

        public CommentPattern(
            string firstLineLeadingToken,
            string firstLineGap = null,
            string secondLineLeadingToken = null,
            string secondLineGap = null,
            string lastLineGap = null,
            string lastLineTrailingToken = null)
        {
            this.FirstLineLeadingToken = firstLineLeadingToken;
            this.FirstLineGap = firstLineGap ?? "";
            this.SecondLineLeadingToken = secondLineLeadingToken ?? firstLineLeadingToken;
            this.SecondLineGap = secondLineGap ?? firstLineGap ?? "";
            this.LastLineGap = lastLineGap ?? firstLineGap ?? "";
            this.LastLineTrailingToken = lastLineTrailingToken;
        }
    }

    internal struct CommentLine
    {
        /// <summary>
        /// The text (including whitespace) before the start of the line's comment text.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Text that is considered banner text and appears on the line instead of comment text
        /// </summary>
        public string Banner { get; }

        /// <summary>
        /// The text of the comment on the line.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The text (including trailing whitespace) after the comment text (if any) on the last line of the comment.
        /// </summary>
        public string Postfix { get; }

        /// <summary>
        /// The end of line characters for the comment line.
        /// </summary>
        public string EndOfLine { get; }

        public CommentLine(string prefix = null, string banner = null, string text = null, string postfix = null, string endOfLine = null)
        {
            Prefix = prefix;
            Banner = banner;
            Text = text;
            Postfix = postfix;
            EndOfLine = endOfLine;
        }
    }

    internal class Comment
    {
        private readonly CommentPattern _pattern;
        private readonly ImmutableArray<CommentLine> _lines;

        public Comment(CommentPattern pattern, ImmutableArray<CommentLine> lines)
        {
            _pattern = pattern;
            _lines = lines;
        }

        public static Comment From(CommentPattern pattern, string text)
        {
            var line = new CommentLine(
                prefix: pattern.FirstLineLeadingToken + pattern.FirstLineGap,
                postfix: pattern.LastLineTrailingToken != null ? pattern.LastLineGap + pattern.LastLineTrailingToken : null);
            return new Comment(pattern, ImmutableArray.Create(line)).WithText(text);
        }

        private string _fullText;
        public string FullText
        {
            get
            {
                if (_fullText == null)
                {
                    var builder = new StringBuilder();
                    foreach (var line in _lines)
                    {
                        builder.Append(line.Prefix);
                        builder.Append(line.Text);

                        if (line.Postfix != null)
                        {
                            builder.Append(line.Postfix);
                        }

                        if (line.EndOfLine != null)
                        {
                            builder.Append(line.EndOfLine);
                        }
                    }

                    _fullText = builder.ToString();
                }

                return _fullText;
            }
        }

        private string _text;
        public string Text
        {
            get
            {
                if (_text == null)
                {
                    var builder = new StringBuilder();
                    foreach (var line in _lines)
                    {
                        builder.Append(line.Text);
                        if (line.EndOfLine != null)
                        {
                            builder.Append(line.EndOfLine);
                        }
                    }

                    _text = builder.ToString();
                }

                return _text;
            }
        }

        public Comment WithText(string text)
        {
            var newLines = new List<CommentLine>();
            var source = SourceText.From(text);

            int reuseIndex = 0;

            // if the first line had a banner, just reuse this line
            if (_lines[0].Banner != null)
            {
                newLines.Add(_lines[0]);
                reuseIndex = 1;
            }

            int lastReuseIndex = _lines.Length - 1;

            // if the last line had a banner, don't use it to expand the comment text.
            if (lastReuseIndex > 0 && _lines[lastReuseIndex].Banner != null)
            {
                lastReuseIndex--;
            }

            var lastSourceLine = source.Lines.Count - 1;
            if (lastSourceLine > 0 && source.Lines[lastSourceLine].SpanIncludingLineBreak.Length == 0)
            {
                lastSourceLine--;
            }

            for (int sourceLineIndex = 0; sourceLineIndex <= lastSourceLine; sourceLineIndex++)
            {
                var line = source.Lines[sourceLineIndex];
                var lineText = source.GetSubText(line.Span).ToString();
                var lineEol = line.SpanIncludingLineBreak.End > line.Span.End ? source.GetSubText(TextSpan.FromBounds(line.Span.End, line.SpanIncludingLineBreak.End)).ToString() : null;

                string prefix = null;
                if (reuseIndex <= lastReuseIndex && (sourceLineIndex == 0 || reuseIndex > 0))
                {
                    // try to reuse any previous line prefix as long as we have them
                    prefix = _lines[reuseIndex].Prefix;
                }
                else
                {
                    // make one up using existing whitespace and secondary prefix
                    prefix = GetLeadingWhitespace(_lines[reuseIndex].Prefix) + _pattern.SecondLineLeadingToken + _pattern.SecondLineGap;
                }

                string postfix = null;
                if (sourceLineIndex == lastSourceLine && _lines[_lines.Length - 1].Banner == null)
                {
                    postfix = _lines[_lines.Length - 1].Postfix;
                }

                string eol = _lines[reuseIndex].EndOfLine ?? lineEol;

                var newLine = new CommentLine(prefix: prefix, text: lineText, postfix: postfix, endOfLine: eol);
                newLines.Add(newLine);

                if (reuseIndex < lastReuseIndex)
                {
                    reuseIndex++;
                }
            }

            // if the last line had a banner, reuse it at the last line of the full comment
            if (_lines.Length > 1 && _lines[_lines.Length - 1].Banner != null)
            {
                newLines.Add(_lines[_lines.Length - 1]);
            }

            return new Comment(_pattern, newLines.ToImmutableArray());
        }

        private string GetLeadingWhitespace(string text)
        {
            int i = 0;
            while (char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            return i > 0 ? text.Substring(0, i) : "";
        }
    }
}
