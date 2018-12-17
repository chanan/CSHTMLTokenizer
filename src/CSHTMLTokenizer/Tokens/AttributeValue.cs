using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class AttributeValue : IToken
    {
        public string Value => _value.ToString();
        public enum QuoteMarkType {  Unquoted, DoubleQuote, SingleQuote}
        public TokenType TokenType => TokenType.AttributeValue;
        public QuoteMarkType QuoteMark { get; set; }
        public bool IsEmpty => _value.Length == 0;
        private StringBuilder _value = new StringBuilder();
        public void Append(char ch)
        {
            _value.Append(ch);
        }

        public string ToHtml()
        {
            var quote = GetQuoteChar();
            return "=" + quote + Value + quote;
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
