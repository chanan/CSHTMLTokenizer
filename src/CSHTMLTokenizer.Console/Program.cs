using CSHTMLTokenizer.Tokens;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string str = @"@page '/'

@foreach(var item in list) {
    <h1>@item</h1>
}

@functions {
    var list = new List<String> { 'hello', 'world' };
}";
            List<IToken> tokens = Tokenizer.Parse(str);
            System.Console.WriteLine(Print(tokens));

            System.Console.WriteLine();
            System.Console.WriteLine("------------------------------------------------------");
            System.Console.WriteLine();

            str = @"<div
    class='test'
/>";

            tokens = Tokenizer.Parse(str);
            System.Console.WriteLine(Print(tokens));
        }

        public static string Print(List<IToken> tokens)
        {
            StringBuilder sb = new StringBuilder();
            foreach (IToken token in tokens)
            {
                sb.Append(token.ToHtml());
            }
            return sb.ToString();
        }
    }
}
