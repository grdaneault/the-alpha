using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SearchulatorGrid.Pods
{
    public class InfoLink
    {
        public string Url { get; private set; }
        public string Text { get; private set; }
        public string Title { get; private set; }
        public Uri Uri { get; private set; }

        public InfoLink(XElement link)
        {
            Url = link.Attribute("url").Value;
            Uri = new Uri(Url);
            Text = link.Attribute("text").Value;

            XAttribute titleAttr = link.Attribute("title");
            Title = titleAttr != null ? link.Attribute("title").Value : Text;
        }

    }
}
