using IGOR.Games.TicTacToe.Converter;
using IGOR.Games.TicTacToe.ViewModels;
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

namespace IGOR.Games.TicTacToe.Views
{
    /// <summary>
    /// Interaktionslogik für TicTacToeTouchView.xaml
    /// </summary>
    public partial class TicTacToeTouchView : UserControl
    {
        private TicTacToeTouchViewModel vm;
        public TicTacToeTouchView()
        {
            InitializeComponent();
            Loaded += (s, e) => DrawBoard();
        }

        private void DrawBoard()
        {

            vm = (TicTacToeTouchViewModel)DataContext;
            for (int y = 0; y < 3; y++)
            {
                //Board.Children.Clear();
                for (int x = 0; x < 3; x++)
                {
                    DrawRectangle(y, x);
                } 
            }
        }

        private void DrawRectangle(int y, int x)
        {
            var grid = new Grid();
            var ellips = new Ellipse()
            {
                Width = 80,
                Height = 80,
            };

            var button = new Button()
            {
                Content = ellips,
                Command = vm.MakeMoveCommand,
                CommandParameter = new FieldPosition(x, y),
                Width = 450 / 3,
                Height = 450 / 3,
            };

            var playerColorBinding = new Binding($"BoardState[{x}][{y}]")
            {
                Source = vm,
                Converter = new PlayerToColorConverter(),
                Mode = BindingMode.OneWay
            };

            //var enabledBinding = new Binding("CanMakeMove")
            //{
            //    Source = vm,
            //    Mode = BindingMode.OneWay
            //};

            ellips.SetBinding(Shape.FillProperty, playerColorBinding);
            //button.SetBinding(IsEnabledProperty, enabledBinding); 

            Grid.SetColumn(button, x);
            Grid.SetRow(button, y);

            Board.Children.Add(button);
        }
    }
}
