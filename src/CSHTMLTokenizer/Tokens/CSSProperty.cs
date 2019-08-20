using System;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class CSSProperty : IToken
    {
        public CSSProperty()
        {

        }
        public CSSProperty(string property)
        {
            _content.Append(property);
        }

        private readonly StringBuilder _content = new StringBuilder();
        public TokenType TokenType => TokenType.CSSProperty;

        public bool IsEmpty => _content.Length == 0;

        public Guid Id => Guid.NewGuid();
        public string Content => _content.ToString();

        public void Append(char ch)
        {
            _content.Append(ch);
        }

        public string ToHtml()
        {
            return _content.ToString();
        }
    }
}
