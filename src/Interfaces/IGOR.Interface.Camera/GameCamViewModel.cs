using IGOR.Interface.Communication;
using IGOR.Interface.Camera;
using IGOR.Service.Navigation.Interface;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IGOR.Interface.Communication.Job;
using Prism.Commands;
using System.Windows;
using System.Collections.ObjectModel;
using System.Threading;
using IGOR.Interface.Game;
using Prism.Regions;

namespace IGOR.Interface.Camera
{
    public abstract class GameCamViewModel<TGame> : GameViewModel<TGame>
        where TGame : class, IGame
    {
        protected readonly ICameraService CameraService;
        protected readonly IPlcCom PlcCom;

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        protected bool _first = true;
        protected bool _reading;
        public const int StoneSize = 90;
        public int Tolerance { get; set; } = 10;

        protected bool _hasDifferences = false;
        public bool HasDifferences { get => _hasDifferences; protected set => SetProperty(ref _hasDifferences, value, nameof(HasDifferences)); }

        public ObservableCollection<Stone> Stones { get; set; } = new ObservableCollection<Stone>();
        public ObservableCollection<Difference> Differences { get; set; } = new ObservableCollection<Difference>();

        CancellationTokenSource CancelJobsTokenSource = new CancellationTokenSource();


        private DelegateCommand _triggerCamCommand;
        public DelegateCommand TriggerCamCommand => _triggerCamCommand ??= new DelegateCommand(async () =>
        {
            await Application.Current.Dispatcher.InvokeAsync(TriggerCameraAsync);
        });


        private DelegateCommand _validateMoveCommand;
        public DelegateCommand ValidateMoveCommand => _validateMoveCommand ??= new DelegateCommand(async () => await ValidateMove(), CanValidateMove);


        protected GameCamViewModel(ICameraService cameraService, IPlcCom plcCom, IDialogService dialogService, INavigationService navigationService, ILogger log, IGameService gameService) : base(dialogService, navigationService, log, gameService)
        {
            CameraService = cameraService;
            PlcCom = plcCom;

            PlcCom.PlcJobDone += (s, e) => OnPlcJobDone(e);
        }



        #region Communication Functions

        int lastvalidcount = 0;
        private void OnPlcJobDone(PlcJob plcJob)
        {
            var (StartX, StartY) = CameraService.PositionToPixels(plcJob.StartPosX, plcJob.StartPosY);
            var (EndX, EndY) = CameraService.PositionToPixels(plcJob.EndPosX, plcJob.EndPosY);

            Dictionary<FieldPosition, (double x, double y)> validPositions = null;
            if (Game.NextState != null)
                validPositions = Game.NextState.Board.ValidPositions;
            else if (Game.CurrentState != null)
                validPositions = Game.CurrentState.Board.ValidPositions;

            if (lastvalidcount != validPositions.Count)
                lastvalidcount = validPositions.Count;

            var startPos = CameraService.ToValidPos(new OpenCvSharp.Point2f((float)StartX, (float)StartY), (uint)Tolerance, validPositions);
            var endPos = CameraService.ToValidPos(new OpenCvSharp.Point2f((float)EndX, (float)EndY), (uint)Tolerance, validPositions);

            var player = plcJob.IsBlack ? Player.Black : Player.White;

            switch (plcJob.MoveType)
            {
                case MoveType.MoveStone:
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (startPos == null || endPos == null)
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

        #endregion


        #region Game Functions

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);

            // remove automatic trigger
            PlcCom.RemoveObserver("HMI.bHandInPortal");
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            // trigger camera automatically
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

        protected override Task StartGame(TGame newGame)
        {
            Game = newGame;
            Application.Current.Dispatcher.Invoke(() => Stones.Clear());
            Game.BoardReady += BoardReady;

            Game.GameOver += On_GameOver;

            return Task.CompletedTask;
        }

        private void On_GameOver(object sender, Player e)
        {
            if (e == Game.HumanPlayer)
                PlcCom.TryWrite("HMI.bLightWin", true);
            else
                PlcCom.TryWrite("HMI.bLightLose", true);
        }

        protected virtual void BoardReady(object sender, EventArgs e)
        {
            Game.BoardReady -= BoardReady;

            Task.Run(async () =>
               {
                   Tolerance = CalculateTolerance();
                   Game.Start();

                   await Task.Delay(250);

                   await ClearField();

                   if (!await ExecuteJobs())
                   {
                       await ResolveDifferences();
                   }

                   // make ai move if player is black
                   if (Game.HumanPlayer == Player.Black)
                   {
                       if (!await ExecuteAiMove())
                       {
                           await ResolveDifferences();
                       }
                   }
               });
        }

        protected override void EndGame()
        {
            base.EndGame();

            Game.GameOver -= On_GameOver;
            CancelJobsTokenSource.Cancel();
        }

        protected async Task ValidateMove()
        {

            // Player move ausführen (nicht warten, der Spieler hat ja gesetzt)
            if (!ExecutePlayerMove())
            {
                // spielzug ungültig
                return;
            }

            // Check, if game is over
            if (CheckGameOver())
                return;

            // If not game over, execute an ai move
            if (!await ExecuteAiMove())
            {
                await ResolveDifferences();
            }

            // Check, if game is over
            if (CheckGameOver())
                return;
        }

        /// <summary>
        /// Executes a Move by the Player
        /// </summary> 
        /// <returns>success-state</returns>
        protected virtual bool ExecutePlayerMove()
        {

            var field = new Player[Game.CurrentBoard.Size][];

            for (int i = 0; i < Game.CurrentBoard.Size; i++)
            {
                field[i] = new Player[Game.CurrentBoard.Size];
            }

            foreach (var stone in Stones)
            {
                if (stone.FieldPosition != null)
                    field[stone.FieldPosition.X][stone.FieldPosition.Y] = stone.Player;
            }

            if (!Game.SetField(field))
            {
                Message = "Spielzug ungültig";
                return false;
            }

            ConfirmNextState();

            return true;
        }

        protected override async Task<bool> ExecuteAiMove()
        {
            // determine next state
            Game.MakeAiMove();

            // send and await jobs
            if (!await ExecuteJobs())
            {
                return false;
            }
            return true;
        }


        // Spielstatus ist unbekannt und muss wiederhergestellt werden 
        //      - Differenzen zum Sollzustand (NextState) herausfinden
        //      - Felder anzeigen, um Sollzustand zu erreichen
        protected virtual async Task ResolveDifferences()
        {
            HasDifferences = true;
            DetermineDifferences();

            CancelJobsTokenSource.Dispose();
            CancelJobsTokenSource = new CancellationTokenSource();
            try
            {
                await Task.Run(() =>
                {
                    while (HasDifferences)
                    {
                        // Task abbrechen, wenn Spiel beendet wird
                        if (CancelJobsTokenSource.Token.IsCancellationRequested)
                        {
                            CancelJobsTokenSource.Token.ThrowIfCancellationRequested();
                        }
                    }
                }, CancelJobsTokenSource.Token);

                ConfirmNextState();

            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        protected abstract bool DetermineDifferences();

        /// <summary>
        /// Bestätigt, dass der nächste Status erreicht ist
        /// </summary>
        protected void ConfirmNextState()
        {

            // NextState wurde erreicht
            Game.ConfirmNextState();
            ValidateMoveCommand.CanExecute();
            ValidateMoveCommand.RaiseCanExecuteChanged();
        }


        protected bool CanValidateMove()
        {
            if (Game == null)
                return false;
            if (Game.CurrentState == null)
                return false;

            return Game.CurrentState.Self == Game.HumanPlayer;
        }


        /// <summary>
        /// Enqueues all Jobs of the current state.
        /// </summary>
        /// <returns>The JobCount and whether the jobs succeeded</returns>
        protected virtual async Task<bool> ExecuteJobs()
        {
            CancelJobsTokenSource.Dispose();
            CancelJobsTokenSource = new CancellationTokenSource();

            var commands = Game.NextState.PortalCommands;
            var plcJobs = CreatePlcJobs(commands);
            var jobsExecuted = await PlcCom.ExecuteJobs(plcJobs, CancelJobsTokenSource.Token);

            if (jobsExecuted)
            {
                // Alle Jobs wurden ausgeführt -> NextState wurde erreicht
                ConfirmNextState();
            }

            return jobsExecuted;
        }

        protected virtual IEnumerable<PlcJob> CreatePlcJobs(IEnumerable<PortalCommand> portalCommands)
        {

            List<PlcJob> jobs = new();
            foreach (var command in portalCommands)
            {
                // color
                var isblack = command.Player == Player.Black;

                // new position
                var (newx, newy) = Game.NextState.Board.ValidPositions[command.NewPos];
                var (endX, endY) = CameraService.PixelsToPosition(newx, newy);

                // old position
                (double startX, double startY) = (0, 0);
                var stone = Stones.Where(st => st.FieldPosition.X == command.OldPos.X && st.FieldPosition.Y == command.OldPos.Y).FirstOrDefault();
                if (stone != null)
                {
                    var oldx = stone.X;
                    var oldy = stone.Y;
                    (startX, startY) = CameraService.PixelsToPosition(oldx, oldy);
                }
                else
                {
                    var (oldx, oldy) = Game.NextState.Board.ValidPositions[command.OldPos];
                    (startX, startY) = CameraService.PixelsToPosition(oldx, oldy);
                }

                jobs.Add(new PlcJob(command.MoveType, startX, startY, endX, endY, isblack));
            }
            return jobs;
        }

        #endregion

        #region Camera Functions

        /// <summary>
        /// Reads the stones with the camera
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ReadStones()
        {
            NavigationService.NavigateToBoard("CameraReferenceView");

            var stones = await Task.Run(() =>
            {
                while (NavigationService.ActualBoardName != "CameraReferenceView")
                {
                    Thread.Sleep(100);
                }

                Dictionary<FieldPosition, (double x, double y)> validPositions = null;
                if (Game.NextState != null)
                    validPositions = Game.NextState.Board.ValidPositions;
                else if (Game.CurrentState != null)
                    validPositions = Game.CurrentState.Board.ValidPositions;

                return CameraService.ReadStonePositions(validPositions, (uint)Tolerance);
            });

            NavigationService.NavigateToBoard(Game.ActualView.board);

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
        /// Detects the stones on the field and sends jobs to clear them back into the stone storage.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<bool> ClearField()
        {
            await Application.Current.Dispatcher.InvokeAsync(TriggerCameraAsync);
            if (Stones == null)
                return true;

            var jobs = new List<PlcJob>();
            foreach (var stone in Stones)
            {
                var (x, y) = CameraService.PixelsToPosition(stone.X, stone.Y);
                var isBlack = stone.Player == Player.Black;
                jobs.Add(new PlcJob(MoveType.RemoveStone, x, y, x, y, isBlack));
            }

            bool jobsSucceeded = false;
            if (jobs.Count > 0)
            {

                CancelJobsTokenSource.Dispose();
                CancelJobsTokenSource = new CancellationTokenSource();
                jobsSucceeded = await PlcCom.ExecuteJobs(jobs, CancelJobsTokenSource.Token, true);
            }

            return jobsSucceeded;
        }

        protected virtual async Task TriggerCameraAsync()
        {
            // Wenn Kamera oder Portal aktiv sind, nichts machen
            if (_reading && PlcCom.JobsExecuting)
                return;
            Message = null;


            _reading = true;
            await ReadStones();
            _reading = false;

            // Wenn Spiel in ungültigem Zustand ist.
            if (HasDifferences)
            {
                DetermineDifferences();
            }
        }


        protected abstract int CalculateTolerance();
        #endregion
    }
}
