using CSHTMLTokenizer.Tokens;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var tokens = Tokenizer.Parse("This is a test!<br>This is a <b n='v'>bold</b>");
            System.Console.WriteLine(Print(tokens));
            System.Console.ReadKey();
        }

        public static string Print(List<IToken> tokens)
        {
            var sb = new StringBuilder();
            foreach (var token in tokens)
            {
                sb.Append(token.ToHtml());
            }
            return sb.ToString();
        }
    }
}
