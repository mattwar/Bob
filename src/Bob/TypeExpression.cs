using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Builders
{
    /// <summary>
    /// Union of a type expression <see cref="SyntaxNode"/> or a <see cref="Type"/>
    /// </summary>
    public struct TypeExpression
    {
        private readonly object _value;

        private TypeExpression(object value)
        {
            _value = value;
        }

        public static implicit operator TypeExpression(SyntaxNode node)
        {
            return new TypeExpression(node);
        }

        public static implicit operator TypeExpression(Type type)
        {
            return new TypeExpression(type);
        }

        internal SyntaxNode ToSyntaxNode(BuilderContext context)
        {
            switch (_value)
            {
                case null:
                default:
                    return null;
                case SyntaxNode node:
                    return node;
                case Type type:
                    return GetTypeNode(type, context.Generator);
            }
        }

        private static SyntaxNode GetTypeNode(Type type, SyntaxGenerator g)
        {
            if (type.IsArray)
            {
                return g.ArrayTypeExpression(GetTypeNode(type.GetElementType(), g));
            }
            else if (type.IsGenericParameter)
            {
                return g.IdentifierName(type.Name);
            }
            else if (type.IsConstructedGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return g.NullableTypeExpression(GetTypeNode(type.GetGenericArguments()[0], g));
                }
                else
                {
                    var args = type.GetGenericArguments().Select(a => GetTypeNode(a, g)).ToArray();

                    var len = type.Name.IndexOf("`");
                    var typeName = g.IdentifierName(type.Name.Substring(0, len));
                    if (type.DeclaringType != null)
                    {
                        var declaringType = GetTypeNode(type.DeclaringType, g);
                        typeName = g.QualifiedName(declaringType, typeName);
                    }
                    else if (!string.IsNullOrEmpty(type.Namespace))
                    {
                        typeName = g.QualifiedName(g.DottedName(type.Namespace), typeName);
                    }

                    return g.WithTypeArguments(typeName, args);
                }
            }
            else if (type == typeof(object))
            {
                return g.TypeExpression(SpecialType.System_Object);
            }
            else if (!type.IsEnum)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        return g.TypeExpression(SpecialType.System_Boolean);
                    case TypeCode.Byte:
                        return g.TypeExpression(SpecialType.System_Byte);
                    case TypeCode.Char:
                        return g.TypeExpression(SpecialType.System_Char);
                    case TypeCode.Decimal:
                        return g.TypeExpression(SpecialType.System_Decimal);
                    case TypeCode.Double:
                        return g.TypeExpression(SpecialType.System_Double);
                    case TypeCode.Int16:
                        return g.TypeExpression(SpecialType.System_Int16);
                    case TypeCode.Int32:
                        return g.TypeExpression(SpecialType.System_Int32);
                    case TypeCode.Int64:
                        return g.TypeExpression(SpecialType.System_Int64);
                    case TypeCode.SByte:
                        return g.TypeExpression(SpecialType.System_SByte);
                    case TypeCode.Single:
                        return g.TypeExpression(SpecialType.System_Single);
                    case TypeCode.String:
                        return g.TypeExpression(SpecialType.System_String);
                    case TypeCode.UInt16:
                        return g.TypeExpression(SpecialType.System_UInt16);
                    case TypeCode.UInt32:
                        return g.TypeExpression(SpecialType.System_UInt32);
                    case TypeCode.UInt64:
                        return g.TypeExpression(SpecialType.System_UInt64);
                }
            }

            return g.DottedName(type.FullName);
        }

        public override string ToString()
        {
            return _value == null ? "<null>" : _value.ToString();
        }
    }
}