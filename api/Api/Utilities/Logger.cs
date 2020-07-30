using System;
using SynthesisAPI.Runtime;

namespace SynthesisAPI.Utilities
{
    public static class Logger
    {
        public static void Log(object o, LogLevel logLevel = LogLevel.Info, string memberName = "", string filePath = "", int lineNumber = 0)
        {
            ApiProvider.Log(o, logLevel, memberName, filePath, lineNumber);
        }
    }
}
