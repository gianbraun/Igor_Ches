using IGOR.Interface.Camera;
using IGOR.Interface.Game;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// Interaktionslogik für KonoBoardView.xaml
    /// </summary>
    public partial class KonoBoardView : UserControl
    {
        private bool first = true;
        public static int FieldSize;
        public static Dictionary<FieldPosition, (double x, double y)> ValidPositions = new();
        public static event EventHandler BoardReady;
        private readonly ICameraService _cameraService;

        public static ObservableCollection<Difference> Differences = new ObservableCollection<Difference>();
        public KonoBoardView(ICameraService cameraService)
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
        private void DrawBoard()
        {
            DrawingCanvas.Width = _cameraService.ScreenWidth;
            DrawingCanvas.Height = _cameraService.ScreenHeight;

            double space = 0;
            double widthBorder = 100;
            double heightBorder = 100;
            if (DrawingCanvas.Height < DrawingCanvas.Width)
            {
                space = (DrawingCanvas.Height - 200) / 4;
                widthBorder = (DrawingCanvas.Width - space * 4) / 2;
            }
            else
            {
                space = (DrawingCanvas.Width - 200) / 4;
                heightBorder = (DrawingCanvas.Height - space * 4) / 2;
            }
            FieldSize = (int)space;

            DrawLine(widthBorder + 0 * space, heightBorder + 3 * space, widthBorder + 1 * space, heightBorder + 4 * space);
            DrawLine(widthBorder + 0 * space, heightBorder + 2 * space, widthBorder + 2 * space, heightBorder + 4 * space);
            DrawLine(widthBorder + 0 * space, heightBorder + 1 * space, widthBorder + 3 * space, heightBorder + 4 * space);
            DrawLine(widthBorder + 0 * space, heightBorder + 0 * space, widthBorder + 4 * space, heightBorder + 4 * space);
            DrawLine(widthBorder + 1 * space, heightBorder + 0 * space, widthBorder + 4 * space, heightBorder + 3 * space);
            DrawLine(widthBorder + 2 * space, heightBorder + 0 * space, widthBorder + 4 * space, heightBorder + 2 * space);
            DrawLine(widthBorder + 3 * space, heightBorder + 0 * space, widthBorder + 4 * space, heightBorder + 1 * space);

            DrawLine(widthBorder + 1 * space, heightBorder + 0 * space, widthBorder + 0 * space, heightBorder + 1 * space);
            DrawLine(widthBorder + 2 * space, heightBorder + 0 * space, widthBorder + 0 * space, heightBorder + 2 * space);
            DrawLine(widthBorder + 3 * space, heightBorder + 0 * space, widthBorder + 0 * space, heightBorder + 3 * space);
            DrawLine(widthBorder + 4 * space, heightBorder + 0 * space, widthBorder + 0 * space, heightBorder + 4 * space);
            DrawLine(widthBorder + 4 * space, heightBorder + 1 * space, widthBorder + 1 * space, heightBorder + 4 * space);
            DrawLine(widthBorder + 4 * space, heightBorder + 2 * space, widthBorder + 2 * space, heightBorder + 4 * space);
            DrawLine(widthBorder + 4 * space, heightBorder + 3 * space, widthBorder + 3 * space, heightBorder + 4 * space);

            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    DrawCircle(widthBorder + x * space, heightBorder + y * space);
                    ValidPositions.Add(new FieldPosition(x, y), (widthBorder + x * space, heightBorder + y * space));
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
                Height = 50,
                Fill = new SolidColorBrush(Colors.White),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 5
            };

            Canvas.SetLeft(circle, x - 25);
            Canvas.SetBottom(circle, y - 25);
            DrawingCanvas.Children.Add(circle);
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
