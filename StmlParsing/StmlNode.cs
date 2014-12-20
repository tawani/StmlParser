namespace StmlParsing
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public abstract class StmlNode
    {
        public StmlNode Parent { get; set; }
        public StmlNode PrevNode { get; set; }
        public StmlNode NextNode { get; set; }
    }

    public class TextElement : StmlNode
    {
        private readonly string _text;

        public TextElement(string text)
        {
            _text = text ?? string.Empty;
        }

        public override string ToString()
        {
            return _text;
        }
    }

    public interface IEmptyElement
    {
        
    }

    public abstract class ContainerElement : StmlNode
    {
        public string NodeName { get; private set; }
        public string TagName { get; private set; }

        public List<StmlNode> ChildNodes { get; set; }

        protected ContainerElement(string nodeName, string tagName)
        {
            NodeName = nodeName;
            TagName = tagName;
            ChildNodes = new List<StmlNode>();
        }

        public virtual StmlNode Add(StmlNode node)
        {
            node.Parent = this;
            if (this.ChildNodes.Count > 0)
            {
                node.PrevNode = this.ChildNodes[this.ChildNodes.Count - 1];
                this.ChildNodes[this.ChildNodes.Count - 1].NextNode = node;
            }

            this.ChildNodes.Add(node);
            return node;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(TagName))
                sb.AppendFormat("<{0}", TagName);

            var attributes = GetAttributes();
            if (attributes != null)
                foreach (var attribute in attributes)
                {
                    sb.AppendFormat(" {0}=\"{1}\"", attribute.Key, attribute.Value);
                }

            if (!string.IsNullOrEmpty(TagName))
            {
                if (ChildNodes.Count == 0 || this is IEmptyElement)
                {
                    sb.Append("/>");
                    return sb.ToString();
                }
                sb.Append(">");
            }
                

            foreach (var child in ChildNodes)
            {
                sb.Append(child);
            }
            if (!string.IsNullOrEmpty(TagName))
                sb.AppendFormat("</{0}>", TagName);
            return sb.ToString();
        }

        public virtual Dictionary<string, string> GetAttributes()
        {
            return new Dictionary<string, string>();
        }
    }

    public class TextContainerElement : ContainerElement
    {
        public TextContainerElement(string tagName)
            : this(tagName, tagName)
        {
        }

        protected TextContainerElement(string nodeName, string tagName)
            : base(nodeName, tagName)
        {
        }

        public void Add(string text)
        {
            base.Add(new TextElement(text));
        }

        protected string GetInnerText()
        {
            var sb = new StringBuilder();
            foreach (var node in ChildNodes)
            {
                if (node is TextElement)
                    sb.Append(node);
                else if (node is TextContainerElement)
                    sb.Append((node as TextContainerElement).GetInnerText());
            }

            return sb.ToString();
        }

    }

    public class FontElement : TextContainerElement
    {
        public string Color { get; set; }
        public FontElement(string nodeName, string color)
            : base(nodeName, "font")
        {
            Color = color;
        }

        public override Dictionary<string, string> GetAttributes()
        {
            var result = base.GetAttributes();
            result["color"] = Color;
            return result;
        }
    }

    public class LinkElement : TextContainerElement
    {
        public string Url { get; set; }
        public LinkElement(string nodeName, string url)
            : base(nodeName, "a")
        {
            Url = url;
        }

        public override Dictionary<string, string> GetAttributes()
        {
            if (string.IsNullOrWhiteSpace(Url))
                Url = this.GetInnerText();

            var result = base.GetAttributes();
            result["href"] = Url;
            result["target"] = "_blank";
            return result;
        }
    }

    public class EmailElement : TextContainerElement
    {
        public string Email { get; set; }
        public EmailElement(string nodeName, string email)
            : base(nodeName, "a")
        {
            Email = email;
        }

        public override Dictionary<string, string> GetAttributes()
        {
            if (string.IsNullOrWhiteSpace(Email))
                Email = this.GetInnerText();

            var result = base.GetAttributes();
            result["href"] = "mailto:"+Email;
            return result;
        }
    }

    public class ImageElement : TextContainerElement, IEmptyElement
    {
        public string Url { get; set; }
        public ImageElement(string nodeName, string url)
            : base(nodeName, "img")
        {
            Url = url;
        }

        public override Dictionary<string, string> GetAttributes()
        {
            var alt = this.GetInnerText();
            if (string.IsNullOrWhiteSpace(Url))
            {
                Url = alt;
                alt = null;
            }

            var result = base.GetAttributes();
            result["src"] = Url;

            if (alt != null)
                result["alt"] = alt;

            return result;
        }
    }

    public class ListElement : ContainerElement
    {
        private readonly string _type;

        public ListElement(string tagName, string type)
            : base(tagName, tagName.Substring(0,2))
        {
            _type = type;
        }

        public override StmlNode Add(StmlNode node)
        {
            if (!(node is ListItemElement))
            {
                if (node is TextElement && node.ToString().Trim() == string.Empty)
                    return null;
                throw new Exception(string.Format("You cannot add a not of type '{0}' to a list container", node.GetType().Name));
            }
            return base.Add(node);
        }

        public override Dictionary<string, string> GetAttributes()
        {
            var result = base.GetAttributes();
            if (!string.IsNullOrWhiteSpace(_type))
            {
                result["type"] = _type;
            }
            return result;
        }
    }

    public class ListItemElement : ContainerElement
    {
        public ListItemElement()
            : base("#","li")
        {
        }
        public ListItemElement(string tagName)
            : base(tagName, tagName)
        {
        }
    }
}