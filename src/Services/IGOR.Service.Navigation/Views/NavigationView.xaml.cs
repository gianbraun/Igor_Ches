using System.Windows;
using System.Windows.Controls;

namespace IGOR.Service.Navigation.Views
{
    /// <summary>
    /// Interaction logic for NavigationView
    /// </summary>
    public partial class NavigationView : UserControl
    {
        public NavigationView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
