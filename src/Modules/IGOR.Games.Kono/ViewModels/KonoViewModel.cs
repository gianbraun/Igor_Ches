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

namespace IGOR.Games.Kono.ViewModels
{
    public class KonoViewModel : GameTouchViewModel<KonoGame>
    { 

        public KonoViewModel(IDialogService dialogService, INavigationService navigationService, ILogger logger, IGameService gameService) : base(dialogService, navigationService, logger, gameService)
        {

            IsHilighted = new bool[5][];
            for (int i = 0; i < 5; i++)
            {
                IsHilighted[i] = new bool[5];
            }
            _boardState = new Player[5][];
            for (int i = 0; i < 5; i++)
            {
                _boardState[i] = new Player[5];
            }
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

        private bool ExecuteMove(FieldPosition pos)
        {
            if (Game.CurrentState.GetPlayer(pos.Y, pos.X) == Player.None)
            {
                return Game.SetField(_firstSelected, pos, default);
            }

            return false;
        }

        private void SelectCell(FieldPosition pos)
        {
            var player = Game.CurrentBoard[pos.X, pos.Y];
            if (player == Game.HumanPlayer)
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

    }
}
