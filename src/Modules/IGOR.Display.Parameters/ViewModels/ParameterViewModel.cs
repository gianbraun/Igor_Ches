using IGOR.Common;
using IGOR.Interface.Parameters;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IGOR.Display.Parameters.ViewModels
{
    public class ParameterViewModel : BindableBase
    {
        private IParameterService _parameterService;
        private IDialogService _dialogService;

        public IReadOnlyCollection<ParameterGroup> ParameterGroups => _parameterService.ParameterGroups;

        public IEnumerable<Parameter> SelectedParameters { get; private set; }

        public bool IsParameterSelected { get; private set; }

        public bool NeedsUpload { get; private set; }

        public bool OutDated => ParameterGroups.Any(g => g.OutDated);

        public Parameter SelectedDataGridParameter { get; set; }
        public DelegateCommand<ParameterGroup> SelectedItemChangedCommand => new DelegateCommand<ParameterGroup>(SelectedItemChanged);

        public DelegateCommand SaveParametersCommand => new DelegateCommand(SaveParameters);

        public DelegateCommand EditParameterCommand => new DelegateCommand(EditParameter);


        public ParameterViewModel(IParameterService parameterService, IDialogService dialogService)
        {
            _parameterService = parameterService;
            _dialogService = dialogService;
        }

        private void SelectedItemChanged(ParameterGroup parameterGroup)
        {
            SelectedParameters = parameterGroup.Parameters;
        }

        private void EditParameter()
        {
            var parameters = new DialogParameters();
            parameters.Add("parameter", SelectedDataGridParameter);
            _dialogService.ShowDialog(DialogNames.EditParameters, parameters, r =>
            {
                if (r.Result != ButtonResult.OK)
                {
                    return;
                }
                var value = r.Parameters.GetValue<double>("value");
                UpdateParameter(value);
            });
            RaisePropertyChanged(nameof(OutDated));
        }

        private void UpdateParameter(double value)
        {
            if (SelectedDataGridParameter is PlcParameter plcParameter)
            {
                plcParameter.UpdatedValue = value;
                if (_parameterService.TryWriteParameterToPlc(plcParameter))
                {
                    SelectedDataGridParameter.OutDated = true;
                    _parameterService.ReadParameterFromPlc(plcParameter);
                }
            }
            else
            {
                SelectedDataGridParameter.OnlineValue = value;
            }
        }

        private void SaveParameters()
        {

            _parameterService.SaveAllOutDateParameters();
            RaisePropertyChanged(nameof(OutDated));

        }

        private void OnSelectedDataGridParameterChanged(object _, object newValue)
        {
            if (SelectedDataGridParameter is null)
            {
                IsParameterSelected = false;
                return;
            }
            IsParameterSelected = true;
        }
    }
}
