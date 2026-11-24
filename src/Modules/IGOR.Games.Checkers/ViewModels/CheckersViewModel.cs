using IGOR.Common;
using IGOR.Interface.Game;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Serilog;
using System.Threading.Tasks;
using System.Windows;

namespace IGOR.Games.Checkers.ViewModels
{
    public class CheckersViewModel : GameTouchViewModel<CheckersGame>
    {

        // TODO: Umbenennen zu CheckersTouchViewModel
        public CheckersViewModel(IDialogService dialogService, INavigationService navigationService, ILogger log, IGameService gameService) : base(dialogService, navigationService, log, gameService)
        { 
            IsHilighted = new bool[8][];
            for (int i = 0; i < 8; i++)
            {
                IsHilighted[i] = new bool[8];
            }
            _boardState = new Player[8][];
            for (int i = 0; i < 8; i++)
            {
                _boardState[i] = new Player[8];
            }
        }


        private bool ExecuteMove(FieldPosition pos)
        {
            if (Game.CurrentState.Board[pos.X, pos.Y] == Player.None)
            {
                return Game.SetField(_firstSelected, pos, default);
            }

            return false;
        }

        private void SelectCell(FieldPosition pos)
        {
            var player = Game.CurrentBoard[pos.X, pos.Y];
            if (player == Game.CurrentState.Self)
            {
                _firstSelected = pos;
                IsHilighted[pos.X][pos.Y] = true;
                RaisePropertyChanged(nameof(IsHilighted));
                return;
            }

            if (_firstSelected != null)
            {
                IsHilighted[_firstSelected.X][_firstSelected.Y] = false;
                RaisePropertyChanged(nameof(IsHilighted));
            }

            _firstSelected = null;

        }

        protected override async Task<bool> ExecutePlayerMove(FieldPosition targetPosition)
        {
            if (_firstSelected == null)
            {
                SelectCell(targetPosition);
                return false;
            }


            if (!ExecuteMove(targetPosition))
            {
                if (_firstSelected != null)
                {
                    IsHilighted[_firstSelected.X][_firstSelected.Y] = false;
                    RaisePropertyChanged(nameof(IsHilighted));
                }
                _firstSelected = null;
                return false;
            }

            if (_firstSelected != null)
            {
                IsHilighted[_firstSelected.X][_firstSelected.Y] = false;
                RaisePropertyChanged(nameof(IsHilighted));
            }

            _firstSelected = null;
            await UpdateBoardState();
            Game.ConfirmNextState();
            return true;
        } 
         
    }
}
