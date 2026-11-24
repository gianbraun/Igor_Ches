using IGOR.Common.WPF;
using IGOR.Games.Kono.ViewModels;
using IGOR.Games.Kono.Converter;
using IGOR.Interface.Game;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IGOR.Games.Kono.Views
{
    /// <summary>
    /// Interaktionslogik für KonoView.xaml
    /// </summary>
    public partial class KonoView : UserControl
    {
        private KonoViewModel vm;
        private bool first = true;
        public KonoView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (!first)
                    return;

                vm = DataContext as KonoViewModel;
                CreateKonoField();
                first = false;
            };
        }



        private void CreateKonoField()
        {
            DrawLine(100, 400, 200, 500);
            DrawLine(100, 300, 300, 500);
            DrawLine(100, 200, 400, 500);
            DrawLine(100, 100, 500, 500);
            DrawLine(200, 100, 500, 400);
            DrawLine(300, 100, 500, 300);
            DrawLine(400, 100, 500, 200);

            DrawLine(200, 100, 100, 200);
            DrawLine(300, 100, 100, 300);
            DrawLine(400, 100, 100, 400);
            DrawLine(500, 100, 100, 500);
            DrawLine(500, 200, 200, 500);
            DrawLine(500, 300, 300, 500);
            DrawLine(500, 400, 400, 500);

            for (int x = 1; x < 6; x++)
            {
                for (int y = 5; y > 0; y--)
                {
                    DrawCircle(x * 100, y * 100);
                }
            }
        }

        private void DrawLine(int x0, int y0, int x1, int y1)
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

        private void DrawCircle(int x, int y)
        {
            var gird = new Grid();
            var circle = new Ellipse()
            {
                Width = 40,
                Height = 40,
                Fill = new SolidColorBrush(Colors.White),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 5
            };
            var playerStone = new Ellipse()
            {
                Width = 80,
                Height = 80,
            };
            gird.Children.Add(circle);
            gird.Children.Add(playerStone);

            var button = new Button
            {
                Content = gird,
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(Colors.Transparent),
                Command = vm.MakeMoveCommand,
                CommandParameter = new FieldPosition((x / 100) - 1, (y / 100) - 1)
            };
            var borderColorBinding = new Binding($"IsHilighted[{(x / 100) - 1}][{(y / 100) - 1}]")
            {
                Source = vm,
                Converter = new BoolToColorConverter(new SolidColorBrush(Colors.Transparent), new SolidColorBrush(Colors.Red)),
            };

            var binding = new Binding()
            {
                Source = vm,
                Path = new PropertyPath($"BoardState[{(x / 100) - 1} ][ {(y / 100) - 1}]"),
                Converter = new PlayerToColorConverter()
            };
            playerStone.SetBinding(Shape.FillProperty, binding);
            button.SetBinding(BorderBrushProperty, borderColorBinding);


            Canvas.SetTop(button, y - 40);
            Canvas.SetLeft(button, x - 40);

            DrawingCanvas.Children.Add(button);
        }
    }
}
