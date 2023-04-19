/******************************************************************************
    CoAPSharp - C# Implementation of CoAP for .NET
    This library was originally written for .NET Micro framework. It is now
    migrated to nanoFramework.
    
    MIT License

    Copyright (c) [2023] [Idaax Inc., www.coapsharp.com]

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
 *****************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Idaax.CoAP.Helpers
{
    /// <summary>
    /// Various log levels supported (just like log4net) but a bit restricted
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Informational message. The lowest level. If this level is specified all messages
        /// are logged
        /// </summary>
        Info = 1,
        /// <summary>
        /// Debug level. If this level is specified, all messages with a level of Debug, Warn and
        /// Error are logged. Info level messages are ignored
        /// </summary>
        Debug= 2,
        /// <summary>
        /// Warn level. If this level is specified, only messages with Warn and Error are logged.
        /// All other levels are ignored
        /// </summary>
        Warn = 3,
        /// <summary>
        /// The highest level. If this level is specified, only Errors are logged.
        /// </summary>
        Error= 4
    }
    /// <summary>
    /// Any professional program should have robust logging solution. Generally you log
    /// on the console, file, database etc. but for .NET Micro Framework, one has to be
    /// a bit more careful. Logging can happen using the standard Debug class, or using
    /// the Serial port, or on an LCD panel...there are many possibilities.
    /// Given that we want logging for CoAPSharp also, we do not make any assumptions
    /// about the logging solution. Instead, this class is provided.
    /// Your application must create a concrete logging class by extending this class.
    /// The only method to implement is called "LogIt"
    /// </summary>
    public abstract class AbstractLogUtil
    {
        #region Implementation
        /// <summary>
        /// Holds the current logging level. Default is error
        /// </summary>
        protected LogLevel _logLevel = LogLevel.Error;
        /// <summary>
        /// Holds the actual instance of the logger
        /// </summary>
        protected static AbstractLogUtil _logger = null;
        #endregion

        #region Properties
        /// <summary>
        /// Accessor/Mutator for the logging level
        /// </summary>
        public LogLevel Level { get { return this._logLevel; } set { this._logLevel = value; } }
        #endregion

        #region Abstracts
        /// <summary>
        /// The abstract method which is the only method to be implemented for this class.
        /// How the message actually gets logged and to where is implementation specific
        /// </summary>
        /// <param name="level">The logging level</param>
        /// <param name="message">The message to log</param>
        protected abstract void LogIt(LogLevel level, string message);
        #endregion

        #region Operations
        /// <summary>
        /// Set the actual logger class instance
        /// </summary>
        /// <param name="logger">A logger class instance of type AbstractLogUtil</param>
        /// <param name="logLevel">The log level</param>
        public static void SetLogger(AbstractLogUtil logger , LogLevel logLevel)
        {
            AbstractLogUtil._logger = logger;
            AbstractLogUtil._logger.Level = logLevel;
        }
        /// <summary>
        /// Get the instance of the logger. If you have not specified your logger
        /// then this will return an instance of DefaultLogger class which silently
        /// ignores all logging calls and nothing gets logged. This is to ensure that
        /// we do not fail by thrown a "null pointer exception"
        /// </summary>
        /// <returns>Instance of type AbstractLogUtil</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static AbstractLogUtil GetLogger()
        {
            if (AbstractLogUtil._logger == null) AbstractLogUtil.SetLogger(new DefaultLogger() , LogLevel.Error);
            return AbstractLogUtil._logger;
        }
        /// <summary>
        /// Log an informational message. This is thread synchronized
        /// </summary>
        /// <param name="msg">The message to log</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void LogInfo(string msg) { if (this._logLevel <= LogLevel.Info) this.LogIt(LogLevel.Info , msg); }
        /// <summary>
        /// Log a debug message. This is thread synchronized
        /// </summary>
        /// <param name="msg">The message to log</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void LogDebug(string msg) { if (this._logLevel <= LogLevel.Debug) this.LogIt(LogLevel.Debug, msg); }
        /// <summary>
        /// Log a warning message. This is thread synchronized
        /// </summary>
        /// <param name="msg">The message to log</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void LogWarn(string msg) { if (this._logLevel <= LogLevel.Warn) this.LogIt(LogLevel.Debug, msg); }
        /// <summary>
        /// Log an error message. This is thread synchronized
        /// </summary>
        /// <param name="msg">The message to log</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void LogError(string msg) { if (this._logLevel <= LogLevel.Error) this.LogIt(LogLevel.Debug, msg); }
        #endregion
    }
}
