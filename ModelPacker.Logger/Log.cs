using JetBrains.Annotations;

namespace ModelPacker.Logger
{
    public static class Log
    {
        public delegate void LogHandler(LogType type, string message);

        public static LogHandler onLog;

        [StringFormatMethod("format")]
        public static void Line(LogType type, string format, params object[] args)
        {
            Line(type, string.Format(format, args));
        }

        public static void Line(LogType type, string message)
        {
            onLog?.Invoke(type, message);
        }
    }
}