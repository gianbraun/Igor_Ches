using DryIoc;
using IGOR.ViewModels;
using Prism.Ioc;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace IGOR.Views
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {

        private readonly IContainerProvider _containerProvider;
        /// <summary>
        /// Interaction logic for SplashScreen.xaml
        /// </summary>
        public SplashScreen(IContainerProvider containerProvider)
        {
            InitializeComponent();
        }
    }
}
