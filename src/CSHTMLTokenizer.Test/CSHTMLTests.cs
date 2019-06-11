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
            string str = @"@page '/'
@addTagHelper *,BlazorPrettyCode
<PrettyCode CodeFile='snippets/demo.html' />";

            System.Collections.Generic.List<IToken> tokens = Tokenizer.Parse(str);
            Assert.AreEqual(5, tokens.Count);

            CSLine csLine1 = (CSLine)tokens[0];
            Assert.AreEqual(" '/'", csLine1.Line);
            Assert.AreEqual(CSLineType.Page, csLine1.LineType);

            CSLine csLine2 = (CSLine)tokens[2];
            Assert.AreEqual(" *,BlazorPrettyCode", csLine2.Line);
            Assert.AreEqual(CSLineType.AddTagHelper, csLine2.LineType);

            Assert.AreEqual(TokenType.StartTag, tokens[4].TokenType);
        }

        [TestMethod]
        public void TestForEach()
        {
            string str = @"@foreach(var item in Items) {
    <RenderItem Item='@item' />
}";

            System.Collections.Generic.List<IToken> tokens = Tokenizer.Parse(str);
            Assert.AreEqual(5, tokens.Count);

            Assert.AreEqual(TokenType.CSBlockStart, tokens[0].TokenType);

            Assert.AreEqual("foreach(var item in Items) {", ((Text)tokens[1]).Content.Trim());

            Assert.AreEqual(TokenType.StartTag, tokens[2].TokenType);
            StartTag startTag = (StartTag)tokens[2];
            Assert.AreEqual(startTag.IsSelfClosingTag, true);
            Assert.AreEqual(startTag.Name, "RenderItem");
            Assert.AreEqual(startTag.Attributes.Count, 1);
            Assert.AreEqual("Item", ((AttributeToken)startTag.Attributes[0]).Name);
            Assert.AreEqual(true, ((AttributeToken)startTag.Attributes[0]).Value.IsCSStatement);
            Assert.AreEqual(false, ((AttributeToken)startTag.Attributes[0]).Value.HasParentheses);
            Assert.AreEqual("item", ((AttributeToken)startTag.Attributes[0]).Value.Content);
            Assert.AreEqual(TokenType.CSBlockEnd, tokens[4].TokenType);
        }

        [TestMethod]
        public void ForEachAndFunctions()
        {
            string str = @"@page '/'

@foreach(var item in list) {
    <h1>@item</h1>
}

@functions {
    var list = new List<string> { 'hello', 'world' };
}";

            System.Collections.Generic.List<IToken> tokens = Tokenizer.Parse(str);
            Assert.AreEqual(20, tokens.Count);

            Assert.AreEqual(TokenType.CSLine, tokens[0].TokenType);
            Assert.AreEqual(CSLineType.Page, ((CSLine)tokens[0]).LineType);
            Assert.AreEqual(" '/'", ((CSLine)tokens[0]).Line);

            Assert.AreEqual(TokenType.CSBlockStart, tokens[2].TokenType);
            Assert.AreEqual(false, ((CSBlockStart)tokens[2]).IsFunctions);
            Assert.AreEqual(false, ((CSBlockStart)tokens[2]).IsOpenBrace);
            Assert.AreEqual(false, ((CSBlockStart)tokens[2]).IsOpenBrace);

            Assert.AreEqual(true, ((Text)tokens[3]).Content.StartsWith("foreach(var item in list)"));

            Assert.AreEqual(TokenType.StartTag, tokens[4].TokenType);
            StartTag startTag = (StartTag)tokens[4];
            Assert.AreEqual(false, startTag.IsSelfClosingTag);
            Assert.AreEqual("h1", startTag.Name);
            Assert.AreEqual(false, startTag.IsGeneric);

            Assert.AreEqual(TokenType.CSBlockStart, tokens[5].TokenType);

            Assert.AreEqual("item", ((Text)tokens[6]).Content);

            Assert.AreEqual(TokenType.EndTag, tokens[7].TokenType);

            Assert.AreEqual(TokenType.Text, tokens[8].TokenType);

            Assert.AreEqual(TokenType.CSBlockEnd, tokens[9].TokenType);

            Assert.AreEqual(TokenType.Text, tokens[10].TokenType);

            Assert.AreEqual(TokenType.CSBlockStart, tokens[11].TokenType);
            Assert.AreEqual(true, ((CSBlockStart)tokens[11]).IsFunctions);

            Assert.AreEqual(TokenType.Text, tokens[12].TokenType);
            Assert.AreEqual(true, ((Text)tokens[12]).Content.Trim().StartsWith("var list"));

            Assert.AreEqual(TokenType.StartTag, tokens[13].TokenType);
            Assert.AreEqual("string", ((StartTag)tokens[13]).Name);
            Assert.AreEqual(false, ((StartTag)tokens[13]).IsSelfClosingTag);
            Assert.AreEqual(true, ((StartTag)tokens[13]).IsGeneric);

            Assert.AreEqual(TokenType.Text, tokens[14].TokenType);

            Assert.AreEqual(TokenType.QuotedString, tokens[15].TokenType);
            Assert.AreEqual(true, ((QuotedString)tokens[15]).Content.Contains("hello"));

            Assert.AreEqual(TokenType.Text, tokens[16].TokenType);

            Assert.AreEqual(TokenType.QuotedString, tokens[17].TokenType);
            Assert.AreEqual(true, ((QuotedString)tokens[17]).Content.Contains("world"));

            Assert.AreEqual(TokenType.Text, tokens[18].TokenType);

            Assert.AreEqual(TokenType.CSBlockEnd, tokens[19].TokenType);
        }
    }
}
