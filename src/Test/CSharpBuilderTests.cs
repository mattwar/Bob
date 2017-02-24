using Builders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Test
{
    [TestClass]
    public class CSharpBuilderTests : CSharpTestBase
    {
        [TestMethod]
        public void TestAddClassInRoot()
        {
            var b = GetBuilder();
            b.AddClass("C");
            b.Format();

            Assert.AreEqual(
@"class C
{
}", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddFieldToExistingClass()
        {
            var b = GetBuilder("class C { }");
            var c = b.Members[0] as TypeBuilder;
            c.AddField("f", b.Generator.TypeExpression(SpecialType.System_Int32));
            b.Format();

            Assert.AreEqual(
@"class C
{
    int f;
}", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddFieldToExistingClassWithChanges()
        {
            var b = GetBuilder("class C { }");
            var c = b.Members[0] as TypeBuilder;
            var f = c.AddField("f", b.Generator.TypeExpression(SpecialType.System_Int32));
            f.Initializer = SyntaxFactory.ParseExpression("123");
            f.Accessibility = Accessibility.Private;
            b.Format();

            Assert.AreEqual(
@"class C
{
    private int f = 123;
}", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeTypeOfExistingField()
        {
            var b = GetBuilder("class C { int f; }");
            var f = b.GetBuilders<FieldBuilder>("f").First();
            f.Type = b.Generator.IdentifierName("T");
            b.Format();

            Assert.AreEqual(@"class C { T f; }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeNameOfExistingField()
        {
            var b = GetBuilder("class C { int f; }");
            var f = b.GetBuilders<FieldBuilder>("f").First();
            f.Name = "f2";
            b.Format();

            Assert.AreEqual(@"class C { int f2; }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeInitializerOfExistingField()
        {
            var b = GetBuilder("class C { int f; }");
            var f = b.GetBuilders<FieldBuilder>("f").First();
            f.Initializer = SyntaxFactory.ParseExpression("123");
            b.Format();

            Assert.AreEqual(@"class C { int f = 123; }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeAccessibilityOfExistingField()
        {
            var b = GetBuilder("class C { int f; }");
            var c = b.Members[0] as TypeBuilder;
            var f = b.GetBuilders<FieldBuilder>("f").First();
            f.Accessibility = Accessibility.Public;
            b.Format();

            Assert.AreEqual(@"class C { public int f; }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddMethodInExistingClass()
        {
            var b = GetBuilder("class C { }");
            var c = b.Members[0] as TypeBuilder;
            c.AddMethod("M");
            b.Format();

            Assert.AreEqual(
@"class C
{
    void M()
    {
    }
}", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddParameterInExistingMethod()
        {
            var b = GetBuilder("class C { void M() { } }");
            var m = b.GetBuilders<MethodBuilder>("M").First();
            m.AddParameter("p", b.Generator.TypeExpression(SpecialType.System_Int32));
            b.Format();

            Assert.AreEqual(@"class C { void M(int p) { } }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeReturnTypeOfExistingMethod()
        {
            var b = GetBuilder("class C { void M() { } }");
            var m = b.GetBuilders<MethodBuilder>("M").First();
            m.ReturnType = b.Generator.TypeExpression(SpecialType.System_Int32);
            b.Format();

            Assert.AreEqual(@"class C { int M() { } }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeAccessibilityOfExistingMethod()
        {
            var b = GetBuilder("class C { void M() { } }");
            var m = b.GetBuilders<MethodBuilder>("M").First();
            m.Accessibility = Accessibility.Public;
            b.Format();

            Assert.AreEqual(@"class C { public void M() { } }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeNameOfExistingMethod()
        {
            var b = GetBuilder("class C { void M() { } }");
            var m = b.GetBuilders<MethodBuilder>("M").First();
            m.Name = "M2";
            b.Format();

            Assert.AreEqual(@"class C { void M2() { } }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeNameOfExistingParameter()
        {
            var b = GetBuilder("class C { void M(int p) { } }");
            var p = b.GetBuilders<ParameterBuilder>("p").First();
            p.Name = "p2";
            b.Format();

            Assert.AreEqual(@"class C { void M(int p2) { } }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeTypeOfExistingParameter()
        {
            var b = GetBuilder("class C { void M(int p) { } }");
            var p = b.GetBuilders<ParameterBuilder>("p").First();
            p.Type = b.Generator.TypeExpression(SpecialType.System_String);
            b.Format();

            Assert.AreEqual(@"class C { void M(string p) { } }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeDefaultOfExistingParameter()
        {
            var b = GetBuilder("class C { void M(int p) { } }");
            var p = b.GetBuilders<ParameterBuilder>("p").First();
            p.Default = SyntaxFactory.ParseExpression("123");
            b.Format();

            Assert.AreEqual(@"class C { void M(int p = 123) { } }", b.CurrentNode.ToFullString());
        }
    }
}