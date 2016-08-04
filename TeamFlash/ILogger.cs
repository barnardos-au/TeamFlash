using System;

namespace TeamFlash
{
    public interface ILogger
    {
        bool VerboseEnabled { get; set; }

        void Error(Exception exception);
        void Verbose(string message, params object[] parameters);
        void WriteLine(string message, params object[] parameters);
    }
}