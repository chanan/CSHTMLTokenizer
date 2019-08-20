using CSHTMLTokenizer.Tokens;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string str = @"color: red;
font-family: ""open sans"", serif;

h1 {
    color: pink;
}

h2 { color: blue; }

@media only screen and (min-width: 320px) and (max-width: 480px) {
    h1 {
        color: green;
    }
}";

            List<Line> lines = Tokenizer.Parse(str);
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
