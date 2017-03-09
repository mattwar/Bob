using System;
using Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class CSharpCommentTests : CSharpTestBase
    {
        [TestMethod]
        public void TestAddComment()
        {
            var b = GetBuilder("class C { }");
            b.Members[0].LeadingComments.AddComment("Howdy!");
            b.Format();

            Assert.AreEqual(
@"// Howdy!
class C { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddLineSpanningComment()
        {
            var b = GetBuilder("class C { }");
            b.Members[0].LeadingComments.AddComment("First Line\r\nSecond Line");
            b.Format();

            Assert.AreEqual(
@"// First Line
// Second Line
class C { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddCommentAsMultiLineComment()
        {
            var b = GetBuilder("class C { }");
            b.Members[0].LeadingComments.AddComment("Howdy!", style: CommentStyle.MultiLineBlock);
            b.Format();

            Assert.AreEqual(
@"/* Howdy! */
class C { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddLineSpanningCommentAsMultiLineComment()
        {
            var b = GetBuilder("class C { }");
            b.Members[0].LeadingComments.AddComment("First Line\r\nSecond Line", style: CommentStyle.MultiLineBlock);
            b.Format();

            Assert.AreEqual(
@"/* First Line
   Second Line */
class C { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddDocumentationComment()
        {
            var b = GetBuilder("class C { }");
            b.Members[0].LeadingComments.AddComment("Howdy!", CommentStyle.Documentation);
            b.Format();

            Assert.AreEqual(
@"/// Howdy!
class C { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestAddCommentWithBadStyle()
        {
            var b = GetBuilder("class C { }");

            Assert.ThrowsException<InvalidOperationException>(
                () => b.Members[0].LeadingComments.AddComment("Howdy!", (CommentStyle)(-1)));
        }

        [TestMethod]
        public void TestGetCommentStyle()
        {
            var b = GetBuilder(
@"// single line block
class A { }

/* multi line block */
Class B { }

/// doc comment
Class C { }");

            Assert.AreEqual(CommentStyle.SingleLineBlock, b.Members[0].LeadingComments[0].Style);
            Assert.AreEqual(CommentStyle.MultiLineBlock, b.Members[1].LeadingComments[0].Style);
            Assert.AreEqual(CommentStyle.Documentation, b.Members[2].LeadingComments[0].Style);
        }

        [TestMethod]
        public void TestSetCommentStyle()
        {
            var b = GetBuilder(
@"// comment
class A { }");

            var c = b.Members[0].LeadingComments[0];

            c.Style = CommentStyle.MultiLineBlock;
            b.Format();
            Assert.AreEqual(CommentStyle.MultiLineBlock, c.Style);
            Assert.AreEqual(
@"/* comment */
class A { }",
            b.CurrentNode.ToFullString());

            c.Style = CommentStyle.Documentation;
            b.Format();
            Assert.AreEqual(CommentStyle.Documentation, c.Style);
            Assert.AreEqual(
@"/// comment
class A { }",
            b.CurrentNode.ToFullString());

            c.Style = CommentStyle.SingleLineBlock;
            b.Format();
            Assert.AreEqual(CommentStyle.SingleLineBlock, c.Style);
            Assert.AreEqual(
@"// comment
class A { }",
            b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestSetSingleLineBlockCommentText()
        {
            var b = GetBuilder(
@"// comment
class A { }");

            var c = b.Members[0].LeadingComments[0];

            c.Text = "Howdy!";
            b.Format();
            Assert.AreEqual(c.Text, "Howdy!");
            Assert.AreEqual(
@"// Howdy!
class A { }",
            b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestSetMultiLineBlockCommentText()
        {
            var b = GetBuilder(
@"/* comment */
class A { }");

            var c = b.Members[0].LeadingComments[0];

            c.Text = "Howdy!";
            b.Format();
            Assert.AreEqual(c.Text, "Howdy!");
            Assert.AreEqual(
@"/* Howdy! */
class A { }",
            b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestSetDocumentationCommentText()
        {
            var b = GetBuilder(
@"/// comment
class A { }");

            var c = b.Members[0].LeadingComments[0];

            c.Text = "Howdy!";
            b.Format();
            Assert.AreEqual(c.Text, "Howdy!");
            Assert.AreEqual(
@"/// Howdy!
class A { }",
            b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestInsertComment()
        {
            var b = GetBuilder("class C { }");
            b.Members[0].LeadingComments.InsertComment(0, "Howdy!");
            b.Format();

            Assert.AreEqual(
@"// Howdy!
class C { }", b.CurrentNode.ToFullString());
        }

        [TestMethod]
        public void TestInsertCommentBeforeExistingComment()
        {
            var b = GetBuilder(
@"// comment
class C { }");

            var m = b.Members[0];
            Assert.AreEqual(m.LeadingComments.Count, 1);

            m.LeadingComments.InsertComment(0, "Howdy!");

            Assert.AreEqual(2, m.LeadingComments.Count);

            b.Format();

            Assert.AreEqual(
@"// Howdy!

// comment
class C { }", b.CurrentNode.ToFullString());

            Assert.AreEqual(2, m.LeadingComments.Count);
        }

        [TestMethod]
        public void TestInsertCommentBetweenExistingComments()
        {
            var b = GetBuilder(
@"// comment1

// comment2
class C { }");

            var m = b.Members[0];
            Assert.AreEqual(m.LeadingComments.Count, 2);

            m.LeadingComments.InsertComment(1, "Howdy!");

            Assert.AreEqual(3, m.LeadingComments.Count);

            b.Format();

            Assert.AreEqual(
@"// comment1

// Howdy!

// comment2
class C { }", b.CurrentNode.ToFullString());

            Assert.AreEqual(3, m.LeadingComments.Count);
        }

    }
}
