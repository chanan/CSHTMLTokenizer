using System;

namespace CSHTMLTokenizer.Tokens
{
    public interface IToken
    {
        string ToHtml();
        TokenType TokenType { get; }
        void Append(char ch);
        bool IsEmpty { get; }
        Guid Id { get; }
    }
}
