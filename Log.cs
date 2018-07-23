using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using JetBrains.Annotations;

namespace ModelPacker
{
    public static class Log
    {
        [StringFormatMethod("format")]
        public static void Line(LogType type, string format, params object[] args)
        {
            Line(type, string.Format(format, args));
        }
        
        public static void Line(LogType type, string message)
        {
            Write("[{0}] ", type);

            Run run = new Run(message);
            switch (type)
            {
                case LogType.Debug:
                case LogType.Info:
                    break;
                case LogType.Warning:
                    run.Foreground = Brushes.Orange;
                    break;
                case LogType.Error:
                    run.Foreground = Brushes.DarkRed;
                    run.FontWeight = FontWeights.Bold;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            Write(
                run,
                new LineBreak()
            );
        }

        [StringFormatMethod("format")]
        public static void Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
        }

        public static void Write(string message)
        {
            if (message != null)
                MainWindow.TextBlock.Inlines.Add(message);
        }

        private static void Write(Inline inline)
        {
            if (inline != null)
                Write(new[] {inline});
        }

        private static void Write(params Inline[] inlines)
        {
            if (inlines == null)
                return;

            MainWindow.TextBlock.Inlines.AddRange(inlines);
        }

        public static void Clear()
        {
            MainWindow.TextBlock.Text = string.Empty;
        }
    }
}