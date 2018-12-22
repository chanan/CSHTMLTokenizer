using System;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class CSBlockStart : IToken
    {
        public TokenType TokenType => TokenType.CSBlockStart;
        public bool IsEmpty => false;
        public bool IsFunctions { get; set; }
        public bool IsOpenBrace { get; set; }
        public Guid Id { get; } = Guid.NewGuid();

        public void Append(char ch)
        {
            throw new InvalidOperationException();
        }

        public string ToHtml()
        {
            var sb = new StringBuilder();
            sb.Append('@');
            if (IsFunctions) sb.Append("functions");
            if(IsOpenBrace) sb.Append(" {");
            return sb.ToString();
        }
    }
}
