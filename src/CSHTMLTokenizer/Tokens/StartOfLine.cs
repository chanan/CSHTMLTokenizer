using System;

namespace CSHTMLTokenizer.Tokens
{
    public class StartOfLine : IToken
    {
        public TokenType TokenType => TokenType.StartOfLine;

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
