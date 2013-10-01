using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SearchulatorGrid.Pods
{
    public class Info
    {
        public ImageResult Image { get; private set; }
        public bool HasImage { get { return Image != null; } }

        public string Text { get; private set; }
        public List<InfoLink> Links { get; private set; }

        public Info(XElement info)
        {
            XAttribute textAttr = info.Attribute("text");

            if (textAttr != null)
            {
                Text = textAttr.Value;
            }
            else
            {
                Text = "";
            }
            XElement img = info.Element("img");
            if (img != null)
            {
                Image = new ImageResult(img);
            }

            Links = new List<InfoLink>();
            foreach (XElement link in info.Elements("link"))
            {
                Links.Add(new InfoLink(link));
            }
        }
    }


}
