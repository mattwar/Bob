using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Builders
{
    public class StatementList
    {
        private readonly SyntaxBuilder _builder;

        private StatementList(SyntaxBuilder builder)
        {
            _builder = builder;
        }

        private static readonly ConditionalWeakTable<SyntaxBuilder, StatementList> s_statementLists
            = new ConditionalWeakTable<SyntaxBuilder, StatementList>();

        internal static StatementList GetList(SyntaxBuilder builder)
        {
            StatementList list;
            if (!s_statementLists.TryGetValue(builder, out list))
            {
                list = s_statementLists.GetValue(builder, b => new StatementList(b));
            }

            return list;
        }

        public int Count => _builder != null ? GetNodeList().Count : 0;

        public SyntaxNode this[int index]
        {
            get
            {
                if (_builder == null) throw new IndexOutOfRangeException();
                return GetNodeList()[index];
            }

            set
            {
                _builder.UpdateCurrentNode(_builder.Generator.ReplaceNode(_builder.CurrentNode, this[index], value));
            }
        }

        public void Add(SyntaxNode statement)
        {
            AddRange(new[] { statement });
        }

        public void AddRange(IEnumerable<SyntaxNode> statements)
        {
            statements = ClearTrivia(statements);

            if (Count > 0)
            {
                _builder.UpdateCurrentNode(_builder.Generator.InsertNodesAfter(_builder.CurrentNode, this[Count - 1], statements));
            }
            else
            {
                _builder.UpdateCurrentNode(_builder.Generator.WithStatements(_builder.CurrentNode, statements));
            }
        }

        public void Insert(int index, SyntaxNode statement)
        {
            InsertRange(index, new[] { statement });
        }

        public void InsertRange(int index, IEnumerable<SyntaxNode> statements)
        {
            if (Count == index)
            {
                AddRange(statements);
            }
            else
            {
                statements = ClearTrivia(statements);
                _builder.UpdateCurrentNode(_builder.Generator.InsertNodesAfter(_builder.CurrentNode, this[index], statements));
            }
        }

        private IEnumerable<SyntaxNode> ClearTrivia(IEnumerable<SyntaxNode> nodes)
        {
            var g = _builder.Generator;
            return nodes.Select(s => g.ClearTrivia(s));
        }

        private int IndexOf(SyntaxNode node)
        {
            var list = GetNodeList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == node)
                {
                    return i;
                }
            }

            return -1;
        }

        public void RemoveAt(int index)
        {
            var node = this[index];
            _builder.UpdateCurrentNode(_builder.Generator.RemoveNode(_builder.CurrentNode, node));
        }

        public void Clear()
        {
            _builder.UpdateCurrentNode(_builder.Generator.WithStatements(_builder.CurrentNode, Array.Empty<SyntaxNode>()));
        }

        private static readonly ConditionalWeakTable<SyntaxNode, IReadOnlyList<SyntaxNode>> s_statementNodes
            = new ConditionalWeakTable<SyntaxNode, IReadOnlyList<SyntaxNode>>();

        private IReadOnlyList<SyntaxNode> GetNodeList()
        {
            IReadOnlyList<SyntaxNode> list;
            if (!s_statementNodes.TryGetValue(_builder.CurrentNode, out list))
            {
                SyntaxBuilder builder = _builder;
                list = s_statementNodes.GetValue(_builder.CurrentNode, n => builder.Generator.GetStatements(n));
            }

            return list;
        }
    }
}
