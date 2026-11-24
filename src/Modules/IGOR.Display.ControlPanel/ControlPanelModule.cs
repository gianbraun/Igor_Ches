using Accessibility;
using IGOR.Common;
using IGOR.Common.Modularity;
using IGOR.Display.ControlPanel.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Display.ControlPanel
{
    public class ControlPanelModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public ControlPanelModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion<ControlPanelView>(RegionNames.ControlPanelRegion);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
    }
}
