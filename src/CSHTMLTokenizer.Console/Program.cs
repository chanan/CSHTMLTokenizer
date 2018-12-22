using CSHTMLTokenizer.Tokens;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = @"@page '/'

@foreach(var item in list) 
{
    <h1></h1>
}

@ functions{
    var list = new List<String /> { 'hello', 'world' };
}";
            var tokens = Tokenizer.Parse(str);
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
