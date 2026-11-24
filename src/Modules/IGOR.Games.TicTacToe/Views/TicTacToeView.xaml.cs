//using IGOR.Common.WPF.Converters;
//using IGOR.Games.TicTacToe.Converter;
//using IGOR.Games.TicTacToe.ViewModels;
//using IGOR.Interface.Game;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

//namespace IGOR.Games.TicTacToe.Views
//{
//    /// <summary>
//    /// Interaktionslogik für TicTacToeView.xaml
//    /// </summary>
//    public partial class TicTacToeView : UserControl
//    {
//        private TicTacToeViewModel vm;
//        public TicTacToeView()
//        {
//            InitializeComponent();
//            Loaded += (s, e) => DrawBoard();
//        }

//        private void DrawBoard()
//        {

//            vm = (TicTacToeViewModel)DataContext;
//            for (int y = 0; y < 3; y++)
//            {
//                var row = new StackPanel()
//                {
//                    Orientation = Orientation.Horizontal,
//                };
//                for (int x = 0; x < 3; x++)
//                {
//                    var ellips = new Ellipse()
//                    {
//                        Width = 80,
//                        Height = 80,
//                    };

//                    var button = new Button()
//                    {
//                        Content = ellips,
//                        Command = vm.MakeMoveCommand,
//                        CommandParameter = new FieldPosition(x, y),
//                        Width = 450 / 3,
//                        Height = 450 / 3,
//                    };

//                    var playerColorBinding = new Binding($"BoardState[{x}][{y}]")
//                    {
//                        Source = vm,
//                        Converter = new PlayerToColorConverter()
//                    };

//                    var enabledBinding = new Binding("CanMakeMove")
//                    {
//                        Source = vm
//                    };

//                    ellips.SetBinding(Shape.FillProperty, playerColorBinding);
//                    button.SetBinding(IsEnabledProperty, enabledBinding);
//                    row.Children.Add(button);
//                }
//                Board.Children.Add(row);
//            }
//        }
//    }
//}
