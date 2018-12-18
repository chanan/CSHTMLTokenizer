using CSHTMLTokenizer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSHTMLTokenizer.Test
{
    [TestClass]
    public class CSAttributeValueTests
    {
        [TestMethod]
        public void TestVariable()
        {
            var tokens = Tokenizer.Parse("This is an <b name='@testname' /> test");
            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(TokenType.StartTag, tokens[1].TokenType);
            var startTag = (StartTag)tokens[1];
            Assert.AreEqual("b", startTag.Name);
            Assert.AreEqual(2, startTag.Attributes.Count);
            Assert.AreEqual(TokenType.AttributeValue, startTag.Attributes[1].TokenType);
            var attributeValue = (AttributeValue)startTag.Attributes[1];
            Assert.AreEqual(1, attributeValue.Tokens.Count);
            Assert.AreEqual(TokenType.AttributeValueStatement, attributeValue.Tokens[0].TokenType);
            var attributeValueStatement = (AttributeValueStatement)attributeValue.Tokens[0];
            Assert.AreEqual(false, attributeValueStatement.HasParentheses);
            Assert.AreEqual("testname", attributeValueStatement.Content);
        }

        [TestMethod]
        public void TestStatement()
        {
            var tokens = Tokenizer.Parse("This is an <div onclick='@(() => onclick(\"hello\"))' /> test");
            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(TokenType.StartTag, tokens[1].TokenType);
            var startTag = (StartTag)tokens[1];
            Assert.AreEqual("div", startTag.Name);
            Assert.AreEqual(2, startTag.Attributes.Count);
            Assert.AreEqual(TokenType.AttributeValue, startTag.Attributes[1].TokenType);
            var attributeValue = (AttributeValue)startTag.Attributes[1];
            Assert.AreEqual(1, attributeValue.Tokens.Count);
            Assert.AreEqual(TokenType.AttributeValueStatement, attributeValue.Tokens[0].TokenType);
            var attributeValueStatement = (AttributeValueStatement)attributeValue.Tokens[0];
            Assert.AreEqual(true, attributeValueStatement.HasParentheses);
            Assert.AreEqual("() => onclick(\"hello\")", attributeValueStatement.Content);
        }
    }
}
