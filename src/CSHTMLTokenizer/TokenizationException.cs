using System;

namespace CSHTMLTokenizer
{
    public class TokenizationException : InvalidOperationException
    {
        public int LineNumber { get; set; }
        public int CharNumber { get; set; }

        public TokenizationException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
