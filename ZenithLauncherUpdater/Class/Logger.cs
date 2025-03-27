using System;
using System.IO;

namespace ZenithLauncherUpdater.Services
{
    public static class Logger
    {
        private static string logFilePath;

        static Logger()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string logDirectory = Path.Combine(appDataPath, "Zenith", "UpdaterLogs");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            logFilePath = Path.Combine(logDirectory, $"application_{timestamp}.log");
            File.Create(logFilePath).Dispose();
        }

        public static void Log(string message)
        {
            try
            {
                string logEntry = $"{DateTime.Now}: {message}";
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception)
            {
                Console.WriteLine("No shot this is ever happening fuck you ");
            }
        }
    }
}
