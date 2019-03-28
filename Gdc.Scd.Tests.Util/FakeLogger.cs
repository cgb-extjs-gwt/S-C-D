using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
using System;

namespace Gdc.Scd.Tests.Util
{
    public class FakeLogger : ILogger
    {
        public ScdLogLevel Level;

        public Exception Error;

        public string Message;

        public void Log(ScdLogLevel level, string message, params object[] args)
        {
            this.Level = level;
            this.Message = message;
            this.Error = null;
        }

        public void Log(ScdLogLevel level, Exception exception, string message, params object[] args)
        {
            this.Level = level;
            this.Message = message;
            this.Error = exception;
        }
    }
}
