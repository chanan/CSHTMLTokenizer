using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class AttributeName : IToken
    {
        public StringBuilder Name = new StringBuilder();
        public TokenType TokenType => TokenType.AttributeName;

        public bool IsEmpty => Name.Length == 0;

        public void Append(char ch)
        {
            Name.Append(ch);
        }

        public string ToHtml()
        {
            return " " + Name.ToString();
        }
    }
}
