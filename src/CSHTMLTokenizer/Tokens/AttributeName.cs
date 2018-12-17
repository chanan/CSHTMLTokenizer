using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class AttributeName : IToken
    {
        public string Name => _name.ToString();
        public TokenType TokenType => TokenType.AttributeName;
        public bool IsEmpty => _name.Length == 0;
        public StringBuilder _name = new StringBuilder();

        public void Append(char ch)
        {
            _name.Append(ch);
        }

        public string ToHtml()
        {
            return " " + Name;
        }
    }
}