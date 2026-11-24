using IGOR.Common;
using IGOR.Interface.Game;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace IGOR.Games.Morris.ViewModels
{
    public class MorrisViewModel : GameTouchViewModel<MorrisGame>
    { 
        private FieldPosition _opponent; 
              

        private string _hint = "Gegenstein Wählen";
        public string Hint
        {
            get => _hint;
            set => SetProperty(ref _hint, value);
        }

        public MorrisViewModel(IDialogService dialogService, INavigationService navigationService, ILogger logger, IGameService gameService):base(dialogService, navigationService, logger, gameService)
        {  
            IsHilighted = new bool[7][];
            for (int i = 0; i < 7; i++)
            {
                IsHilighted[i] = new bool[7];
            }
            _boardState = new Player[3][];
            for (int i = 0; i < 3; i++)
            {
                _boardState[i] = new Player[3];
            }
        }

        protected override async Task<bool> ExecutePlayerMove(FieldPosition targetPosition)
        {

            Hint = "Gegenstein Wählen";

            if (Game.CurrentState.GamePhase == GamePhase.Placing)
            {
                _firstSelected = new FieldPosition(0, 0);
            } 

            if (_opponent == null && Game.CurrentState.PortalCommands.Count > 0)
            {
                SelectOpponent(targetPosition);
                Hint = "Eigene Pos wählen";
                return false;
            }

            if (_firstSelected == null && Game.CurrentState.GamePhase != GamePhase.Placing)
            {
                SelectCell(targetPosition);
                Hint = "Ziel Pos Wählen";
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

            if (_opponent != null)
            {
                IsHilighted[_opponent.X][_opponent.Y] = false;
                RaisePropertyChanged(nameof(IsHilighted));
            }

            _firstSelected = null;
            _opponent = null;
            await UpdateBoardState();
            Game.ConfirmNextState();
            return true;
        }
         

        private void SelectOpponent(FieldPosition pos)
        {
            var player = Game.CurrentBoard[pos.X, pos.Y];
            if (player == Game.CurrentState.Opponent)
            {
                _opponent = pos;
                IsHilighted[pos.X][pos.Y] = true;
                RaisePropertyChanged(nameof(IsHilighted));
            }
        }

        private bool ExecuteMove(FieldPosition pos)
        {
            if (Game.CurrentBoard[pos.X, pos.Y] == Player.None)
            {
                return Game.SetField(_firstSelected, pos, _opponent);
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


    }
}
