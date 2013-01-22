using System;
using System.Xml.Linq;
using Windows.UI.Xaml.Media;

namespace SearchulatorGrid.Pods
{
    public class ImageResult
    {
        private static int _maxSnappedWidth = 282;

        public string URL { get; set; }
        public string Alt { get; set; }

        public int Height { get; set; }
        public int Width { get; set; }

        public Pod Owner { get; set; }

        public int ImageAreaHeight
        {
            get { return Owner.ImageAreaHeight; }
        }

        public int ImageAreaWidth
        {
            get { return Owner.ImageAreaWidth; }
        }

        public ImageSource Source { get; set; }

        public ImageResult(string url, int width, int height)
        {
            URL = url;
            Width = width;
            Height = height;
            Alt = "";
        }

        public ImageResult(XElement subpod)
        {
            XElement img = subpod.Element("img");
            URL = img.Attribute("src").Value;
            Alt = img.Attribute("alt").Value;

            Height = int.Parse(img.Attribute("height").Value);
            Width = int.Parse(img.Attribute("width").Value);
        }

        public int SnappedHeight { get {return (int)(Height * Math.Min(1.0, ((double)_maxSnappedWidth) / Width)); } }
        
        public int SnappedWidth { get { return Math.Min(_maxSnappedWidth, Width); } }
    }
}