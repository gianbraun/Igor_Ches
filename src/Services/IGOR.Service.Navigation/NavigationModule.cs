using System;
using IGOR.Common;
using IGOR.Common.Modularity;
using IGOR.Service.Navigation.Interface;
using IGOR.Service.Navigation.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace IGOR.Service.Navigation
{
    [ModulePriority(ModuleType.Service)]
    public class NavigationModule : IModule
    {
        private IRegionManager _regionManager;

        public NavigationModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(NavigationView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<INavigationService, NavigationService>();
        }
    }
}
