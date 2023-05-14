using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace LoggingLib;

public class Logger
{
    /// <summary>
    /// Logger's name.
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// Set the lowest importance level. Default level is Warn.
    /// Log that is greater than or equal to the lowest importance will be recorded.
    /// </summary>
    public LogLevel Level { get; set; } = LogLevel.Warn;
    /// <summary>
    /// Path to save logs. If logs are not to be saved to a file, set <see langword="null"/>.
    /// Default value is <see langword="null"/>.
    /// </summary>
    public string? Path { get; set; }
    /// <summary>
    /// Log's format.
    /// <para>
    /// The following keywords can be used.
    /// <c>(level), (name), (lineno), (time), (message)</c>.
    /// </para>
    /// <para>
    /// (time) can be set as <c>(time:utc) (time:offset) (time:"param")</c>.
    /// "param" follows <c>DateTime.ToString()</c>, suchlike (time:T).
    /// </para>
    /// Default format is <c>(linenum)\t(level)\t(name): (message)</c>.
    /// </summary>
    public string Format { get; set; } = "(linenum)\t(level)\t(name): (message)";

    /// <summary>
    /// Logger class.
    /// </summary>
    /// <param name="name">Logger's name.</param>
    public Logger(string name)
    {
        Name = name;
    }

    internal string GenerateLog(LogLevel level, string message, bool write = true)
    {
        var matches = Regex.Matches(Format, @"\(.+?\)");

        var log = Format;

        foreach (var match in matches.Cast<Match>())
        {
            log = match.Value switch
            {
                "(level)" => log.Replace(match.Value, level.ToString().ToUpper()),
                "(name)" => log.Replace(match.Value, Name),
                "(linenum)" => log.Replace(match.Value, (GetLineNumber(Path) + 1).ToString()),
                "(message)" => log.Replace(match.Value, message),
                _ when IsTimeFormat(match.Value, out string time) => log.Replace(match.Value, time),
                _ => log,
            };
        }

        if (write && Path is not null)
        {
            if (level >= Level)
            {
                if (!TryWriteLog(Path, log))
                {
                    var errMessage = "An unexpected error occurred when logger attempt to read the log file.";
                    Logging.PublishLogBase(Logging.Root.Name, LogLevel.Error, errMessage, internalLog: true);
                }
            }
        }

        return log;
    }

    private static int GetLineNumber(string? path)
    {
        if (path is null) return 0;
        if (!File.Exists(path)) return 0;

        var count = 0;
        try
        {
            using var reader = new StreamReader(path);
            while (!reader.EndOfStream)
            {
                reader.ReadLine();
                count++;
            }
        }
        catch
        {
            var level = LogLevel.Error;
            var message = "An unexpected error occurred when logger attempt to read the log file.";
            Logging.PublishLogBase(Logging.Root.Name, level, message, internalLog: true);
        }
        return count;
    }

    private static bool IsTimeFormat(string value, out string time)
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

    private static bool TryWriteLog(string path, string log)
    {
        path = System.IO.Path.GetFullPath(path);

        try
        {
            var dirName = System.IO.Path.GetDirectoryName(path);
            if (!Directory.Exists(dirName))
            {
#pragma warning disable CS8604 // Possible null reference argument.
                Directory.CreateDirectory(dirName);
#pragma warning restore CS8604 // Possible null reference argument.
            }

            using var writer = new StreamWriter(path, append: true);
            writer.WriteLine(log);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
