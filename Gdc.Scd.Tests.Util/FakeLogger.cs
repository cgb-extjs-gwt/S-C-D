using Gdc.Scd.Core.Interfaces;
using System;

namespace Gdc.Scd.Tests.Util
{
    public class FakeLogger : ILogger
    {
        public bool IsTrace;
        public bool IsDebug;
        public bool IsInfo;
        public bool IsWarn;
        public bool IsError;
        public bool IsFatal;
        public bool IsOff;

        public Exception Exception;

        public string Message;

        public void Log(string message, params object[] args)
        {
            Log(new Exception(message), message, args);
        }

        public void Log(Exception exception, string message, params object[] args)
        {
            this.Exception = exception;
            this.Message = message;
            //
            IsTrace = false;
            IsDebug = false;
            IsInfo = false;
            IsWarn = false;
            IsError = false;
            IsFatal = false;
            IsOff = false;
        }


        public void Debug(string message, params object[] args)
        {
            Log(message, args);
            IsDebug = true;
        }

        public void Debug(Exception exception, string message, params object[] args)
        {
            Log(exception, message, args);
            IsDebug = true;
        }

        public void Error(string message, params object[] args)
        {
            Log(message, args);
            IsError = true;
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            Log(exception, message, args);
            IsError = true;
        }

        public void Fatal(string message, params object[] args)
        {
            Log(message, args);
            IsFatal = true;
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            Log(exception, message, args);
            IsFatal = true;
        }

        public void Info(string message, params object[] args)
        {
            Log(message, args);
            IsInfo = true;
        }

        public void Info(Exception exception, string message, params object[] args)
        {
            Log(exception, message, args);
            IsInfo = true;
        }

        public void Off(string message, params object[] args)
        {
            Log(message, args);
            IsOff = true;
        }

        public void Off(Exception exception, string message, params object[] args)
        {
            Log(exception, message, args);
            IsOff = true;
        }

        public void Trace(string message, params object[] args)
        {
            Log(message, args);
            IsTrace = true;
        }

        public void Trace(Exception exception, string message, params object[] args)
        {
            Log(exception, message, args);
            IsTrace = true;
        }

        public void Warn(string message, params object[] args)
        {
            Log(message, args);
            IsWarn = true;
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            Log(exception, message, args);
            IsWarn = true;
        }
    }
}
