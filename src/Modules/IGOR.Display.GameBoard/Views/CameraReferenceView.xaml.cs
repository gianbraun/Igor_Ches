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

namespace IGOR.Display.GameBoard.Views
{
    /// <summary>
    /// Interaktionslogik für CameraReferenceView.xaml
    /// </summary>
    public partial class CameraReferenceView : UserControl
    {
        public CameraReferenceView()
        {
            InitializeComponent();
            Loaded += (s, e) => DrawReferences();
        }

        private void DrawReferences()
        {
            var topLeft = new Image()
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/IGOR.Display.GameBoard;component/Icons/ArucoRef0.jpg"))
            };

            var topRight = new Image()
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/IGOR.Display.GameBoard;component/Icons/ArucoRef1.jpg"))
            };

            var bottomLeft = new Image()
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/IGOR.Display.GameBoard;component/Icons/ArucoRef3.jpg"))
            };

            var bottomRight = new Image()
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/IGOR.Display.GameBoard;component/Icons/ArucoRef2.jpg"))
            };

            Canvas.Children.Add(topLeft);
            Canvas.Children.Add(topRight);
            Canvas.Children.Add(bottomLeft);
            Canvas.Children.Add(bottomRight);

            Canvas.SetLeft(topLeft, 20);
            Canvas.SetTop(topLeft, 20);

            Canvas.SetTop(topRight, 20);
            Canvas.SetLeft(topRight, ActualWidth - 120);

            Canvas.SetTop(bottomLeft, Canvas.ActualHeight - 120);
            Canvas.SetLeft(bottomLeft, 20);

            Canvas.SetTop(bottomRight, Canvas.ActualHeight - 120);
            Canvas.SetLeft(bottomRight, Canvas.ActualWidth - 120);
        }
    }
}
