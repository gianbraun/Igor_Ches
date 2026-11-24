using IGOR.Common;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
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
    public abstract class GameViewModel<TGame> : BindableBase, INavigationAware
        where TGame : class, IGame
    {
        protected readonly IDialogService DialogService;
        protected readonly INavigationService NavigationService;
        protected readonly ILogger Log;
        protected readonly IGameService GameService;

        protected TGame _game;
        public TGame Game
        {
            get => _game;
            set => SetProperty(ref _game, value, nameof(Game));
        }

        private DelegateCommand _openGameRulesCommand;
        public DelegateCommand OpenGameRulesCommand => _openGameRulesCommand ??= new DelegateCommand(OpenGameRules);

        protected GameViewModel(IDialogService dialogService, INavigationService navigationService, ILogger log, IGameService gameService)
        {
            DialogService = dialogService;
            NavigationService = navigationService;
            Log = log;
            GameService = gameService; 
        }


        #region Methods 


        /// <summary>
        /// Executes a Move by the ai
        /// </summary> 
        /// <returns>success-state</returns>
        protected abstract Task<bool> ExecuteAiMove();


        /// <summary>
        /// Checks wether the game is over.
        /// In case the game is over an endgame dialog is displayed.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckGameOver()
        {
            // TODO: IsGameOver wird nicht gesetzt
            if (!Game.CurrentState.IsGameOver)
            {
                return false;
            }
            var winner = Game.CurrentState.GetWinner();

            Game.RaiseGameOver(winner);
             
            var message = "Schade, du hast leider verloren.";
            var parameters = new DialogParameters();
            if (winner == Game.HumanPlayer)
                message = "Herzlichen Glückwunsch, du hast gewonnen!";
            else if (winner == Player.None)
                message = "Unentschieden";

            parameters.Add(DialogGameParameterNames.Message, message);
            OpenStartGameDialog(parameters);

            return true;
        }

        /// <summary>
        /// Opens a dialog to set parameters and start the game.
        /// </summary>
        public void OpenStartGameDialog(DialogParameters parameters = null)
        { 
            if(parameters == null)
                parameters = new DialogParameters();
            parameters.Add(DialogGameParameterNames.Game, Game);
            parameters.Add(DialogGameParameterNames.GameOver, Game.IsGameOver);

            DialogService.ShowDialog(DialogNames.GameParameters, parameters, r =>
            {
                var difficulty = r.Parameters.GetValue<Difficulty>(DialogGameParameterNames.Difficulty);
                var player = r.Parameters.GetValue<Player>(DialogGameParameterNames.SelectedPlayer);
                var key = r.Parameters.GetValue<string>(DialogGameParameterNames.SelectedBoardKey);
                if (r.Result == ButtonResult.Cancel)
                {
                    NavigationService.NavigateTo("HomeView");
                }
                else
                {
                    // End game, damit ein neues gestartet wird
                    Game.End();
                    Game.Difficulty = difficulty;
                    Game.HumanPlayer = player;
                    NavigationService.NavigateTo(Game.Views[key].hmi, ("game", Game));
                    NavigationService.NavigateToBoard(Game.Views[key].board);
                }
            });
        }

        /// <summary>
        /// Starts a new Game
        /// </summary>
        protected abstract Task StartGame(TGame newGame);

        /// <summary>
        /// Ends the current game immediately
        /// </summary>
        protected virtual void EndGame()
        {
            Game.End();
        }

        #endregion

        #region INavigationAware implementation

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // TODO: Entscheiden, wann Spiel beendet werden soll.
            var endGame = false;
            if(navigationContext.Uri.ToString() == "HomeView")
            {
                endGame = true;
            }

            if (endGame)
                EndGame();
        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            // neues spiel starten, wenn kein bisheriges läuft
            if (Game == null || (Game.IsGameOver.HasValue && Game.IsGameOver.Value))
            {
                if (navigationContext.Parameters["game"] is TGame newGame)
                {
                    StartGame(newGame); 
                }
            }
             
            // aktuelles spiel im gameservice setzen
            GameService.CurrentGame = Game;
        }
        #endregion

        #region Command implementation

        protected virtual void OpenGameRules()
        {
            var parameters = new DialogParameters();
            parameters.Add(DialogGameRuleParameters.FilePath, Game.GameRulePath);
            DialogService.ShowDialog(DialogNames.GameRules, parameters, null);
        }
        #endregion
    }
}
