using System;

namespace Stonkbot.Utils
{
    public enum LogType
    {
        Info,
        Warning,
        Error,
    }

    public static class Logger
    {
        public static void Log(LogType  logType, string message)
        {
            Console.WriteLine($"[{logType.ToString().ToUpper()}] {message}");
        }

        public static void Log(string message)
        {
            Console.WriteLine("[INFO] " + message);
        }

        public static void LogWarning(string message)
        {
            Console.WriteLine("[WARNING] " + message);
        }

        public static void LogError(string message)
        {
            Console.WriteLine("[ERROR] " + message);
        }
    }
}