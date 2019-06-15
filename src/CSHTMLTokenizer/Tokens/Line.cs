using System;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class Line : IToken
    {
        public TokenType TokenType => TokenType.Line;

        public bool IsEmpty => false;

        public Guid Id => Guid.NewGuid();
        public bool LastLine { get; set; }

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
            if (!LastLine)
            {
                sb.Append(Environment.NewLine);
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
