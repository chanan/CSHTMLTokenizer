using System;
namespace CSHTMLTokenizer.Tokens
{
    public enum TokenType
    {
        DOCTYPE,
        StartTag,
        EndTag,
        Comment,
        Character,
        Text,
        AttributeName,
        AttributeValue,
        EOF,
        QuotedString,
        AttributeValueStatement
    }
}
