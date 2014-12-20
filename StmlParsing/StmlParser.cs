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
            AddNodes(root, str);
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

        private static void AddNodes(ContainerElement parent, string stml)
        {
            var len = stml.Length;
            var i = -1;
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

                    var node = CreateNode(stml, start, ref stop);
                    if (node != null)
                    {
                        //add the previous node first
                        parent.Add(new TextElement(text));
                        text = "";

                        parent.Add(node);
                        i = stop;

                        continue;
                    }                 
                }

                text += s;
            }

            parent.Add(new TextElement(text));
        }

        //private static void AddListNodes(ListElement parent, string stml)
        //{
        //    if (stml.Contains("[olist") || stml.Contains("[ulist"))
        //        AddListNodes2(parent, stml);

        //    var arr = stml.Trim().Split(new[] { "[#]" }, StringSplitOptions.RemoveEmptyEntries);
        //    foreach (var str in arr)
        //    {
        //        var element = new ListItemElement();
        //        AddNodes(element, str.Trim());
        //        parent.Add(element);
        //    }
        //}

        private static void AddListNodes(ListElement parent, string stml)
        {
            var str = stml.Trim();
            var len = str.Length;
            if (len == 0)
                return;

            var ist = str.IndexOf("[#]", StringComparison.InvariantCultureIgnoreCase);
            while (ist > -1 && ist < len)
            {
                var ien = str.IndexOf("[#]", ist + 1, StringComparison.InvariantCultureIgnoreCase);
                //if (ien == -1)
                //    ien = str.IndexOf("[/" + parent.NodeName + "]", ist + 1, StringComparison.InvariantCultureIgnoreCase);
                if (ien == -1)
                    ien = len;

                var tmp = str.Substring(ist, ien - ist);

                var count1 = CountSameInnerTags(tmp, "olist");
                if (count1 > 0)
                {
                    ien = GetStopIndex(str, "olist", count1, ien);
                    var ien2 = str.IndexOf("[#]", ien, StringComparison.InvariantCultureIgnoreCase);
                    if (ien2 > 0)
                        ien = ien2;
                    else if (ien < len)
                    {
                        ien2 = str.IndexOf("[/olist]", ien + 1, StringComparison.InvariantCultureIgnoreCase);
                        if (ien2 < 0)
                            ien = len;
                    }
                    if (ien == len)
                        tmp = stml.Substring(ist);
                    else
                        tmp = stml.Substring(ist, ien - ist);
                }

                var count2 = CountSameInnerTags(tmp, "ulist");
                if (count2 > 0)
                {
                    ien = GetStopIndex(str, "ulist", count2, ien);
                    var ien2 = str.IndexOf("[#]", ien, StringComparison.InvariantCultureIgnoreCase);
                    if (ien2 > 0)
                        ien = ien2;
                    else if (ien < len)
                    {
                        ien2 = str.IndexOf("[/ulist]", ien+1, StringComparison.InvariantCultureIgnoreCase);
                        if (ien2 < 0)
                            ien = len;
                    }
                    if (ien == len)
                        tmp = stml.Substring(ist);
                    else
                        tmp = stml.Substring(ist, ien - ist);
                }

                var element = new ListItemElement();
                AddNodes(element, tmp.Trim().Substring(3));
                parent.Add(element);

                ist =  ien;
            }
        }

        private static StmlNode CreateNode(string stml, int start, ref int stop)
        {
            var fragment = stml.Substring(start, stop - start);
            var node = CreateNode(fragment.Trim());
            if (!(node is ContainerElement))
                return node;
            var element = node as ContainerElement;
            
            
            var endTag =   string.Format("[/{0}]", element.NodeName);

            var ist = stop + 1;
            var ien = stml.IndexOf(endTag, ist, StringComparison.InvariantCultureIgnoreCase);

            if (ien < 0)
                ien = stml.Length;

            var tmp = stml.Substring(ist, ien - ist);

            //find the actual closing node
            var count = CountSameInnerTags(tmp, element.NodeName);
            if (count > 0)
            {
                ien = GetStopIndex(stml, element.NodeName,count,ien);
                tmp = stml.Substring(ist, ien - ist);
            }
            if(element is ListElement && element.NodeName.EndsWith("list"))
                AddListNodes(element as ListElement, tmp);
            else 
                AddNodes(element, tmp);

            stop = ien + endTag.Length - 1;
            return element;
        }

        private static int CountSameInnerTags(string fragment, string nodeName)
        {
            var startTag1 = string.Format("[{0}]", nodeName);
            var startTag2 = string.Format("[{0}=", nodeName);
            var startTag3 = string.Format("[{0} ", nodeName);

            if (!(fragment.Contains(startTag1) || fragment.Contains(startTag2) || fragment.Contains(startTag3)))
                return 0;

            var count = CountOccurences(startTag1, fragment);
            count += CountOccurences(startTag2, fragment);
            count += CountOccurences(startTag3, fragment);
            return count;
        }

        static int CountOccurences(string needle, string haystack)
        {
            return (haystack.Length - haystack.Replace(needle, "").Length) / needle.Length;
        }

        private static int GetStopIndex(string stml, string nodeName, int skip, int startIndex)
        {
            var endTag = string.Format("[/{0}]", nodeName);
            
            var len = stml.Length;
            var stopIndex = stml.IndexOf(endTag, startIndex,StringComparison.InvariantCultureIgnoreCase);
            if (stopIndex == -1)
                stopIndex = len;
            while (stopIndex < len && skip > 0)
            {

                var ist = stopIndex + endTag.Length;
                var nextStopIndex = stml.IndexOf(endTag, ist, StringComparison.InvariantCultureIgnoreCase);
                if (nextStopIndex < 0)
                    break;
                stopIndex = nextStopIndex;

                skip--;
            }
            return stopIndex;
        }

        private static StmlNode CreateNode(string fragment)
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
            return  CreateElement(name.ToLower(),args);
        }

        private static StmlNode CreateElement(string name, string args)
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
                    return null;
                    //throw new Exception("ListItem should not be called here");
                case "li":
                    return new ListItemElement(name);

            }
            return null;
        }

    }
}
