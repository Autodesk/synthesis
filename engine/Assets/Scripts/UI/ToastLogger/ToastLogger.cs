using System;
using SynthesisAPI.Utilities;
using System.Runtime.CompilerServices;
using Synthesis.UI.Dynamic;
using Synthesis.Util;
using ILogger = SynthesisAPI.Utilities.ILogger;
using Debug = UnityEngine.Debug;

#nullable enable

namespace Engine {
    /// <summary>
    /// Logs to the toast feed
    /// </summary>
    public class ToastLogger : ILogger {

        private bool _isEnabled               = true;
        private bool _debugLogsEnabled        = true;
        private static bool _currentlyLogging;

        public void Log(object o, LogLevel logLevel = LogLevel.Info, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
            if (!_currentlyLogging && ModuleLoader.Api.IsMainThread) {
                _currentlyLogging = true;
                if (logLevel != LogLevel.Debug || _debugLogsEnabled) {
                    Action<ToastHandler>? optionCallback = null;
                    if (logLevel == LogLevel.Error) {
                        optionCallback = th => {
                            DynamicUIManager.CreateModal<ToastModal>(o.ToString(), logLevel);
                            if (th.LinkedNode != null) {
                                Toaster.RemoveToast(th.LinkedNode);
                            }
                        };
                        Debug.Log("Making Optional Callback");
                    }
                    Toaster.MakeToast(o.ToString(), level: logLevel, optionalCallback: optionCallback);
                }
                _currentlyLogging = false;
            }
        }

        public void SetEnableDebugLogs(bool enable) {
            _debugLogsEnabled = enable;
        }

        public void SetEnabled(bool enabled) {
            _isEnabled = enabled;
        }

        public bool IsEnabled() => _isEnabled;
    }
}
