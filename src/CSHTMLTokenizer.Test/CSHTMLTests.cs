using CSHTMLTokenizer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSHTMLTokenizer.Test
{
    [TestClass]
    public class CSHTMLTests
    {
        [TestMethod]
        public void TestTagHelper()
        {
            var str = @"@page '/'
@addTagHelper *,BlazorPrettyCode
<PrettyCode CodeFile='snippets/demo.html' />";

            var tokens = Tokenizer.Parse(str);
            Assert.AreEqual(5, tokens.Count);

            var csLine1 = (CSLine)tokens[0];
            Assert.AreEqual(" '/'", csLine1.Line);
            Assert.AreEqual(CSLineType.Page, csLine1.LineType);

            var csLine2 = (CSLine)tokens[2];
            Assert.AreEqual(" *,BlazorPrettyCode", csLine2.Line);
            Assert.AreEqual(CSLineType.AddTagHelper, csLine2.LineType);

            Assert.AreEqual(TokenType.StartTag, tokens[4].TokenType);
        }

        [TestMethod]
        public void TestForEach()
        {
            var str = @"@foreach(var item in Items) {
    <RenderItem Item='@item' />
}";

            var tokens = Tokenizer.Parse(str);
            Assert.AreEqual(5, tokens.Count);

            Assert.AreEqual(TokenType.CSBlockStart, tokens[0].TokenType);

            Assert.AreEqual("foreach(var item in Items) {", ((Text)tokens[1]).Content.Trim());

            Assert.AreEqual(TokenType.StartTag, tokens[2].TokenType);
            var startTag = (StartTag)tokens[2];
            Assert.AreEqual(startTag.IsSelfClosingTag, true);
            Assert.AreEqual(startTag.Name, "RenderItem");
            Assert.AreEqual(startTag.Attributes.Count, 1);
            Assert.AreEqual("Item", ((AttributeToken)startTag.Attributes[0]).Name);
            Assert.AreEqual(true, ((AttributeToken)startTag.Attributes[0]).Value.IsCSStatement);
            Assert.AreEqual(false, ((AttributeToken)startTag.Attributes[0]).Value.HasParentheses);
            Assert.AreEqual("item", ((AttributeToken)startTag.Attributes[0]).Value.Content);
            Assert.AreEqual(TokenType.CSBlockEnd, tokens[4].TokenType);
        }
    }
}
