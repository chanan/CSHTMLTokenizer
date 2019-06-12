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
            List<IToken> tokens = Tokenizer.Parse("This is a test");
            Assert.AreEqual(3, tokens.Count);

            Assert.AreEqual(TokenType.StartOfLine, tokens[0].TokenType);

            Assert.AreEqual(TokenType.Text, tokens[1].TokenType);
            Assert.AreEqual("This is a test", ((Text)tokens[1]).Content);

            Assert.AreEqual(TokenType.EndOfFile, tokens[2].TokenType);
        }

        [TestMethod]
        public void TestSelfClosingTag()
        {
            List<IToken> tokens = Tokenizer.Parse("This is a test<br />This is another line");
            Assert.AreEqual(5, tokens.Count);

            Assert.AreEqual(TokenType.StartOfLine, tokens[0].TokenType);

            Assert.AreEqual(TokenType.Text, tokens[1].TokenType);
            Assert.AreEqual("This is a test", ((Text)tokens[1]).Content);

            Assert.AreEqual(TokenType.StartTag, tokens[2].TokenType);
            Assert.AreEqual("br", ((StartTag)tokens[2]).Name.ToString());
            Assert.AreEqual(true, ((StartTag)tokens[2]).IsSelfClosingTag);

            Assert.AreEqual(TokenType.Text, tokens[3].TokenType);
            Assert.AreEqual("This is another line", ((Text)tokens[3]).Content);

            Assert.AreEqual(TokenType.EndOfFile, tokens[4].TokenType);
        }

        [TestMethod]
        public void TestFixSelfClosingTag()
        {
            List<IToken> tokens = Tokenizer.Parse("This is a test<br>This is another line");
            Assert.AreEqual(5, tokens.Count);

            Assert.AreEqual(TokenType.StartOfLine, tokens[0].TokenType);

            Assert.AreEqual(TokenType.Text, tokens[1].TokenType);
            Assert.AreEqual("This is a test", ((Text)tokens[1]).Content);

            Assert.AreEqual(TokenType.StartTag, tokens[2].TokenType);
            Assert.AreEqual("br", ((StartTag)tokens[2]).Name.ToString());
            Assert.AreEqual(true, ((StartTag)tokens[2]).IsSelfClosingTag);

            Assert.AreEqual(TokenType.Text, tokens[3].TokenType);
            Assert.AreEqual("This is another line", ((Text)tokens[3]).Content);

            Assert.AreEqual(TokenType.EndOfFile, tokens[4].TokenType);
        }

        [TestMethod]
        public void TestContainerTag()
        {
            List<IToken> tokens = Tokenizer.Parse("This is <b>bold</b>");
            Assert.AreEqual(6, tokens.Count);

            Assert.AreEqual(TokenType.StartOfLine, tokens[0].TokenType);

            Assert.AreEqual(TokenType.Text, tokens[1].TokenType);
            Assert.AreEqual("This is ", ((Text)tokens[1]).Content);

            Assert.AreEqual(TokenType.StartTag, tokens[2].TokenType);
            Assert.AreEqual("b", ((StartTag)tokens[2]).Name);
            Assert.AreEqual(false, ((StartTag)tokens[2]).IsSelfClosingTag);

            Assert.AreEqual(TokenType.Text, tokens[3].TokenType);
            Assert.AreEqual("bold", ((Text)tokens[3]).Content);

            Assert.AreEqual(TokenType.EndTag, tokens[4].TokenType);
            Assert.AreEqual("b", ((EndTag)tokens[4]).Name);

            Assert.AreEqual(TokenType.EndOfFile, tokens[5].TokenType);
        }

        [TestMethod]
        public void TestAttributes()
        {
            List<IToken> tokens = Tokenizer.Parse("This is <div class='boldClass'>bold</div>");
            Assert.AreEqual(6, tokens.Count);

            Assert.AreEqual(TokenType.StartOfLine, tokens[0].TokenType);

            Assert.AreEqual(TokenType.Text, tokens[1].TokenType);
            Assert.AreEqual("This is ", ((Text)tokens[1]).Content);

            Assert.AreEqual(TokenType.StartTag, tokens[2].TokenType);
            Assert.AreEqual("div", ((StartTag)tokens[2]).Name);
            Assert.AreEqual(false, ((StartTag)tokens[2]).IsSelfClosingTag);

            Assert.AreEqual(1, ((StartTag)tokens[2]).Attributes.Count);

            Assert.AreEqual(TokenType.Attribute, ((StartTag)tokens[2]).Attributes[0].TokenType);
            Assert.AreEqual("class", ((AttributeToken)((StartTag)tokens[2]).Attributes[0]).Name);
            Assert.AreEqual("boldClass", ((AttributeToken)((StartTag)tokens[2]).Attributes[0]).Value.Content);
            Assert.AreEqual(QuoteMarkType.SingleQuote, ((AttributeToken)((StartTag)tokens[2]).Attributes[0]).Value.QuoteMark);

            Assert.AreEqual(TokenType.Text, tokens[3].TokenType);
            Assert.AreEqual("bold", ((Text)tokens[3]).Content);

            Assert.AreEqual(TokenType.EndTag, tokens[4].TokenType);
            Assert.AreEqual("div", ((EndTag)tokens[4]).Name);

            Assert.AreEqual(TokenType.EndOfFile, tokens[5].TokenType);
        }

        [TestMethod]
        public void TestQuotedString()
        {
            List<IToken> tokens = Tokenizer.Parse("This is a 'quoted string'");
            Assert.AreEqual(4, tokens.Count);

            Assert.AreEqual(TokenType.StartOfLine, tokens[0].TokenType);

            Assert.AreEqual(TokenType.Text, tokens[1].TokenType);
            Assert.AreEqual("This is a ", ((Text)tokens[1]).Content);

            Assert.AreEqual(TokenType.QuotedString, tokens[2].TokenType);
            Assert.AreEqual("quoted string", ((QuotedString)tokens[2]).Content);
            Assert.AreEqual(QuoteMarkType.SingleQuote, ((QuotedString)tokens[2]).QuoteMark);

            Assert.AreEqual(TokenType.EndOfFile, tokens[3].TokenType);
        }

        [TestMethod]
        public void TestMultiLineTag()
        {
            string str = @"<div
    class='test'
/>";
            List<IToken> tokens = Tokenizer.Parse(str);
            Assert.AreEqual(3, tokens.Count);

            Assert.AreEqual(TokenType.StartOfLine, tokens[0].TokenType);

            Assert.AreEqual(TokenType.StartTag, tokens[1].TokenType);
            StartTag startTag = (StartTag)tokens[1];
            Assert.AreEqual("div", startTag.Name);
            Assert.AreEqual(true, startTag.IsSelfClosingTag);

            List<IToken> atrributes = startTag.Attributes;
            Assert.AreEqual(6, atrributes.Count);

            Assert.AreEqual(TokenType.EndOfLine, atrributes[0].TokenType);

            Assert.AreEqual(TokenType.StartOfLine, atrributes[1].TokenType);

            Assert.AreEqual(TokenType.Text, atrributes[2].TokenType);

            Assert.AreEqual(TokenType.Attribute, atrributes[3].TokenType);
            AttributeToken attribute = (AttributeToken)atrributes[3];
            Assert.AreEqual("class", attribute.Name);
            Assert.AreEqual("test", attribute.Value.Content);

            Assert.AreEqual(TokenType.EndOfLine, atrributes[4].TokenType);

            Assert.AreEqual(TokenType.StartOfLine, atrributes[5].TokenType);

            Assert.AreEqual(TokenType.EndOfFile, tokens[2].TokenType);
        }
    }
}