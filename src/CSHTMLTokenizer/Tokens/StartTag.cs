using System;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class StartTag : IToken
    {
        public StringBuilder Name = new StringBuilder();
        public bool IsSelfClosingTag { get; set; } = false;
        public TokenType TokenType => TokenType.StartTag;
        public List<IToken> Attributes { get; set; } = new List<IToken>();
        public bool IsEmpty => Name.Length == 0;

        public string ToHtml()
        {
            var sb = new StringBuilder();
            sb.Append('<').Append(Name.ToString());
            if(Attributes.Count > 0)
            {
                foreach (var token in Attributes) sb.Append(token.ToHtml());
            }
            if (IsSelfClosingTag) sb.Append(" /");
            sb.Append('>');
            return sb.ToString();
        }

        public void Append(Char ch)
        {
            Name.Append(ch);
        }
    }
}
