using System;
using System.Collections.Generic;
using System.Text;

namespace LoggingLib;

public record LogEventArgs
{
    public Logger Logger { get; init; }
    public LogLevel Level { get; init; }
    public string RawMessage { get; init; }
    public string Log { get; init; }

    public LogEventArgs(Logger logger, LogLevel level, string rawMessage, string log)
    {
        this.Logger = logger;
        this.Level = level;
        this.RawMessage = rawMessage;
        this.Log = log;
    }
}
