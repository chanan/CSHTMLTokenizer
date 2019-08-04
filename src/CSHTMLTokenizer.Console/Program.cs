using CSHTMLTokenizer.Tokens;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string str = @"<Styled @bind-Classname=""@hover"">
    &:hover {
        color: @color;
    }
</Styled>";
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

            System.Console.WriteLine();
            System.Console.WriteLine("------------------------------------------------------");
            System.Console.WriteLine();

            str = @"<div class=""@hover"">
    Hover to change color.
</div>

@code {
    private string color = ""white"";

        protected override async Task OnInitAsync()
        {
            hover = await Styled.Css($@""
            padding: 32px;
            background-color: hotpink;
            font-size: 24px;
            border-radius: 4px;
            &:hover {{
                color: {color};
            }}
        "");
        }
    }";

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
