using IGOR.Common.WPF.Converters;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace IGOR.Common.WPF
{
    public class BoolToColorConverter : IValueConverter
    {
        private readonly Brush _defaultColor;
        private readonly Brush _valueColor;
        public BoolToColorConverter(Brush defaultColor, Brush valueColor)
        {
            _defaultColor = defaultColor;
            _valueColor = valueColor;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? _valueColor : _defaultColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(nameof(BoolToColorConverter));
        }
    }
}