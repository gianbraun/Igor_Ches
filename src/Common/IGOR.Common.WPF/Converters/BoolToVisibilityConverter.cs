using IGOR.Common.WPF.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IGOR.Common.WPF.Converters
{
    public class BoolToVisibilityConverter : BaseValueConverter<BoolToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null ? (bool)value ? Visibility.Visible : Visibility.Collapsed : Visibility.Visible;

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(nameof(BoolToVisibilityConverter));
        }
    }
}
