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
        EndOfFile,
        QuotedString,
        CSBlockStart,
        CSBlockEnd,
        CSLine,
        StartOfLine,
        EndOfLine
    }
}
