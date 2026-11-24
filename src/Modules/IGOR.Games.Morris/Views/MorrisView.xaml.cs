using IGOR.Common.WPF;
using IGOR.Games.Morris.Converter;
using IGOR.Games.Morris.ViewModels;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IGOR.Games.Morris.Views
{
    /// <summary>
    /// Interaktionslogik für MorrisView.xaml
    /// </summary>
    public partial class MorrisView : UserControl
    {
        private MorrisViewModel vm;
        public MorrisView()
        {
            InitializeComponent();
            Loaded += (s, e) => DrawGameBoard();
        }

        private void DrawGameBoard()
        {
            vm = DataContext as MorrisViewModel;
            DrawLine(300, 0, 300, 200);
            DrawLine(300, 600, 300, 400);
            DrawLine(0, 300, 200, 300);
            DrawLine(400, 300, 600, 300);
            for (int i = 0; i < 3; i++)
            {
                DrawRectangle(i * 100, 600 - i * 200);
            }
            
        }


        private void DrawRectangle(int pos, int size)
        {
            var rectangle = new Rectangle()
            {
                Width = size,
                Height = size,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 5,
            };

            Canvas.SetTop(rectangle, pos);
            Canvas.SetLeft(rectangle, pos);
            DrawingCanvas.Children.Add(rectangle);
            DrawCircle(pos, pos);
            DrawCircle(pos + size / 2, pos);
            DrawCircle(pos + size, pos);
            DrawCircle(pos, pos + size / 2);
            DrawCircle(pos + size, pos + size / 2);
            DrawCircle(pos, pos + size);
            DrawCircle(pos + size / 2, pos + size);
            DrawCircle(pos + size, pos + size);
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
                Height = 80
            };
            gird.Children.Add(circle);
            gird.Children.Add(playerStone);
            
            var button = new Button
            {
                Content = gird,
                BorderThickness = new Thickness(1),
                Command = vm.MakeMoveCommand,
                CommandParameter = new FieldPosition(x / 100, y / 100)
            };
            var borderColorBinding = new Binding($"IsHilighted[{x/100}][{y/100}]")
            {
                Source = vm,
                Converter = new BoolToColorConverter(new SolidColorBrush(Colors.Transparent), new SolidColorBrush(Colors.Red)),
            };

            var binding = new Binding()
            {
                Source = vm,
                Path = new PropertyPath($"BoardState[{x/100}][{y/100}]"),
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
