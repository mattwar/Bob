using System;
using System.Collections.Generic;
using System.Linq;
using Builders;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class CSharpExpressionTests : CSharpTestBase
    {
        [TestMethod]
        public void TestLiteralExpressions()
        {
            TestLiteral((byte)123, "123");
            TestLiteral(123, "123");
            TestLiteral(123L, "123L");
            TestLiteral(123U, "123U");
            TestLiteral(123UL, "123UL");
            TestLiteral(4.1f, "4.1F");
            TestLiteral(4.1, "4.1D"); // should not need the D for double...
            TestLiteral(4.10M, "4.10M");
            TestLiteral("text", "\"text\"");
            TestLiteral('z', "'z'");
            TestLiteral(Expression.Null, "null");
        }

        private void TestLiteral(Expression value, string expected)
        {
            var b = GetBuilder();
            var f = b.AddClass("c").AddField("f", typeof(object));
            f.Initializer = value;
            b.Format();
            var expectedCU = $"class c\r\n{{\r\n    object f = {expected};\r\n}}";
            var actualCU = b.CurrentNode.ToFullString();
            Assert.AreEqual(expectedCU, actualCU);
        }

        [TestMethod]
        public void TestTypeExpressions()
        {
            TestType(typeof(SByte), "sbyte");
            TestType(typeof(Int16), "short");
            TestType(typeof(Int32), "int");
            TestType(typeof(Int64), "long");
            TestType(typeof(Byte), "byte");
            TestType(typeof(UInt16), "ushort");
            TestType(typeof(UInt32), "uint");
            TestType(typeof(UInt64), "ulong");
            TestType(typeof(Single), "float");
            TestType(typeof(Double), "double");
            TestType(typeof(Decimal), "decimal");
            TestType(typeof(String), "string");
            TestType(typeof(Object), "object");
            TestType(typeof(Boolean), "bool");

            TestType(typeof(DateTime), "System.DateTime");
            TestType(typeof(Guid), "System.Guid");
            TestType(typeof(ConsoleColor), "System.ConsoleColor");

            TestType(typeof(int?), "int?");
            TestType(typeof(IEnumerable<int>), "System.Collections.Generic.IEnumerable<int>");
            TestType(typeof(IDictionary<int, string>), "System.Collections.Generic.IDictionary<int, string>");

            TestType(typeof(IEnumerable<int>).GetGenericTypeDefinition().GetGenericArguments()[0], "T");
            TestType(typeof(IEnumerable<>).GetGenericArguments()[0], "T");
        }

        private void TestType(Type type, string expected)
        {
            var b = GetBuilder();
            b.AddClass("c").AddField("f", type);
            b.Format();
            var expectedCU = $"class c\r\n{{\r\n    {expected} f;\r\n}}";
            var actualCU = b.CurrentNode.ToFullString();
            Assert.AreEqual(expectedCU, actualCU);
        }
    }
}
