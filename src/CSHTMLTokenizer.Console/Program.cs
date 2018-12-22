﻿using CSHTMLTokenizer.Tokens;
using System.Collections.Generic;
using System.Text;

namespace CSHTMLTokenizer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = "This is an <div onclick='@(() => onclick(\"hello\"))' /> test";
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
