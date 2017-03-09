using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Builders
{
    public class CommentBuilderList : IReadOnlyList<CommentBuilder>
    {
        private readonly SyntaxBuilder _nodeBuilder;
        private readonly List<CommentBuilder> _list;

        internal CommentBuilderList(SyntaxBuilder nodeBuilder)
        {
            _nodeBuilder = nodeBuilder;
            _list = Enumerable.Range(0, Editor.GetCommentCount(nodeBuilder.CurrentNode)).Select(n => new CommentBuilder(this)).ToList();
        }

        internal SyntaxBuilder NodeBuilder => _nodeBuilder;
        internal CommentEditor Editor => _nodeBuilder.CommentEditor;

        public CommentBuilder AddComment(string comment, CommentStyle style = CommentStyle.SingleLineBlock)
        {
            var newNode = Editor.AddComment(_nodeBuilder.CurrentNode, comment, style);
            _nodeBuilder.UpdateCurrentNode(newNode);
            var newBuilder = new CommentBuilder(this);
            _list.Add(newBuilder);
            return newBuilder;
        }

        public CommentBuilder InsertComment(int index, string comment, CommentStyle style = CommentStyle.SingleLineBlock)
        {
            var newNode = Editor.InsertComment(_nodeBuilder.CurrentNode, index, comment, style);
            _nodeBuilder.UpdateCurrentNode(newNode);
            var newBuilder = new CommentBuilder(this);
            _list.Insert(index, newBuilder);
            return newBuilder;
        }

        public void Remove(CommentBuilder builder)
        {
            throw new NotImplementedException();
        }

        public int Count => _list.Count;

        public CommentBuilder this[int index] => _list[index];

        public int IndexOf(CommentBuilder builder)
        {
            return _list.IndexOf(builder);
        }

        public IEnumerator<CommentBuilder> GetEnumerator()
        {
            return ((IReadOnlyList<CommentBuilder>)_list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IReadOnlyList<CommentBuilder>)_list).GetEnumerator();
        }
    }
}
