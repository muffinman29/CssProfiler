using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SearchApplication.BusinessLogic
{
    public static class Logger
    {

        public static void WriteToFile(string message)
        {
            var loggingDirectory = String.Format("{0}\\Logs", Environment.CurrentDirectory);
            if (!Directory.Exists(loggingDirectory))
            {
                Directory.CreateDirectory(loggingDirectory);
            }
            
            using(StreamWriter writer = new StreamWriter(String.Format("{0}\\errors.txt", loggingDirectory),true))
            {
                writer.WriteLine(message);
            }
        }
    }
}
