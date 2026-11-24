using IGOR.Interface.Logging;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Mvvm;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace IGOR.Display.Logging.ViewModels
{
    public class ErrorIndicatorViewModel : BindableBase
    {
        private readonly IAppLogger _appLogger;
        private readonly INavigationService _navigationService;
        
        public int InformationCount { get; set; }

        public int WarningCount { get; set; }

        public int ErrorCount { get; set; }

        private DelegateCommand navigateToMessageListCommand;
        public ICommand NavigateToMessageListCommand => navigateToMessageListCommand ??= new DelegateCommand(NavigateToMessageList);

        

        public ErrorIndicatorViewModel(IAppLogger appLogger, INavigationService navigationService)
        {
            _appLogger = appLogger;
            _navigationService = navigationService;

            InformationCount = _appLogger.Logs.Where(l => l.LogLevel == LogEventLevel.Information).Count();
            WarningCount = _appLogger.Logs.Where(l => l.LogLevel == LogEventLevel.Warning).Count();
            ErrorCount = _appLogger.Logs.Where(l => l.LogLevel == LogEventLevel.Error).Count();

            appLogger.Logs.CollectionChanged += (s, e) =>
            {
                InformationCount = _appLogger.Logs.Where(l => l.LogLevel == LogEventLevel.Information).Count();
                WarningCount = _appLogger.Logs.Where(l => l.LogLevel == LogEventLevel.Warning).Count();
                ErrorCount = _appLogger.Logs.Where(l => l.LogLevel == LogEventLevel.Error).Count();
            };
        }
        private void NavigateToMessageList()
        {
            _navigationService.NavigateTo("ErrorListView");
        }

    }
}
