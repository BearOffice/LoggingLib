using System;
using System.Collections.Generic;
using System.Text;

namespace LoggingLib
{
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
}
