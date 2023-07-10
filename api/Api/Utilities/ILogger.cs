using System.Runtime.CompilerServices;

namespace SynthesisAPI.Utilities
{
    public interface ILogger
    {
        void Log(object o, LogLevel logLevel = LogLevel.Info, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);

        void SetEnableDebugLogs(bool enable);

        void SetEnabled(bool enabled);
        bool IsEnabled();
    }
}
