using SearchulatorGrid.Common;
using SearchulatorGrid.Pods;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SearchulatorGrid
{
    public sealed class PodButton : Button
    {
        public Pod PodOwner
        {
            get
            {
                return (Pod)GetValue(PodOwnerProperty);
            }

            set
            {
                SetValue(PodOwnerProperty, value);
            }
        }

        public static readonly DependencyProperty PodOwnerProperty = DependencyProperty.Register("PodOwner", typeof(Pod), typeof(PodButton), new PropertyMetadata(null));
    }
}
