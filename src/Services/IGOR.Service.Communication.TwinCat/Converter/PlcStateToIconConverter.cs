using IGOR.Common;
using IGOR.Common.WPF.Converters;
using System;
using TwinCAT.Ads;
using System.Globalization;
using IGOR.Interface.Communication;
using IGOR.Common.WPF.Icons;
using System.Windows;
using System.Windows.Media.Imaging;

namespace IGOR.Service.Communication.TwinCat.Converter
{
    public class PlcStateToIconConverter : BaseValueConverter<PlcStateToIconConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (PlcState)value;
            string key;
            switch (state)
            {
                case PlcState.Invalid:
                    key = IconKey.StatusInvalid;
                    break;
                case PlcState.Init:
                    key = IconKey.StatusConfig;
                    break;
                case PlcState.Config:
                    key = IconKey.StatusConfig;
                    break;
                case PlcState.Start:
                    key = IconKey.StatusRun;
                    break;
                case PlcState.Run:
                    key = IconKey.StatusRun;
                    break;
                case PlcState.Stop:
                    key = IconKey.StatusStop;
                    break;
                case PlcState.Error:
                    key = IconKey.StatusInvalid;
                    break;
                default:
                    key = null;
                    break;
            }

            if (key == null) return null;
            var image = (BitmapImage)Application.Current.Resources[key];
            return image;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
