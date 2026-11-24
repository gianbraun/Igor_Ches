using IGOR.Common.WPF.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace IGOR.Common.WPF.Converters
{
    public class BoolToWarningColorConverter : BaseValueConverter<BoolToWarningColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool v)
            {
                return v ? Application.Current.Resources["WarningBrush"] : new SolidColorBrush(Colors.Gray);
            }

            return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(nameof(BoolToWarningColorConverter));
        }
    }
}
