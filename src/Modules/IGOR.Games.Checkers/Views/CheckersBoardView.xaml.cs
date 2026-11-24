using IGOR.Common.WPF.Icons;
using IGOR.Interface.Camera;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IGOR.Games.Checkers.Views
{
    /// <summary>
    /// Interaktionslogik für CheckersBoardView.xaml
    /// </summary>
    public partial class CheckersBoardView : UserControl
    {
        private bool first = true;
        public static int FieldSize;
        public static Dictionary<FieldPosition, (double x, double y)> ValidPositions = new();
        public static ObservableCollection<Stone> Stones = new ObservableCollection<Stone>();
        public static ObservableCollection<FieldPosition> Kings = new ObservableCollection<FieldPosition>();
        private Dictionary<FieldPosition, Image> _kingPositions = new Dictionary<FieldPosition, Image>();
        public static event EventHandler BoardReady;
        private readonly ICameraService _cameraService;
        public static ObservableCollection<Difference> Differences = new ObservableCollection<Difference>();

        public CheckersBoardView(ICameraService cameraService)
        {
            _cameraService = cameraService;
            Kings.CollectionChanged += Kings_CollectionChanged;
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

        private void Kings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (FieldPosition pos in e.NewItems)
                {
                    DrawKing(pos);
                }
            }

            if (e.OldItems is not null)
            {
                foreach (FieldPosition position in e.OldItems)
                {
                    var image = _kingPositions[position];
                    KingCanvas.Children.Remove(image);
                }
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                _kingPositions.Clear();
                KingCanvas.Children.Clear();
            }
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
            FieldSize = (int)(_cameraService.ScreenHeight / 8);

            var startposX = (_cameraService.ScreenWidth - _cameraService.ScreenHeight) / 2;

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    bool isWhite = y % 2 == 0
                        ? x % 2 != 0
                        : x % 2 == 0;
                    var posX = startposX + FieldSize * x;
                    var posY = FieldSize * y;
                    DrawRectangle(isWhite, new Point(posX, posY), FieldSize);
                    ValidPositions.Add(new FieldPosition(x, y), (posX + FieldSize / 2, posY + FieldSize / 2));
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
            DrawingCanvas.Children.Add(rect);
        }

        private void DrawKing(FieldPosition position)
        {
            var size = _cameraService.ScreenHeight / 8 - 20;
            var bitMpaimage = (BitmapImage)Application.Current.Resources[IconKey.KingIcon];
            var image = new Image()
            {
                Height = size,
            };

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.Fant);
            image.Source = bitMpaimage;
            var (x, y) = ValidPositions[position];

            Canvas.SetTop(image, y - size / 2);
            Canvas.SetLeft(image, x - size / 2);
            KingCanvas.Children.Add(image);
            _kingPositions.Add(position, image);

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
    }
}
