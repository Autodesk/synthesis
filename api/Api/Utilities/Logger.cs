using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace SynthesisAPI.Utilities
{
    public static class Logger
    {
        private static List<ILogger> Loggers => Inner.Loggers;

        public static void RegisterLogger(ILogger logger)
        {
            Loggers.Add(logger);
        }

        // TODO ability to de-register a logger

        public static void Log(object o, LogLevel logLevel = LogLevel.Info, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            foreach(var logger in Loggers)
            {
                logger.Log(o, logLevel, memberName, filePath, lineNumber);
            }
        }

        public static void SetEnableDebugLogs(bool enable) {
            foreach (var logger in Loggers) {
                if (logger.IsEnabled())
                    logger.SetEnableDebugLogs(enable);
            }
        }

        private static class Inner
        {
            static Inner() { }
            internal static List<ILogger> Loggers = new List<ILogger>();
        }
    }
}
