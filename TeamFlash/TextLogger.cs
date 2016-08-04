using System;
using System.IO;

namespace TeamFlash
{
    public class TextLogger : ILogger
    {
        private readonly string _filename;

        public TextLogger(string filename)
        {
            _filename = filename;
        }

        public void WriteLine(string message, params object[] parameters)
        {
            var messageToLog = string.Format(message, parameters);

            using (var writer = new StreamWriter(_filename, true))
            {
                try
                {
                    writer.WriteLine("{0} {1}", DateTime.Now.ToShortTimeString(), messageToLog);
                }
                finally
                {
                    writer.Close();
                }
            }
        }

        public void Verbose(string message, params object[] parameters)
        {
            if (VerboseEnabled)
            {
                WriteLine(string.Format("VERBOSE: {0}", message), parameters);
            }
        }

        public void Error(Exception exception)
        {
            WriteLine(exception.ToString());
        }

        public bool VerboseEnabled { get; set; }
    }
}
