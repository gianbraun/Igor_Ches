using IGOR.Interface.Communication;
using Prism.Mvvm;
using System.ComponentModel;
using TwinCAT.Ads;

namespace IGOR.Service.Communication.TwinCat.ViewModels
{
    /// <summary>
    /// ViewModel for TwinCatStatus bar
    /// </summary>
    public class TwinCatStatusBarViewModel : BindableBase
    {
        private readonly IPlcCom _spsCom;

        /// <summary>
        /// Current state of the plc
        /// </summary>
        public PlcState PlcState => _spsCom.PlcState;

        /// <summary>
        /// Current state of the system
        /// </summary>
        public PlcState SysState => _spsCom.SysState;

        /// <summary>
        /// ViewModel for TwinCatStatus bar
        /// </summary>
        public TwinCatStatusBarViewModel(IPlcCom spsCom)
        {
            _spsCom = spsCom;
            _spsCom.PropertyChanged += PlcCom_PropertyChanged;

        }

        private void PlcCom_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_spsCom.PlcState))
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(PlcState)));

            if (e.PropertyName == nameof(_spsCom.SysState))
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SysState)));                
        }
    }
}