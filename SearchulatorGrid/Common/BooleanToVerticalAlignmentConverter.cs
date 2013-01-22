using System;
using Windows.UI.Xaml.Data;

namespace SearchulatorGrid.Common
{
    class BooleanToVerticalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var squashed = (bool)value;

            if (squashed)
            {
                return Windows.UI.Xaml.VerticalAlignment.Top;
            }

            return Windows.UI.Xaml.VerticalAlignment.Center;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var state = (Windows.UI.Xaml.VerticalAlignment)value;

            if (state == Windows.UI.Xaml.VerticalAlignment.Center)
            {
                return false;
            }
            
            return true; 
        }
    }
}
