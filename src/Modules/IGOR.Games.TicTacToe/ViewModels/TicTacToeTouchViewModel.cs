using IGOR.Common;
using IGOR.Games.TicTacToe.Views;
using IGOR.Interface.Game;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Serilog;
using System.Threading.Tasks;
using System.Windows;

namespace IGOR.Games.TicTacToe.ViewModels
{
    public class TicTacToeTouchViewModel : GameTouchViewModel<TicTacToeGame>
    {  

        private bool _canMakeMove;
        public bool CanMakeMove
        {
            get => _canMakeMove;
            set => SetProperty(ref _canMakeMove, value);
        } 

        public TicTacToeTouchViewModel(IDialogService dialogService, INavigationService navigationService, ILogger logger, IGameService gameService) : base(dialogService, navigationService, logger, gameService)
        { 
            _boardState = new Player[3][];
            for (int i = 0; i < 3; i++)
            {
                _boardState[i] = new Player[3];
            }
        }

        protected override async Task<bool> ExecutePlayerMove(FieldPosition targetPosition)
        {
            Game.SetField(targetPosition, null, null);
            await UpdateBoardState();
            Game.ConfirmNextState();
            return true;
        }
    }

}
