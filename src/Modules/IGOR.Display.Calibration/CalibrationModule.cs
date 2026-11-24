using IGOR.Common.Modularity;
using IGOR.Common.WPF.Icons;
using IGOR.Display.Calibration.Views;
using IGOR.Interface.Camera;
using IGOR.Service.Navigation.Interface;
using Prism.Ioc;
using Prism.Modularity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Display.Calibration
{
    public class CalibrationModule : IModule
    {

        private readonly INavigationService _navigationService;

        public CalibrationModule(INavigationService navigationService, ILogger logger, ICameraService cameraService)
        {
            _navigationService = navigationService;

        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // register navigation button
            //_navigationService.AddNavigationButton("Calibrate", IconKey.Parameter, nameof(CameraCalibrationView));
            // TODO: Icon hinzufügen
            // TODO: Reihenfolge ändern, in der die Module geladen werden
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CameraCalibrationBoardView>();
            containerRegistry.RegisterForNavigation<CameraCalibrationView>();
        }
    }
}
