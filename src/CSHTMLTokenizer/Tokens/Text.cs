using System;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class Text : IToken
    {
        public Text() { }
        public Text(string text)
        {
            _content.Append(text);
        }
        public string Content => _content.ToString();
        public TokenType TokenType => TokenType.Text;
        public bool IsEmpty => _content.Length == 0;
        private readonly StringBuilder _content = new StringBuilder();
        public Guid Id { get; } = Guid.NewGuid();

        public void Append(char ch)
        {
            _content.Append(ch);
        }

        public string ToHtml()
        {
            return _content.ToString();
        }

        public override string ToString()
        {
            return ToHtml();
        }
    }
}