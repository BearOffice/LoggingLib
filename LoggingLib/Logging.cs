using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace LoggingLib
{
    /// <summary>
    /// A Logging Lib
    /// </summary>
    public class Logging
    {
        /// <summary>
        /// Every event will be published.
        /// </summary>
        public static event Action<LogEventArgs> LogEvent;
        /// <summary>
        /// Only the log that is greater than or equal to the lowest importance will be published.
        /// </summary>
        public static event Action<LogEventArgs> Broadcast;
        /// <summary>
        /// Root Logger.
        /// </summary>
        public static Logging Logger { get; } = new Logging();
        private readonly string _name;
        private LogLevel _level;
        private string _filename;
        private string _format;

        private Logging()
        {
            _name = "root";
            BasicConfig(new LogBasicConfig());
        }

        private Logging(string name, LogBasicConfig basicConfig)
        {
            _name = name;
            BasicConfig(basicConfig);
        }
        /// <summary>
        /// Get a new logger.
        /// </summary>
        /// <param name="name">New logger's name</param>
        /// <returns>Return a new logger with the name that is specified.</returns>
        public static Logging GetLogger(string name)
            => new Logging(name, new LogBasicConfig());
        /// <summary>
        /// Get a new logger.
        /// </summary>
        /// <param name="name">New logger's name.</param>
        /// <param name="basicConfig">New logger's config.</param>
        /// <returns>Return a new logger with the name that is specified.</returns>
        public static Logging GetLogger(string name, LogBasicConfig basicConfig)
            => new Logging(name, basicConfig);
        /// <summary>
        /// Set the logger's config.
        /// </summary>
        /// <param name="basicConfig">Logger's config</param>
        public void BasicConfig(LogBasicConfig basicConfig)
        {
            _level = basicConfig.Level;
            _filename = basicConfig.FileName;
            _format = basicConfig.Format;
        }
        /// <summary>
        /// Publish a log. Level is Debug.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Debug(string message)
            => CreateLog(LogLevel.Debug, message);
        /// <summary>
        /// Publish a log. Level is Info.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Info(string message)
            => CreateLog(LogLevel.Info, message);
        /// <summary>
        /// Publish a log. Level is Warn.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Warn(string message)
            => CreateLog(LogLevel.Warn, message);
        /// <summary>
        /// Publish a log. Level is Error.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Error(string message)
            => CreateLog(LogLevel.Error, message);
        /// <summary>
        /// Publish a log. Level is Critical.
        /// </summary>
        /// <param name="message">Log message.</param>
        public void Critical(string message)
            => CreateLog(LogLevel.Critical, message);

        private void CreateLog(LogLevel level, string message)
        {
            var log = FormatInterpreter(level, message);

            if (level == LogLevel.Debug)
                System.Diagnostics.Debug.Print(log);

            AddLogEvent(level, message, log);

            if ((int)level >= (int)_level)
            {
                AddBroadcast(level, message, log);
                if (_filename != null)
                    LogRecorder(log);
            }
        }

        private string FormatInterpreter(LogLevel level, string message)
        {
            var log = _format;
            var matches = Regex.Matches(_format, @"\(.+?\)");
            foreach (Match match in matches)
            {
                log = match.Value switch
                {
                    "(level)" => log.Replace(match.Value, level.ToString().ToUpper()),
                    "(name)" => log.Replace(match.Value, _name),
                    "(lineno)" when _filename != null => log.Replace(match.Value, (GetLineNo() + 1).ToString()),
                    "(lineno)" when _filename == null => log.Replace(match.Value, ""),
                    "(message)" => log.Replace(match.Value, message),
                    _ when IsTime(match.Value, out string time) => log.Replace(match.Value, time),
                    _ => log,
                };
            }
            return log;

            int GetLineNo()
            {
                var path = Path.GetFullPath(_filename);
                if (!File.Exists(path)) return 0;

                using var reader = new StreamReader(_filename);
                var count = 0;
                while (reader.ReadLine() != null)
                    count++;
                return count;
            }

            static bool IsTime(string value, out string time)
            {
                var match = Regex.Match(value, @"\(time\)|\(time:(.+?)\)");
                if (!match.Success)
                {
                    time = "";
                    return false;
                }

                time = match.Groups[1].Value switch
                {
                    "utc" => DateTime.UtcNow.ToString(),
                    "offset" => DateTimeOffset.Now.ToString(),
                    _ => DateTime.Now.ToString(match.Groups[1].Value),
                };
                return true;
            }
        }

        private static void AddLogEvent(LogLevel level, string message, string log)
            => LogEvent?.Invoke(new LogEventArgs(level, message, log));

        private static void AddBroadcast(LogLevel level, string message, string log)
            => Broadcast?.Invoke(new LogEventArgs(level, message, log));

        private void LogRecorder(string log)
        {
            var path = Path.GetFullPath(_filename);
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            using var writer = new StreamWriter(_filename, append: true);
            writer.WriteLine(log);
        }
    }

    public class LogBasicConfig
    {
        /// <summary>
        /// Set the lowest importance level. Default level is Warn.
        /// Log that is greater than or equal to the lowest importance will be recorded.
        /// </summary>
        public LogLevel Level { get; set; } = LogLevel.Warn;
        /// <summary>
        /// Logging to a file. Relative path is allowed.
        /// <para>"logs\logfile.log" will be represented as "{AppPath}\logs\logfile.log"</para>
        /// </summary>
        public string FileName { get; set; } = null;
        /// <summary>
        /// Specify the format of the log. There are several parameters can be used.
        /// <para>(level), (name), (lineno), (time), (message)</para>
        /// <para>The number of lines will not be displayed if the log file output is not set.</para>
        /// <para>(time) is a little special. Several parameters can be used with it.</para>
        /// <para>(time:utc) (time:offset) (time:"params")</para>
        /// <para>"params" format follows DateTime.ToString(). Suchlike (time:T).</para>
        /// Default format is (level) (name): (message).
        /// </summary>
        public string Format { get; set; } = "(level) (name): (message)";
    }

    public enum LogLevel
    {
        /// <summary>
        /// Detailed information, typically of interest only when diagnosing problems.
        /// </summary>
        Debug,
        /// <summary>
        /// Confirmation that things are working as expected.
        /// </summary>
        Info,
        /// <summary>
        /// An indication that something unexpected happened, or indicative of some problem in the near future.
        /// The software is still working as expected.
        /// </summary>
        Warn,
        /// <summary>
        /// Due to a more serious problem, the software has not been able to perform some function.
        /// </summary>
        Error,
        /// <summary>
        /// A serious error, indicating that the program itself may be unable to continue running.
        /// </summary>
        Critical,
    }

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
