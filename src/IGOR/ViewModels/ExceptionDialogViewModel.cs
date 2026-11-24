using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.ViewModels
{
    public class ExceptionDialogViewModel : BindableBase, IDialogAware
    {
        public Exception Exception { get; set; }

        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand => _closeCommand 
            ??= new DelegateCommand(() => RequestClose?.Invoke(new DialogResult()));

        public string Title => "Ein unerwarteter Fehler ist aufgetreten";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Exception = parameters.GetValue<Exception>("excpetion");
        }
    }
}
