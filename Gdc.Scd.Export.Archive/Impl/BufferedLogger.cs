using Gdc.Scd.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Export.Archive.Impl
{
    /// <summary>
    /// Decorator, writes logs into buffer only
    /// Flushes all logs exactly when needed into logging context 'ILogger'
    /// </summary>
    class BufferedLogger : ILogger
    {
        private IList<Action<ILogger>> buffer;

        private ILogger logger;

        public BufferedLogger(ILogger logger)
        {
            buffer = new List<Action<ILogger>>(25);
            this.logger = logger;
        }

        public void Flush()
        {
            for (var i = 0; i < buffer.Count; i++)
            {
                buffer[i](logger);
            }
            buffer.Clear();
        }

        public void Debug(string message, params object[] args)
        {
            buffer.Add(x => x.Debug(message, args));
        }

        public void Debug(Exception exception, string message, params object[] args)
        {
            buffer.Add(x => x.Debug(exception, message, args));
        }

        public void Error(string message, params object[] args)
        {
            buffer.Add(x => x.Error(message, args));
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            buffer.Add(x => x.Error(exception, message, args));
        }

        public void Fatal(string message, params object[] args)
        {
            buffer.Add(x => x.Fatal(message, args));
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            buffer.Add(x => x.Fatal(exception, message, args));
        }

        public void Info(string message, params object[] args)
        {
            buffer.Add(x => x.Info(message, args));
        }

        public void Info(Exception exception, string message, params object[] args)
        {
            buffer.Add(x => x.Info(exception, message, args));
        }

        public void Off(string message, params object[] args)
        {
            buffer.Add(x => x.Off(message, args));
        }

        public void Off(Exception exception, string message, params object[] args)
        {
            buffer.Add(x => x.Off(exception, message, args));
        }

        public void Trace(string message, params object[] args)
        {
            buffer.Add(x => x.Trace(message, args));
        }

        public void Trace(Exception exception, string message, params object[] args)
        {
            buffer.Add(x => x.Trace(exception, message, args));
        }

        public void Warn(string message, params object[] args)
        {
            buffer.Add(x => x.Warn(message, args));
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            buffer.Add(x => x.Warn(exception, message, args));
        }
    }
}
