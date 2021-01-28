using System;
using System.IO;
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

        private void CreateLog(LogLevel level, string message, bool accident = false)
        {
            var log = FormatInterpreter(level, message);

            if (level == LogLevel.Debug)
                System.Diagnostics.Debug.Print(log);

            AddLogEvent(level, message, log);

            if ((int)level >= (int)_level)
            {
                AddBroadcast(level, message, log);
                if (_filename != null && !accident)
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

                var count = 0;
                try
                {
                    using var reader = new StreamReader(_filename);
                    while (reader.ReadLine() != null)
                        count++;
                }
                catch
                {
                    var level = LogLevel.Error;
                    var message = "An unexpected error occurred when logger attempt to read the log file.";
                    CreateLog(level, message, accident: true);
                }
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

            try
            {
                using var writer = new StreamWriter(_filename, append: true);
                writer.WriteLine(log);
            }
            catch
            {
                var level = LogLevel.Error;
                var message = "An unexpected error occurred when logger attempt to output the log.";
                CreateLog(level, message, accident: true);
            }
        }
    }
}
