using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

namespace LoggingLib;

/// <summary>
/// A Logging Library.
/// </summary>
public static class Logging
{
    /// <summary>
    /// Log's event.
    /// Every event will be published.
    /// </summary>
    public static event Action<LogEventArgs>? LogEvent;
    /// <summary>
    /// Log's broadcast. Only the log that is greater than or equal to the lowest importance will be published.
    /// </summary>
    public static event Action<LogEventArgs>? Broadcast;
    /// <summary>
    /// Root Logger.
    /// </summary>
    public static Logger Root { get; } = new Logger("root");
    private static readonly ConcurrentDictionary<string, Logger> _loggerDic = new ConcurrentDictionary<string, Logger>();
    private static readonly object _rwLock = new object();

    static Logging()
    {
        _loggerDic.TryAdd(Root.Name, Root);
    }

    /// <summary>
    /// Get the registered logger. Register a new one and return it, if the specified logger not exists.
    /// </summary>
    /// <param name="name">Logger's name</param>
    public static Logger GetLogger(string name)
    {
        if (_loggerDic.TryGetValue(name, out var logger)) return logger;

        var newLogger = new Logger(name);
        _loggerDic.TryAdd(name, newLogger);
        return newLogger;
    }

    /// <summary>
    /// Determine whether the specified logger is registered.
    /// </summary>
    /// <param name="name">Logger's name</param>
    /// <returns><see langword="true"/> if the specified logger is registered; otherwise, <see langword="false"/></returns>
    public static bool IsRegistered(string name)
    {
        return _loggerDic.ContainsKey(name);
    }

    /// <summary>
    /// Register the specified logger.
    /// </summary>
    /// <param name="logger"></param>
    public static void RegisterLogger(Logger logger)
    {
        if (!_loggerDic.TryAdd(logger.Name, logger))
        {
            PublishLogBase(Root.Name, LogLevel.Error, $"Failed to add logger '{logger.Name}'.", internalLog: true);
        }
    }

    /// <summary>
    /// Unregister logger. Root logger cannot be unregister.
    /// </summary>
    /// <param name="name">The logger to be deregistered.</param>
    public static void UnregisterLogger(string name)
    {
        if (name == Root.Name)
        {
            PublishLogBase(Root.Name, LogLevel.Error, "Root logger cannot be deregistered.", internalLog: true);
            return;
        }
        
        if (!_loggerDic.TryRemove(name, out _))
        {
            PublishLogBase(Root.Name, LogLevel.Error, $"Failed to remove logger '{name}'.", internalLog: true);
        }
    }

    /// <summary>
    /// Publish a log. Level is Debug.
    /// </summary>
    /// <param name="message">Message.</param>
    public static void Debug(string message)
        => PublishLog(LogLevel.Debug, message);

    /// <summary>
    /// Publish a log. Level is Info.
    /// </summary>
    /// <param name="message">Message.</param>
    public static void Info(string message)
        => PublishLog(LogLevel.Info, message);

    /// <summary>
    /// Publish a log. Level is Warn.
    /// </summary>
    /// <param name="message">Message.</param>
    public static void Warn(string message)
        => PublishLog(LogLevel.Warn, message);

    /// <summary>
    /// Publish a log. Level is Error.
    /// </summary>
    /// <param name="message">Message.</param>
    public static void Error(string message)
        => PublishLog(LogLevel.Error, message);

    /// <summary>
    /// Publish a log. Level is Critical.
    /// </summary>
    /// <param name="message">Message.</param>
    public static void Critical(string message)
        => PublishLog(LogLevel.Crit, message);

    /// <summary>
    /// Publish a log.
    /// </summary>
    /// <param name="level">Log level.</param>
    /// <param name="message">Message.</param>
    public static void PublishLog(LogLevel level, string message)
        => PublishLogBase(Root.Name, level, message, internalLog: false);

    /// <summary>
    /// Publish a log. Level is Debug.
    /// </summary>
    /// <param name="name">Logger name.</param>
    /// <param name="message">Message.</param>
    public static void Debug(string name, string message)
        => PublishLog(name, LogLevel.Debug, message);

    /// <summary>
    /// Publish a log. Level is Info.
    /// </summary>
    /// <param name="name">Logger name.</param>
    /// <param name="message">Message.</param>
    public static void Info(string name, string message)
        => PublishLog(name, LogLevel.Info, message);

    /// <summary>
    /// Publish a log. Level is Warn.
    /// </summary>
    /// <param name="name">Logger name.</param>
    /// <param name="message">Message.</param>
    public static void Warn(string name, string message)
        => PublishLog(name, LogLevel.Warn, message);

    /// <summary>
    /// Publish a log. Level is Error.
    /// </summary>
    /// <param name="name">Logger name.</param>
    /// <param name="message">Message.</param>
    public static void Error(string name, string message)
        => PublishLog(name, LogLevel.Error, message);

    /// <summary>
    /// Publish a log. Level is Critical.
    /// </summary>
    /// <param name="name">Logger name.</param>
    /// <param name="message">Message.</param>
    public static void Critical(string name, string message)
        => PublishLog(name, LogLevel.Crit, message);

    /// <summary>
    /// Publish a log.
    /// </summary>
    /// <param name="name">Logger name.</param>
    /// <param name="level">Log level.</param>
    /// <param name="message">Message.</param>
    public static void PublishLog(string name, LogLevel level, string message)
        => PublishLogBase(name, level, message, internalLog: false);

    internal static void PublishLogBase(string name, LogLevel level, string message, bool internalLog = false)
    {
        if (!_loggerDic.TryGetValue(name, out var logger))
        {
            PublishLogBase(Root.Name, LogLevel.Error, "The logger specified does not exist.", internalLog: true);
            return;
        }

        var log = default(string);
        
        lock (_rwLock)
        {
            var allowToWrite = !internalLog;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            log = logger.GenerateLog(level, message, allowToWrite);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        if (level == LogLevel.Debug)
            System.Diagnostics.Debug.Print(log);

        LogEvent?.Invoke(new LogEventArgs(logger, level, message, log));
        if (level >= logger.Level)
            Broadcast?.Invoke(new LogEventArgs(logger, level, message, log));
    }
}
