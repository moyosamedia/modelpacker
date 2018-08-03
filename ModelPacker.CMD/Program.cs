using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using ModelPacker.Logger;
using ModelPacker.Processor;

namespace ModelPacker.CMD
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Log.onLog = OnLog;

            Parser parser = new Parser(with =>
            {
                with.EnableDashDash = true;
                with.CaseInsensitiveEnumValues = true;
                with.CaseSensitive = false;
                with.EnableDashDash = true;
                with.HelpWriter = Console.Out;
            });
            try
            {
                parser.ParseArguments<ArgumentOptions>(args)
                    .WithParsed(x => Processor.Processor.Run(x))
                    .WithNotParsed(HandleParseErrors);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
            finally
            {
                parser.Dispose();
            }
        }

        private static void HandleParseErrors(IEnumerable<Error> errors)
        {
//            Log.Line(LogType.Error, "Failed to parse arguments:");
//            foreach (Error error in errors)
//            {
//                Log.Line(error.StopsProcessing ? LogType.Error : LogType.Warning, error.Tag);
//            }
        }

        private static void OnLog(LogType type, string message)
        {
            if (Log.ShouldFilter(type, LogType.Debug))
                return;

            switch (type)
            {
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }

            Console.WriteLine("[{0}]: {1}", type, message);
            Console.ResetColor();
        }
    }
}