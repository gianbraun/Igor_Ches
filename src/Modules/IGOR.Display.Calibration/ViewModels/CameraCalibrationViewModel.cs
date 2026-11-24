using IGOR.Display.Calibration.Views;
using IGOR.Interface.Camera;
using IGOR.Service.Navigation.Interface;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace IGOR.Display.Calibration.ViewModels
{
    public class CameraCalibrationViewModel : BindableBase, INavigationAware
    {
        private readonly ICameraService _cameraService;
        private readonly INavigationService _navigationService;
        private readonly ILogger _logger;

        private BitmapSource _imageSource;
        public BitmapSource ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }
        private BitmapSource _imageUndistorted;
        public BitmapSource ImageUndistorted
        {
            get => _imageUndistorted;
            set => SetProperty(ref _imageUndistorted, value);
        }

        private DelegateCommand _calibrateCameraCommand;
        public DelegateCommand CalibrateCameraCommand => _calibrateCameraCommand ??= new DelegateCommand(async () =>
        {
            await CalibrateCamera();
            //Application.Current.Dispatcher.Invoke(() => );
        });

        public CameraCalibrationViewModel(ICameraService cameraService, INavigationService navigationService, ILogger logger)
        {
            _navigationService = navigationService;
            _cameraService = cameraService;
            _logger = logger.ForContext(typeof(CalibrationModule));

        }

        private async Task CalibrateCamera()
        { 
            var currentBoardView = _navigationService.ActualBoardName;

            _navigationService.NavigateToBoard(nameof(CameraCalibrationBoardView));
            await Task.Run(() =>
            {
                while (_navigationService.ActualBoardName != nameof(CameraCalibrationBoardView))
                {
                    Thread.Sleep(100);
                }
            });

            Mat image;
            Mat imageUndistorted;
            var sw = Stopwatch.StartNew();
            bool succeeded = false;

            await Task.Run(() =>
            {
                while (sw.Elapsed.TotalMilliseconds < 20000 && !succeeded)
                {
                    succeeded = _cameraService.CalibrateCamera(CameraCalibrationBoardView.PatternSize, out image, out imageUndistorted);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (image.Total() > 0)
                            ImageSource = image.ToBitmapSource();
                        if (imageUndistorted.Total() > 0)
                            ImageUndistorted = imageUndistorted.ToBitmapSource();
                    });
                    Thread.Sleep(100);
                }
            });

            if (!succeeded)
            {
                Log.Warning("Die Kamera konnte nicht kalibriert werden.");
            }

            _navigationService.NavigateToBoard(currentBoardView);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
