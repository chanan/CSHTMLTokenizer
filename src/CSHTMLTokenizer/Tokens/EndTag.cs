using System;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class EndTag : IToken
    {
        public StringBuilder Name = new StringBuilder();
        public TokenType TokenType => TokenType.EndTag;
        public bool IsEmpty => Name.Length == 0;
        public string ToHtml()
        {
            return "</" + Name.ToString() + ">";
        }

        public void Append(Char ch)
        {
            Name.Append(ch);
        }
    }
}
