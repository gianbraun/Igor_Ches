using IGOR.Common.WPF.Controls;
using IGOR.Interface.Communication;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using System.Windows.Input;

namespace IGOR.Display.ControlPanel.ViewModels
{
    internal class ControlPanelViewModel : BindableBase
    {
        private readonly string ControlOffSoftKey = "stSoftKeyCtrlOff";
        private readonly string ControlOnSoftKey = "stSoftKeyCtrlOn";
        private readonly string AutomaticOffSoftKey = "stSoftKeyAutoOff";
        private readonly string AutomaticOnSoftKey = "stSoftKeyAutoOn";
        private readonly string AcknowledgeSoftKey = "stSoftKeyQuit";
        private readonly ILogger _log;


        private readonly IPlcCom _plcCom;

        public ControlButtonState ControlOffState { get; set; }

        public ControlButtonState ControlOnState { get; set; }

        public ControlButtonState AutomaticOnState { get; set; }

        public ControlButtonState AutomaticOffState { get; set; }

        public ControlButtonState AcknowledgeState { get; set; }

        public ICommand ControlOffPressedCommand => new DelegateCommand<bool?>( p => SoftKeyPressed(p , ControlOffSoftKey));
        public ICommand ControlOnPressedCommand => new DelegateCommand<bool?>( p => SoftKeyPressed(p , ControlOnSoftKey));
        public ICommand AutomaticOffPressedCommand => new DelegateCommand<bool?>( p => SoftKeyPressed(p , AutomaticOffSoftKey));
        public ICommand AutomaticOnPressedCommand => new DelegateCommand<bool?>( p => SoftKeyPressed(p , AutomaticOnSoftKey));
        public ICommand AcknowledgeOnPressedCommand => new DelegateCommand<bool?>( p => SoftKeyPressed(p , AcknowledgeSoftKey));



        public ControlPanelViewModel(IPlcCom plcCom, ILogger logger)
        {
            _plcCom = plcCom;
            _plcCom.PlcStateChanged += (s, e) => OnPlcStateChanged(e);
            _log = logger.ForContext<ControlPanelViewModel>();
        }

        private void OnPlcStateChanged(PlcState state)
        {
            if (state != PlcState.Run)
            {
                _plcCom.RemoveObserver($"HMI.{ControlOffSoftKey}.eState");
                _plcCom.RemoveObserver($"HMI.{ControlOnSoftKey}.eState");
                _plcCom.RemoveObserver($"HMI.{AutomaticOffSoftKey}.eState");
                _plcCom.RemoveObserver($"HMI.{AutomaticOnSoftKey}.eState");
                _plcCom.RemoveObserver($"HMI.{AcknowledgeSoftKey}.eState");

                ControlOffState = ControlButtonState.Off;
                ControlOnState = ControlButtonState.Off;
                AutomaticOffState = ControlButtonState.Off;
                AutomaticOnState = ControlButtonState.Off;
                AcknowledgeState = ControlButtonState.Off;

                return;
            }

            short controlOffState, controlOnState;

            if (!_plcCom.TryRead<short>($"HMI.{ControlOffSoftKey}.eState", out controlOffState) &
                !_plcCom.TryObserve<short>($"HMI.{ControlOffSoftKey}.eState", s => ControlOffState = (ControlButtonState)s))
                _log.Error("Status des Tasters \"Steuerung Aus\" kann nicht gelesen werden");
            else
                ControlOffState = (ControlButtonState)controlOffState;

            if (!_plcCom.TryRead<short>($"HMI.{ControlOnSoftKey}.eState", out controlOnState) &
                !_plcCom.TryObserve<short>($"HMI.{ControlOnSoftKey}.eState", s => ControlOnState = (ControlButtonState)s))
                _log.Error("Status des Tasters \"Steuerung Ein\" kann nicht gelesen werden");
            else
                ControlOnState = (ControlButtonState)controlOnState;

            if (!_plcCom.TryObserve<short>($"HMI.{AutomaticOffSoftKey}.eState", s => AutomaticOffState = (ControlButtonState)s))
                _log.Error("Status des Kontrollpanelbuttons \"Automatik Ein\" kann nicht gelesen werden");

            if (!_plcCom.TryObserve<short>($"HMI.{AutomaticOnSoftKey}.eState", s => AutomaticOnState = (ControlButtonState)s))
                _log.Error("Status des Tasters \"Automatik Ein\" kann nicht gelesen werden");
            
            if (!_plcCom.TryObserve<short>($"HMI.{AcknowledgeSoftKey}.eState", s => AcknowledgeState = (ControlButtonState)s))
                _log.Error("Status des Tasters \"Quttieren\" kann nicht gelesen werden");
        }

        private void SoftKeyPressed(bool? pressed, string softKeyVariableName)
        {
            if (pressed is not bool p)
                return;

            if (!_plcCom.TryWrite($"HMI.{softKeyVariableName}.bIsPressed", pressed) && p)
                _log.Error("Taster \"{buttonName}\" ist nicht mit der SPS verbunden", softKeyVariableName);
        }
    }
}
