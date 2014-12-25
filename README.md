StmlParser
==========

Simple Text Markup Language Parser is a custom HTML parser to convert user entered text into formatted XHTML documents that can be converted to XSL-FO and eventually PDF or RTF.
It parses tags similar to <b><a href="http://en.wikipedia.org/wiki/BBCode">BBCode</a></b> tags and other extension tags relevant to generated [PDF] documents.

Convert the following markup to HTML
<h3>Input Text</h3>
<pre>
Some random [blue]blue[/blue] text Base[sub]64[/sub] for 1[sup]st[/sup] game
This is a simple [b]bold[/b], [i]italic[/i], [u]underline[/u]
[olist]
    [#]This is line 1
    [#]This is line 2
        [olist=a]
            [#]This is [orange]orange[/orange] line 2.1
            [#]This is [green]green[/green] line 2.2
            [#]This is [red]red[/red] line 2.3
    and it continues here
        [/olist]
    [#]This is line 3
[/olist]
Another line
</pre>
<h3>Output HTML</h3>
<pre>
Some random &lt;font color="blue">blue&lt;/font> text Base&lt;sub>64&lt;/sub> for 1&lt;sup>st&lt;/sup>
game This is a simple &lt;b>bold&lt;/b>, &lt;i>italic&lt;/i>, &lt;u>underline&lt;/u>
&lt;ol>
    &lt;li>This is line 1&lt;/li>
    &lt;li>This is line 2
        &lt;ol type="a">
            &lt;li>This is &lt;font color="orange">orange&lt;/font> line 2.1&lt;/li>
            &lt;li>This is &lt;font color="green">green&lt;/font> line 2.2&lt;/li>
            &lt;li>This is &lt;font color="red">red&lt;/font> line 2.3 
            and it continues here&lt;/li>
        &lt;/ol>
    &lt;/li>
    &lt;li>This is line 3&lt;/li>
&lt;/ol>
Another line
</pre>

<h3>Sample Usage</h3>
<pre>
const string input = "This is [b]bold[/b], [i]italic[i] and [blue]blue[/blue] for now";
const string expected = "This is &lt;b>bold&lt;/b>, &lt;i>italic&lt;/i> and &lt;font color=\"blue\">blue&lt;/font> for now";
var actual = StmlParser.Parse(input).ToString();

Assert.AreEqual(expected, actual);
</pre>

