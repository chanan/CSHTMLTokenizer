using CSHTMLTokenizer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSHTMLTokenizer.Test
{
    [TestClass]
    public class HTMLTests
    {
        [TestMethod]
        public void TestText()
        {
            var tokens = Tokenizer.Parse("This is a test");
            Assert.AreEqual(1, tokens.Count);
            Assert.AreEqual(TokenType.Text, tokens[0].TokenType);
            Assert.AreEqual("This is a test", ((Text)tokens[0]).Content);
        }

        [TestMethod]
        public void TestSelfClosingTag()
        {
            var tokens = Tokenizer.Parse("This is a test<br />This is another line");
            Assert.AreEqual(3, tokens.Count);

            Assert.AreEqual(TokenType.Text, tokens[0].TokenType);
            Assert.AreEqual("This is a test", ((Text)tokens[0]).Content);

            Assert.AreEqual(TokenType.StartTag, tokens[1].TokenType);
            Assert.AreEqual("br", ((StartTag)tokens[1]).Name.ToString());
            Assert.AreEqual(true, ((StartTag)tokens[1]).IsSelfClosingTag);

            Assert.AreEqual(TokenType.Text, tokens[2].TokenType);
            Assert.AreEqual("This is another line", ((Text)tokens[2]).Content);
        }

        [TestMethod]
        public void TestFixSelfClosingTag()
        {
            var tokens = Tokenizer.Parse("This is a test<br>This is another line");
            Assert.AreEqual(3, tokens.Count);

            Assert.AreEqual(TokenType.Text, tokens[0].TokenType);
            Assert.AreEqual("This is a test", ((Text)tokens[0]).Content);

            Assert.AreEqual(TokenType.StartTag, tokens[1].TokenType);
            Assert.AreEqual("br", ((StartTag)tokens[1]).Name.ToString());
            Assert.AreEqual(true, ((StartTag)tokens[1]).IsSelfClosingTag);

            Assert.AreEqual(TokenType.Text, tokens[2].TokenType);
            Assert.AreEqual("This is another line", ((Text)tokens[2]).Content);
        }

        [TestMethod]
        public void TestContainerTag()
        {
            var tokens = Tokenizer.Parse("This is <b>bold</b>");
            Assert.AreEqual(4, tokens.Count);

            Assert.AreEqual(TokenType.Text, tokens[0].TokenType);
            Assert.AreEqual("This is ", ((Text)tokens[0]).Content);

            Assert.AreEqual(TokenType.StartTag, tokens[1].TokenType);
            Assert.AreEqual("b", ((StartTag)tokens[1]).Name);
            Assert.AreEqual(false, ((StartTag)tokens[1]).IsSelfClosingTag);

            Assert.AreEqual(TokenType.Text, tokens[2].TokenType);
            Assert.AreEqual("bold", ((Text)tokens[2]).Content);

            Assert.AreEqual(TokenType.EndTag, tokens[3].TokenType);
            Assert.AreEqual("b", ((EndTag)tokens[3]).Name);
        }

        [TestMethod]
        public void TestAttributes()
        {
            var tokens = Tokenizer.Parse("This is <div class='boldClass'>bold</div>");
            Assert.AreEqual(4, tokens.Count);

            Assert.AreEqual(TokenType.Text, tokens[0].TokenType);
            Assert.AreEqual("This is ", ((Text)tokens[0]).Content);

            Assert.AreEqual(TokenType.StartTag, tokens[1].TokenType);
            Assert.AreEqual("div", ((StartTag)tokens[1]).Name);
            Assert.AreEqual(false, ((StartTag)tokens[1]).IsSelfClosingTag);

            Assert.AreEqual(1, ((StartTag)tokens[1]).Attributes.Count);

            Assert.AreEqual(TokenType.Attribute, ((StartTag)tokens[1]).Attributes[0].TokenType);
            Assert.AreEqual("class", ((AttributeToken)((StartTag)tokens[1]).Attributes[0]).Name);
            Assert.AreEqual("boldClass", ((AttributeToken)((StartTag)tokens[1]).Attributes[0]).Value.Content);
            Assert.AreEqual(QuoteMarkType.SingleQuote, ((AttributeToken)((StartTag)tokens[1]).Attributes[0]).Value.QuoteMark);
        
            Assert.AreEqual(TokenType.Text, tokens[2].TokenType);
            Assert.AreEqual("bold", ((Text)tokens[2]).Content);

            Assert.AreEqual(TokenType.EndTag, tokens[3].TokenType);
            Assert.AreEqual("div", ((EndTag)tokens[3]).Name);
        }

        [TestMethod]
        public void TestQuotedString()
        {
            var tokens = Tokenizer.Parse("This is a 'quoted string'");
            Assert.AreEqual(2, tokens.Count);

            Assert.AreEqual(TokenType.Text, tokens[0].TokenType);
            Assert.AreEqual("This is a ", ((Text)tokens[0]).Content);

            Assert.AreEqual(TokenType.QuotedString, tokens[1].TokenType);
            Assert.AreEqual("quoted string", ((QuotedString)tokens[1]).Content);
            Assert.AreEqual(QuoteMarkType.SingleQuote, ((QuotedString)tokens[1]).QuoteMark);
        }
    }
}