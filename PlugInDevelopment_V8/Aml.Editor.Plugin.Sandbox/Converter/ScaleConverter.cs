using Aml.Toolkit.XamlClasses;
using AvalonDock.Layout;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Aml.Editor.Plugin.Sandbox.Converter
{
    internal class ScaleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var zoomFactor = (double)values[0];
            var layoutElement = VisualTreeUtilities.FindParentWithType<ILayoutControl> ((DependencyObject)values[1]);

            if ( layoutElement?.Model?.FindParent<LayoutFloatingWindow>() == null)
            {
                return 1.0;
            }
            return zoomFactor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
