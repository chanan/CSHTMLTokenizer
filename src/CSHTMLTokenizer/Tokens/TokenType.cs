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
        EOF,
        QuotedString,
        CSBlockStart,
        CSBlockEnd,
        CSLine
    }
}
