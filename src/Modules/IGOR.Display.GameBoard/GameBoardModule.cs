using IGOR.Common;
using IGOR.Display.GameBoard.Views;
using IGOR.Interface.Camera;
using IGOR.Service.Navigation.Interface;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace IGOR.Display.GameBoard
{
    public class GameBoardModule : IModule
    {
        private readonly INavigationService _navigationService;
        private readonly ILogger _log;
        private readonly ICameraService _cameraService;

        public GameBoardModule(INavigationService navigationService, ILogger logger, ICameraService cameraService)
        {
            _navigationService = navigationService;
            _cameraService = cameraService;
            _log = logger.ForContext<GameBoardModule>();
                        
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var gameboard = containerProvider.Resolve<GameBoardView>();
            gameboard.ShowOnSecondMonitor();
            _navigationService.NavigateToBoard("DefaultGameView");

        } 

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DefaultGameView>("DefaultGameView");
            containerRegistry.RegisterForNavigation<CameraReferenceView>();
            containerRegistry.RegisterForNavigation<NoPortalView>();
            containerRegistry.RegisterDialog<GameParametersDialogView>(DialogNames.GameParameters);
            containerRegistry.RegisterDialog<GameRulesDialog>(DialogNames.GameRules);
        }
    }
}
