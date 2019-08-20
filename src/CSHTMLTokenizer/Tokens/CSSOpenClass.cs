using System;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class CSSOpenClass : IToken
    {
        public CSSOpenClass()
        {

        }

        public CSSOpenClass(string cssOpenClass)
        {
            _content.Append(cssOpenClass);
        }

        public string Content => _content.ToString();
        public TokenType TokenType => TokenType.CSSOpenClass;

        public bool IsEmpty => _content.Length == 0;
        private readonly StringBuilder _content = new StringBuilder();
        public Guid Id { get; } = Guid.NewGuid();

        public void Append(char ch)
        {
            _content.Append(ch);
        }

        public string ToHtml()
        {
            return _content.ToString() + " {";
        }
    }
}
