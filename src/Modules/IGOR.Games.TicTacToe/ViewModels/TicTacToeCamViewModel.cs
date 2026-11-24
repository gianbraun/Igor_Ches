using IGOR.Games.TicTacToe.Views;
using IGOR.Interface.Camera;
using IGOR.Interface.Communication;
using IGOR.Interface.Game;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using Serilog;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IGOR.Games.TicTacToe.ViewModels
{
    public class TicTacToeCamViewModel : GameCamViewModel<TicTacToeGame>
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

        public TicTacToeCamViewModel(ICameraService cameraService, IPlcCom plcCom, IDialogService dialogService, ILogger logger, INavigationService navigationService, IGameService gameService)
            : base(cameraService, plcCom, dialogService, navigationService, logger.ForContext<TicTacToeCamViewModel>(), gameService)
        {

            CenterFieldX = CameraService.ScreenWidth / 2;
            CenterFieldY = CameraService.ScreenHeight / 2;
        }

        protected override int CalculateTolerance()
        {
            return (TicTacToeBoardView.FieldSize - StoneSize) / 2;
        }

        protected override bool DetermineDifferences()
        {
            var nextBoard = Game.NextState.Board; 

            // Differences zurücksetzen
            Application.Current.Dispatcher.Invoke(() => Differences.Clear());
            Application.Current.Dispatcher.Invoke(() => TicTacToeBoardView.Differences.Clear());
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
                        Application.Current.Dispatcher.Invoke(() => TicTacToeBoardView.Differences.Add(difference));
                    }
                }
            }

            HasDifferences = hasDifferences;
            return HasDifferences;
        }
    }
}
