using System;
using System.Collections.Generic;
using System.Text;

namespace LoggingLib
{
    public class LogEventArgs
    {
        public LogLevel Level { get; private set; }
        public string Message { get; private set; }
        public string Log { get; private set; }

        public LogEventArgs(LogLevel level, string message, string log)
        {
            this.Level = level;
            this.Message = message;
            this.Log = log;
        }
    }
}
