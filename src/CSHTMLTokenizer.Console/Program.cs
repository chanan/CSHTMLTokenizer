using CSHTMLTokenizer.Tokens;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string str = @"@media only screen and (min-width: 320px) and (max-width: 480px) {
    color: green;
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
