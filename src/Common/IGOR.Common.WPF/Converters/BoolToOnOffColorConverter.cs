using IGOR.Common.WPF.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace IGOR.Common.WPF.Converters
{
    public class BoolToOnOffColorConverter : BaseValueConverter<BoolToOnOffColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = parameter as SolidColorBrush;
            if((bool)value == true)
            {
                return brush;
            }

            return new SolidColorBrush(brush.Color.ChangeColorBrightness(-0.8f));

        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(nameof(BoolToOnOffColorConverter));
        }
    }
}
