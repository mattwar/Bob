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
            f.Initializer = 123;
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
            f.Type = typeof(string);
            b.Format();

            Assert.AreEqual(@"class C { string f; }", b.CurrentNode.ToFullString());
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
            f.Initializer = 123;
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
            p.Type = typeof(string);
            b.Format();

            Assert.AreEqual(@"class C { void M(string p) { } }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeDefaultOfExistingParameter()
        {
            var b = GetBuilder("class C { void M(int p) { } }");
            var p = b.GetBuilders<ParameterBuilder>("p").First();
            p.Default = 123;
            b.Format();

            Assert.AreEqual(@"class C { void M(int p = 123) { } }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestMethodStatementCount()
        {
            var b = GetBuilder("class C { void M(int p) { var x = 10; var y = x + 5; } }");
            var m = b.GetBuilders<MethodBuilder>().First();
            Assert.AreEqual(2, m.Statements.Count);
        }

        [TestMethod]
        public void TestMethodRemoveFirstStatement()
        {
            var b = GetBuilder("class C { void M(int p) { var x = 10; var y = x + 5; } }");
            var m = b.GetBuilders<MethodBuilder>().First();
            m.Statements.RemoveAt(0);

            b.Format();
            Assert.AreEqual(@"class C { void M(int p) { var y = x + 5; } }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestMethodRemoveLastStatement()
        {
            var b = GetBuilder("class C { void M(int p) { var x = 10; var y = x + 5; } }");
            var m = b.GetBuilders<MethodBuilder>().First();
            m.Statements.RemoveAt(m.Statements.Count - 1);

            b.Format();
            Assert.AreEqual(@"class C { void M(int p) { var x = 10; } }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestMethodClearStatements()
        {
            var b = GetBuilder(
@"class C
{
    void M(int p)
    {
        var x = 10;
        var y = x + 5;
    }
}");
            var m = b.GetBuilders<MethodBuilder>().First();
            m.Statements.Clear();

            b.Format();
            Assert.AreEqual(
@"class C
{
    void M(int p)
    {
    }
}", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestMethodAddStatement()
        {
            var b = GetBuilder("class C { void M(int p) { } }");
            var m = b.GetBuilders<MethodBuilder>().First();
            m.Statements.Add(b.Generator.ReturnStatement());
            b.Format();

            Assert.AreEqual(
@"class C
{
    void M(int p)
    {
        return;
    }
}", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestMethodAddMultipleStatements()
        {
            var b = GetBuilder("class C { void M(int p) { } }");
            var m = b.GetBuilders<MethodBuilder>().First();
            m.Statements.Add(SyntaxFactory.ParseStatement("var x = 10;"));
            m.Statements.Add(SyntaxFactory.ParseStatement("var y = x + x;"));
            b.Format();

            Assert.AreEqual(
@"class C
{
    void M(int p)
    {
        var x = 10;
        var y = x + x;
    }
}", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestMethodAddRangeStatements()
        {
            var b = GetBuilder("class C { void M(int p) { } }");
            var m = b.GetBuilders<MethodBuilder>().First();
            m.Statements.AddRange(new[] { SyntaxFactory.ParseStatement("var x = 10;"), SyntaxFactory.ParseStatement("var y = x + x;") });
            b.Format();

            Assert.AreEqual(
@"class C
{
    void M(int p)
    {
        var x = 10;
        var y = x + x;
    }
}", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestMethodInsertStatement()
        {
            var b = GetBuilder(
@"class C
{
    void M(int p)
    {
        var x = 10;
        var y = x + 5;
    }
}");
            var m = b.GetBuilders<MethodBuilder>().First();
            m.Statements.Insert(0, SyntaxFactory.ParseStatement("var z = x + p;"));

            b.Format();
            Assert.AreEqual(
@"class C
{
    void M(int p)
    {
        var x = 10;
        var z = x + p;
        var y = x + 5;
    }
}", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddClassToCompilationUnit()
        {
            var b = GetBuilder();
            b.AddClass("c");
            b.Format();

            Assert.AreEqual(
@"class c
{
}", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddTypeParameterToClass()
        {
            var b = GetBuilder("class c { }");
            var c = b.GetBuilders<TypeBuilder>().First();
            c.TypeParameters.Add("T");
            b.Format();

            Assert.AreEqual(@"class c<T> { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddMultipleTypeParametersToClass()
        {
            var b = GetBuilder("class c { }");
            var c = b.GetBuilders<TypeBuilder>().First();
            c.TypeParameters.AddRange(new[] { "T", "S" });
            b.Format();

            Assert.AreEqual(@"class c<T, S> { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestRemoveTypeParameterFromClass()
        {
            var b = GetBuilder("class c<T, S> { }");
            var c = b.GetBuilders<TypeBuilder>().First();
            c.TypeParameters.RemoveAt(0);
            b.Format();

            Assert.AreEqual(@"class c<S> { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestRemoveAllTypeParametersFromClass()
        {
            var b = GetBuilder("class c<T, S> { }");
            var c = b.GetBuilders<TypeBuilder>().First();
            c.TypeParameters.Clear();
            b.Format();

            Assert.AreEqual(@"class c { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeTypeParameterNameInClass()
        {
            var b = GetBuilder("class c<T> { }");
            var c = b.GetBuilders<TypeBuilder>().First();
            c.TypeParameters[0].Name = "S";
            b.Format();

            Assert.AreEqual(@"class c<S> { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddTypeConstraintToClass()
        {
            var b = GetBuilder("class c<T> { }");
            var c = b.GetBuilders<TypeBuilder>().First();
            c.TypeParameters["T"].TypeConstraints.Add(typeof(string));
            b.Format();

            Assert.AreEqual(@"class c<T> where T : string { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddStructConstraintToClass()
        {
            var b = GetBuilder("class c<T> { }");
            var c = b.GetBuilders<TypeBuilder>().First();
            c.TypeParameters["T"].SpecialConstraints = SpecialTypeConstraintKind.ValueType;
            b.Format();

            Assert.AreEqual(@"class c<T> where T : struct { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddStructAndTypeConstraintToClass()
        {
            var b = GetBuilder("class c<T> { }");
            var c = b.GetBuilders<TypeBuilder>().First();
            c.TypeParameters["T"].SpecialConstraints = SpecialTypeConstraintKind.ValueType;
            c.TypeParameters["T"].TypeConstraints.Add(typeof(string));
            b.Format();

            Assert.AreEqual(@"class c<T> where T : struct, string { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestChangeTypeParameterNameWithConstraintsInClass()
        {
            var b = GetBuilder("class c<T> where T : X { }");
            var c = b.GetBuilders<TypeBuilder>().First();
            var tp = c.TypeParameters["T"];
            tp.Name = "S";

            Assert.AreEqual("S", tp.Name);
            var tps = c.TypeParameters["S"];
            Assert.AreSame(tp, tps);

            b.Format();
            Assert.AreEqual(@"class c<S> where S : X { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestRemoveTypeParameterWithConstraintsInClass()
        {
            var b = GetBuilder("class c<T> where T : X { }");
            var c = b.GetBuilders<TypeBuilder>().First();
            c.TypeParameters.Remove("T");

            b.Format();
            Assert.AreEqual(@"class c { }", b.CurrentNode.ToFullString());
        }
    }
}