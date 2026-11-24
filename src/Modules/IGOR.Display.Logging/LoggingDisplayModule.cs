using IGOR.Common;
using IGOR.Common.Modularity;
using IGOR.Display.Logging.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Display.Logging
{
    [ModulePriority(ModuleType.Standard, priority: -1)]
    public class LoggingDisplayModule : IModule
    {
        private readonly IRegionManager _regionManager;
        public LoggingDisplayModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion<ErrorIndicatorView>(RegionNames.ErrorIndicator);
            _regionManager.RegisterViewWithRegion<ErrorBannerView>(RegionNames.ErrorBanner);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ErrorListView>("ErrorListView");
        }
    }
}
