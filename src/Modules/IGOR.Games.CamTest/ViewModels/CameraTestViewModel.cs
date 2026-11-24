using IGOR.Interface.Camera;
using IGOR.Interface.Game;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IGOR.Games.CamTest.ViewModels
{
    public class CameraTestViewModel : BindableBase, INavigationAware
    {
        private ICameraService _cameraService;
        private INavigationService _navigationService;
        private IEnumerable<Stone> _stonePositions;
        public IEnumerable<Stone> StonePositions
        {
            get => _stonePositions;
            set => SetProperty(ref _stonePositions, value);
        }

        private DelegateCommand _triggerCamCommand;
        public DelegateCommand TriggerCamCommand => _triggerCamCommand ??= new DelegateCommand(TriggerCam);

        public CameraTestViewModel(ICameraService cameraService, INavigationService navigationService, IEventAggregator eventAggregator)
        {
            _cameraService = cameraService;
            _navigationService = navigationService;
        }

        private void TriggerCam()
        {
            Application.Current.Dispatcher.Invoke(() => _navigationService.NavigateToBoard("CameraReferenceView"));
           // _cameraService.ReadStonePositions();
        }

        private void StonesDetected(IEnumerable<(double x, double y)> whiteStones, IEnumerable<(double x, double y)> blackStones)
        {
            _navigationService.NavigateToBoard("CameraTestView");
            var stones = new List<Stone>();
            foreach (var stone in whiteStones)
            {
                (var x, var y) = _cameraService.PixelsToPosition(stone.x, stone.y);
                stones.Add(new Stone(x, y, Player.White, null));
            }

            foreach (var stone in blackStones)
            {
                (var x, var y) = _cameraService.PixelsToPosition(stone.x, stone.y);
                stones.Add(new Stone(x, y, Player.Black, null));
            }
            StonePositions = stones;
        }

        #region Navigation

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }

        #endregion

    }
}
