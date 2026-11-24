using IGOR.Interface.Camera;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace IGOR.Games.Checkers.Views
{
    /// <summary>
    /// Interaktionslogik für CheckerCamView.xaml
    /// </summary>
    public partial class CheckersCamView : UserControl
    {
        public int Size { get; private set; }
        public int DifferenceRenderTransform { get; private set; }

        private readonly ICameraService _cameraService;
        public CheckersCamView(ICameraService cameraService)
        {
            _cameraService = cameraService;
            Loaded += DrawBoard;
            InitializeComponent();
        }

        private void DrawBoard(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Width = _cameraService.ScreenWidth;
            DrawingCanvas.Height = _cameraService.ScreenHeight;
            Size = (int)(_cameraService.ScreenHeight / 8);
            DifferenceRenderTransform = -Size / 2;
            var startposX = (_cameraService.ScreenWidth - Size * 8) / 2;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    bool isWhite = y % 2 == 0
                        ? x % 2 != 0
                        : x % 2 == 0;
                    var posX = startposX + Size * x;
                    var posY = Size * y;
                    DrawRectangle(isWhite, new Point(posX, posY), Size);
                }
            }

            Loaded -= DrawBoard;
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
            DrawingCanvas.Children.Add(rect);
        }
    }
}
