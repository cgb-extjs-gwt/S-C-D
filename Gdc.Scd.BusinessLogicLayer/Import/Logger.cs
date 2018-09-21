using Gdc.Scd.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Gdc.Scd.BusinessLogicLayer.Import
{
    public class Logger : ILogger<LogLevel>
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public void Log(LogLevel level, string message, params object[] args)
        {
            _logger.Log(level, message, args);
        }

        public void Log(LogLevel level, Exception exception, string message, params object[] args)
        {
            _logger.Log(level, exception, message, args);
        }
    }
}
