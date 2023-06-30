using SynthesisAPI.Modules;
using SynthesisAPI.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using ILogger = SynthesisAPI.Utilities.ILogger;
using UnityEngine;

namespace Engine {
    /// <summary>
    /// Logs to the toast feed
    /// </summary>
    public class ToastLogger : ILogger {
        /// <summary>
        /// Represents a single toast notification
        /// </summary>
        private struct Toast {
            /// <summary>
            /// Raw text from Log call
            /// </summary>
            public readonly string RawText;
            /// <summary>
            /// Formatted text
            /// </summary>
            public readonly LogLevel LogLevel;
            /// <summary>
            /// Tooltip on hover
            /// </summary>
            public readonly string Tooltip;

            public Toast(string content, LogLevel logLevel, string tooltip) {
                RawText  = content;
                LogLevel = logLevel;
                Tooltip  = tooltip;
            }
        }

        private bool debugLogsEnabled        = true;
        private static bool currentlyLogging = false;

        private static List<Toast> toastList = new List<Toast>();
        private static int currentToastID    = 0;

        private static GameObject toastObject;
        private static Transform scrollTransform;
        private static ToastManager toastManager;

        private static void SendToast(Toast toast) {
            toastList.Add(toast);
            currentToastID++;
            var initiatedToast = GameObject.Instantiate(toastObject, scrollTransform);
            initiatedToast.GetComponentInChildren<ToastConfig>().Init(toast.RawText, toast.LogLevel, toastManager);
        }

        public void Log(object o, LogLevel logLevel = LogLevel.Info, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
            if (!currentlyLogging && ModuleLoader.Api.IsMainThread) {
                currentlyLogging = true;
                if (logLevel != LogLevel.Debug || debugLogsEnabled) {
                    var type = new StackTrace().GetFrame(2).GetMethod().DeclaringType;
                    var tooltip =
                        $"{ModuleManager.GetDeclaringModule(type)}\\{filePath.Split('\\').Last()}:{lineNumber}";
                    var msg = new Toast(o.ToString(), logLevel, tooltip);
                    SendToast(msg);
                }
                currentlyLogging = false;
                toastManager.onAddToast();
            }
        }

        public void SetUpToastObject(GameObject o, Transform t, ToastManager m) {
            toastObject     = o;
            scrollTransform = t;
            toastManager    = m;
        }

        public void SetEnableDebugLogs(bool enable) {
            debugLogsEnabled = enable;
        }
    }
}
