using CSHTMLTokenizer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CSHTMLTokenizer.Test
{
    [TestClass]
    public class HTMLTests
    {
        [TestMethod]
        public void TestText()
        {
            List<Line> lines = Tokenizer.Parse("This is a test");
            Assert.AreEqual(1, lines.Count);
            List<IToken> lineTokens = lines[0].Tokens;
            Assert.AreEqual(1, lineTokens.Count);

            Assert.AreEqual(TokenType.Text, lineTokens[0].TokenType);
            Assert.AreEqual("This is a test", ((Text)lineTokens[0]).Content);
        }

        [TestMethod]
        public void TestSelfClosingTag()
        {
            List<Line> lines = Tokenizer.Parse("This is a test<br />This is another line");
            Assert.AreEqual(1, lines.Count);
            List<IToken> lineTokens = lines[0].Tokens;
            Assert.AreEqual(3, lineTokens.Count);

            Assert.AreEqual(TokenType.Text, lineTokens[0].TokenType);
            Assert.AreEqual("This is a test", ((Text)lineTokens[0]).Content);

            Assert.AreEqual(TokenType.StartTag, lineTokens[1].TokenType);
            Assert.AreEqual("br", ((StartTag)lineTokens[1]).Name.ToString());
            Assert.AreEqual(true, ((StartTag)lineTokens[1]).IsSelfClosingTag);

            Assert.AreEqual(TokenType.Text, lineTokens[2].TokenType);
            Assert.AreEqual("This is another line", ((Text)lineTokens[2]).Content);
        }

        [TestMethod]
        public void TestFixSelfClosingTag()
        {
            List<Line> lines = Tokenizer.Parse("This is a test<br>This is another line");
            Assert.AreEqual(1, lines.Count);

            List<IToken> lineTokens = lines[0].Tokens;
            Assert.AreEqual(3, lineTokens.Count);

            Assert.AreEqual(TokenType.Text, lineTokens[0].TokenType);
            Assert.AreEqual("This is a test", ((Text)lineTokens[0]).Content);

            Assert.AreEqual(TokenType.StartTag, lineTokens[1].TokenType);
            Assert.AreEqual("br", ((StartTag)lineTokens[1]).Name.ToString());
            Assert.AreEqual(true, ((StartTag)lineTokens[1]).IsSelfClosingTag);

            Assert.AreEqual(TokenType.Text, lineTokens[2].TokenType);
            Assert.AreEqual("This is another line", ((Text)lineTokens[2]).Content);
        }

        [TestMethod]
        public void TestContainerTag()
        {
            List<Line> lines = Tokenizer.Parse("This is <b>bold</b>");
            Assert.AreEqual(1, lines.Count);
            List<IToken> lineTokens = lines[0].Tokens;
            Assert.AreEqual(4, lineTokens.Count);

            Assert.AreEqual(TokenType.Text, lineTokens[0].TokenType);
            Assert.AreEqual("This is ", ((Text)lineTokens[0]).Content);

            Assert.AreEqual(TokenType.StartTag, lineTokens[1].TokenType);
            Assert.AreEqual("b", ((StartTag)lineTokens[1]).Name);
            Assert.AreEqual(false, ((StartTag)lineTokens[1]).IsSelfClosingTag);

            Assert.AreEqual(TokenType.Text, lineTokens[2].TokenType);
            Assert.AreEqual("bold", ((Text)lineTokens[2]).Content);

            Assert.AreEqual(TokenType.EndTag, lineTokens[3].TokenType);
            Assert.AreEqual("b", ((EndTag)lineTokens[3]).Name);
        }

        [TestMethod]
        public void TestAttributes()
        {
            List<Line> lines = Tokenizer.Parse("This is <div class='boldClass'>bold</div>");
            Assert.AreEqual(1, lines.Count);
            List<IToken> lineTokens = lines[0].Tokens;
            Assert.AreEqual(4, lineTokens.Count);

            Assert.AreEqual(TokenType.Text, lineTokens[0].TokenType);
            Assert.AreEqual("This is ", ((Text)lineTokens[0]).Content);

            Assert.AreEqual(TokenType.StartTag, lineTokens[1].TokenType);
            Assert.AreEqual("div", ((StartTag)lineTokens[1]).Name);
            Assert.AreEqual(false, ((StartTag)lineTokens[1]).IsSelfClosingTag);
            Assert.AreEqual(1, ((StartTag)lineTokens[1]).Attributes.Count);

            Assert.AreEqual(TokenType.Attribute, ((StartTag)lineTokens[1]).Attributes[0].TokenType);
            Assert.AreEqual("class", ((AttributeToken)((StartTag)lineTokens[1]).Attributes[0]).Name);
            Assert.AreEqual("boldClass", ((AttributeToken)((StartTag)lineTokens[1]).Attributes[0]).Value.Content);
            Assert.AreEqual(QuoteMarkType.SingleQuote, ((AttributeToken)((StartTag)lineTokens[1]).Attributes[0]).Value.QuoteMark);

            Assert.AreEqual(TokenType.Text, lineTokens[2].TokenType);
            Assert.AreEqual("bold", ((Text)lineTokens[2]).Content);

            Assert.AreEqual(TokenType.EndTag, lineTokens[3].TokenType);
            Assert.AreEqual("div", ((EndTag)lineTokens[3]).Name);
        }

        [TestMethod]
        public void TestQuotedString()
        {
            List<Line> lines = Tokenizer.Parse("This is a 'quoted string'");
            Assert.AreEqual(1, lines.Count);
            List<IToken> lineTokens = lines[0].Tokens;
            Assert.AreEqual(2, lineTokens.Count);

            Assert.AreEqual(TokenType.Text, lineTokens[0].TokenType);
            Assert.AreEqual("This is a ", ((Text)lineTokens[0]).Content);

            Assert.AreEqual(TokenType.QuotedString, lineTokens[1].TokenType);
            Assert.AreEqual("quoted string", ((QuotedString)lineTokens[1]).Content);
            Assert.AreEqual(QuoteMarkType.SingleQuote, ((QuotedString)lineTokens[1]).QuoteMark);
        }

        [TestMethod]
        public void TestMultiLineTag()
        {
            string str = @"<div
    class='test'
/>";
            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(3, lines.Count);
            List<IToken> lineTokens = lines[0].Tokens;
            Assert.AreEqual(1, lineTokens.Count);

            Assert.AreEqual(TokenType.StartTag, lineTokens[0].TokenType);
            StartTag startTag = (StartTag)lineTokens[0];
            Assert.AreEqual("div", startTag.Name);
            Assert.AreEqual(true, startTag.IsSelfClosingTag);
            Assert.AreEqual(LineType.MultiLineStart, startTag.LineType);
            Assert.AreEqual(0, startTag.Attributes.Count);

            lineTokens = lines[1].Tokens;
            Assert.AreEqual(1, lineTokens.Count);

            Assert.AreEqual(TokenType.StartTag, lineTokens[0].TokenType);
            startTag = (StartTag)lineTokens[0];
            Assert.AreEqual("div", startTag.Name);
            Assert.AreEqual(true, startTag.IsSelfClosingTag);
            Assert.AreEqual(LineType.MultiLine, startTag.LineType);
            Assert.AreEqual(2, startTag.Attributes.Count);

            List<IToken> attributes = startTag.Attributes;
            Assert.AreEqual(TokenType.Text, attributes[0].TokenType);

            Assert.AreEqual(TokenType.Attribute, attributes[1].TokenType);
            AttributeToken attribute = (AttributeToken)attributes[1];
            Assert.AreEqual("class", attribute.Name);
            Assert.AreEqual("test", attribute.Value.Content);

            lineTokens = lines[2].Tokens;
            Assert.AreEqual(1, lineTokens.Count);

            Assert.AreEqual(TokenType.StartTag, lineTokens[0].TokenType);
            startTag = (StartTag)lineTokens[0];
            Assert.AreEqual("div", startTag.Name);
            Assert.AreEqual(true, startTag.IsSelfClosingTag);
            Assert.AreEqual(LineType.MultiLineEnd, startTag.LineType);
            Assert.AreEqual(0, startTag.Attributes.Count);
        }

        [TestMethod]
        public void TestApostropheTag()
        {
            string str = @"<Card>
                The card's content.
            </Card>";
            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(3, lines.Count);

            Assert.AreEqual(TokenType.Text, lines[1].Tokens[0].TokenType);
            Assert.AreEqual("The card'", ((Text)lines[1].Tokens[0]).Content.Trim());
            Assert.AreEqual(TokenType.Text, lines[1].Tokens[1].TokenType);
            Assert.AreEqual("s content.", ((Text)lines[1].Tokens[1]).Content.Trim());

        }

        [TestMethod]
        public void TestApostropheEOF()
        {
            string str = "The card's content.";
            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(1, lines.Count);
            Assert.AreEqual(TokenType.Text, lines[0].Tokens[0].TokenType);
            Assert.AreEqual("The card'", ((Text)lines[0].Tokens[0]).Content);
            Assert.AreEqual(TokenType.Text, lines[0].Tokens[1].TokenType);
            Assert.AreEqual("s content.", ((Text)lines[0].Tokens[1]).Content);
        }
    }
}