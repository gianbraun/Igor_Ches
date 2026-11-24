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

namespace IGOR.Interface.Game
{
    public abstract class GameCamViewModel<TGame> : GameViewModel<TGame>
        where TGame : class, IGame
    {
        protected readonly ICameraService CameraService;
        protected readonly IPlcCom PlcCom;


        protected bool _first = true;
        protected bool _reading;
        public const int StoneSize = 90;
        public int Tolerance { get; set; } = 10;

        public ObservableCollection<Stone> Stones { get; set; } = new ObservableCollection<Stone>();


        private DelegateCommand _triggerCamCommand;
        public DelegateCommand TriggerCamCommand => _triggerCamCommand ??= new DelegateCommand(async () =>
        {
            await Application.Current.Dispatcher.InvokeAsync(TriggerCameraAsync);
        });


        private DelegateCommand _validateMoveCommand;
        public DelegateCommand ValidateMoveCommand => _validateMoveCommand ??= new DelegateCommand(ValidateMove, CanValidateMove);


        protected GameCamViewModel(ICameraService cameraService, IPlcCom plcCom, IDialogService dialogService, INavigationService navigationService, ILogger log, IGameService gameService) : base(dialogService, navigationService, log, gameServcie)
        {
            CameraService = cameraService;
            PlcCom = plcCom;

            PlcCom.PlcJobDone += (s, e) => OnPlcJobDone(e);
        }

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

        #region Camera Functions

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

         
        #endregion
    }
}
