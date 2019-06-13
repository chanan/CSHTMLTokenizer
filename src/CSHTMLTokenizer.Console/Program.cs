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
            List<Line> lines = Tokenizer.Parse(str);
            System.Console.WriteLine(Print(lines));

            System.Console.WriteLine();
            System.Console.WriteLine("------------------------------------------------------");
            System.Console.WriteLine();

            str = @"<div
    class='test'
>Test?</div>";

            lines = Tokenizer.Parse(str);
            System.Console.WriteLine(Print(lines));
        }

        public static string Print(List<Line> lines)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Line line in lines)
            {
                sb.Append(line.ToHtml());
            }
            return sb.ToString();
        }
    }
}
