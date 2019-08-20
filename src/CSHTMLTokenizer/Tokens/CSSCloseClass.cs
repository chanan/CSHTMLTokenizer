using System;

namespace CSHTMLTokenizer.Tokens
{
    public class CSSCloseClass : IToken
    {
        public TokenType TokenType => TokenType.CSSCloseClass;

        public bool IsEmpty => false;
        public Guid Id { get; } = Guid.NewGuid();

        public void Append(char ch)
        {
            throw new NotImplementedException();
        }

        public string ToHtml()
        {
            return ";";
        }
    }
}
