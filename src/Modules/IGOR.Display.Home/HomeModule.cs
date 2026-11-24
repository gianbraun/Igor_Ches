using IGOR.Common;
using IGOR.Common.Modularity;
using IGOR.Common.WPF.Icons;
using IGOR.Display.GameBoard.Views;
using IGOR.Display.Home.Views;
using IGOR.Service.Navigation.Interface;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace IGOR.Display.Home
{
    [ModulePriority(ModuleType.Standard, -100)]
    public class HomeModule : IModule
    {
        private INavigationService _navigationService;

        public HomeModule(INavigationService navigationService, IRegionManager regionManager)
        {
            _navigationService = navigationService;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _navigationService.AddNavigationButton("Home", IconKey.Home, "HomeView");
            _navigationService.NavigateTo("HomeView");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<HomeView>("HomeView");
        }
    }
}
