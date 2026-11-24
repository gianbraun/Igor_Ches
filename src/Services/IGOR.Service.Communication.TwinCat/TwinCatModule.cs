using IGOR.Common;
using IGOR.Common.Modularity;
using IGOR.Interface.Communication;
using IGOR.Service.Communication.TwinCat.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Serilog;

namespace IGOR.Service.Communication.TwinCat
{
    /// <summary>
    /// Communicationmodul for TwinCat 3
    /// </summary>
    [ModulePriority(ModuleType.Service, 100)]
    public class TwinCatModule : IModule
    {
        private readonly ILogger log;

        private readonly IRegionManager _regionManager;

        /// <summary>
        /// Communicationmodul for TwinCat 3
        /// </summary>
        public TwinCatModule(IRegionManager regionManager, ILogger logger)
        {
            _regionManager = regionManager;
            log = logger.ForContext<TwinCatModule>();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion(RegionNames.PlcStatusBar, typeof(TwinCatStatusBar));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IPlcCom, TwinCatCom>();
        }
    }
}