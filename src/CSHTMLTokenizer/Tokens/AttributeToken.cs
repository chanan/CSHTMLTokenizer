using System;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class AttributeToken : IToken
    {
        private readonly StringBuilder _name = new StringBuilder();
        public string Name => _name.ToString();
        public QuotedString Value { get; } = new QuotedString();
        public TokenType TokenType => TokenType.Attribute;
        public bool IsEmpty => _name.Length == 0;
        public Guid Id { get; } = Guid.NewGuid();
        public bool NameOnly { get; set; } = true;
        public bool IsFirstInLine { get; set; } = false;

        public void Append(char ch)
        {
            _name.Append(ch);
        }

        public string ToHtml()
        {
            string spacer = IsFirstInLine ? string.Empty : " ";
            if (NameOnly)
            {
                return spacer + Name;
            }

            return spacer + Name + "=" + Value.ToHtml();

        }
    }
}