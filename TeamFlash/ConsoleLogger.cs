using System;

namespace TeamFlash
{
    public class ConsoleLogger : ILogger
    {
        public void WriteLine(string message, params object[] parameters)
        {
            var messageToLog = string.Format(message, parameters);
            Console.WriteLine("{0} {1}", DateTime.Now.ToShortTimeString(), messageToLog);
        }

        public void Verbose(string message, params object[] parameters)
        {
            if (VerboseEnabled)
            {
                Console.WriteLine("VERBOSE: {0} {1}", DateTime.Now.ToShortTimeString(), String.Format(message, parameters));
            }
        }

        public void Error(Exception exception)
        {
            WriteLine(exception.ToString());
        }

        public bool VerboseEnabled { get; set; }
    }
}
