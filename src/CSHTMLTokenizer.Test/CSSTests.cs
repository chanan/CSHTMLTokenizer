using CSHTMLTokenizer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CSHTMLTokenizer.Test
{
    [TestClass]
    public class CSSTests
    {
        [TestMethod]
        public void TestCss()
        {
            string str = @"color: red;
font-family: ""open sans"", serif;

h1 {
    color: pink;
}

h2 { color: blue; }

@media only screen and (min-width: 320px) and (max-width: 480px) {
    h1 {
        color: green;
    }
}";

            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(14, lines.Count);
            IToken token = lines[0].Tokens[0];
            Assert.AreEqual(TokenType.CSSProperty, token.TokenType);
            Assert.AreEqual("color", ((CSSProperty)token).Content.Trim());
            token = lines[0].Tokens[1];
            Assert.AreEqual(TokenType.CSSValue, token.TokenType);
            Assert.AreEqual("red;", ((Text)((CSSValue)token).Tokens[0]).Content.Trim());

            token = lines[1].Tokens[0];
            Assert.AreEqual(TokenType.CSSProperty, token.TokenType);
            Assert.AreEqual("font-family", ((CSSProperty)token).Content.Trim());
            token = lines[1].Tokens[1];
            Assert.AreEqual(TokenType.CSSValue, token.TokenType);
            Assert.AreEqual("open sans", ((QuotedString)((CSSValue)token).Tokens[1]).Content.Trim());
            Assert.AreEqual(", serif;", ((Text)((CSSValue)token).Tokens[2]).Content.Trim());

            Assert.AreEqual(TokenType.CSSOpenClass, lines[3].Tokens[0].TokenType);
            Assert.AreEqual("h1", ((CSSOpenClass)lines[3].Tokens[0]).Content.Trim());

            Assert.AreEqual(TokenType.CSSOpenClass, lines[7].Tokens[0].TokenType);
            Assert.AreEqual("h2", ((CSSOpenClass)lines[7].Tokens[0]).Content.Trim());
            token = lines[7].Tokens[1];
            Assert.AreEqual(TokenType.CSSProperty, token.TokenType);
            Assert.AreEqual("color", ((CSSProperty)token).Content.Trim());
            token = lines[7].Tokens[2];
            Assert.AreEqual(TokenType.CSSValue, token.TokenType);
            Assert.AreEqual("blue;", ((Text)((CSSValue)token).Tokens[0]).Content.Trim());
            Assert.AreEqual(TokenType.CSSCloseClass, lines[7].Tokens[3].TokenType);

        }

        [TestMethod]
        public void EnsureNoCSStartBlock()
        {
            string str = @"@media only screen and (min-width: 320px) and (max-width: 480px) {
    color: green;
}";

            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(3, lines.Count);
            Assert.AreEqual(2, lines[0].Tokens.Count);
            Assert.AreEqual(TokenType.CSSOpenClass, lines[0].Tokens[0].TokenType);
            Assert.IsTrue(((CSSOpenClass)lines[0].Tokens[0]).Content.Trim().StartsWith("@"));
        }

        [TestMethod]
        public void TestCSSClass()
        {
            string str = @"<Styled @bind-Classname=""@hover"">
    &:hover {
        color: @color;
    }
</Styled>";
            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(5, lines.Count);
            Assert.AreEqual(TokenType.CSSOpenClass, lines[1].Tokens[0].TokenType);
            Assert.AreEqual("&:hover", ((CSSOpenClass)lines[1].Tokens[0]).Content.Trim());
            Assert.AreEqual(TokenType.CSSCloseClass, lines[3].Tokens[1].TokenType);
        }

        [TestMethod]
        public void EnsureNoCSSCloseTag()
        {
            string str = @"@code {
    private void onclick()
	{
        _showBig = !_showBig;
    }
}";
            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(6, lines.Count);
            Assert.AreEqual(TokenType.CSBlockEnd, lines[5].Tokens[0].TokenType);
        }
    }
}
