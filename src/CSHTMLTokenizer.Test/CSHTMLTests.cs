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
@using BlazorPrettyCode
<PrettyCode CodeFile='snippets/demo.html' />";

            System.Collections.Generic.List<IToken> tokens = Tokenizer.Parse(str);
            Assert.AreEqual(9, tokens.Count);

            CSLine csLine1 = (CSLine)tokens[1];
            Assert.AreEqual(" '/'", csLine1.Line);
            Assert.AreEqual(CSLineType.Page, csLine1.LineType);

            CSLine csLine2 = (CSLine)tokens[4];
            Assert.AreEqual(" BlazorPrettyCode", csLine2.Line);
            Assert.AreEqual(CSLineType.Using, csLine2.LineType);

            Assert.AreEqual(TokenType.StartTag, tokens[7].TokenType);
        }

        [TestMethod]
        public void TestForEach()
        {
            string str = @"@foreach(var item in Items) {
    <RenderItem Item='@item' />
}";

            System.Collections.Generic.List<IToken> tokens = Tokenizer.Parse(str);
            Assert.AreEqual(11, tokens.Count);

            Assert.AreEqual(TokenType.CSBlockStart, tokens[1].TokenType);

            Assert.AreEqual("foreach(var item in Items) {", ((Text)tokens[2]).Content.Trim());

            Assert.AreEqual(TokenType.StartTag, tokens[6].TokenType);
            StartTag startTag = (StartTag)tokens[6];
            Assert.AreEqual(startTag.IsSelfClosingTag, true);
            Assert.AreEqual(startTag.Name, "RenderItem");
            Assert.AreEqual(startTag.Attributes.Count, 1);
            Assert.AreEqual("Item", ((AttributeToken)startTag.Attributes[0]).Name);
            Assert.AreEqual(true, ((AttributeToken)startTag.Attributes[0]).Value.IsCSStatement);
            Assert.AreEqual(false, ((AttributeToken)startTag.Attributes[0]).Value.HasParentheses);
            Assert.AreEqual("item", ((AttributeToken)startTag.Attributes[0]).Value.Content);

            Assert.AreEqual(TokenType.CSBlockEnd, tokens[9].TokenType);
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
            Assert.AreEqual(36, tokens.Count);

            Assert.AreEqual(TokenType.CSLine, tokens[1].TokenType);
            Assert.AreEqual(CSLineType.Page, ((CSLine)tokens[1]).LineType);
            Assert.AreEqual(" '/'", ((CSLine)tokens[1]).Line);

            Assert.AreEqual(TokenType.CSBlockStart, tokens[6].TokenType);
            Assert.AreEqual(false, ((CSBlockStart)tokens[6]).IsFunctions);
            Assert.AreEqual(false, ((CSBlockStart)tokens[6]).IsOpenBrace);
            Assert.AreEqual(false, ((CSBlockStart)tokens[6]).IsOpenBrace);

            Assert.AreEqual(true, ((Text)tokens[7]).Content.StartsWith("foreach(var item in list)"));

            Assert.AreEqual(TokenType.StartTag, tokens[11].TokenType);
            StartTag startTag = (StartTag)tokens[11];
            Assert.AreEqual(false, startTag.IsSelfClosingTag);
            Assert.AreEqual("h1", startTag.Name);
            Assert.AreEqual(false, startTag.IsGeneric);

            Assert.AreEqual(TokenType.CSBlockStart, tokens[12].TokenType);

            Assert.AreEqual("item", ((Text)tokens[13]).Content);

            Assert.AreEqual(TokenType.EndTag, tokens[14].TokenType);

            Assert.AreEqual(TokenType.CSBlockEnd, tokens[17].TokenType);

            Assert.AreEqual(TokenType.CSBlockStart, tokens[22].TokenType);
            Assert.AreEqual(true, ((CSBlockStart)tokens[22]).IsFunctions);

            Assert.AreEqual(TokenType.Text, tokens[25].TokenType);
            Assert.AreEqual(true, ((Text)tokens[25]).Content.Trim().StartsWith("var list"));

            Assert.AreEqual(TokenType.StartTag, tokens[26].TokenType);
            Assert.AreEqual("string", ((StartTag)tokens[26]).Name);
            Assert.AreEqual(false, ((StartTag)tokens[26]).IsSelfClosingTag);
            Assert.AreEqual(true, ((StartTag)tokens[26]).IsGeneric);

            Assert.AreEqual(TokenType.Text, tokens[27].TokenType);

            Assert.AreEqual(TokenType.QuotedString, tokens[28].TokenType);
            Assert.AreEqual(true, ((QuotedString)tokens[28]).Content.Contains("hello"));

            Assert.AreEqual(TokenType.Text, tokens[29].TokenType);

            Assert.AreEqual(TokenType.QuotedString, tokens[30].TokenType);
            Assert.AreEqual(true, ((QuotedString)tokens[30]).Content.Contains("world"));

            Assert.AreEqual(TokenType.Text, tokens[31].TokenType);

            Assert.AreEqual(TokenType.CSBlockEnd, tokens[34].TokenType);
        }
    }
}
