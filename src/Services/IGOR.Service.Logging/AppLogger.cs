using IGOR.Interface.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Templates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace IGOR.Service.Logging
{
    internal class AppLogger : IAppLogger
    {
        private readonly ILogger _logger;
        private ErrorMapping _errorMapping;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<AppLog> Logs { get; private set; } = new ObservableCollection<AppLog>();
        public ObservableCollection<AppLog> PlcLogs { get; } = new ObservableCollection<AppLog>();

        public AppLogger(ILogger logger)
        {
            _logger = logger is null ? CreateDefaultLogger() : logger;
            _errorMapping = ErrorMapping.GetErrorMapping();
            if (_errorMapping == null)
            {
                _logger.Error("ErrorMessages.json konnte nicht gelesen werden");
            }
        }

        public AppLogger() : this(null) { }



        private ILogger CreateDefaultLogger()
            => IAppLogger.DefaultLoggerConfig
                .WriteTo.App(this, new MessageTemplateTextFormatter("{Message:l}"))
                .CreateLogger();

        public void AddLog(AppLog log)
        {
            Logs.Add(log);
        }

        public void AddPlcLog(PlcError plcError)
        {
            var log = CreatePlcLog(plcError);
            if (log == null)
                return;
            PlcLogs.Add(log);
            AddLog(log);
        }

        private AppLog CreatePlcLog(PlcError plcError)
        {
            if (_errorMapping == null)
                return null;

            if (!_errorMapping.SourceNames.TryGetValue(plcError.Source, out var sourceName))
                ((ILogger)this).Error("Fehlerquelle {0} nicht definiert.", plcError.Source);

            if (!_errorMapping.Descriptions.TryGetValue(plcError.Type, out var description))
                ((ILogger)this).Error("Fehlerbeschreibung {0} nicht definiert.", plcError.Type);

            if(string.IsNullOrEmpty(sourceName) || string.IsNullOrEmpty(description))
            {
                return null;
            } 

            var log = new AppLog($"{sourceName}: {description}", _errorMapping.SourceNames[plcError.Source], _errorMapping.Descriptions[plcError.Type], plcError.Msg, null, LogEventLevel.Error, DateTimeOffset.Now);
            return log;

        }

        public void Write(LogEvent logEvent)
        {
            _logger.Write(logEvent);
        }

        public void ClearPlcLog()
        {
            foreach (var log in PlcLogs)
            {
                Logs.Remove(log);
            }
            PlcLogs.Clear();
        }

        public void ClearAppLog()
        {
            Logs.Clear();
        }
    }
}
