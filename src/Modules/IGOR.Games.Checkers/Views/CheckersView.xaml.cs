using IGOR.Common.WPF;
using IGOR.Games.Checkers.Converter;
using IGOR.Games.Checkers.ViewModels;
using IGOR.Interface.Game;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IGOR.Games.Checkers.Views
{
    /// <summary>
    /// Interaktionslogik für CheckersView.xaml
    /// </summary>
    public partial class CheckersView : UserControl
    {
        private CheckersViewModel vm;
        private bool first = true;
        public CheckersView()
        {
            Loaded += (s, e) =>
            {
                if (!first)
                    return;

                vm = DataContext as CheckersViewModel;
                CreateCheckersField();
                first = false;
            };
            InitializeComponent();
            
        }



        private void CreateCheckersField()
        {

            for (int y = 0; y < 8; y++)
            {
                var row = new StackPanel()
                {
                    Orientation = Orientation.Horizontal
                };
                MainStackPanel.Children.Add(row);
                for (int x = 0; x < 8; x++)
                {
                    SolidColorBrush buttonColor = y % 2 == 0
                        ? x % 2 == 0 ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.White)
                        : x % 2 == 0 ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Gray);
                    var ellipse = new Ellipse()
                    {
                        Width = 80,
                        Height = 80,
                        Fill = new SolidColorBrush(Colors.Transparent)
                    };

                    var fieldButton = new Button()
                    {
                        Content = ellipse,
                        Background = buttonColor,
                        Width = 100,
                        Height = 100,
                        BorderThickness = new Thickness(1),
                        Command = vm.MakeMoveCommand,
                        CommandParameter = new FieldPosition(x, y)
                    };
                    var borderColorBinding = new Binding($"IsHilighted[{x}][{y}]")
                    {
                        Source = vm,
                        Converter = new BoolToColorConverter(new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Red)),
                    };
                    var binding = new Binding()
                    {
                        Source = vm,
                        Path = new PropertyPath($"BoardState[{x}][{y}]"),
                        Converter = new PlayerToColorConverter()
                    };
                    ellipse.SetBinding(Shape.FillProperty, binding);
                    fieldButton.SetBinding(BorderBrushProperty, borderColorBinding);
                    row.Children.Add(fieldButton);
                }
            }
        }
    }
}
