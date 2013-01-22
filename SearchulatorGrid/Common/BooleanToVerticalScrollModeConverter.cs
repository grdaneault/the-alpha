using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace SearchulatorGrid.Common
{
    /// <summary>
    /// Value converter that translates true to <see cref="Visibility.Visible"/> and false to
    /// <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public sealed class BooleanToVerticalScrollModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool)value) ? ScrollMode.Auto : ScrollMode.Disabled;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is ScrollMode && (ScrollMode)value == ScrollMode.Auto;
        }
    }
}