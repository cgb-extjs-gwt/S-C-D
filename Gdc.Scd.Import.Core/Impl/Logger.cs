using NLog;
using System;

namespace Gdc.Scd.Import.Core.Impl
{
    public class Logger : Gdc.Scd.Core.Interfaces.ILogger<LogLevel>
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public void Log(LogLevel level, string message, params object[] args)
        {
            _logger.Log(level, message, args);
        }

        public void Log(LogLevel level, Exception exception, string message, params object[] args)
        {
            _logger.Log(level, exception, String.Format("{0}: {1}", message, exception), args);
        }
    }
}
