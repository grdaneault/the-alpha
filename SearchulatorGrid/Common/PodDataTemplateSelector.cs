using SearchulatorGrid.Pods;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SearchulatorGrid.Common
{
    public class PodDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultPodLayout { get; set; }
        public DataTemplate MultiImagePodLayout { get; set; }
        public DataTemplate AdPodLayout { get; set; }
        
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;

            if (element != null && item != null && item is Pod)
            {
                var p = item as Pod;

                if (p is AdPod)
                    return AdPodLayout;
                if (p.NumSubpods == 1)
                    return DefaultPodLayout;
                return MultiImagePodLayout;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}