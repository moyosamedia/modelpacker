using System;
using JetBrains.Annotations;

namespace ModelPacker.Logger
{
    public static class Log
    {
        public delegate void LogHandler(LogType type, string message);

        public static LogHandler onLog;

        public static void Line(LogType type, object obj)
        {
            Line(type, obj.ToString());
        }

        [StringFormatMethod("format")]
        public static void Line(LogType type, string format, params object[] args)
        {
            Line(type, string.Format(format, args));
        }

        public static void Exception(Exception e)
        {
            Line(LogType.Error, "{0}: {1}\n{2}",
                e.GetType().Name,
                e.Message,
                e.StackTrace);
        }

        public static void Line(LogType type, string message)
        {
            onLog?.Invoke(type, message);
        }

        public static bool ShouldFilter(LogType log, LogType minLevel)
        {
            return (int) log < (int) minLevel;
        }
    }
}