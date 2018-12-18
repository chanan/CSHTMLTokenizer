using System;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class AttributeValueStatement : IToken
    {
        public TokenType TokenType => TokenType.AttributeValueStatement;
        public bool IsEmpty => _content.Length == 0;
        public bool HasParentheses { get; set; }
        private StringBuilder _content = new StringBuilder();
        public string Content => _content.ToString();
        public void Append(char ch)
        {
            _content.Append(ch);
        }

        public string ToHtml()
        {
            var sb = new StringBuilder();
            sb.Append('@');
            if (HasParentheses) sb.Append('(');
            sb.Append(_content);
            if (HasParentheses) sb.Append(')');
            return sb.ToString();
        }
    }
}
