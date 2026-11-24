using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IGOR.Interface.Game
{
    public abstract class GameTouchViewModel<TGame> : GameViewModel<TGame>
        where TGame : class, IGame
    {

        protected FieldPosition _firstSelected;

        public DelegateCommand<FieldPosition> MakeMoveCommand { get; }


        protected Player[][] _boardState;
        public Player[][] BoardState
        {
            get => _boardState;
            protected set => SetProperty(ref _boardState, value);
        }

        public bool[][] IsHilighted { get; protected set; }

        protected GameTouchViewModel(IDialogService dialogService, INavigationService navigationService, ILogger log, IGameService gameService) : base(dialogService, navigationService, log, gameService)
        {
            MakeMoveCommand = new DelegateCommand<FieldPosition>(async (fp) => await MakeNextMoveAsync(fp));
        }

        protected async virtual Task MakeNextMoveAsync(FieldPosition targetPosition)
        {
            // Player move ausführen und auf Ausführung (Portal) warten
            if (!await ExecutePlayerMove(targetPosition))
            {
                // TODO: was machen, wenn Player Move nicht ausgeführt werden konnte?
                return;
            }

            // Check, if game is over
            if (CheckGameOver())
                return;

            // If not game over, execute the next ai move
            if (!await ExecuteAiMove())
            {
                // kann immer ausgeführt werden, deshalb nicht unterbrechen
                //return;
            }

            if (CheckGameOver())
                return;
        }


        /// <summary>
        /// Executes a Move by the Player
        /// </summary>
        /// <param name="targetPosition">target position</param>
        /// <returns>success-state</returns>
        protected abstract Task<bool> ExecutePlayerMove(FieldPosition targetPosition);


        #region Implementations

        protected override async Task<bool> ExecuteAiMove()
        {
            Game.MakeAiMove();
            if (await UpdateBoardState())
            {
                // confirm state if board state was updated successfully
                Game.ConfirmNextState();
                return true;
            }
            return false;

        }

        protected override async Task StartGame(TGame newGame)
        {
            Game = newGame;
            Game.Start();
            if (await UpdateBoardState())
                Game.ConfirmNextState();

            if (Game.HumanPlayer == Player.Black)
            {
                await ExecuteAiMove();
            }
        }

        protected override void EndGame()
        {
            base.EndGame();
            _ = UpdateBoardState();
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Updates the Board to the next state
        /// </summary>
        /// <returns></returns>
        protected async Task<bool> UpdateBoardState()
        {
            // Für Opponent das Setzen des Steins simulieren
            if (Game.CurrentState != default && Game.NextState.Self == Game.HumanPlayer && !Game.CurrentState.IsGameOver)
            {
                await Task.Delay(500);
            }

            BoardState = (Player[][])(Game.NextState.Board.Field);
            return true;
        }


        #endregion
    }
}
