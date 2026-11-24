using IGOR.Common.WPF.Converters;
using IGOR.Interface.Game;
using System;
using System.Globalization;
using System.Windows.Media;

namespace IGOR.Games.TicTacToe.Converter
{
    public class DifferencePlayerToColorConverter : BaseValueConverter<DifferencePlayerToColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                Player.None => new SolidColorBrush(Colors.Red),
                Player.Black => new SolidColorBrush(Colors.Black),
                Player.White => new SolidColorBrush(Colors.Wheat),
                _ => null,
            };
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
