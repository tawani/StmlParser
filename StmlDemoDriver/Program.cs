using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StmlDemoDriver
{
    using System.IO;
    using StmlParsing;

    class Program
    {
        static void Main(string[] args)
        {
            var text = File.ReadAllText("sample.txt");
            //var text = "This [b]is[/b] [#] a game";
            var html = StmlParser.Parse(text).ToString();
            File.WriteAllText(@"C:\temp\tests\sample.htm", html + "<pre>" + html + "</pre>");

            Console.WriteLine(text);
            Console.WriteLine(html);

            Console.WriteLine("\nHit any key to Quite...");
            Console.ReadKey();
        }
    }
}
