using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Editing;

namespace Builders
{
    public class TypeParameterList
    {
        private readonly SyntaxBuilder _builder;

        private TypeParameterList(SyntaxBuilder builder)
        {
            _builder = builder;
        }

        private static readonly ConditionalWeakTable<SyntaxBuilder, TypeParameterList> s_typeParameterList
            = new ConditionalWeakTable<SyntaxBuilder, TypeParameterList>();

        internal static TypeParameterList GetList(SyntaxBuilder builder)
        {
            TypeParameterList list;
            if (!s_typeParameterList.TryGetValue(builder, out list))
            {
                list = s_typeParameterList.GetValue(builder, b => new TypeParameterList(b));
            }

            return list;
        }

        public int Count => GetNames().Count;

        public TypeParameter this[int index]
        {
            get { return this[GetNames()[index]]; }
        }

        private readonly Dictionary<string, TypeParameter> _parameters
            = new Dictionary<string, TypeParameter>();

        public TypeParameter this[string name]
        {
            get
            {
                TypeParameter tp;
                if (!_parameters.TryGetValue(name, out tp))
                {
                    if (!GetNames().Contains(name))
                    {
                        throw new ArgumentOutOfRangeException(nameof(name));
                    }

                    tp = new TypeParameter(this, name);
                    _parameters.Add(name, tp);
                }

                return tp;
            }
        }

        internal void SetName(string oldName, string newName)
        {
            var p = this[oldName];
            _parameters.Remove(oldName);
            var index = IndexOf(oldName);
            _builder.UpdateCurrentNode(_builder.CommentEditor.WithTypeParameterNameChanged(_builder.CurrentNode, oldName, newName));
            _parameters.Add(newName, p);
        }

        internal int IndexOf(string name)
        {
            return GetNames().IndexOf(name);
        }

        public TypeParameter Add(string typeParameterName)
        {
            AddRange(new[] { typeParameterName });
            return this[typeParameterName];
        }

        public void AddRange(IEnumerable<string> typeParameterNames)
        {
            _builder.UpdateCurrentNode(_builder.Generator.WithTypeParameters(_builder.CurrentNode, GetNames().Concat(typeParameterNames).ToArray()));
        }

        public TypeParameter Insert(int index, string typeParameterName)
        {
            InsertRange(index, new[] { typeParameterName });
            return this[typeParameterName];
        }

        public void InsertRange(int index, IEnumerable<string> typeParameterNames)
        {
            _builder.UpdateCurrentNode(_builder.Generator.WithTypeParameters(_builder.CurrentNode, GetNames().InsertRange(index, typeParameterNames).ToArray()));
        }

        public void RemoveAt(int index)
        {
            Remove(this[index]);
        }

        public void Remove(string typeParameterName)
        {
            Remove(this[typeParameterName]);
        }

        public void Remove(TypeParameter typeParameter)
        {
            var index = IndexOf(typeParameter.Name);

            if (typeParameter.SpecialConstraints != SpecialTypeConstraintKind.None)
            {
                typeParameter.SpecialConstraints = SpecialTypeConstraintKind.None;
            }

            if (typeParameter.TypeConstraints.Count > 0)
            {
                typeParameter.TypeConstraints.Clear();
            }

            _builder.UpdateCurrentNode(_builder.Generator.WithTypeParameters(_builder.CurrentNode, GetNames().RemoveAt(index).ToArray()));
        }

        public void Clear()
        {
            _builder.UpdateCurrentNode(_builder.Generator.WithTypeParameters(_builder.CurrentNode, null));
        }

        public bool Contains(string name)
        {
            return GetNames().Contains(name);
        }

        private static readonly ConditionalWeakTable<SyntaxNode, ImmutableList<string>> s_typeParameterLists
            = new ConditionalWeakTable<SyntaxNode, ImmutableList<string>>();

        private ImmutableList<string> GetNames()
        {
            ImmutableList<string> list;
            if (!s_typeParameterLists.TryGetValue(_builder.CurrentNode, out list))
            {
                SyntaxBuilder builder = _builder;
                list = s_typeParameterLists.GetValue(_builder.CurrentNode, n => builder.CommentEditor.GetTypeParameterNames(n).ToImmutableList());
            }

            return list;
        }

        private static readonly ConditionalWeakTable<SyntaxNode, Dictionary<string, ImmutableList<TypeExpression>>> s_typeConstraintLists
            = new ConditionalWeakTable<SyntaxNode, Dictionary<string, ImmutableList<TypeExpression>>>();

        internal ImmutableList<TypeExpression> GetTypeConstraints(string typeParameterName)
        {
            Dictionary<string, ImmutableList<TypeExpression>> map;
            if (!s_typeConstraintLists.TryGetValue(_builder.CurrentNode, out map))
            {
                map = s_typeConstraintLists.GetValue(_builder.CurrentNode, n => new Dictionary<string, ImmutableList<TypeExpression>>());
            }

            ImmutableList<TypeExpression> list;
            if (!map.TryGetValue(typeParameterName, out list))
            {
                list = _builder.CommentEditor.GetTypeConstraints(_builder.CurrentNode, typeParameterName).Select(n => (TypeExpression)n).ToImmutableList();
                map.Add(typeParameterName, list);
            }

            return list;
        }

        internal void SetTypeConstraints(string typeParameterName, IEnumerable<TypeExpression> typeConstraints)
        {
            var kinds = GetSpecialTypeConstraints(typeParameterName);
            var nodes = typeConstraints.Select(tc => tc.ToSyntaxNode(_builder.Context));
            _builder.UpdateCurrentNode(_builder.Generator.WithTypeConstraint(_builder.CurrentNode, typeParameterName, kinds, nodes));
        }

        internal SpecialTypeConstraintKind GetSpecialTypeConstraints(string typeParameterName)
        {
            return _builder.CommentEditor.GetSpecialTypeConstraints(_builder.CurrentNode, typeParameterName);
        }

        internal void SetSpecialTypeConstraints(string typeParameterName, SpecialTypeConstraintKind kinds)
        {
            _builder.UpdateCurrentNode(_builder.Generator.WithTypeConstraint(_builder.CurrentNode, typeParameterName, kinds));
        }
    }

    public class TypeParameter
    {
        private readonly TypeParameterList _list;
        private string _name;

        internal TypeParameter(TypeParameterList list, string name)
        {
            _list = list;
            _name = name;
        }

        internal TypeParameterList List => _list;

        public string Name
        {
            get { return _name; }

            set
            {
                _list.SetName(_name, value);
                _name = value;
            }
        }

        public SpecialTypeConstraintKind SpecialConstraints
        {
            get { return _list.GetSpecialTypeConstraints(_name); }
            set { _list.SetSpecialTypeConstraints(_name, value); }
        }

        private TypeConstraintList _typeContraints;
        public TypeConstraintList TypeConstraints
        {
            get
            {
                if (_typeContraints == null)
                {
                    _typeContraints = new TypeConstraintList(this);
                }

                return _typeContraints;
            }
        }
    }

    public class TypeConstraintList
    {
        public readonly TypeParameter _typeParameter;

        internal TypeConstraintList(TypeParameter typeParameter)
        {
            _typeParameter = typeParameter;
        }

        public int Count => _typeParameter.List.GetTypeConstraints(_typeParameter.Name).Count;

        public TypeExpression this[int index]
        {
            get { return _typeParameter.List.GetTypeConstraints(_typeParameter.Name)[index]; }
            set { _typeParameter.List.SetTypeConstraints(_typeParameter.Name, _typeParameter.List.GetTypeConstraints(_typeParameter.Name).SetItem(index, value)); }
        }

        public void Add(TypeExpression typeConstraint)
        {
            AddRange(new[] { typeConstraint });
        }

        public void AddRange(IEnumerable<TypeExpression> typeConstraints)
        {
            _typeParameter.List.SetTypeConstraints(_typeParameter.Name, _typeParameter.List.GetTypeConstraints(_typeParameter.Name).AddRange(typeConstraints));
        }

        public void Insert(int index, TypeExpression typeConstraint)
        {
            InsertRange(index, new[] { typeConstraint });
        }

        public void InsertRange(int index, IEnumerable<TypeExpression> typeConstraints)
        {
            _typeParameter.List.SetTypeConstraints(_typeParameter.Name, _typeParameter.List.GetTypeConstraints(_typeParameter.Name).InsertRange(index, typeConstraints));
        }

        public void RemoveAt(int index)
        {
            _typeParameter.List.SetTypeConstraints(_typeParameter.Name, _typeParameter.List.GetTypeConstraints(_typeParameter.Name).RemoveAt(index));
        }

        public void Clear()
        {
            _typeParameter.List.SetTypeConstraints(_typeParameter.Name, Array.Empty<TypeExpression>());
        }
    }
}
