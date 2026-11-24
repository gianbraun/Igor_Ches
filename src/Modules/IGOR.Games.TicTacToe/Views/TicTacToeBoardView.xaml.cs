using IGOR.Games.TicTacToe.Converter;
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

namespace IGOR.Games.TicTacToe.Views
{
    /// <summary>
    /// Interaktionslogik für TicTacToeBoard.xaml
    /// </summary>
    public partial class TicTacToeBoardView : UserControl
    {
        private bool first = true;
        public static Dictionary<FieldPosition, (double x, double y)> ValidPositions = new();
        public static event EventHandler BoardReady;
        private readonly ICameraService _cameraService;
        public static int FieldSize; 

        public static ObservableCollection<Difference> Differences = new ObservableCollection<Difference>();

        public TicTacToeBoardView(ICameraService cameraService)
        {
            _cameraService = cameraService;
            Differences.CollectionChanged += Differences_CollectionChanged;
            Loaded += (s, e) =>
            {
                if (first)
                {
                    DrawBoard();
                    first = false;
                }
                BoardReady?.Invoke(this, null);
            };
            InitializeComponent();
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

        private void DrawDifference(Difference newDifference)
        {
            var field = new Border()
            {
                Width = FieldSize,
                Height = FieldSize,
                BorderThickness = new Thickness(5),
                BorderBrush = Brushes.Red,
                RenderTransform = new TranslateTransform(-FieldSize / 2, -FieldSize / 2),
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
                VerticalAlignment = VerticalAlignment.Center,
            };

            field.Child = stone;
            Canvas.SetLeft(field, newDifference.FieldX);
            Canvas.SetTop(field, newDifference.FieldY); 

            DifferencesCanvas.Children.Add(field); 
        }

        private void DrawBoard()
        {
            FieldSize = (int)_cameraService.ScreenHeight / 8; 
            var centerX = _cameraService.ScreenWidth / 2;
            var centerY = _cameraService.ScreenHeight / 2;

            ValidPositions.Add(new FieldPosition(0, 0), (centerX - FieldSize, centerY - FieldSize));
            ValidPositions.Add(new FieldPosition(1, 0), (centerX, centerY - FieldSize));
            ValidPositions.Add(new FieldPosition(2, 0), (centerX + FieldSize, centerY - FieldSize));

            ValidPositions.Add(new FieldPosition(0, 1), (centerX - FieldSize, centerY));
            ValidPositions.Add(new FieldPosition(1, 1), (centerX, centerY));
            ValidPositions.Add(new FieldPosition(2, 1), (centerX + FieldSize, centerY));

            ValidPositions.Add(new FieldPosition(0, 2), (centerX - FieldSize, centerY + FieldSize));
            ValidPositions.Add(new FieldPosition(1, 2), (centerX, centerY + FieldSize));
            ValidPositions.Add(new FieldPosition(2, 2), (centerX + FieldSize, centerY + FieldSize));

            DrawLine(centerX - FieldSize / 2, centerY + FieldSize * 1.5, centerX - FieldSize / 2, centerY - FieldSize * 1.5);
            DrawLine(centerX + FieldSize / 2, centerY + FieldSize / 2 + FieldSize, centerX + FieldSize / 2, centerY - FieldSize * 1.5);

            DrawLine(centerX - FieldSize * 1.5, centerY + FieldSize / 2, centerX + FieldSize * 1.5, centerY + FieldSize / 2);
            DrawLine(centerX - FieldSize * 1.5, centerY - FieldSize / 2, centerX + FieldSize * 1.5, centerY - FieldSize / 2);


            BoardReady?.Invoke(this, null);
        }

        private void DrawLine(double x1, double y1, double x2, double y2)
        {
            var line = new Line()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 3
            };

            DrawingCanvas.Children.Add(line);
        }
    }
}
