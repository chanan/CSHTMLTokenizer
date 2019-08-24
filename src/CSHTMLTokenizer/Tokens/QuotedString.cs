using System;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class QuotedString : IToken
    {
        private readonly StringBuilder _content = new StringBuilder();
        public string Content => _content.ToString();
        public TokenType TokenType => TokenType.QuotedString;
        public bool IsEmpty => !IsMultiLineStatement && _content.Length == 0;
        public QuoteMarkType QuoteMark { get; set; }
        public Guid Id { get; } = Guid.NewGuid();
        public bool HasParentheses { get; set; }
        public bool IsCSStatement { get; set; }
        public bool IsMultiLineStatement { get; set; }
        public LineType LineType { get; set; } = LineType.SingleLine;

        public void Append(char ch)
        {
            _content.Append(ch);
        }

        public string ToHtml()
        {
            string quote = GetQuoteChar();
            StringBuilder sb = new StringBuilder();

            if (LineType == LineType.SingleLine || LineType == LineType.MultiLineStart)
            {
                if (IsMultiLineStatement)
                {
                    sb.Append('@');
                }
                sb.Append(quote);
                if (IsCSStatement)
                {
                    sb.Append('@');
                }

                if (HasParentheses)
                {
                    sb.Append('(');
                }
            }

            sb.Append(_content);

            if (LineType == LineType.SingleLine || LineType == LineType.MultiLineEnd)
            {
                if (HasParentheses)
                {
                    sb.Append(')');
                }
                sb.Append(quote);
            }
            return sb.ToString();
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

        public override string ToString()
        {
            return ToHtml();
        }
    }
}
