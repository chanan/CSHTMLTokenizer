using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class Text : IToken
    {
        public StringBuilder Content = new StringBuilder();
        public TokenType TokenType => TokenType.Text;
        public bool IsEmpty => Content.Length == 0;

        public void Append(char ch)
        {
            Content.Append(ch);
        }

        public string ToHtml()
        {
            return Content.ToString();
        }
    }
}
