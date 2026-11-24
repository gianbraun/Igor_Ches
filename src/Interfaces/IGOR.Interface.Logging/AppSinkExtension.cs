using Serilog;
using Serilog.Configuration;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.Text;

namespace IGOR.Interface.Logging
{
    public static class AppSinkExtension
    {
        public static LoggerConfiguration App(this LoggerSinkConfiguration sinkConfiguration, IAppLogger appLogg, ITextFormatter textFormatter, IFormatProvider formatProvider = null)
        {
            return sinkConfiguration.Sink(new AppSink(appLogg, formatProvider, textFormatter));
        }
    }
}
