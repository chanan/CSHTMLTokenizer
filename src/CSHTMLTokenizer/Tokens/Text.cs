using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class Text : IToken
    {
        public string Content => _content.ToString();
        public TokenType TokenType => TokenType.Text;
        public bool IsEmpty => _content.Length == 0;
        private StringBuilder _content = new StringBuilder();
        public void Append(char ch)
        {
            _content.Append(ch);
        }

        public string ToHtml()
        {
            return Content;
        }
    }
}