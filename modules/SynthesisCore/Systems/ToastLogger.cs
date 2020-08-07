using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SynthesisCore.Systems
{
    /// <summary>
    /// Logs to the toast feed in SynthesisCore
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

            public Toast(string content, LogLevel logLevel)
            {
                RawText = content;
                LogLevel = logLevel;
                Lines = Utilities.Text.SplitLines(RawText, CharsPerToastLine);
            }
        }

        private bool debugLogsEnabled = true;
        private static bool initialized = false;
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
            if (!initialized)
                Setup();

            toastList.Add(toast);

            #region MakeElement

            // Create a toast visual element

            var element = new VisualElement();
            
            element.SetStyleProperty("flex-direction", "row");
            element.SetStyleProperty("align-content", "center");
            element.SetStyleProperty("padding-top", "5px");
            element.SetStyleProperty("padding-bottom", "5px");
            element.SetStyleProperty("padding-left", "5px");
            element.SetStyleProperty("padding-right", "5px");
            element.SetStyleProperty("margin-bottom", "10px");
            element.SetStyleProperty("background-color", "rgb(255, 255, 255)");
            var heightNoPadding = Toast.LineHeight * toast.Lines.Count;
            var height = Toast.PaddingHeight + heightNoPadding;

            element.SetStyleProperty("height", height.ToString() + "px");
            element.SetStyleProperty("max-height", height.ToString() + "px");
            element.SetStyleProperty("min-height", height.ToString() + "px");

            #region MakeIcon

            var icon = new VisualElement();
            icon.SetStyleProperty("height", "25px");
            icon.SetStyleProperty("min-height", "25px");
            icon.SetStyleProperty("max-height", "25px");
            icon.SetStyleProperty("width", "25px");
            icon.SetStyleProperty("min-width", "25px");
            icon.SetStyleProperty("max-width", "25px");
            icon.SetStyleProperty("background-image", "/modules/synthesis_core/UI/images/add-icon.png");
            icon.SetStyleProperty("border-top-width", "0px");
            icon.SetStyleProperty("border-bottom-width", "0px");
            icon.SetStyleProperty("border-left-width", "0px");
            icon.SetStyleProperty("border-right-width", "0px");

            switch (toast.LogLevel) // Log-level-specific formatting
            {
                case LogLevel.Debug:
                    {
                        icon.SetStyleProperty("background-image", "/modules/synthesis_core/UI/images/wrench-icon.png");
                        element.SetStyleProperty("background-color", "rgba(187, 187, 187, 1)");
                        break;
                    }
                case LogLevel.Warning:
                    {
                        icon.SetStyleProperty("background-image", "/modules/synthesis_core/UI/images/warning-icon-white-solid.png");
                        element.SetStyleProperty("background-color", "rgba(255, 165, 0, 1)");
                        break;
                    }
                case LogLevel.Error:
                    {
                        icon.SetStyleProperty("background-image", "/modules/synthesis_core/UI/images/error-icon-white-solid.png");
                        element.SetStyleProperty("background-color", "rgba(255, 24, 66, 1)");
                        break;
                    }
                default:
                case LogLevel.Info:
                    {
                        icon.SetStyleProperty("background-image", "/modules/synthesis_core/UI/images/info-icon-white-solid.png");
                        element.SetStyleProperty("background-color", "rgba(0, 173, 222, 1)");
                        break;
                    }
            }

            element.Add(icon);

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
            
            element.Add(textArea);

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
            closeButton.SetStyleProperty("background-image", "/modules/synthesis_core/UI/images/close-icon-white.png");
            closeButton.Subscribe(e =>
            {
                if (e is ButtonClickableEvent be && be.Name == closeButton.Name)
                {
                    toastFeed.Remove(element);
                    toastList.Remove(toast);
                    UpdateContainerHeight();
                }
            });
            element.Add(closeButton);

            #endregion

            toastFeed.Add(element);

            #endregion

            scrollToBottom = true; // Scroll on next frame (wait for UI to update positions first)
            currentToastID++;
            UpdateContainerHeight();
        }

        private static void Setup()
        {
            if (initialized)
                return;

            toastContainer = UIManager.RootElement.Get("toast-notification-container");
            toastFeed = UIManager.RootElement.Get("toast-feed");
            toastScrollView = (ScrollView)UIManager.RootElement.Get("toast-scroll-view");

            // This invisible element is used to scroll to the bottom of the scroll view
            toastScrollViewBottom = new VisualElement { Name = "toast-list-container-bottom" };
            toastScrollViewBottom.SetStyleProperty("width", "100%");
            toastScrollViewBottom.SetStyleProperty("height", "1px");
            toastScrollViewBottom.SetStyleProperty("background-color", "rgba(0, 0, 0, 0)");
            toastScrollView.Add(toastScrollViewBottom);

            initialized = true;
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
            if (!currentlyLogging)
            {
                currentlyLogging = true;
                if (logLevel != LogLevel.Debug || debugLogsEnabled)
                { 
                    var msg = new Toast(o.ToString(), logLevel);
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
