namespace Builders
{
    public enum CommentStyle
    {
        /// <summary>
        /// This is a comment block that is formed from one or more single line comments
        /// </summary>
        SingleLineBlock = 0,

        Default = SingleLineBlock,

        /// <summary>
        /// This is a comment block formed by a single multi-line style comment  /* .. */
        /// </summary>
        MultiLineBlock = 1,

        /// <summary>
        /// This is a comment block that specifies a documentation comment.
        /// </summary>
        Documentation = 2
    }
}
