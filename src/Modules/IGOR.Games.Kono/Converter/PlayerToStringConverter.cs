using IGOR.Common.WPF.Converters;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace IGOR.Games.Kono.Converter
{
    public class PlayerToStringConverter : BaseValueConverter<PlayerToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                Player.None => "None",
                Player.Black => "Schwarz",
                Player.White => "Weiß",
                _ => null,
            };
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } 
}
