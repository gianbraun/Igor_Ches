using IGOR.Interface.Logging;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IGOR.Display.Logging.ViewModels
{
    public class ErrorListViewModel : BindableBase
    {
        private readonly IAppLogger _logger;

        private readonly List<LogEventLevel> SelectedLogLevels;

        public bool IsErrorLogLevelSelected { get; set; } = true;
        public bool IsWarningLogLevelSelected { get; set; } = true;
        public bool IsInformationLogLevelSelected { get; set; } = true;

        public IEnumerable<AppLog> Logs { get; private set; }

        private DelegateCommand _selectErrorLogLevelCommand;
        public ICommand SelectErrorLogLevelCommand => _selectErrorLogLevelCommand ??= new DelegateCommand(() => SelectLogLevel(LogEventLevel.Error));

        private DelegateCommand _selectWarningLogLevelCommand;
        public DelegateCommand SelectWarningLogLevelCommand => _selectWarningLogLevelCommand ??= new DelegateCommand(() => SelectLogLevel(LogEventLevel.Warning));

        private DelegateCommand _selectInfromationLogLevelCommand;
        public DelegateCommand SelectInfromationLogLevelCommand => _selectInfromationLogLevelCommand ??= new DelegateCommand(() => SelectLogLevel(LogEventLevel.Information));

        public ErrorListViewModel(IAppLogger appLogger)
        {
            _logger = appLogger;
            SelectedLogLevels = new List<LogEventLevel>()
            {
                LogEventLevel.Error,
                LogEventLevel.Information,
                LogEventLevel.Warning
            };
            appLogger.Logs.CollectionChanged += (s, e) => UpdateLogs();
            UpdateLogs();
        }

        private void UpdateLogs()
        {
            Logs = _logger.Logs.Where(l => SelectedLogLevels.Contains(l.LogLevel));
        }



        private void SelectLogLevel(LogEventLevel logEventLevel)
        {
            switch (logEventLevel)
            {
                case LogEventLevel.Information:
                    IsInformationLogLevelSelected = !IsInformationLogLevelSelected;

                    if (IsInformationLogLevelSelected)
                        SelectedLogLevels.Add(LogEventLevel.Information);
                    else
                        SelectedLogLevels.Remove(LogEventLevel.Information);
                    break;

                case LogEventLevel.Warning:
                    IsWarningLogLevelSelected = !IsWarningLogLevelSelected;
                    if (IsWarningLogLevelSelected)
                        SelectedLogLevels.Add(LogEventLevel.Warning);
                    else
                        SelectedLogLevels.Remove(LogEventLevel.Warning);
                    break;

                case LogEventLevel.Error:
                    IsErrorLogLevelSelected = !IsErrorLogLevelSelected;
                    
                    if (IsErrorLogLevelSelected)
                        SelectedLogLevels.Add(LogEventLevel.Error);
                    else
                        SelectedLogLevels.Remove(LogEventLevel.Error);
                    
                    break;

                default:
                    break;
            }
            UpdateLogs();
        }
    }
}
