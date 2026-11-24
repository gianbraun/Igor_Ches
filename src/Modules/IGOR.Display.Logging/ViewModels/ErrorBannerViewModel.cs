using IGOR.Interface.Logging;
using IGOR.Service.Navigation.Interface;
using Prism.Commands;
using Prism.Mvvm;
using Serilog.Events;
using System.Linq;
using System.Timers;
using System.Windows.Input;

namespace IGOR.Display.Logging.ViewModels
{
    public class ErrorBannerViewModel : BindableBase
    {
        private readonly IAppLogger _logger;
        private readonly INavigationService _navigationService;

        private int _cycleIndex = 1;
        private Timer _cycleTimer;

        public LogEventLevel LogLevel { get; set; }

        private bool _show = false;
        public bool Show
        {
            get => _show;
            set => SetProperty(ref _show, value);
        }

        public string Message { get; set; }

        private DelegateCommand _hideBannerCommand;
        public ICommand HideBannerCommand => _hideBannerCommand ??= new DelegateCommand(HideBanner);

        private void HideBanner()
        {
            Show = false;
        }

        private DelegateCommand openErrorListCommand;
        public ICommand OpenErrorListCommand => openErrorListCommand ??= new DelegateCommand(OpenErrorList);

        public ErrorBannerViewModel(IAppLogger appLogger, INavigationService navigationService)
        {
            _navigationService = navigationService;
            _logger = appLogger;
            _cycleTimer = new Timer()
            {
                Interval = 3000,
                AutoReset = true,
            };
            _cycleTimer.Elapsed += (s, e) => CycleError();
            appLogger.Logs.CollectionChanged += (s,e) => UpdateBanner();

            if (appLogger.Logs.Count > 0)
                UpdateBanner();
        }

        private void UpdateBanner()
        {
            _cycleTimer.Enabled = false;
            var log = _logger.Logs.FirstOrDefault();
            if (log == null)
            {
                Show = false;
                return;
            }
            if(_logger.Logs.Count > 1)
            {
                _cycleTimer.Enabled = true;
            }
            Message = $"{log.SourceName}: {log.ErrorDescription}";
            LogLevel = log.LogLevel;
            Show = true;
        }
        
        private void CycleError()
        {
            if (_cycleIndex >= _logger.Logs.Count)
                _cycleIndex = 0;
            var log = _logger.Logs[_cycleIndex];
            Message = $"{log.SourceName}: {log.ErrorDescription}";
            LogLevel = log.LogLevel;
            _cycleIndex++;
        }

      

        private void OpenErrorList()
        {
            _navigationService.NavigateTo("ErrorListView");
        }
    }
}
