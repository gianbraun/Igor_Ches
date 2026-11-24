using IGOR.Interface.Game;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.IO;
using System.Text;

namespace IGOR.Display.GameBoard.ViewModels
{
    public class GameRulesDialogViewModel : BindableBase, IDialogAware
    {

        public string Title => "Spielregeln";

        public event Action<IDialogResult> RequestClose;

        private DelegateCommand _closeDialogCommand;
        public DelegateCommand CloseDialogCommand => _closeDialogCommand ??= new DelegateCommand(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)));

        private string _content;
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
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
            var path = parameters.GetValue<string>(DialogGameRuleParameters.FilePath);
            Content = File.ReadAllText(path, Encoding.UTF8);
        }

    }
}
