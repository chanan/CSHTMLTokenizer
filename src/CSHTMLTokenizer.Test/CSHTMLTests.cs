﻿using CSHTMLTokenizer.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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

            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(3, lines.Count);

            CSLine csLine1 = (CSLine)lines[0].Tokens[0];
            Assert.AreEqual(" '/'", csLine1.Line);
            Assert.AreEqual(CSLineType.Page, csLine1.LineType);

            CSLine csLine2 = (CSLine)lines[1].Tokens[0];
            Assert.AreEqual(" BlazorPrettyCode", csLine2.Line);
            Assert.AreEqual(CSLineType.Using, csLine2.LineType);

            Assert.AreEqual(TokenType.StartTag, lines[2].Tokens[0].TokenType);
        }

        [TestMethod]
        public void TestForEach()
        {
            string str = @"@foreach(var item in Items) {
    <RenderItem Item='@item' />
}";

            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(3, lines.Count);

            Assert.AreEqual(TokenType.CSBlockStart, lines[0].Tokens[0].TokenType);

            Assert.AreEqual("foreach(var item in Items) {", ((Text)lines[0].Tokens[1]).Content.Trim());

            Assert.AreEqual(TokenType.StartTag, lines[1].Tokens[1].TokenType);
            StartTag startTag = (StartTag)lines[1].Tokens[1];
            Assert.AreEqual(true, startTag.IsSelfClosingTag);
            Assert.AreEqual("RenderItem", startTag.Name);
            Assert.AreEqual(1, startTag.Attributes.Count);
            Assert.AreEqual(LineType.SingleLine, startTag.LineType);
            Assert.AreEqual("Item", ((AttributeToken)startTag.Attributes[0]).Name);
            Assert.AreEqual(true, ((AttributeToken)startTag.Attributes[0]).Value.IsCSStatement);
            Assert.AreEqual(false, ((AttributeToken)startTag.Attributes[0]).Value.HasParentheses);
            Assert.AreEqual("item", ((AttributeToken)startTag.Attributes[0]).Value.Content);

            Assert.AreEqual(TokenType.CSBlockEnd, lines[2].Tokens[0].TokenType);
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

            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(9, lines.Count);

            Assert.AreEqual(TokenType.CSLine, lines[0].Tokens[0].TokenType);
            Assert.AreEqual(CSLineType.Page, ((CSLine)lines[0].Tokens[0]).LineType);
            Assert.AreEqual(" '/'", ((CSLine)lines[0].Tokens[0]).Line);

            Assert.AreEqual(TokenType.CSBlockStart, lines[2].Tokens[0].TokenType);
            Assert.AreEqual(false, ((CSBlockStart)lines[2].Tokens[0]).IsFunctions);
            Assert.AreEqual(false, ((CSBlockStart)lines[2].Tokens[0]).IsOpenBrace);
            Assert.AreEqual(false, ((CSBlockStart)lines[2].Tokens[0]).IsOpenBrace);

            Assert.AreEqual(true, ((Text)lines[2].Tokens[1]).Content.StartsWith("foreach(var item in list)"));

            Assert.AreEqual(TokenType.StartTag, lines[3].Tokens[1].TokenType);
            StartTag startTag = (StartTag)lines[3].Tokens[1];
            Assert.AreEqual(false, startTag.IsSelfClosingTag);
            Assert.AreEqual("h1", startTag.Name);
            Assert.AreEqual(false, startTag.IsGeneric);

            Assert.AreEqual(TokenType.CSBlockStart, lines[3].Tokens[2].TokenType);

            Assert.AreEqual("item", ((Text)lines[3].Tokens[3]).Content);

            Assert.AreEqual(TokenType.EndTag, lines[3].Tokens[4].TokenType);

            Assert.AreEqual(TokenType.CSBlockEnd, lines[4].Tokens[0].TokenType);

            Assert.AreEqual(TokenType.CSBlockStart, lines[6].Tokens[0].TokenType);
            Assert.AreEqual(true, ((CSBlockStart)lines[6].Tokens[0]).IsFunctions);

            Assert.AreEqual(TokenType.Text, lines[7].Tokens[0].TokenType);
            Assert.AreEqual(true, ((Text)lines[7].Tokens[0]).Content.Trim().StartsWith("var list"));

            Assert.AreEqual(TokenType.StartTag, lines[7].Tokens[1].TokenType);
            Assert.AreEqual("string", ((StartTag)lines[7].Tokens[1]).Name);
            Assert.AreEqual(false, ((StartTag)lines[7].Tokens[1]).IsSelfClosingTag);
            Assert.AreEqual(true, ((StartTag)lines[7].Tokens[1]).IsGeneric);

            Assert.AreEqual(TokenType.Text, lines[7].Tokens[2].TokenType);

            Assert.AreEqual(TokenType.QuotedString, lines[7].Tokens[3].TokenType);
            Assert.AreEqual(true, ((QuotedString)lines[7].Tokens[3]).Content.Contains("hello"));

            Assert.AreEqual(TokenType.Text, lines[7].Tokens[4].TokenType);

            Assert.AreEqual(TokenType.QuotedString, lines[7].Tokens[5].TokenType);
            Assert.AreEqual(true, ((QuotedString)lines[7].Tokens[5]).Content.Contains("world"));

            Assert.AreEqual(TokenType.Text, lines[7].Tokens[6].TokenType);

            Assert.AreEqual(TokenType.CSBlockEnd, lines[8].Tokens[0].TokenType);
        }

        [TestMethod]
        public void NewRazorFeaturesInPreview6()
        {
            string str = @"@page '/'
@namespace MyNamespace
@attribute [Authorize]

<div @key='key' @onclick='@Clicked' @bind='myValue' @directive @directive='value' 
@directive:key @directive:key='value' @directive-suffix @directive-suffix='value'
@directive-suffix:key @directive-suffix:key='value' />

@code {
    public void Clicked()
    {
        //Comments arent supported yet
    }
}
";

            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(15, lines.Count);

            Assert.AreEqual(TokenType.CSLine, lines[1].Tokens[0].TokenType);
            Assert.AreEqual(CSLineType.Namespace, ((CSLine)lines[1].Tokens[0]).LineType);

            Assert.AreEqual(TokenType.CSLine, lines[2].Tokens[0].TokenType);
            Assert.AreEqual(CSLineType.Attribute, ((CSLine)lines[2].Tokens[0]).LineType);

            Assert.AreEqual(TokenType.StartTag, lines[4].Tokens[0].TokenType);
            StartTag startTag = (StartTag)lines[4].Tokens[0];
            Assert.AreEqual(TokenType.Attribute, startTag.Attributes[0].TokenType);
            AttributeToken attribute = (AttributeToken)startTag.Attributes[0];
            Assert.AreEqual("@key", attribute.Name);
            Assert.AreEqual(TokenType.QuotedString, attribute.Value.TokenType);
            Assert.AreEqual("key", attribute.Value.Content);
            attribute = (AttributeToken)startTag.Attributes[1];
            Assert.AreEqual("@onclick", attribute.Name);
            Assert.AreEqual("Clicked", attribute.Value.Content);
            Assert.AreEqual(true, attribute.Value.IsCSStatement);

            Assert.AreEqual(TokenType.CSBlockStart, lines[8].Tokens[0].TokenType);
            Assert.AreEqual(true, ((CSBlockStart)lines[8].Tokens[0]).IsCode);
        }

        [TestMethod]
        public void MultiLineString()
        {
            string str = @"hover = await Styled.Css($@""
                padding: 32px;
                background-color: hotpink;""";

            List<Line> lines = Tokenizer.Parse(str);
            Assert.AreEqual(3, lines.Count);

            Assert.AreEqual(TokenType.Text, lines[0].Tokens[0].TokenType);

            Assert.AreEqual(TokenType.QuotedString, lines[0].Tokens[1].TokenType);
            QuotedString quotedString = (QuotedString)lines[0].Tokens[1];
            Assert.AreEqual(LineType.MultiLineStart, quotedString.LineType);
            Assert.AreEqual(QuoteMarkType.DoubleQuote, quotedString.QuoteMark);
            Assert.AreEqual(true, quotedString.IsMultiLineStatement);

            Assert.AreEqual(TokenType.QuotedString, lines[1].Tokens[0].TokenType);
            quotedString = (QuotedString)lines[1].Tokens[0];
            Assert.AreEqual(LineType.MultiLine, quotedString.LineType);
            Assert.AreEqual(QuoteMarkType.DoubleQuote, quotedString.QuoteMark);

            Assert.AreEqual(TokenType.QuotedString, lines[2].Tokens[0].TokenType);
            quotedString = (QuotedString)lines[2].Tokens[0];
            Assert.AreEqual(LineType.MultiLineEnd, quotedString.LineType);
            Assert.AreEqual(QuoteMarkType.DoubleQuote, quotedString.QuoteMark);
        }
    }
}
