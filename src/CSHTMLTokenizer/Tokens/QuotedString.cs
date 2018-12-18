using System;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class QuotedString : IToken
    {
        private StringBuilder _content = new StringBuilder();
        public string Content => _content.ToString();
        public TokenType TokenType => TokenType.QuotedString;
        public bool IsEmpty => _content.Length == 0;
        public QuoteMarkType QuoteMark { get; set; }
        public void Append(char ch)
        {
            _content.Append(ch);
        }

        public string ToHtml()
        {
            var quote = GetQuoteChar();
            return quote + _content.ToString() + quote;
        }

        private string GetQuoteChar()
        {
            switch (QuoteMark)
            {
                case QuoteMarkType.Unquoted:
                    return string.Empty;
                case QuoteMarkType.DoubleQuote:
                    return "\"";
                case QuoteMarkType.SingleQuote:
                    return "'";
                default:
                    return string.Empty;
            }
        }
    }
}
