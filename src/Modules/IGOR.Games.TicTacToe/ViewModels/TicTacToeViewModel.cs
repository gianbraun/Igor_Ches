//using IGOR.Games.TicTacToe.Views;
//using IGOR.Interface.Camera;
//using IGOR.Interface.Communication;
//using IGOR.Interface.Communication.Job;
//using IGOR.Interface.Game;
//using IGOR.Service.Navigation.Interface;
//using Prism.Events;
//using Prism.Regions;
//using Prism.Services.Dialogs;
//using Serilog;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;

//namespace IGOR.Games.TicTacToe.ViewModels
//{
//    public class TicTacToeViewModel : TicTacToeTouchViewModel
//    {
//        private readonly IPlcCom _plcCom;
//        private readonly ICameraService _cameraService;
//        private readonly ILogger _log;
//        private bool _jobsSended;
//        private bool _playerMove;

//        private IEnumerable<Stone> _camStonePos = new List<Stone>();
//        public IEnumerable<Stone> CamStonePos
//        {
//            get => _camStonePos;
//            set => SetProperty(ref _camStonePos, value);
//        }

//        private IEnumerable<Stone> _touchStones = new List<Stone>();

//        public IEnumerable<Stone> TouchStones
//        {
//            get => _touchStones;
//            set => SetProperty(ref _touchStones, value);
//        }

//        public TicTacToeViewModel(IDialogService dialogService, INavigationService navigationService, IPlcCom plcCom, ICameraService cameraService, IEventAggregator eventAggregator, ILogger logger)
//            : base(dialogService, navigationService)
//        {
//            _plcCom = plcCom;
//            _cameraService = cameraService;
//            _log = logger.ForContext<TicTacToeViewModel>();
//        }

//        private void StonesDetected(IEnumerable<(double x, double y)> whiteStones, IEnumerable<(double x, double y)> blackStones)
//        {
//            Application.Current.Dispatcher.Invoke(() => NavigationService.NavigateToBoard("TicTacToeBoardView"));

//            if (whiteStones is null || blackStones is null)
//            {
//                if (_playerMove)
//                {
//                    _playerMove = false;
//                    Game.MakeAiMove();
//                    UpdateBoardState();
//                    Application.Current.Dispatcher.Invoke(CheckGameOver);
//                }

//                _log.Error("Die Kamera konnte keine Spielsteine erkennen");
//                return;
//            }

//            var stones = new List<Stone>();
//            foreach (var stone in whiteStones)
//            {
//                var pos = StonePosToFieldPos(stone.x, stone.y);
//                (var x, var y) = _cameraService.PixelsToPosition(stone.x, stone.y);
//                stones.Add(new Stone(x, y, Player.White, pos));
//            }

//            foreach (var stone in blackStones)
//            {
//                var pos = StonePosToFieldPos(stone.x, stone.y);
//                (var x, var y) = _cameraService.PixelsToPosition(stone.x, stone.y);
//                stones.Add(new Stone(x, y, Player.Black, pos));
//            }
//            CamStonePos = stones;
//            if (_playerMove)
//            {
//                _playerMove = false;
//                Game.MakeAiMove();
//                UpdateBoardState();
//                Application.Current.Dispatcher.Invoke(CheckGameOver);
//            }

//        }

//        protected virtual IEnumerable<PlcJob> CreatePlcJobs(IEnumerable<PortalCommand> portalCommands)
//        {
//            var isblack = Game.CurrentState.Self != Player.Black;
//            List<PlcJob> jobs = new();
//            foreach (var command in portalCommands)
//            {
//                var (oldx, oldy) = TicTacToeBoardView.ValidPositions[command.OldPos];
//                (double newx, double newy) = TicTacToeBoardView.ValidPositions[command.NewPos];
//                (var startX, var startY) = _cameraService.PixelsToPosition(oldx, oldy);
//                (var endX, var endY) = _cameraService.PixelsToPosition(newx, newy);
//                jobs.Add(new PlcJob(command.MoveType, startX, startY, endX, endY, isblack));
//            }
//            return jobs;
//        }

//        protected virtual void SendJob()
//        {
//            var stones = new List<Stone>(TouchStones);
//            var plcJobs = CreatePlcJobs(Game.CurrentState.PortalCommands);
//            var i = 0;
//            foreach (var job in plcJobs)
//            {
//                var player = job.IsBlack ? Player.Black : Player.White;
//                var stone = new Stone(job.EndPosX, job.EndPosY, player, Game.CurrentState.PortalCommands[i].NewPos);
//                stones.Add(stone);
//                i++;
//            }
//            TouchStones = stones.OrderBy(s => s.FieldPosition.X).ThenBy(s => s.FieldPosition.Y);
//            _plcCom.EnqueueJobs(plcJobs);
//            _jobsSended = true;
//        }

//        protected override void MakeMove(FieldPosition fieldPosition)
//        {
//            Game.SetField(fieldPosition, null, null);
//            _playerMove = true;
//            UpdateBoardState();
//        }

//        protected override void UpdateBoardState()
//        {
//            base.UpdateBoardState();
//            SendJob();
//        }

//        private FieldPosition StonePosToFieldPos(double x, double y)
//        {
//            var delta = 25;
//            foreach ((var pos, (var vx, var vy)) in TicTacToeBoardView.ValidPositions)
//            {
//                if (Math.Abs(x - vx) < delta && Math.Abs(y - vy) < delta)
//                {
//                    return pos;
//                }
//            }
//            return null;
//        }

//        private void ClearField()
//        {
//            var jobs = new List<PlcJob>();
//            for (int y = 0; y < Game.CurrentBoard.Size; y++)
//            {
//                for (int x = 0; x < Game.CurrentBoard.Size; x++)
//                {
//                    (var bx, var by) = TicTacToeBoardView.ValidPositions[new FieldPosition(x, y)];
//                    var pos = _cameraService.PixelsToPosition(bx, by);
//                    switch (Game.CurrentBoard[x, y])
//                    {
//                        case Player.None:
//                            continue;
//                        case Player.Black:

//                            jobs.Add(new PlcJob(MoveType.RemoveStone, pos.x, pos.y, pos.x, pos.y, true));
//                            break;
//                        case Player.White:
//                            jobs.Add(new PlcJob(MoveType.RemoveStone, pos.x, pos.y, pos.x, pos.y, false));
//                            break;
//                        default:
//                            break;
//                    }
//                }
//            }
//            _plcCom.EnqueueJobs(jobs);
//        }

//        private void PortalBusyChanged(bool portalBusy)
//        {
//            CanMakeMove = !portalBusy;

//            if (_jobsSended && !portalBusy)
//            {
//                _jobsSended = false;
//                Application.Current.Dispatcher.Invoke(() =>
//                    NavigationService.NavigateToBoard("CameraReferenceView", async r =>
//                    {
//                        if (r.Result != true)
//                            return;
//                        var stones = await Task.Run(() =>
//                        {
//                            Thread.Sleep(500);
//                            return _cameraService.ReadStonePositions(Game.CurrentBoard.ValidPositions, 10);
//                        });

//                        if (stones != null)
//                        {
//                            var sortedstones = stones.Select(s =>
//                            {
//                                var (x, y) = _cameraService.PixelsToPosition(s.X, s.Y);
//                                return new Stone(x, y, s.Player, s.FieldPosition);
//                            })
//                            .OrderBy(s => s.FieldPosition.X)
//                            .ThenBy(s => s.FieldPosition.Y);
//                            Application.Current.Dispatcher.Invoke(() => CamStonePos = sortedstones);
//                        }



//                        Application.Current.Dispatcher.Invoke(() => NavigationService.NavigateToBoard("TicTacToeBoardView"));
//                    }));
//            }
//        }
//        #region Navigation

//        public override void OnNavigatedFrom(NavigationContext navigationContext)
//        {
//            _plcCom.RemoveObserver("HMI.bPortalbusy");
//            ClearField();
//        }

//        public override void OnNavigatedTo(NavigationContext navigationContext)
//        {
//            TouchStones = new List<Stone>();
//            CamStonePos = new List<Stone>();
//            Game = (TicTacToeGame)navigationContext.Parameters["game"];
//            if (TicTacToeBoardView.ValidPositions.Count == 0)
//            {
//                TicTacToeBoardView.BoardReady += GameReady;
//            }
//            else
//            {
//                GameReady(null, null);
//            }

//            _plcCom.TryObserve<bool>("HMI.bPortalbusy", PortalBusyChanged);
//            _plcCom.TryRead<bool>("HMI.bPortalbusy", out var busy);
//            PortalBusyChanged(busy);
//        }

//        private void GameReady(object sender, System.EventArgs e)
//        {
//            TicTacToeBoardView.BoardReady -= GameReady;
//            Game.Start();
//            if (Game.HumanPlayer == Player.White)
//            {
//                Game.MakeAiMove();
//            }
//            UpdateBoardState();
//        }


//        #endregion

//    }
//}
