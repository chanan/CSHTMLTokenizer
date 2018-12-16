using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class AttributeValue : IToken
    {
        public StringBuilder Value = new StringBuilder();
        public enum QuoteMarkType {  Unquoted, DoubleQuote, SingleQuote}
        public TokenType TokenType => TokenType.AttributeValue;
        public QuoteMarkType QuoteMark { get; set; }
        public bool IsEmpty => Value.Length == 0;
        public void Append(char ch)
        {
            Value.Append(ch);
        }

        public string ToHtml()
        {
            var quote = GetQuoteChar();
            return "=" + quote + Value.ToString() + quote;
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
