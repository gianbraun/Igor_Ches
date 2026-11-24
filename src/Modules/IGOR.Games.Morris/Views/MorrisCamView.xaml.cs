using IGOR.Interface.Camera;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IGOR.Games.Morris.Views
{
    /// <summary>
    /// Interaktionslogik für MorrisCamView.xaml
    /// </summary>
    public partial class MorrisCamView : UserControl
    {
        public int Size { get; private set; } = 150;
        public int DifferenceRenderTransform { get; private set; } = -55;

        private readonly ICameraService _cameraService;
        public MorrisCamView(ICameraService cameraService)
        {
            _cameraService = cameraService;
            Loaded += DrawBoard;
            InitializeComponent();
        }

        private void DrawBoard(object sender, RoutedEventArgs e)
        {

            DrawingCanvas.Width = _cameraService.ScreenWidth;
            DrawingCanvas.Height = _cameraService.ScreenHeight;

            var initialSize = _cameraService.ScreenHeight - 200;
            var startpointX = (_cameraService.ScreenWidth - initialSize) / 2;
            var sizeShrinking = 250;
            for (int i = 0; i < 3; i++)
            {
                var size = initialSize - i * sizeShrinking;
                var offset = i * sizeShrinking / 2;
                var pointX = startpointX + offset;
                var pointY = 200 / 2 + offset;
                DrawRectangle(size, pointX, pointY, i);
            }

            DrawLine(startpointX, initialSize / 2 + 100, startpointX + sizeShrinking, initialSize / 2 + 100);
            DrawLine(startpointX + initialSize, initialSize / 2 + 100, startpointX + initialSize - sizeShrinking, initialSize / 2 + 100);

            DrawLine(startpointX + initialSize / 2, 100, startpointX + initialSize / 2, 100 + sizeShrinking);
            DrawLine(startpointX + initialSize / 2, 100 + initialSize, startpointX + initialSize / 2, 100 + initialSize - sizeShrinking);

            Loaded -= DrawBoard;
        }

        private void DrawLine(double startX, double startY, double endX, double endy)
        {
            var line = new Line()
            {
                X1 = startX,
                Y1 = startY,
                X2 = endX,
                Y2 = endy,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 5,
            };

            DrawingCanvas.Children.Add(line);
        }

        private void DrawRectangle(double size, double posX, double posY, int count)
        {
            var rect = new Rectangle()
            {
                Width = size,
                Height = size,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 5,
            };

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (y == 1 && x == 1)
                        continue;
                    var circlePosX = posX + x * size / 2;
                    var circlePosY = posY + y * size / 2;
                    DrawCircle(circlePosX, circlePosY);
                }
            }

            Canvas.SetLeft(rect, posX);
            Canvas.SetTop(rect, posY);

            DrawingCanvas.Children.Add(rect);
        }

        private void DrawCircle(double centerX, double centerY)
        {
            var ellipse = new Ellipse()
            {
                Width = 50,
                Height = 50,
                Fill = new SolidColorBrush(Colors.Black)
            };

            Canvas.SetLeft(ellipse, centerX - 25);
            Canvas.SetTop(ellipse, centerY - 25);
            DrawingCanvas.Children.Add(ellipse);
        }
    }
}
