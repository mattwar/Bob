namespace Builders
{
    public class CommentBuilder
    {
        private readonly CommentBuilderList _list;

        internal CommentBuilder(CommentBuilderList list)
        {
            _list = list;
        }

        private string _text;
        public string Text
        {
            get
            {
                if (_text == null)
                {
                    _text = _list.Editor.GetCommentText(_list.NodeBuilder.CurrentNode, _list.IndexOf(this));
                }

                return _text;
            }

            set
            {
                var newNode = _list.Editor.WithCommentText(_list.NodeBuilder.CurrentNode, _list.IndexOf(this), value);
                _list.NodeBuilder.UpdateCurrentNode(newNode);
                _text = value;
            }
        }

        public CommentStyle Style
        {
            get
            {
                return _list.Editor.GetCommentStyle(_list.NodeBuilder.CurrentNode, _list.IndexOf(this));
            }

            set
            {
                var newNode = _list.Editor.WithCommentStyle(_list.NodeBuilder.CurrentNode, _list.IndexOf(this), value);
                _list.NodeBuilder.UpdateCurrentNode(newNode);
            }
        }
    }
}