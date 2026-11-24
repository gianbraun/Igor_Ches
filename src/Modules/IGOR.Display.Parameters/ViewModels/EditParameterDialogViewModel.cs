using IGOR.Interface.Parameters;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGOR.Display.Parameters.ViewModels
{
    internal class EditParameterDialogViewModel : BindableBase, IDialogAware
    {

        private double _minValue;
        private double _maxValue;
        private double _value;

        private string _title;
        public string Title
        {
            get => _title;
            private set => SetProperty(ref _title, value);
        }

        public event Action<IDialogResult> RequestClose;

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private string _inputValue;
        public string InputValue
        {
            get => _inputValue;
            set => ValidateValue(value);

        }

        private bool _isValid;
        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        private void ValidateValue(string valueString)
        {
            IsValid = true;

            if (!double.TryParse(valueString, out var value))
            {
                ErrorMessage = "Die Zeichenkette entspricht nicht einer Fließkommer zahl";
                _value = double.NaN;
                IsValid = false;
            }

            if (value < _minValue || value > _maxValue)
            {
                IsValid = false;
                ErrorMessage = $"Der Wert muss zwischen {_minValue} und {_maxValue} liegen";
            }
            if (IsValid)
            {
                ErrorMessage = "";
            }
            _value = value;
            SetProperty(ref _inputValue, valueString);
            return;
        }

        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand => _closeDialogCommand ??= new DelegateCommand<string>(CloseDialog);

        private void CloseDialog(string stringValue)
        {
            if (!bool.TryParse(stringValue, out var value))
            {
                return;
            }
            var result = new DialogResult(ButtonResult.Cancel);
            if (value)
            {
                result = new DialogResult(ButtonResult.OK);
                result.Parameters.Add("value", _value);
            }

            RequestClose?.Invoke(result);
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
            var parameter = parameters.GetValue<Parameter>("parameter");
            _minValue = parameter.Min ?? double.NegativeInfinity;
            _maxValue = parameter.Max ?? double.PositiveInfinity;
            Title = $"Parameter \"{parameter.Name}\" bearbeiten";
        }
    }
}
