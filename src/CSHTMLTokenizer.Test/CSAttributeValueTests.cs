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
            System.Collections.Generic.List<IToken> tokens = Tokenizer.Parse("This is an <b name='@testname' /> test");
            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(TokenType.StartTag, tokens[1].TokenType);
            StartTag startTag = (StartTag)tokens[1];
            Assert.AreEqual("b", startTag.Name);
            Assert.AreEqual(1, startTag.Attributes.Count);
            Assert.AreEqual(TokenType.Attribute, startTag.Attributes[0].TokenType);
            AttributeToken attribute = (AttributeToken)startTag.Attributes[0];
            Assert.AreEqual("name", attribute.Name);
            Assert.AreEqual(true, attribute.Value.IsCSStatement);
            Assert.AreEqual(false, attribute.Value.HasParentheses);
            Assert.AreEqual("testname", attribute.Value.Content);
        }

        [TestMethod]
        public void TestStatement()
        {
            System.Collections.Generic.List<IToken> tokens = Tokenizer.Parse("This is an <div onclick='@(() => onclick(\"hello\"))' /> test");
            Assert.AreEqual(3, tokens.Count);
            Assert.AreEqual(TokenType.StartTag, tokens[1].TokenType);
            StartTag startTag = (StartTag)tokens[1];
            Assert.AreEqual("div", startTag.Name);
            Assert.AreEqual(1, startTag.Attributes.Count);
            Assert.AreEqual(TokenType.Attribute, startTag.Attributes[0].TokenType);
            AttributeToken attribute = (AttributeToken)startTag.Attributes[0];
            Assert.AreEqual("onclick", attribute.Name);
            Assert.AreEqual(true, attribute.Value.IsCSStatement);
            Assert.AreEqual(true, attribute.Value.HasParentheses);
            Assert.AreEqual("() => onclick(\"hello\")", attribute.Value.Content);
        }
    }
}
