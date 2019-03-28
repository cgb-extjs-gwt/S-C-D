using System;

namespace Gdc.Scd.Core.Interfaces
{
    public interface ILogger<T>
    {
        void Log(T level, string message, params object[] args);

        void Log(T level, Exception exception, string message, params object[] args);
    }

    public interface ILogger
    {
        void Trace(string message, params object[] args);

        void Trace(Exception exception, string message, params object[] args);

        void Debug(string message, params object[] args);

        void Debug(Exception exception, string message, params object[] args);

        void Info(string message, params object[] args);

        void Info(Exception exception, string message, params object[] args);

        void Warn(string message, params object[] args);

        void Warn(Exception exception, string message, params object[] args);

        void Error(string message, params object[] args);

        void Error(Exception exception, string message, params object[] args);

        void Fatal(string message, params object[] args);

        void Fatal(Exception exception, string message, params object[] args);

        void Off(string message, params object[] args);

        void Off(Exception exception, string message, params object[] args);
    }
}
