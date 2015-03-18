using System;
using System.IO;

namespace SearchApplication.BusinessLogic
{
    public static class Logger
    {

        public static void WriteToFile(string message)
        {
            try
            {
                var loggingDirectory = String.Format("{0}\\Logs", Environment.CurrentDirectory);
                if (!Directory.Exists(loggingDirectory))
                {
                    Directory.CreateDirectory(loggingDirectory);
                }

                using (var writer = new StreamWriter(String.Format("{0}\\errors.txt", loggingDirectory), true))
                {
                    writer.WriteLine("{0}\t{1}", DateTime.Now, message);
                }
            }
            catch (Exception)
            {
                
               
            }
            
        }
    }
}
