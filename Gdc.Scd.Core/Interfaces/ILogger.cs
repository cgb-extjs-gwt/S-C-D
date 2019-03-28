using Gdc.Scd.Core.Enums;
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
        void Log(ScdLogLevel level, string message, params object[] args);

        void Log(ScdLogLevel level, Exception exception, string message, params object[] args);
    }
}
