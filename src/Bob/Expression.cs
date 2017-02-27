using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Builders
{
    /// <summary>
    /// Union of either an expression <see cref="SyntaxNode"/> or a literal value.
    /// </summary>
    public struct Expression
    {
        private readonly object _value;

        private static readonly object _null = new object();

        private Expression(object value)
        {
            _value = value;
        }

        public static Expression Null = new Expression(_null);

        public static implicit operator Expression(SyntaxNode node)
        {
            return new Expression(node);
        }

        public static implicit operator Expression(char value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(string value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(byte value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(int value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(long value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(uint value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(ulong value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(float value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(double value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(decimal value)
        {
            return new Expression(value);
        }

        public static implicit operator Expression(bool value)
        {
            return new Expression(value);
        }

        internal SyntaxNode ToSyntaxNode(BuilderContext context)
        {
            if (_value == _null)
            {
                return context.Generator.NullLiteralExpression();
            }

            switch (_value)
            {
                case null:
                default:
                    return null;
                case string _:
                case char _:
                case long _:
                case int _:
                case ulong _:
                case uint _:
                case byte _:
                case bool _:
                case float _:
                case double _:
                case decimal _:
                    return context.Generator.LiteralExpression(_value);
                case SyntaxNode node:
                    return node;
            }
        }

        public override string ToString()
        {
            return _value == null ? "<null>" : _value.ToString();
        }
    }
}