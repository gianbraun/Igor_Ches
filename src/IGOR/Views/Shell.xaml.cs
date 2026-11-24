using System.Windows;

namespace IGOR.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Shell : Window
    {
        /// <summary>
        /// Interaction logic for MainWindow.xaml
        /// </summary>
        public Shell()
        {
            InitializeComponent();
#if DEBUG
            Loaded += Shell_Loaded;
#endif
        }

#if DEBUG
        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            // not fullscreen in debug mode
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.ThreeDBorderWindow;
        }
#endif
    }
}
