using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace IGOR.Interface.Logging
{
    public interface IAppLogger : ILogger, INotifyPropertyChanged
    {
        static LoggerConfiguration DefaultLoggerConfig =>
            new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(@"logs\AppLog-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:HH:mm:ss} [{Level}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");

        ObservableCollection<AppLog> Logs { get; }

        ObservableCollection<AppLog> PlcLogs { get; }

        void AddLog(AppLog log);

        void AddPlcLog(PlcError plcError);

        void ClearPlcLog();
        void ClearAppLog();
    }
}
