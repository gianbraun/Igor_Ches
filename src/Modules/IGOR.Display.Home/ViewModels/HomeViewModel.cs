using IGOR.Common;
using IGOR.Display.GameBoard.Views;
using IGOR.Interface.Camera;
using IGOR.Interface.Communication;
using IGOR.Interface.Communication.Job;
using IGOR.Interface.Game;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IGOR.Display.Home.ViewModels
{
    public class HomeViewModel: INavigationAware
    //public class HomeViewModel 
    {
        private readonly IGameService _gameService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ICameraService _cameraService;
        private readonly IPlcCom _plcCom;

        public IEnumerable<IGame> Games => _gameService.RegisteredGames;

        private bool _first = true;
        public DelegateCommand<IGame> StartGameCommand { get; }

        private DelegateCommand _clearFieldCommand;
        public DelegateCommand ClearFieldCommand => _clearFieldCommand ??= new DelegateCommand(async () =>
        {
            //await ClearField();
            await Application.Current.Dispatcher.InvokeAsync(ClearField);
        });

        // public HomeViewModel(IGameService gameService, INavigationService navigationService, IDialogService dialogService)
        public HomeViewModel(IGameService gameService, INavigationService navigationService, IDialogService dialogService, ICameraService cameraService, IPlcCom plcCom)
        {
            _gameService = gameService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _cameraService = cameraService;
            _plcCom = plcCom;
            StartGameCommand = new DelegateCommand<IGame>(StartGame);

        }


        private void StartGame(IGame game)
        { 
            var parameters = new DialogParameters();
            parameters.Add(DialogGameParameterNames.Game, game);
            parameters.Add(DialogGameParameterNames.GameOver, false);

            _dialogService.ShowDialog(DialogNames.GameParameters, parameters, r =>
            {
                var difficulty = r.Parameters.GetValue<Difficulty>(DialogGameParameterNames.Difficulty);
                var player = r.Parameters.GetValue<Player>(DialogGameParameterNames.SelectedPlayer);
                var key = r.Parameters.GetValue<string>(DialogGameParameterNames.SelectedBoardKey);
                if(r.Result == ButtonResult.OK)
                {
                    game.Difficulty = difficulty;
                    game.HumanPlayer = player;
                    _navigationService.NavigateToBoard(game.Views[key].board);
                    _navigationService.NavigateTo(game.Views[key].hmi, ("game", game));
                }
            });
        }

        protected async Task ClearField()
        {
            await ClearField(null, 0);
        }

        // TODO: in CameraService oder PlcComService verschieben?
        protected async Task ClearField(Dictionary<FieldPosition, (double x, double y)>? validPositions, uint tolerance)
        {
            _navigationService.NavigateToBoard("CameraReferenceView");

            var stones = await Task.Run(() =>
            {
                while (_navigationService.ActualBoardName != "CameraReferenceView")
                {
                    Thread.Sleep(100);
                }
                return _cameraService.ReadStonePositions(validPositions, tolerance);
            });

            if (stones == null)
                return;

            var jobs = new List<PlcJob>();
            foreach (var stone in stones)
            {
                var (x, y) = _cameraService.PixelsToPosition(stone.X, stone.Y);
                var isBlack = stone.Player == Player.Black;
                jobs.Add(new PlcJob(MoveType.RemoveStone, x, y, x, y, isBlack));
            }

            if (jobs.Count > 0)
                _plcCom.EnqueueJobs(jobs, true);

            _navigationService.NavigateToBoard("DefaultGameView");
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (_gameService.CurrentGame != null &&  ((_gameService.CurrentGame.IsGameOver.HasValue && !_gameService.CurrentGame.IsGameOver.Value) || _gameService.CurrentGame.CurrentState == null))
            {
                _navigationService.NavigateToBoard(_gameService.CurrentGame.ActualView.board);
                _navigationService.NavigateTo(_gameService.CurrentGame.ActualView.hmi, ("game", _gameService.CurrentGame));
                return;
            }

            if (_first)
            {
                _first = false;
                return;
            }
            _ = ClearField();
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
