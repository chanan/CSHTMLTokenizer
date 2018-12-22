using System;

namespace CSHTMLTokenizer.Tokens
{
    public class CSBlockEnd : IToken
    {
        public TokenType TokenType => TokenType.CSBlockEnd;
        public bool IsEmpty => false;
        public Guid Id { get; } = Guid.NewGuid();

        public void Append(char ch)
        {
            throw new NotImplementedException();
        }

        public string ToHtml()
        {
            return "}";
        }
    }
}
