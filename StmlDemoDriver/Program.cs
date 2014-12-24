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
            //var text = "This [b]is[/b] [#] a game";
            //var html = StmlParser.Parse(text).ToString();

            var text = File.ReadAllText("sample2.txt");
            var html = StmlParser.Parse(text, true).ToString();
            const string tmpl = "<html><head></head><body><div style=\"white-space: pre-wrap;\">{0}</div><pre>{0}</pre></body></html>";
            File.WriteAllText(@"C:\temp\tests\sample.htm", string.Format(tmpl, html));

            Console.WriteLine(text);
            Console.WriteLine(html);

            Console.WriteLine("\nHit any key to Quite...");
            Console.ReadKey();
        }
    }
}
