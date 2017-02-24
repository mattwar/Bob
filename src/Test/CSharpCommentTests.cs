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
    }
}
