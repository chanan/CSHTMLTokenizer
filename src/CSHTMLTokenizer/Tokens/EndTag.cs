using System;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class EndTag : IToken
    {
        public string Name => _name.ToString();
        public TokenType TokenType => TokenType.EndTag;
        public bool IsEmpty => _name.Length == 0;
        public StringBuilder _name = new StringBuilder();
        public string ToHtml()
        {
            return "</" + Name + ">";
        }

        public void Append(Char ch)
        {
            _name.Append(ch);
        }
    }
}