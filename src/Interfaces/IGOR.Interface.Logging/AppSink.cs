using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace IGOR.Interface.Logging
{
    public class AppSink : ILogEventSink, IDisposable
    {
        private readonly IAppLogger _appLogg;
        private readonly IFormatProvider _formatProvider;
        private readonly ITextFormatter _formatter;
        private readonly object _lockObject = new object();
        private StringBuilder _messageBuilder;
        private StringWriter _writer;

        public AppSink(IAppLogger appLogg, IFormatProvider formatProvider, ITextFormatter textFormatter)
        {
            _appLogg = appLogg;
            _formatProvider = formatProvider;
            _formatter = textFormatter;
            _messageBuilder = new StringBuilder();
            if(textFormatter != null)
            {
                _writer = new StringWriter(_messageBuilder);
            }
        }

        

        public void Emit(LogEvent logEvent)
        {
            lock (_lockObject)
            {
                if (_formatter != null)
                {
                    _formatter.Format(logEvent, _writer);
                }
                else
                {
                    _messageBuilder.Append(logEvent.RenderMessage(_formatProvider));
                }

                var newAppLog = new AppLog(_messageBuilder.ToString(), "HMI", _messageBuilder.ToString(), _messageBuilder.ToString(), logEvent.Exception, logEvent.Level, logEvent.Timestamp);

                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    _appLogg.AddLog(newAppLog);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() => _appLogg.AddLog(newAppLog));
                }
                _messageBuilder.Clear();
            }
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
