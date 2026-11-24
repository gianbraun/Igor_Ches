using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace IGOR.Interface.Logging
{
    public interface ILog
    {
        DateTimeOffset TimeStamp { get; }
        string Message { get; }

        Exception Exception { get; }

        LogEventLevel LogLevel { get; }
    }
}
