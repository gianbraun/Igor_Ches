using IGOR.Display.GameBoard.Views;
using IGOR.Interface.Camera;
using IGOR.Interface.Communication;
using IGOR.Interface.Logging;
using IGOR.Interface.Parameters;
using IGOR.Service.Navigation.Interface;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IGOR.ViewModels
{
    public class SplashScreenViewModel : BindableBase
    {
        private readonly IModuleCatalog _moduleCatalog;
        private ILogger _log;
        private readonly IContainerProvider _containerProvider;

        public event EventHandler ModulesLoaded;

        public string InfoText { get; set; } = "Initialisiere";

        public SplashScreenViewModel(IModuleCatalog moduleCatalog, IContainerProvider containerProvider)
        {
            _moduleCatalog = moduleCatalog;
            _containerProvider = containerProvider; 
            _log = IAppLogger.DefaultLoggerConfig.CreateLogger().ForContext<App>();
              
        }


        public async Task LoadModulesAsync()
            => await Task.Run(() =>
            {
                var moduleManager = _containerProvider.Resolve<IModuleManager>();
                _moduleCatalog.Initialize();
                foreach (var module in _moduleCatalog.CompleteListWithDependencies(_moduleCatalog.Modules))
                {
                    if (module.State == ModuleState.Initialized)
                        return Task.CompletedTask;

                    try
                    {
                        InfoText = $"Lade Modul: {module.ModuleName}";
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            moduleManager.LoadModule(module.ModuleName);
                        });

                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "Module: {module} kann nicht geladen werden", module.ModuleName);
                    }

                    _log.Information("Modul \"{module}\" geladen", module.ModuleName);
                }
                ConnectToPlc(); 

                ModulesLoaded?.Invoke(this, null);
                return Task.CompletedTask;
            }); 

        private void ConnectToPlc()
        {
            InfoText = "Verbindung zur SPS wird hergestellt";
            var plcCom = _containerProvider.Resolve<IPlcCom>();
#if DEBUG
            plcCom.Connect("192.168.1.165.1.1");
#elif RELEASE
            plcCom.Connect();
#endif

        }
    }
}
