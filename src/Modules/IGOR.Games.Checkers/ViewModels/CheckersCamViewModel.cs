using IGOR.Games.Checkers.Views;
using IGOR.Interface.Camera;
using IGOR.Interface.Communication;
using IGOR.Interface.Game;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace IGOR.Games.Checkers.ViewModels
{
    public class CheckersCamViewModel : GameCamViewModel<CheckersGame>
    {

        private double _centerFieldX;
        public double CenterFieldX
        {
            get => _centerFieldX;
            set => SetProperty(ref _centerFieldX, value);
        }

        private double _centerFieldY;

        public double CenterFieldY
        {
            get => _centerFieldY;
            set => SetProperty(ref _centerFieldY, value);
        }
        public ObservableCollection<System.Drawing.Point> Kings { get; } = new ObservableCollection<System.Drawing.Point>();


        public CheckersCamViewModel(ICameraService cameraService, IPlcCom plcCom, IDialogService dialogService, INavigationService navigationService, ILogger log, IGameService gameService)
            : base(cameraService, plcCom, dialogService, navigationService, log, gameService)
        {
            CenterFieldX = CameraService.ScreenWidth / 2;
            CenterFieldY = CameraService.ScreenHeight / 2;
        }

        protected override int CalculateTolerance()
        {
            // set field tolerance 
            return (CheckersBoardView.FieldSize - StoneSize) / 2;
        }
        protected override bool DetermineDifferences()
        {
            var nextBoard = Game.NextState.Board; 

            // Differences zurücksetzen
            Application.Current.Dispatcher.Invoke(() => Differences.Clear());
            Application.Current.Dispatcher.Invoke(() => CheckersBoardView.Differences.Clear());
            var hasDifferences = false;

            for (int y = 0; y < nextBoard.Size; y++)
            {
                for (int x = 0; x < nextBoard.Size; x++)
                {
                    var desiredPlayer = nextBoard[x, y]; 
                    var assoziatedStone = Stones.Where(st => st.FieldPosition != null && st.FieldPosition.X == x && st.FieldPosition.Y == y).FirstOrDefault();
                    var currentPlayer = assoziatedStone == null ? Player.None : assoziatedStone.Player;

                    if (desiredPlayer != currentPlayer)
                    {
                        // Bei unterschiedlichen Spielern 
                        hasDifferences = true; 
                        var differenceFieldPos = nextBoard.ValidPositions[new FieldPosition(x, y)];
                        var differenceStonePos = differenceFieldPos;
                        if (assoziatedStone != null)
                            differenceStonePos = (assoziatedStone.X, assoziatedStone.Y);

                        var difference = new Difference(differenceStonePos.x, differenceStonePos.y, desiredPlayer, differenceFieldPos.x, differenceFieldPos.y);
                        Application.Current.Dispatcher.Invoke(() => Differences.Add(difference));
                        Application.Current.Dispatcher.Invoke(() => CheckersBoardView.Differences.Add(difference));
                    }
                }
            }

            HasDifferences = hasDifferences;
            return HasDifferences;
        }

        protected override void BoardReady(object sender, EventArgs e)
        {
            SetKings();
            base.BoardReady(sender, e);
        }

        protected override void EndGame()
        {
            SetKings();
            base.EndGame();
        }

        protected override async Task<bool> ExecuteAiMove()
        {
            var succeeded = await base.ExecuteAiMove();
            SetKings();
            return succeeded;
        }

        protected override bool ExecutePlayerMove()
        {
            var succeeded = base.ExecutePlayerMove();
            SetKings();
            return succeeded;
        } 

        private void SetKings()
        {
            Application.Current.Dispatcher.Invoke(() => CheckersBoardView.Kings.Clear());
            Application.Current.Dispatcher.Invoke(() => Kings.Clear());
            var kings = new List<FieldPosition>();

            if (Game.CurrentState == null || Game.CurrentState.Board == null)
                return;

            for (int y = 0; y < Game.CurrentState.Board.Size; y++)
            {
                for (int x = 0; x < Game.CurrentState.Board.Size; x++)
                {
                    var king = Game.CurrentState.Board.GetKing(y, x);
                    if (king == Player.None)
                        continue;

                    Application.Current.Dispatcher.Invoke(() => kings.Add(new FieldPosition(x, y)));
                    var (kx, ky) = CheckersBoardView.ValidPositions[new FieldPosition(x, y)];
                    Application.Current.Dispatcher.Invoke(() => Kings.Add(new System.Drawing.Point((int)kx, (int)ky)));
                }
            }

            Application.Current.Dispatcher.Invoke(() => CheckersBoardView.Kings.AddRange(kings));
        }

    }
}

