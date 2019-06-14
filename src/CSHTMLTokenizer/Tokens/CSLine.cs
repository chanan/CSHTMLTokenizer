using System;
using System.Text;

namespace CSHTMLTokenizer.Tokens
{
    public class CSLine : IToken
    {
        private readonly StringBuilder _line = new StringBuilder();
        public string Line => _line.ToString();
        public TokenType TokenType => TokenType.CSLine;
        public CSLineType LineType { get; set; }
        public bool IsEmpty => _line.Length == 0;
        public Guid Id { get; } = Guid.NewGuid();

        public void Append(char ch)
        {
            _line.Append(ch);
        }

        public string ToHtml()
        {
            return '@' + GetLineType(LineType) + " " + _line.ToString();
        }

        private string GetLineType(CSLineType lineType)
        {
            switch (lineType)
            {
                case CSLineType.AddTagHelper:
                    return "addTagHelper";
                case CSLineType.Implements:
                    return "implements";
                case CSLineType.Inherit:
                    return "inherit";
                case CSLineType.Inject:
                    return "inject";
                case CSLineType.Layout:
                    return "layout";
                case CSLineType.Page:
                    return "page";
                case CSLineType.Using:
                    return "using";
                case CSLineType.Typeparam:
                    return "typeparam";
                case CSLineType.Namespace:
                    return "namespace";
                case CSLineType.Attribute:
                    return "attribute";
                default:
                    return "";
            }

        }
    }
}
