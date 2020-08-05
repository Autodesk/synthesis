using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace SynthesisAPI.Utilities
{
    public static class Logger
    {
        private static ILogger? Instance => Inner.Instance;

        public static void RegisterLogger(ILogger logger)
        {
            if (Inner.Instance != null)
            {
                throw new Exception("Attempt to register multiple logger instances");
            }

            Inner.Instance = logger;
        }

        public static void Log(object o, LogLevel logLevel = LogLevel.Info, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            Instance?.Log(o, logLevel, memberName, filePath, lineNumber);
        }

        public static void SetEnableDebugLogs(bool enable) => Instance?.SetEnableDebugLogs(enable);

        private static class Inner
        {
            static Inner() { }
            internal static ILogger? Instance;
        }
    }
}
