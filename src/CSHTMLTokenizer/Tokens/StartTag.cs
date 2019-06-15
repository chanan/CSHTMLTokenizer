using System;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class StartTag : IToken
    {
        public string Name => _name.ToString();
        public bool IsSelfClosingTag { get; set; } = false;
        public bool IsGeneric { get; set; } = false;
        public TokenType TokenType => TokenType.StartTag;
        public List<IToken> Attributes { get; set; } = new List<IToken>();
        public bool IsEmpty => _name.Length == 0;
        public Guid Id { get; } = Guid.NewGuid();
        public LineType LineType { get; set; } = LineType.SingleLine;

        private readonly StringBuilder _name = new StringBuilder();

        public string ToHtml()
        {
            StringBuilder sb = new StringBuilder();
            if (LineType == LineType.SingleLine || LineType == LineType.MultiLineStart)
            {
                sb.Append('<').Append(Name);
            }

            if (Attributes.Count > 0)
            {
                foreach (IToken token in Attributes)
                {
                    sb.Append(token.ToHtml());
                }
            }

            if (LineType == LineType.SingleLine || LineType == LineType.MultiLineEnd)
            {
                if (IsSelfClosingTag && !IsGeneric)
                {
                    string spacer =
                        (LineType == LineType.MultiLineEnd && Attributes.Count == 0)
                        ? string.Empty : " ";
                    sb.Append(spacer).Append("/");
                }
                sb.Append('>');
            }
            return sb.ToString();
        }

        public void Append(char ch)
        {
            _name.Append(ch);
        }
    }
}
