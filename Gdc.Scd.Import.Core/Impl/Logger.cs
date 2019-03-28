using NLog;
using System;

namespace Gdc.Scd.Import.Core.Impl
{
    public class Logger : Gdc.Scd.Core.Interfaces.ILogger<LogLevel>, Gdc.Scd.Core.Interfaces.ILogger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public void Log(LogLevel level, string message, params object[] args)
        {
            _logger.Log(level, message, args);
        }

        public void Log(LogLevel level, Exception exception, string message, params object[] args)
        {
            _logger.Log(level, exception, string.Format("{0}: {1}", message, exception), args);
        }

        public void Debug(string message, params object[] args)
        {
            Log(LogLevel.Debug, message, args);
        }

        public void Debug(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Debug, exception, message, args);
        }

        public void Error(string message, params object[] args)
        {
            Log(LogLevel.Error, message, args);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Error, exception, message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            Log(LogLevel.Fatal, message, args);
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Fatal, exception, message, args);
        }

        public void Info(string message, params object[] args)
        {
            Log(LogLevel.Info, message, args);
        }

        public void Info(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Info, exception, message, args);
        }

        public void Off(string message, params object[] args)
        {
            Log(LogLevel.Off, message, args);
        }

        public void Off(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Off, exception, message, args);
        }

        public void Trace(string message, params object[] args)
        {
            Log(LogLevel.Trace, message, args);
        }

        public void Trace(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Trace, exception, message, args);
        }

        public void Warn(string message, params object[] args)
        {
            Log(LogLevel.Warn, message, args);
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Warn, exception, message, args);
        }
    }
}
