using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace IGOR.Common.WPF.Converters
{
    public class RessourceKeyToIconConverter : BaseValueConverter<RessourceKeyToIconConverter>
    {

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string ressourceKey) return null;

            return (BitmapImage)Application.Current.Resources[ressourceKey];
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(nameof(RessourceKeyToIconConverter));
        }
    }
}
