using System;

namespace CSHTMLTokenizer.Tokens
{
    internal class EndOfFile : IToken
    {
        public TokenType TokenType => TokenType.EndOfFile;

        public bool IsEmpty => false;

        public Guid Id => Guid.NewGuid();

        public void Append(char ch)
        {
            throw new NotImplementedException();
        }

        public string ToHtml()
        {
            return string.Empty;
        }
    }
}
