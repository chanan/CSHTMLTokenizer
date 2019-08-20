using System;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class CSSValue : IToken
    {
        public TokenType TokenType => TokenType.CSSValue;

        public bool IsEmpty => Tokens.Count == 0;

        public Guid Id => Guid.NewGuid();

        public void Append(char ch)
        {
            throw new NotImplementedException();
        }

        public string ToHtml()
        {
            StringBuilder sb = new StringBuilder();
            foreach (IToken token in Tokens)
            {
                sb.Append(token.ToHtml());
            }
            return sb.ToString();
        }
        public List<IToken> Tokens { get; set; } = new List<IToken>();

        public void Add(IToken token)
        {
            Tokens.Add(token);
        }
    }
}
