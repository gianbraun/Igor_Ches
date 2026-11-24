using IGOR.Interface.Camera;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IGOR.Games.Morris.Views
{
    /// <summary>
    /// Interaktionslogik für MorrisBoardView.xaml
    /// </summary>
    public partial class MorrisBoardView : UserControl
    {
        private bool first = true;
        public static Dictionary<FieldPosition, (double x, double y)> ValidPositions = new();
        public static event EventHandler BoardReady;
        private readonly ICameraService _cameraService;
        public static ObservableCollection<Difference> Differences = new ObservableCollection<Difference>();
        public static int FieldSize;

        public MorrisBoardView(ICameraService cameraService)
        {
            InitializeComponent();
            Differences.CollectionChanged += Differences_CollectionChanged;
            _cameraService = cameraService;
            Loaded += (s, e) =>
            {
                if (first)
                {
                    first = false;
                    DrawBoard();
                }
                BoardReady?.Invoke(this, null);
            };
        }

        private void Differences_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (Difference newDifference in e.NewItems)
                {
                    DrawDifference(newDifference);
                }
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                DifferencesCanvas.Children.Clear();
            }
        }
        private void DrawBoard()
        {
            DrawingCanvas.Children.Clear();
            FieldSize = (int)(_cameraService.ScreenHeight - 200);
            var startpointX = (_cameraService.ScreenWidth - FieldSize) / 2;
            var sizeShrinking = 250;
            for (int i = 0; i < 3; i++)
            {
                var size = FieldSize - i * sizeShrinking;
                var offset = i * sizeShrinking / 2;
                var pointX = startpointX + offset;
                var pointY = 200 / 2 + offset;
                DrawRectangle(size, pointX, pointY, i);
            }

            DrawLine(startpointX, FieldSize / 2 + 100, startpointX + sizeShrinking, FieldSize / 2 + 100);
            DrawLine(startpointX + FieldSize, FieldSize / 2 + 100, startpointX + FieldSize - sizeShrinking, FieldSize / 2 + 100);

            DrawLine(startpointX + FieldSize / 2, 100, startpointX + FieldSize / 2, 100 + sizeShrinking);
            DrawLine(startpointX + FieldSize / 2, 100 + FieldSize, startpointX + FieldSize / 2, 100 + FieldSize - sizeShrinking);


        }
        private void DrawDifference(Difference newDifference)
        {

            var field = new Border()
            {
                Width = 150,
                Height = 150,
                BorderThickness = new Thickness(5),
                BorderBrush = Brushes.Red,
                RenderTransform = new TranslateTransform(-150 / 2, -150 / 2),
            };

            var stone = new Ellipse()
            {
                Width = 100,
                Height = 100,
                StrokeThickness = 5,
                Stroke = Brushes.Red,
                Fill = newDifference.Player == Player.None ? Brushes.Red : newDifference.Player == Player.Black ? Brushes.Black : Brushes.Wheat,
                Opacity = 0.7,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            field.Child = stone;
            Canvas.SetLeft(field, newDifference.FieldX);
            Canvas.SetTop(field, newDifference.FieldY);

            DifferencesCanvas.Children.Add(field);
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
                    switch (count)
                    {
                        case 0:
                            ValidPositions.Add(new FieldPosition(x * 3, y * 3), (circlePosX, circlePosY));
                            break;
                        case 1:
                            ValidPositions.Add(new FieldPosition(2 * x + 1, 2 * y + 1), (circlePosX, circlePosY));
                            break;
                        case 2:
                            ValidPositions.Add(new FieldPosition(x + 2, y + 2), (circlePosX, circlePosY));
                            break;
                        default:
                            break;
                    }
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
