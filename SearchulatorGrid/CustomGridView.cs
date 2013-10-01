using SearchulatorGrid.Pods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
namespace SearchulatorGrid
{
    public class CustomGridView : GridView
    {
        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            try
            {
                dynamic _Item = item;
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.ColumnSpanProperty, _Item.ColSpan);
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.RowSpanProperty, _Item.RowSpan);
            }
            catch
            {
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.ColumnSpanProperty, 1);
                element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.RowSpanProperty, 1);
            }
            finally
            {
                base.PrepareContainerForItemOverride(element, item);
            }
        }
    }
}