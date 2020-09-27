using System;
using System.Collections.Generic;
using System.Text;

namespace LoggingLib
{
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
}
