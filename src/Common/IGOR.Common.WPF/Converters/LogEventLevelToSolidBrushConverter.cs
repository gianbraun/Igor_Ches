using Serilog.Events;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace IGOR.Common.WPF.Converters
{
    public class LogEventLevelToSolidBrushConverter : BaseValueConverter<LogEventLevelToSolidBrushConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not LogEventLevel logEventLevel)
                return null;

            switch (logEventLevel)
            {
                case LogEventLevel.Information:
                    return (SolidColorBrush)Application.Current.Resources["InformationBrush"];
                case LogEventLevel.Warning:
                    return (SolidColorBrush)Application.Current.Resources["WarningBrush"];
                case LogEventLevel.Error:
                    return (SolidColorBrush)Application.Current.Resources["ErrorBrush"];
                default:
                    return null;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(nameof(LogEventLevelToSolidBrushConverter));
        }
    }
}
