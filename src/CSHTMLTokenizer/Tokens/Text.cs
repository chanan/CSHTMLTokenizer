using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class Text : IToken
    {
        public string Content => _content.ToString();
        public TokenType TokenType => TokenType.Text;
        public bool IsEmpty => _content.Length == 0;
        private StringBuilder _content = new StringBuilder();
        public List<IToken> Tokens { get; set; } = new List<IToken>();
        public void Append(char ch)
        {
            _content.Append(ch);
        }

        public string ToHtml()
        {
            var sb = new StringBuilder();
            if (Tokens.Count == 0) return _content.ToString();
            foreach (var token in Tokens) sb.Append(token.ToHtml());
            return sb.ToString();
        }
    }
}