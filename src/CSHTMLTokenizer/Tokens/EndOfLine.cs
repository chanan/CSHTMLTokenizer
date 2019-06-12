using System;

namespace CSHTMLTokenizer.Tokens
{
    internal class EndOfLine : IToken
    {
        public TokenType TokenType => TokenType.EndOfLine;

        public bool IsEmpty => false;

        public Guid Id => Guid.NewGuid();

        public void Append(char ch)
        {
            throw new NotImplementedException();
        }

        public string ToHtml()
        {
            return System.Environment.NewLine;
        }
    }
}
