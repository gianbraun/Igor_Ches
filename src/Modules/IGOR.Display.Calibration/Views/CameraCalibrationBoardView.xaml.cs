using IGOR.Interface.Camera;
using IGOR.Service.Navigation.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IGOR.Display.Calibration.Views
{
    /// <summary>
    /// Interaktionslogik für CameraCalibrationBoardView.xaml
    /// </summary>
    public partial class CameraCalibrationBoardView : UserControl
    {
        private readonly ICameraService _cameraService;
        private readonly INavigationService _navigationService;
         
        public CameraCalibrationBoardView(ICameraService cameraService, INavigationService navigationService)
        {  
            _navigationService = navigationService;
            _cameraService = cameraService;
            InitializeComponent();
            Loaded += (s, e) => DrawCalibrationPattern(); 
        }

        public static readonly OpenCvSharp.Size PatternSize = new OpenCvSharp.Size(9, 7);

        private void DrawCalibrationPattern()
        { 

            var size = (int)(_cameraService.ScreenHeight / (PatternSize.Height+3));
            var startposX = (_cameraService.ScreenWidth - size * (PatternSize.Width+3)) / 2;

            for (int y = 1; y < PatternSize.Height+2; y++)
            {
                for (int x = 1; x < PatternSize.Width+2; x++)
                {
                    bool isWhite = y % 2 == 0
                        ? x % 2 != 0
                        : x % 2 == 0;
                    var posX = startposX + size * x;
                    var posY = size * y;
                    DrawRectangle(isWhite, new Point(posX, posY), size);
                }
            }
        }



        private void DrawRectangle(bool isWhite, Point pos, int size)
        {
            var rect = new Rectangle()
            {
                Width = size,
                Height = size,
                Fill = isWhite ? new SolidColorBrush(Color.FromRgb(220, 220, 220)) : new SolidColorBrush(Colors.Black),
            };

            Canvas.SetTop(rect, pos.Y);
            Canvas.SetLeft(rect, pos.X);
            Canvas.Children.Add(rect);
        }
    }
}
