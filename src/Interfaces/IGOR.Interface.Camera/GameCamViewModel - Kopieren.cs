using IGOR.Common;
using IGOR.Interface.Communication;
using IGOR.Interface.Communication.Job;
using IGOR.Interface.Game;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IGOR.Interface.Camera
{
    public abstract class GameCamViewModelOld<T> : BindableBase, INavigationAware
        where T : class, IGame
    {

        protected readonly ICameraService CameraService;
        protected readonly IPlcCom PlcCom;
        protected readonly IDialogService DialogService;
        protected readonly INavigationService NavigationService;
        protected readonly ILogger Log;

        protected T _game;
        public T Game
        {
            get => _game;
            set => SetProperty(ref _game, value, nameof(Game));
        }

        protected bool _first = true;
        protected bool _reading;
        public const int StoneSize = 90;
        public int Tolerance { get; set; } = 10;
        public ObservableCollection<Stone> Stones { get; set; } = new ObservableCollection<Stone>();

        private DelegateCommand _openGameRulesCommand;
        public DelegateCommand OpenGameRulesCommand => _openGameRulesCommand ??= new DelegateCommand(OpenGameRules);


        private DelegateCommand _triggerCamCommand;
        public DelegateCommand TriggerCamCommand => _triggerCamCommand ??= new DelegateCommand(async () =>
        {
            await Application.Current.Dispatcher.InvokeAsync(TriggerCameraAsync);
        });


        private DelegateCommand _validateMoveCommand;
        public DelegateCommand ValidateMoveCommand => _validateMoveCommand ??= new DelegateCommand(ValidateMove, CanValidateMove);

        protected GameCamViewModelOld(ICameraService cameraService, IPlcCom plcCom, IDialogService dialogService, INavigationService navigationService, ILogger log)
        {
            CameraService = cameraService;
            PlcCom = plcCom;
            PlcCom.PlcJobDone += (s, e) => OnPlcJobDone(e);
            DialogService = dialogService;
            NavigationService = navigationService;
            Log = log;
        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual async void OnNavigatedFrom(NavigationContext navigationContext)
        {
            await ClearField();

            PlcCom.RemoveObserver("HMI.bHandInPortal");
        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

            Game = (T)navigationContext.Parameters["game"];
            Application.Current.Dispatcher.Invoke(() => Stones.Clear());
            if (Game.CurrentState == null)
            {
                Game.BoardReady += GameReady;

            }
            else
            {
                GameReady(null, null);
            }

            PlcCom.TryObserve<bool>("HMI.bHandInPortal", async v =>
            {
                if (v || _reading || _first)
                {
                    _first = false;
                    return;
                }

                await Application.Current.Dispatcher.InvokeAsync(TriggerCameraAsync);
            });
        }

        /// <summary>
        /// Is called when the game is set up and ready to play.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void GameReady(object sender, EventArgs e)
        {
            Game.BoardReady -= GameReady;
            Game.Start();
            SendJobs();
            if (Game.HumanPlayer == Player.Black)
            {
                Game.MakeAiMove();
                SendJobs();
            }
        }

        /// <summary>
        /// Reads the stones with the camera
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ReadStones()
        {
            var previousBoardName = NavigationService.ActualBoardName;
            NavigationService.NavigateToBoard("CameraReferenceView");

            var stones = await Task.Run(() =>
            {
                while (NavigationService.ActualBoardName != "CameraReferenceView")
                {
                    Thread.Sleep(100);
                }
                return CameraService.ReadStonePositions(Game.CurrentBoard.ValidPositions, (uint)Tolerance);
            });


            NavigationService.NavigateToBoard(previousBoardName);
            if (stones == null)
            {
                // in warning geändert und bricht die methode nicht mehr ab
                Log.Warning("Die Kamera konnte keine Spielsteine erkennen.");
                return;
            }
            Application.Current.Dispatcher.Invoke(() => Stones.Clear());
            Application.Current.Dispatcher.Invoke(() => Stones.AddRange(stones));
        }

        /// <summary>
        /// Enqueues all Jobs of the current state.
        /// </summary>
        protected virtual void SendJobs()
        {
            var commands = Game.CurrentState.PortalCommands;
            var plcJobs = CreatePlcJobs(commands);
            PlcCom.EnqueueJobs(plcJobs);
            // TODO: Überprüfen, ob alle gesendeten Jobs abgearbeitet wurden. Ansonsten den Spielstatus anpassen
        }
        
        /// <summary>
        /// Is called if a PlcJob is done.
        /// </summary>
        /// <param name="plcJob"></param>
        private void OnPlcJobDone(PlcJob plcJob)
        {
            var (StartX, StartY) = CameraService.PositionToPixels(plcJob.StartPosX, plcJob.StartPosY);
            var (EndX, EndY) = CameraService.PositionToPixels(plcJob.EndPosX, plcJob.EndPosY);
            var startPos = CameraService.ToValidPos(new OpenCvSharp.Point2f((float)StartX, (float)StartY), (uint)Tolerance, Game.CurrentBoard.ValidPositions);
            var endPos = CameraService.ToValidPos(new OpenCvSharp.Point2f((float)EndX, (float)EndY), (uint)Tolerance, Game.CurrentBoard.ValidPositions);

            var player = plcJob.IsBlack ? Player.Black : Player.White;

            switch (plcJob.MoveType)
            {
                case MoveType.MoveStone:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (startPos is null && endPos is null)
                            return;
                        var stone = Stones.FirstOrDefault(s => s.FieldPosition.X == startPos.X && s.FieldPosition.Y == startPos.Y);
                        if (stone != null)
                            Stones.Remove(stone);
                        Stones.Add(new Stone(EndX, EndY, player, endPos));
                    });
                    break;
                case MoveType.NewStone:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (endPos is null)
                            return;
                        Stones.Add(new Stone(EndX, EndY, player, endPos));
                    });
                    break;
                case MoveType.RemoveStone:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (startPos is null)
                            return;
                        var stone = Stones.FirstOrDefault(s => s.FieldPosition.X == startPos.X && s.FieldPosition.Y == startPos.Y);
                        if (stone != null)
                            Stones.Remove(stone);
                    });
                    break;
                default:
                    break;
            }
        }


        protected virtual IEnumerable<PlcJob> CreatePlcJobs(IEnumerable<PortalCommand> portalCommands)
        {

            List<PlcJob> jobs = new();
            foreach (var command in portalCommands)
            {
                var isblack = command.Player == Player.Black;
                var (oldx, oldy) = Game.CurrentBoard.ValidPositions[command.OldPos];
                (double newx, double newy) = Game.CurrentBoard.ValidPositions[command.NewPos];
                (var startX, var startY) = CameraService.PixelsToPosition(oldx, oldy);
                (var endX, var endY) = CameraService.PixelsToPosition(newx, newy);
                jobs.Add(new PlcJob(command.MoveType, startX, startY, endX, endY, isblack));
            }
            return jobs;
        }

        protected virtual void OpenGameRules()
        {
            var parameters = new DialogParameters();
            parameters.Add(DialogGameRuleParameters.FilePath, Game.GameRulePath);
            DialogService.ShowDialog(DialogNames.GameRules, parameters, null);
        }

        protected virtual bool CheckGameOver()
        {
            if (!Game.CurrentState.IsGameOver)
            {
                return false;
            }
            var winner = Game.CurrentState.GetWinner();
            var message = "Schade, du hast leider verloren.";
            var parameters = new DialogParameters();
            if (winner == Game.HumanPlayer)
                message = "Herzlichen Glückwunsch, du hast gewonnen!";
            else if (winner == Player.None)
                message = "Unentschieden";

            parameters.Add(DialogGameParameterNames.Message, message);
            parameters.Add(DialogGameParameterNames.GameOver, true);
            parameters.Add(DialogGameParameterNames.Game, Game);
            Application.Current.Dispatcher.Invoke(() =>
           DialogService.ShowDialog(DialogNames.GameParameters, parameters, async r =>
           {
               var difficulty = r.Parameters.GetValue<Difficulty>(DialogGameParameterNames.Difficulty);
               var player = r.Parameters.GetValue<Player>(DialogGameParameterNames.SelectedPlayer);
               var key = r.Parameters.GetValue<string>(DialogGameParameterNames.SelectedBoardKey);
               if (r.Result == ButtonResult.Cancel)
               {
                   NavigationService.NavigateTo("HomeView");
               }
               else
               {
                   Game.Difficulty = difficulty;
                   Game.HumanPlayer = player;
                   NavigationService.NavigateTo(Game.Views[key].hmi, ("game", Game));
                   NavigationService.NavigateToBoard(Game.Views[key].board);
                   //await ClearField();

               }
           }));

            return true;
        }

        /// <summary>
        /// Detects the stones on the field and sends jobs to clear them back into the stone storage.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ClearField()
        {
            NavigationService.NavigateToBoard("CameraReferenceView");
            await ReadStones();
            if (Stones == null)
                return;

            var jobs = new List<PlcJob>();
            foreach (var stone in Stones)
            {
                var (x, y) = CameraService.PixelsToPosition(stone.X, stone.Y);
                var isBlack = stone.Player == Player.Black;
                jobs.Add(new PlcJob(MoveType.RemoveStone, x, y, x, y, isBlack));
            }

            if (jobs.Count > 0)
                PlcCom.EnqueueJobs(jobs, true);

            NavigationService.NavigateToBoard("DefaultGameView");
        }

        protected abstract Task TriggerCameraAsync();

        protected abstract void ValidateMove();

        protected bool CanValidateMove()
        {
            if (Game == null)
                return false;
            if (Game.CurrentState == null)
                return false;

            return Game.CurrentState.Self == Game.HumanPlayer;
        }


        private void Game_CurrentStateChanged(object sender, EventArgs e)
        {
            ValidateMoveCommand.RaiseCanExecuteChanged();
        }

    }
}
