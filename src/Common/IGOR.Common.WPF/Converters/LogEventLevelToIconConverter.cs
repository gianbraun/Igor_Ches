using Serilog.Events;
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
    public class LogEventLevelToIconConverter : BaseValueConverter<LogEventLevelToIconConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not LogEventLevel logEventLevel)
                return null;

            switch (logEventLevel)
            {
                case LogEventLevel.Information:
                    return (DrawingImage)Application.Current.Resources["Information"];
                case LogEventLevel.Warning:
                    return (DrawingImage)Application.Current.Resources["Warning"];
                case LogEventLevel.Error:
                    return (DrawingImage)Application.Current.Resources["Error"];
                default:
                    return null;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(nameof(LogEventLevelToIconConverter));
        }
    }
}
