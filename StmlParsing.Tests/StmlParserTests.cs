using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StmlParsing.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class StmlParserTests
    {
        [Test]
        public void Can_parse_colors_int_font()
        {
            const string input = "[navy]M[/navy]y [red]red[/red] [fuchsia]color[/fuchsia] [blue]goes[/blue] [green]h[orange]e[/orange]r[/green]e";
            const string expected = "<font color=\"navy\">M</font>y <font color=\"red\">red</font> <font color=\"fuchsia\">color</font> <font color=\"blue\">goes</font> <font color=\"green\">h<font color=\"orange\">e</font>r</font>e";
            var actual = StmlParser.Parse(input).ToString();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_simple_tags()
        {
            const string input = "[b]M[/b]y [i]red [s]lines[/s][/i] [small]color[/small] [u]goes[/u] [dir]h[sub]e[/sub]r[/dir]e and [code]45[sup]2[/sup][/code]";
            const string expected = "<b>M</b>y <i>red <s>lines</s></i> <small>color</small> <u>goes</u> <dir>h<sub>e</sub>r</dir>e and <code>45<sup>2</sup></code>";
            var actual = StmlParser.Parse(input).ToString();

            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void Can_parse_combined_simple_tags()
        {
            const string input = "[b][i][u]bold3[/u][/i][/b]";
            const string expected = "<b><i><u>bold3</u></i></b>";
            var actual = StmlParser.Parse(input).ToString();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_tags_within_similar_tags()
        {
            const string input = "This is [red]My red and [blue]blue[/blue] and [red]another red[/red] text[/red] for sample";
            const string expected = "This is <font color=\"red\">My red and <font color=\"blue\">blue</font> and <font color=\"red\">another red</font> text</font> for sample";
            var actual = StmlParser.Parse(input).ToString();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_inner_tags_of_same_type()
        {
            const string input = "[b][b][b]bold3[/b][/b][/b]";
            const string expected = "<b><b><b>bold3</b></b></b>";
            var actual = StmlParser.Parse(input).ToString();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_image_tag()
        {
            const string input1 = "This is [img]file:///L:/Media/Pictures/Mickey-Mouse-and-Donald-Duck-in-No-Service.jpg[/img]";
            const string expected1 = "This is <img src=\"file:///L:/Media/Pictures/Mickey-Mouse-and-Donald-Duck-in-No-Service.jpg\"/>";
            var actual1 = StmlParser.Parse(input1).ToString();

            Assert.AreEqual(expected1, actual1);
        }

        [Test]
        public void Can_parse_image_tag_with_alt_text()
        {
            const string input = "This is [img=http://www.samples/images/1.jpg]My Pic[/img]";
            const string expected = "This is <img src=\"http://www.samples/images/1.jpg\" alt=\"My Pic\"/>";
            var actual = StmlParser.Parse(input).ToString();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_hyperlink()
        {
            const string input1 = "This is [url]http://whichman.com[/url]";
            const string expected1 = "This is <a href=\"http://whichman.com\" target=\"_blank\">http://whichman.com</a>";
            var actual1 = StmlParser.Parse(input1).ToString();

            Assert.AreEqual(expected1, actual1);
        }

        [Test]
        public void Can_parse_hyperlink_with_text()
        {
            const string input = "This is [url=http://whichman.com]Whichman, INC.[/url]";
            const string expected = "This is <a href=\"http://whichman.com\" target=\"_blank\">Whichman, INC.</a>";
            var actual = StmlParser.Parse(input).ToString();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_email_link()
        {
            const string input = "This is [email]support@whichman.com[/email]";
            const string expected = "This is <a href=\"mailto:support@whichman.com\">support@whichman.com</a>";
            var actual = StmlParser.Parse(input).ToString();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_email_link_with_text()
        {
            const string input = "This is [email=support@whichman.com]Whichman Support[/email]";
            const string expected = "This is <a href=\"mailto:support@whichman.com\">Whichman Support</a>";
            var actual = StmlParser.Parse(input).ToString();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_ordered_list()
        {
            const string input = @"This is an [b]Ordered[/b] list
[olist]
   [#]This is line 1
   [#]This is line 2
   [#]This is line 3
[/olist]
This is a line after";
            const string expected = @"This is an <b>Ordered</b> list
<ol><li>This is line 1</li><li>This is line 2</li><li>This is line 3</li></ol>
This is a line after";
            var actual = StmlParser.Parse(input).ToString();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_unordered_list()
        {
            const string input = @"This is an [b]Ordered[/b] list
[ulist]
   [#]This is line 1
   [#]This is line 2
   [#]This is line 3
[/ulist]
This is a line after";
            const string expected = @"This is an <b>Ordered</b> list
<ul><li>This is line 1</li><li>This is line 2</li><li>This is line 3</li></ul>
This is a line after";
            var actual = StmlParser.Parse(input).ToString();
            //System.IO.File.WriteAllText(@"C:\temp\tests\simple.htm", actual1);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Line_breaks_after_block_tags_are_removed()
        {
            const string input1 = @"[h1]Header 1[/h1]
[ul=square]
[li]This is line 1[/li]
[li]This is line 2[/li]
[/ul]

[h2]Header 2[/h2]

[h3]Header 3[/h3]
";
            const string expected = @"<h1>Header 1</h1><ul type=""square""><li>This is line 1</li><li>This is line 2</li></ul><h2>Header 2</h2><h3>Header 3</h3>";
            var actual = StmlParser.Parse(input1, true).ToString();
            //System.IO.File.WriteAllText(@"C:\temp\tests\simple.htm", actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_sub_olists_of_the_same_type()
        {
            const string input = @"[olist]
    [#]This is line 1
    [#]This is line 2
        [olist]
            [#]This is line 2.1
            [#]This is line 2.2
            [#]This is line 2.3
    and it continues here
        [/olist]
    [#]This is line 3
[/olist]
";
            const string expected = @"<ol><li>This is line 1</li><li>This is line 2
        <ol><li>This is line 2.1</li><li>This is line 2.2</li><li>This is line 2.3
    and it continues here</li></ol></li><li>This is line 3</li></ol>";
            var actual = StmlParser.Parse(input, true).ToString();
            //System.IO.File.WriteAllText(@"C:\temp\tests\simple.htm", actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_sub_ulists_of_the_same_type()
        {
            const string input = @"[ulist]
    [#]This is line 1
    [#]This is line 2
        [ulist]
            [#]This is line 2.1
            [#]This is line 2.2
            [#]This is line 2.3
    and it continues here
        [/ulist]
    [#]This is line 3
[/ulist]
";
            const string expected = @"<ul><li>This is line 1</li><li>This is line 2
        <ul><li>This is line 2.1</li><li>This is line 2.2</li><li>This is line 2.3
    and it continues here</li></ul></li><li>This is line 3</li></ul>";
            var actual = StmlParser.Parse(input, true).ToString();
            //System.IO.File.WriteAllText(@"C:\temp\tests\simple.htm", actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_sub_ulists_within_last_list_item()
        {
            const string input = @"[ulist]
    [#]This is line 1
    [#]This is line 2
        [ulist]
            [#]This is line 2.1
            [#]This is line 2.2
            [#]This is line 2.3
    and it continues here
        [/ulist]
[/ulist]
";
            const string expected = @"<ul><li>This is line 1</li><li>This is line 2
        <ul><li>This is line 2.1</li><li>This is line 2.2</li><li>This is line 2.3
    and it continues here</li></ul></li></ul>";
            var actual = StmlParser.Parse(input, true).ToString();
            //System.IO.File.WriteAllText(@"C:\temp\tests\simple.htm", actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_parse_sub_olists_within_last_list_item()
        {
            const string input = @"[olist]
    [#]This is line 1
    [#]This is line 2
        [olist]
            [#]This is line 2.1
            [#]This is line 2.2
            [#]This is line 2.3
    and it continues here
        [/olist]
[/olist]
";
            const string expected = @"<ol><li>This is line 1</li><li>This is line 2
        <ol><li>This is line 2.1</li><li>This is line 2.2</li><li>This is line 2.3
    and it continues here</li></ol></li></ol>";
            var actual = StmlParser.Parse(input, true).ToString();
            //System.IO.File.WriteAllText(@"C:\temp\tests\simple.htm", actual);
            Assert.AreEqual(expected, actual);
        }

    }

}
