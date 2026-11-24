using Prism.Regions;
using IGOR.Interface.Camera;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace IGOR.Display.GameBoard.Views
{
    /// <summary>
    /// Interaktionslogik für GameBoardView.xaml
    /// </summary>
    public partial class GameBoardView : Window
    {

        private ICameraService _cameraService;
        private Screen secondScreen;

        public GameBoardView(IRegionManager regionManager, ICameraService cameraService)
        {
            RegionManager.SetRegionManager(this, regionManager);
            _cameraService = cameraService;

            InitializeComponent();
        }

        public void ShowOnSecondMonitor()
        {
            secondScreen = Screen.AllScreens.Where(s => s.DeviceName != Screen.PrimaryScreen.DeviceName).FirstOrDefault();
            if (secondScreen == null)
                secondScreen = Screen.PrimaryScreen;
                
            var bounds = secondScreen.Bounds;
            Left = secondScreen.Bounds.Left;
            Top = secondScreen.Bounds.Top;
            Width = secondScreen.Bounds.Width;
            Height = secondScreen.Bounds.Height;
            Loaded += GameBoardLoaded;
            _cameraService.SetScreenSize(bounds.Width, bounds.Height);
            Show();
        }

        private void GameBoardLoaded(object sender, RoutedEventArgs e)
        {
            if (secondScreen != Screen.PrimaryScreen) 
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }
    }
}
