using IGOR.Common.WPF;
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

namespace IGOR.Games.Kono.Views
{
    /// <summary>
    /// Interaktionslogik für KonoCamView.xaml
    /// </summary>
    public partial class KonoCamView : UserControl
    {
        public int Size { get; private set; }
        public int DifferenceRenderTransform { get; private set; }

        private readonly ICameraService _cameraService;
        public KonoCamView(ICameraService cameraService)
        {
            _cameraService = cameraService;
            Loaded += DrawBoard;
            InitializeComponent();
        }
        private void DrawBoard(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Width = _cameraService.ScreenWidth;
            DrawingCanvas.Height = _cameraService.ScreenHeight;

            Size = 0;
            double widthBorder = 100;
            double heightBorder = 100;
            if(DrawingCanvas.Height < DrawingCanvas.Width)
            {
                Size = (int)(DrawingCanvas.Height - 200) / 4;
                widthBorder = (DrawingCanvas.Width - Size * 4) / 2;
            }
            else
            {
                Size = (int)(DrawingCanvas.Width - 200) / 4;
                heightBorder = (DrawingCanvas.Height - Size * 4) / 2;
            }

            DifferenceRenderTransform = -Size / 4;

            DrawLine(widthBorder + 0 * Size, heightBorder + 3 * Size, widthBorder + 1 * Size, heightBorder + 4 * Size);
            DrawLine(widthBorder + 0 * Size, heightBorder + 2 * Size, widthBorder + 2 * Size, heightBorder + 4 * Size);
            DrawLine(widthBorder + 0 * Size, heightBorder + 1 * Size, widthBorder + 3 * Size, heightBorder + 4 * Size);
            DrawLine(widthBorder + 0 * Size, heightBorder + 0 * Size, widthBorder + 4 * Size, heightBorder + 4 * Size);
            DrawLine(widthBorder + 1 * Size, heightBorder + 0 * Size, widthBorder + 4 * Size, heightBorder + 3 * Size);
            DrawLine(widthBorder + 2 * Size, heightBorder + 0 * Size, widthBorder + 4 * Size, heightBorder + 2 * Size);
            DrawLine(widthBorder + 3 * Size, heightBorder + 0 * Size, widthBorder + 4 * Size, heightBorder + 1 * Size);

            DrawLine(widthBorder + 1 * Size, heightBorder + 0 * Size, widthBorder + 0 * Size, heightBorder + 1 * Size);
            DrawLine(widthBorder + 2 * Size, heightBorder + 0 * Size, widthBorder + 0 * Size, heightBorder + 2 * Size);
            DrawLine(widthBorder + 3 * Size, heightBorder + 0 * Size, widthBorder + 0 * Size, heightBorder + 3 * Size);
            DrawLine(widthBorder + 4 * Size, heightBorder + 0 * Size, widthBorder + 0 * Size, heightBorder + 4 * Size);
            DrawLine(widthBorder + 4 * Size, heightBorder + 1 * Size, widthBorder + 1 * Size, heightBorder + 4 * Size);
            DrawLine(widthBorder + 4 * Size, heightBorder + 2 * Size, widthBorder + 2 * Size, heightBorder + 4 * Size);
            DrawLine(widthBorder + 4 * Size, heightBorder + 3 * Size, widthBorder + 3 * Size, heightBorder + 4 * Size);

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    DrawCircle(widthBorder + x * Size, heightBorder + y * Size);
                }
            }
        }

        private void DrawLine(double x0, double y0, double x1, double y1)
        {
            var line = new Line()
            {
                Y1 = y0,
                X1 = x0,
                X2 = x1,
                Y2 = y1,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 5,
            };
            DrawingCanvas.Children.Add(line);
        }

        private void DrawCircle(double x, double y)
        {
            var circle = new Ellipse()
            {
                Width = 50,
                Height =50,
                Fill = new SolidColorBrush(Colors.White),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 5
            };

            Canvas.SetLeft(circle, x - 25);
            Canvas.SetBottom(circle, y - 25);
            DrawingCanvas.Children.Add(circle);
        }
    }
}
