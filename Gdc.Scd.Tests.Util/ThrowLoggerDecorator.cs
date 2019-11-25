using Gdc.Scd.Core.Interfaces;
using System;

namespace Gdc.Scd.Tests.Util
{
    public class ThrowLoggerDecorator : ILogger
    {
        private ILogger log;

        public ThrowLoggerDecorator(ILogger log)
        {
            this.log = log;
        }

        public void Debug(string message, params object[] args)
        {
            log.Debug(message, args);
        }

        public void Debug(Exception exception, string message, params object[] args)
        {
            ThrowError(exception);
        }

        public void Error(string message, params object[] args)
        {
            ThrowError(message);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            ThrowError(exception);
        }

        public void Fatal(string message, params object[] args)
        {
            ThrowError(message);
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            ThrowError(exception);
        }

        public void Info(string message, params object[] args)
        {
            log.Info(message, args);
        }

        public void Info(Exception exception, string message, params object[] args)
        {
            ThrowError(exception);
        }

        public void Off(string message, params object[] args)
        {
            log.Off(message, args);
        }

        public void Off(Exception exception, string message, params object[] args)
        {
            ThrowError(exception);
        }

        public void Trace(string message, params object[] args)
        {
            log.Trace(message, args);
        }

        public void Trace(Exception exception, string message, params object[] args)
        {
            ThrowError(exception);
        }

        public void Warn(string message, params object[] args)
        {
            log.Warn(message);
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            ThrowError(exception);
        }

        private static void ThrowError(string err)
        {
            ThrowError(new Exception(err));
        }

        private static void ThrowError(Exception exception)
        {
            throw exception ?? new Exception();
        }
    }
}
