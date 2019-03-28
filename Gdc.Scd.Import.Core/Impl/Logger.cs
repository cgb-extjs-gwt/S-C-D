using Gdc.Scd.Core.Enums;
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

        public void Log(ScdLogLevel level, string message, params object[] args)
        {
            Log(AsNlogLevel(level), message, args);
        }

        public void Log(ScdLogLevel level, Exception exception, string message, params object[] args)
        {
            Log(AsNlogLevel(level), exception, message, args);
        }

        public static LogLevel AsNlogLevel(ScdLogLevel level)
        {
            switch (level)
            {
                case ScdLogLevel.Trace: return LogLevel.Trace;

                case ScdLogLevel.Debug: return LogLevel.Debug;

                case ScdLogLevel.Info: return LogLevel.Info;

                case ScdLogLevel.Warn: return LogLevel.Warn;

                case ScdLogLevel.Error: return LogLevel.Error;

                case ScdLogLevel.Fatal: return LogLevel.Fatal;

                case ScdLogLevel.Off: return LogLevel.Off;

                default:
                    throw new NotImplementedException("Unknown Scd.Core.Enums.LogLevel: " + level.ToString());
            }
        }

    }
}
