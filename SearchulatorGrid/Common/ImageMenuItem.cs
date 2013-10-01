using Callisto.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace SearchulatorGrid.Common
{
    public sealed class ImageMenuItem : MenuItem
    {
        public ImageMenuItem()
        {
            DefaultStyleKey = typeof(ImageMenuItem);
        }

        public string ImgSource
        {
            get
            {
                return (string) GetValue(ImgSourceProperty);
            }
            set
            {
                SetValue(ImgSourceProperty, value);
            }
        }

        public int ImgWidth
        {
            get
            {
                return (int) GetValue(ImgWidthProperty);
            }
            set
            {
                SetValue(ImgWidthProperty, value);
            }
        }

        public int ImgHeight
        {
            get
            {
                return (int) GetValue(ImgHeightProperty);
            }
            set
            {
                SetValue(ImgHeightProperty, value);
            }
        }

        public static readonly DependencyProperty ImgSourceProperty =
            DependencyProperty.Register("ImgSource", typeof(string), typeof(ImageMenuItem), null);
        public static readonly DependencyProperty ImgWidthProperty =
            DependencyProperty.Register("ImgWidth", typeof(int), typeof(ImageMenuItem), null);
        public static readonly DependencyProperty ImgHeightProperty =
            DependencyProperty.Register("ImgHeight", typeof(int), typeof(ImageMenuItem), null);
    }
}
