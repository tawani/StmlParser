namespace StmlParsing
{
    using System;

    public class StmlParser
    {
        public static StmlNode Parse(string text, bool removeBlockLineBreaks = false)
        {
            var str = text.Trim();

            //HACK: Simple fix for now
            if (removeBlockLineBreaks)
                str = RemoveLineBreaks(str);

            var root = new TextContainerElement(null);
            var start = -1;
            AddNodes(root, str, ref start);
            var result = root.ChildNodes.Count == 1 ? root.ChildNodes[0] : root;

            return result;
        }

        private static string RemoveLineBreaks(string text)
        {
            text = text.Replace("center]\r\n", "center]")
                       .Replace("li]\r\n", "li]")
                       .Replace("ol]\r\n", "ol]")
                       .Replace("ul]\r\n", "ul]")
                       .Replace("center]\n", "center]")
                       .Replace("li]\n", "li]")
                       .Replace("ol]\n", "ol]")
                       .Replace("ul]\n", "ul]")

                       .Replace("h1]\r\n", "h1]")
                       .Replace("h2]\r\n", "h2]")
                       .Replace("h3]\r\n", "h3]")
                       .Replace("h4]\r\n", "h4]")
                       .Replace("h1]\n", "h1]")
                       .Replace("h2]\n", "h2]")
                       .Replace("h3]\n", "h3]")
                       .Replace("h4]\n", "h4]")

                       .Replace("\r\n[h1]", "[h1]")
                       .Replace("\r\n[h2]", "[h2]")
                       .Replace("\r\n[h3]", "[h3]")
                       .Replace("\r\n[h4]", "[h4]")
                       .Replace("\n[h1]", "[h1]")
                       .Replace("\n[h2]", "[h2]")
                       .Replace("\n[h3]", "[h3]")
                       .Replace("\n[h4]", "[h4]")
                       ;
            return text;
        }


        private static void AddNodes(ContainerElement current, string stml, ref int startIndex)
        {
            var len = stml.Length;
            var i = startIndex;
            var text = "";
            while (i < len - 1)
            {
                i++;

                if (i >= stml.Length)
                    break;

                var s = stml.Substring(i, 1);

                if (s == "[")
                {
                    var start = i + 1;
                    var stop = stml.IndexOf("]", i, StringComparison.Ordinal);
                    if (stop < i)
                        continue;

                    //check if this is the end tag
                    var endNodeTag = stml.Substring(i, stop - i + 1).Replace(" ", "");
                    if (current.NodeName == "#")
                    {
                        if (endNodeTag == "[#]" || endNodeTag == "[/olist]" || endNodeTag == "[/ulist]")
                        {
                            current.Add(new TextElement(text.TrimEnd()));
                            var itemNode = CreateNode(current.Parent as ListElement, stml, start, ref stop);

                            startIndex = stop;
                            return;
                        }
                    }
                    else if (current.NodeName == "olist" || current.NodeName == "ulist")
                    {
                        if (endNodeTag == "[/olist]" || endNodeTag == "[/ulist]")
                        {
                            if (!string.IsNullOrWhiteSpace(text) && current.Parent is ContainerElement)
                                (current.Parent as ContainerElement).Add(new TextElement(text.TrimEnd()));

                            startIndex = stop;
                            return;
                        }
                        else if (endNodeTag == "[#]")
                        {
                            //if (!string.IsNullOrWhiteSpace(text) && current.Parent is ContainerElement)
                            //    (current.Parent as ContainerElement).Add(new TextElement(text.TrimEnd()));

                            var itemNode = CreateNode(current as ListElement, stml, start, ref stop);
                            startIndex = stop;
                            return;
                        }
                    }
                    else
                    {
                        if (endNodeTag == string.Format("[/{0}]", current.NodeName))
                        {
                            if (current is ListElement)
                            {
                                //add text to parent of list container 
                                if (!string.IsNullOrWhiteSpace(text) && current.Parent is ContainerElement)
                                    (current.Parent as ContainerElement).Add(new TextElement(text.TrimEnd()));
                            }
                            else
                                current.Add(new TextElement(text));
                            startIndex = stop;
                            return;
                        }
                    }

                    var node = CreateNode(current, stml, start, ref stop);
                    if (node != null)
                    {
                        //add the previous node first
                        if (current is ListElement)
                        {
                            //add text to parent of list container 
                            if (!string.IsNullOrWhiteSpace(text) && current.Parent is ContainerElement)
                                (current.Parent as ContainerElement).Add(new TextElement(text.Trim()));
                        }
                        else
                            current.Add(new TextElement(text));
                        text = "";

                        current.Add(node);
                        i = stop;

                        continue;
                    }
                }

                text += s;
                startIndex = i;
            }

            current.Add(new TextElement(text));
        }


        private static StmlNode CreateNode(ContainerElement parent, string stml, int start, ref int stop)
        {
            var fragment = stml.Substring(start, stop - start);
            var node = CreateNode(parent, fragment.Trim());
            if (!(node is ContainerElement))
                return node;
            var element = node as ContainerElement;

            AddNodes(element, stml, ref stop);

            return element;
        }


        private static StmlNode CreateNode(ContainerElement parent, string fragment)
        {
            //verify if this is a valid node
            if (fragment.Contains("[") || fragment.Contains("]"))
                return null;

            var ix = fragment.IndexOf("=", StringComparison.Ordinal);

            var args = "";
            var name = fragment;
            if (ix > 0)
            {
                name = fragment.Substring(0, ix).Trim();
                args = fragment.Substring(ix + 1).Trim();
            }
            return CreateElement(parent, name.ToLower(), args);
        }

        private static StmlNode CreateElement(ContainerElement parent, string name, string args)
        {
            switch (name)
            {
                case "b":
                case "i":
                case "u":
                case "s":
                case "sub":
                case "sup":
                case "em":
                case "strong":
                case "small":
                case "dir":
                case "center":
                case "big":
                case "blockquote":
                case "pre":
                case "code":
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                    return new TextContainerElement(name);
                case "red":
                case "green":
                case "blue":
                case "navy":
                case "fuchsia":
                case "orange":
                case "yellow":
                case "gray":
                case "purple":
                    return new FontElement(name, name);
                case "color":
                    return new FontElement(name, args);
                case "url":
                    return new LinkElement(name, args);
                case "email":
                    return new EmailElement(name, args);
                case "img":
                    return new ImageElement(name, args);
                case "ul":
                case "ulist":
                    return new ListElement(name, args);
                case "ol":
                case "olist":
                    return new ListElement(name, args);
                case "#":
                    if (parent is ListElement)
                        return parent.Add(new ListItemElement());
                    return null;
                //throw new Exception("ListItem should not be called here");
                case "li":
                    return new ListItemElement(name);
                case "check":
                    return new DingbatElement("&#x274F;");
                case "check-on":
                    return new DingbatElement("<big>&#9745;</big>");
                case "hr":
                    return new NoEndTagElement("hr");

            }
            return null;
        }

    }
}
