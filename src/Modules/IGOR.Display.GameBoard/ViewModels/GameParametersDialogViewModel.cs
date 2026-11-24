using IGOR.Interface.Game;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IGOR.Display.GameBoard.ViewModels
{
    public class GameParametersDialogViewModel : BindableBase, IDialogAware
    {
        private IGame _game;

        public IGame Game
        {
            get => _game;
            set => SetProperty(ref _game, value);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public event Action<IDialogResult> RequestClose;

        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand => _closeDialogCommand ??= new DelegateCommand<string>(CloseDialog);

        private int _playerIndex;
        public int PlayerIndex
        {
            get => _playerIndex;
            set => SetProperty(ref _playerIndex, value);
        }

        private int _difficultyIndex;
        public int DifficultyIndex
        {
            get => _difficultyIndex;
            set => SetProperty(ref _difficultyIndex, value);
        }

        private string _selectedKey;
        public string SelectedKey 
        {
            get => _selectedKey;
            set => SetProperty(ref _selectedKey, value);
        }

        private string message;
        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        private string okButtonText;
        public string OkButtonText
        {
            get => okButtonText;
            set => SetProperty(ref okButtonText, value);
        }

        private string cancleButtonText;
        public string CancleButtonText
        {
            get => cancleButtonText;
            set => SetProperty(ref cancleButtonText, value);
        }

        private void CloseDialog(string parameter)
        {
            var result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "false")
                result = ButtonResult.Cancel;


            var parameters = new DialogParameters();
            parameters.Add(DialogGameParameterNames.SelectedPlayer, (Player)PlayerIndex);
            parameters.Add(DialogGameParameterNames.Difficulty, (Difficulty)DifficultyIndex);
            parameters.Add(DialogGameParameterNames.SelectedBoardKey, SelectedKey);
            Game.ActualView = Game.Views[SelectedKey];

            var dialogResult = new DialogResult(result, parameters);
            RequestClose?.Invoke(dialogResult);
        }


        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Game = parameters.GetValue<IGame>(DialogGameParameterNames.Game);
            DifficultyIndex = (int)Game.Difficulty;
            if (Game.HumanPlayer == Player.None)
                Game.HumanPlayer = Player.White;
            PlayerIndex = (int)Game.HumanPlayer;

            if (string.IsNullOrWhiteSpace(Game.ActualView.hmi))
                Game.ActualView = Game.Views.Values.First();

            SelectedKey = Game.Views.First((k) => k.Value.board == Game.ActualView.board).Key;

            var gameOver = parameters.GetValue<bool>(DialogGameParameterNames.GameOver);
            if (gameOver)
            {
                OkButtonText = "Neustart";
                CancleButtonText = "Beenden";
                Title = "Spiel zu Ende";
                Message = parameters.GetValue<string>(DialogGameParameterNames.Message);
            }
            else
            {
                OkButtonText = "Start";
                CancleButtonText = "Abbrechen";
                Title = "Neues Spiel Starten";
                Message = null;
            }

        }


    }
}
