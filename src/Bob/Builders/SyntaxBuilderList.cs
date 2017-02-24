using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Builders
{
    public class SyntaxBuilderList<T> : IReadOnlyList<T>
        where T : SyntaxBuilder
    {
        private readonly SyntaxBuilder _parent;
        private readonly List<T> _list;
        private readonly SyntaxBuilder.NodeAdder _adder;
        private readonly Func<SyntaxBuilder, SyntaxNode, SyntaxBuilder> _creator;

        internal SyntaxBuilderList(
            SyntaxBuilder parent,
            IEnumerable<T> builders,
            SyntaxBuilder.NodeAdder adder,
            Func<SyntaxBuilder, SyntaxNode, T> creator = null)
        {
            _parent = parent;
            _list = builders.ToList();
            _adder = adder;
            _creator = creator;
        }

        public SyntaxBuilder Parent => _parent;

        public int Count => _list.Count;

        public T this[int index] => _list[index];

        public TBuilder Add<TBuilder>(TBuilder builder) where TBuilder : T
        {
            // add unattached builder
            if (builder.Parent == null)
            {
                var newNode = _parent.AddNode(builder.CurrentNode, _adder);
                var newBuilder = (TBuilder)(_creator != null ? _creator(_parent, newNode) : SyntaxBuilder.CreateChild(_parent, newNode));
                _list.Add(newBuilder);
                return newBuilder;
            }
            else
            {
                return (TBuilder)Add(builder.CurrentNode);
            }

            throw new InvalidOperationException("Builder is already attached to a different tree.");
        }

        internal T Add(SyntaxNode newDeclaration)
        {
            newDeclaration = SyntaxBuilder.ClearTracking(newDeclaration);

            var newNode = _parent.AddNode(newDeclaration, _adder);
            var newBuilder = (T)(_creator != null ? _creator(_parent, newNode) : SyntaxBuilder.CreateChild(_parent, newNode));
            _list.Add(newBuilder);
            return newBuilder;
        }

#if false
        public void Clear()
        {
            _list.Clear();
        }
#endif

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public bool Remove(T item)
        {
            if (_list.Contains(item))
            {
                _list.Remove(item);
                item.DetachFromParent();
                return true;
            }
            else
            {
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}