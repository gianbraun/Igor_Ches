using IGOR.Common.Modularity;
using IGOR.Interface.Logging;
using Prism.Ioc;
using Prism.Modularity;
using Serilog;
using System;

namespace IGOR.Service.Logging
{
    [ModulePriority(ModuleType.Service, int.MaxValue)]
    public class LoggingModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var applogger = new AppLogger();
            containerRegistry.RegisterInstance<IAppLogger>(applogger);
            containerRegistry.RegisterInstance<ILogger>(applogger);
        }
    }
}
