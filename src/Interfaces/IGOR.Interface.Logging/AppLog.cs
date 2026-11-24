using Serilog.Events;
using System;

namespace IGOR.Interface.Logging
{
    public class AppLog : ILog
    {
        public string Message { get; }

        public string SourceName { get; }

        public string ErrorDescription { get; }

        public string ErrorMessage { get; }

        public Exception Exception { get; }

        public LogEventLevel LogLevel { get; }
        public DateTimeOffset TimeStamp { get; }

        public AppLog(string message, string sourceName, string errorDescription, string errorMessage, Exception exception, LogEventLevel logLevel, DateTimeOffset timeStamp)
        {
            Message = message;
            SourceName = sourceName;
            ErrorDescription = errorDescription;
            ErrorMessage = errorMessage;
            Exception = exception;
            LogLevel = logLevel;
            TimeStamp = timeStamp;
        }
    }
}
