using SynthesisAPI.Modules;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using SynthesisAPI.AssetManager;
using SynthesisAPI.VirtualFileSystem;
using ILogger = SynthesisAPI.Utilities.ILogger;

namespace Engine
{
    /// <summary>
    /// Logs to the toast feed
    /// </summary>
    public class ToastLogger : ILogger
    {
        /// <summary>
        /// Represents a single toast notification
        /// </summary>
        private struct Toast
        {
            /// <summary>
            /// Cutoff point for lines in toast notification
            /// </summary>
            public const int CharsPerToastLine = 35;
            /// <summary>
            /// Height of one line of toast text in px
            /// </summary>
            public const int LineHeight = 25; // px
            /// <summary>
            /// Height of padding around a toast in px
            /// </summary>
            public const int PaddingHeight = 10; // px

            /// <summary>
            /// Raw text from Log call
            /// </summary>
            public readonly string RawText;
            /// <summary>
            /// Formatted text
            /// </summary>
            public readonly List<string> Lines;
            /// <summary>
            /// Log level of the log call
            /// </summary>
            public readonly LogLevel LogLevel;
            /// <summary>
            /// Tooltip on hover
            /// </summary>
            public readonly string Tooltip;

            public Toast(string content, LogLevel logLevel, string tooltip)
            {
                RawText = content;
                LogLevel = logLevel;
                Tooltip = tooltip;
                Lines = Text.SplitLines(RawText, CharsPerToastLine);
            }
        }

        private bool debugLogsEnabled = true;
        public static bool IsInitialized { get; private set; } = false;
        private static bool currentlyLogging = false;
        private static bool scrollToBottom = false;

        private static List<Toast> toastList = new List<Toast>();
        private static int currentToastID = 0;

        private static VisualElement toastContainer;
        private static ScrollView toastScrollView;
        private static VisualElement toastFeed;
        private static VisualElement toastScrollViewBottom;

        private static void SendToast(Toast toast)
        {
            toastList.Add(toast);
            if (!IsInitialized)
                Setup();

            #region MakeElement

            // Create a toast visual element

            var toastElement = new VisualElement();

            toastElement.SetStyleProperty("flex-direction", "row");
            toastElement.SetStyleProperty("align-content", "center");
            toastElement.SetStyleProperty("padding-top", "5px");
            toastElement.SetStyleProperty("padding-bottom", "5px");
            toastElement.SetStyleProperty("padding-left", "5px");
            toastElement.SetStyleProperty("padding-right", "5px");
            toastElement.SetStyleProperty("margin-bottom", "10px");
            toastElement.SetStyleProperty("background-color", "rgb(255, 255, 255)");
            var heightNoPadding = Toast.LineHeight * toast.Lines.Count;
            var height = Toast.PaddingHeight + heightNoPadding;

            toastElement.SetStyleProperty("height", height.ToString() + "px");
            toastElement.SetStyleProperty("max-height", height.ToString() + "px");
            toastElement.SetStyleProperty("min-height", height.ToString() + "px");

            #region MakeIcon
            var icon = new VisualElement();
            icon.SetStyleProperty("height", "25px");
            icon.SetStyleProperty("min-height", "25px");
            icon.SetStyleProperty("max-height", "25px");
            icon.SetStyleProperty("width", "25px");
            icon.SetStyleProperty("min-width", "25px");
            icon.SetStyleProperty("max-width", "25px");
            icon.SetStyleProperty("border-top-width", "0px");
            icon.SetStyleProperty("border-bottom-width", "0px");
            icon.SetStyleProperty("border-left-width", "0px");
            icon.SetStyleProperty("border-right-width", "0px");

            switch (toast.LogLevel) // Log-level-specific formatting
            {
                case LogLevel.Debug:
                    {
                        icon.SetStyleProperty("background-image", "/runtime/wrench-icon.png");
                        toastElement.SetStyleProperty("background-color", "rgba(187, 187, 187, 1)");
                        break;
                    }
                case LogLevel.Warning:
                    {
                        icon.SetStyleProperty("background-image", "/runtime/warning-icon-white-solid.png");
                        toastElement.SetStyleProperty("background-color", "rgba(255, 165, 0, 1)");
                        break;
                    }
                case LogLevel.Error:
                    {
                        icon.SetStyleProperty("background-image", "/runtime/error-icon-white-solid.png");
                        toastElement.SetStyleProperty("background-color", "rgba(255, 24, 66, 1)");
                        break;
                    }
                default:
                case LogLevel.Info:
                    {
                        icon.SetStyleProperty("background-image", "/runtime/info-icon-white-solid.png");
                        toastElement.SetStyleProperty("background-color", "rgba(0, 173, 222, 1)");
                        break;
                    }
            }

            toastElement.Add(icon);

            #endregion

            #region MakeTextArea

            // Create the toast text area, a vertical flex box of labels containing each line of the toast

            var textArea = new VisualElement();

            textArea.SetStyleProperty("flex-direction", "column");
            textArea.SetStyleProperty("justify-content", "center");
            textArea.SetStyleProperty("margin-left", "5px");
            textArea.SetStyleProperty("margin-right", "5px");
            textArea.SetStyleProperty("height", heightNoPadding.ToString() + "px");
            textArea.SetStyleProperty("max-height", heightNoPadding.ToString() + "px");
            textArea.SetStyleProperty("min-height", heightNoPadding.ToString() + "px");
            textArea.SetStyleProperty("width", "100%");
            textArea.SetStyleProperty("background-color", "rgba(0, 0, 0, 0)");

            // Create labels for each line of text in the toast

            for (var i = 0; i < toast.Lines.Count; i++)
            {
                var label = new Label
                {
                    Name = $"sel-toast-label-{currentToastID}-{i}",
                    Text = toast.Lines[i]
                };
                label.SetStyleProperty("height", Toast.LineHeight.ToString() + "px");
                label.SetStyleProperty("width", "100%");
                label.SetStyleProperty("background-color", "rgba(0, 0, 0, 0)");
                label.SetStyleProperty("font-size", "20");
                label.SetStyleProperty("color", "rgb(255, 255, 255)");
                textArea.Add(label);
            }
            
            toastElement.Add(textArea);

            #endregion

            #region MakeCloseButton

            var closeButton = new Button { Name = $"toat-close-button-{currentToastID}" };
            // closeButton.SetStyleProperty("align-self", "");
            closeButton.SetStyleProperty("height", "25px");
            closeButton.SetStyleProperty("width", "25px");
            closeButton.SetStyleProperty("background-color", "rgba(0, 0, 0, 0)");
            closeButton.SetStyleProperty("border-top-width", "0px");
            closeButton.SetStyleProperty("border-bottom-width", "0px");
            closeButton.SetStyleProperty("border-left-width", "0px");
            closeButton.SetStyleProperty("border-right-width", "0px");
            closeButton.SetStyleProperty("background-image", "/runtime/close-icon-white.png");
            closeButton.Subscribe(e =>
            {
                if (e is ButtonClickableEvent be && be.Name == closeButton.Name)
                {
                    toastFeed.Remove(toastElement);
                    toastList.Remove(toast);
                    UpdateContainerHeight();
                }
            });
            toastElement.Add(closeButton);

            #endregion
            toastFeed.Add(toastElement);

            #endregion

            scrollToBottom = true; // Scroll on next frame (wait for UI to update positions first)
            currentToastID++;
            UpdateContainerHeight();
        }

        private static void Setup()
        {
            if (IsInitialized)
                return;

            
            // Load icons into virtual file system
            var closeIconStream = File.OpenRead("Assets/Resources/UI/Toasts/close-icon-white.png");
            AssetManager.Import<SpriteAsset>("image/sprite", closeIconStream, "/runtime", "close-icon-white.png", Permissions.PublicReadOnly, "");
            var wrenchStream = File.OpenRead("Assets/Resources/UI/Toasts/wrench-icon.png");
            AssetManager.Import<SpriteAsset>("image/sprite", wrenchStream, "/runtime", "wrench-icon.png", Permissions.PublicReadOnly, "");
            var warningStream = File.OpenRead("Assets/Resources/UI/Toasts/warning-icon-white-solid.png");
            AssetManager.Import<SpriteAsset>("image/sprite", warningStream, "/runtime", "warning-icon-white-solid.png", Permissions.PublicReadOnly, "");
            var errorStream = File.OpenRead("Assets/Resources/UI/Toasts/error-icon-white-solid.png");
            AssetManager.Import<SpriteAsset>("image/sprite", errorStream, "/runtime", "error-icon-white-solid.png", Permissions.PublicReadOnly, "");
            var infoStream = File.OpenRead("Assets/Resources/UI/Toasts/info-icon-white-solid.png"); 
            AssetManager.Import<SpriteAsset>("image/sprite", infoStream, "/runtime", "info-icon-white-solid.png", Permissions.PublicReadOnly, "");
           
            toastContainer = UIManager.RootElement.Get("toast-notification-container");
            toastFeed = UIManager.RootElement.Get("toast-feed");
            toastScrollView = (ScrollView)UIManager.RootElement.Get("toast-scroll-view");

            // This invisible element is used to scroll to the bottom of the scroll view
            toastScrollViewBottom = new VisualElement { Name = "toast-list-container-bottom" };
            toastScrollViewBottom.SetStyleProperty("width", "100%");
            toastScrollViewBottom.SetStyleProperty("height", "1px");
            toastScrollViewBottom.SetStyleProperty("background-color", "rgba(0, 0, 0, 0)");
            toastScrollView.Add(toastScrollViewBottom);

            IsInitialized = true;
        }

        private static void UpdateContainerHeight()
        {
            // Updating the main toast container height is needed in order to give it the appearance that it grows
            // from the bottom of the screen. Other methods to do this didn't work.

            var height = 0;
            foreach (var toast in toastList)
            {
                height += toast.Lines.Count * Toast.LineHeight + Toast.PaddingHeight;
            }
            toastContainer.SetStyleProperty("height", height.ToString() + "px");
        }

        /// <summary>
        /// Scroll to the bottom of the toast feed
        /// </summary>
        public static void ScrollToBottom()
        {
            if (scrollToBottom)
            {
                toastScrollView.ScrollTo(toastScrollViewBottom);
                scrollToBottom = false;
            }
        }

        public void Log(object o, LogLevel logLevel = LogLevel.Info, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (!currentlyLogging && ModuleLoader.Api.IsMainThread)
            {
                currentlyLogging = true;
                if (logLevel != LogLevel.Debug || debugLogsEnabled)
                {
                    var type = new StackTrace().GetFrame(2).GetMethod().DeclaringType;
                    var tooltip = $"{ModuleManager.GetDeclaringModule(type)}\\{filePath.Split('\\').Last()}:{lineNumber}";
                    var msg = new Toast(o.ToString(), logLevel, tooltip);
                    SendToast(msg);
                }
                currentlyLogging = false;
            }

        }

        public void SetEnableDebugLogs(bool enable)
        {
            debugLogsEnabled = enable;
        }
    }
}
