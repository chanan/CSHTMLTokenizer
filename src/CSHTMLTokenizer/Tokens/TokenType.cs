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
        Attribute,
        QuotedString,
        CSBlockStart,
        CSBlockEnd,
        CSLine,
        Line,
        CSSProperty,
        CSSValue,
        CSSOpenClass,
        CSSCloseClass
    }
}
