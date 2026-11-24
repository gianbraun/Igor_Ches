using IGOR.Interface.Camera;
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

namespace IGOR.Games.TicTacToe.Views
{
    /// <summary>
    /// Interaktionslogik für TicTacToeCamView.xaml
    /// </summary>
    public partial class TicTacToeCamView : UserControl
    {
        public int Size { get; private set; } 
        public int DifferenceRenderTransform { get; private set; }
        private readonly ICameraService _cameraService;
        public TicTacToeCamView(ICameraService cameraService)
        {
            _cameraService = cameraService;
            Loaded += DrawBoard;

            InitializeComponent();
        }

        private void DrawBoard(object sender, RoutedEventArgs e)
        {

            DrawingCanvas.Width = _cameraService.ScreenWidth;
            DrawingCanvas.Height = _cameraService.ScreenHeight;
            Size = (int)DrawingCanvas.Height / 8;
            DifferenceRenderTransform = -Size / 2;
            var centerX = DrawingCanvas.Width / 2;
            var centerY = DrawingCanvas.Height / 2;
            
            DrawingCanvas.RenderTransform = new ScaleTransform(2, 2, centerX, centerY);
            
            DrawLine(centerX - Size / 2, centerY + Size * 1.5, centerX - Size /2, centerY - Size * 1.5);
            DrawLine(centerX + Size / 2, centerY + Size / 2 + Size, centerX + Size / 2, centerY - Size * 1.5);

            DrawLine(centerX - Size * 1.5, centerY + Size / 2, centerX + Size * 1.5, centerY + Size / 2);
            DrawLine(centerX - Size * 1.5, centerY - Size / 2, centerX + Size * 1.5, centerY - Size / 2);

            Loaded -= DrawBoard;
        }

        private void DrawLine(double startX, double startY, double endX, double endY)
        {
            var line = new Line()
            {
                SnapsToDevicePixels = true,
                X1 = startX,
                Y1 = startY,
                X2 = endX,
                Y2 = endY,
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 5,
            };

            DrawingCanvas.Children.Add(line);
        }
    }
}
