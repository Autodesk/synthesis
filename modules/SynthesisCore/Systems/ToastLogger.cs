using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SynthesisCore.Systems
{
    public class ToastLogger : ILogger
    {
        private struct Toast
        {
            public const int CharsPerToastLine = 35;
            public const int LineHeight = 25; // px
            public const int PaddingHeight = 15; // px

            public readonly string RawText;
            public readonly List<string> Lines;
            public readonly LogLevel LogLevel;

            public Toast(string content, LogLevel logLevel)
            {
                RawText = content;
                LogLevel = logLevel;
                Lines = Utilities.Text.SplitLines(RawText, CharsPerToastLine);
            }
        }

        private bool debugLogsEnabled = false;
        private static bool initialized = false;
        private static bool currentlyLogging = false;
        private static bool scrollToBottom = false;

        private static VisualElement toastFeed = new VisualElement();
        private static List<Toast> toastList = new List<Toast>();
        private static int currentToastID = 0;
        private static VisualElement toastContainer;
        private static ScrollView toastScrollView;
        private static VisualElement toastScrollViewBottom;

        private static void SendToast(Toast toast)
        {
            if (!initialized)
                Setup();

            toastList.Add(toast);

            #region MakeElement

            var element = new VisualElement();
            
            element.SetStyleProperty("flex-direction", "row");
            element.SetStyleProperty("padding-top", "5px");
            element.SetStyleProperty("padding-bottom", "5px");
            element.SetStyleProperty("padding-left", "5px");
            element.SetStyleProperty("padding-right", "5px");
            element.SetStyleProperty("margin-bottom", "10px");
            var height = Toast.PaddingHeight + Toast.LineHeight * toast.Lines.Count;

            element.SetStyleProperty("height", height.ToString() + "px");
            element.SetStyleProperty("max-height", height.ToString() + "px");
            element.SetStyleProperty("min-height", height.ToString() + "px");

            #region MakeTextArea

            var textArea = new VisualElement();

            textArea.SetStyleProperty("flex-direction", "column");
            textArea.SetStyleProperty("margin-right", "5px");
            textArea.SetStyleProperty("height", height.ToString() + "px");
            textArea.SetStyleProperty("max-height", height.ToString() + "px");
            textArea.SetStyleProperty("min-height", height.ToString() + "px");
            textArea.SetStyleProperty("width", "100%");
            //textArea.SetStyleProperty("max-width", "100%");
            //textArea.SetStyleProperty("min-width", "390px");
            textArea.SetStyleProperty("background-color", "rgba(0, 0, 0, 0)");

            switch (toast.LogLevel)
            {
                case LogLevel.Debug:
                    {
                        element.SetStyleProperty("background-color", "rgba(187, 187, 187, 1)");
                        break;
                    }
                case LogLevel.Warning:
                    {
                        element.SetStyleProperty("background-color", "rgba(255, 165, 0, 1)");
                        break;
                    }
                case LogLevel.Error:
                    {
                        element.SetStyleProperty("background-color", "rgba(255, 24, 66, 1)");
                        break;
                    }
                default:
                case LogLevel.Info:
                    {
                        element.SetStyleProperty("background-color", "rgba(0, 173, 222, 1)");
                        break;
                    }
            }

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
            closeButton.SetStyleProperty("height", "25px");
            closeButton.SetStyleProperty("width", "25px");
            closeButton.SetStyleProperty("background-color", "rgba(0, 0, 0, 0)");
            closeButton.Text = "X"; // TODO replace with image
                                    // closeButton.SetStyleProperty("background-image", "url(&apos;/Assets/UI/Toolbar/Icons/add-icon.png&apos;)");
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

            toastScrollViewBottom = new VisualElement { Name = "toast-list-container-bottom" };
            toastScrollViewBottom.SetStyleProperty("width", "100%");
            toastScrollViewBottom.SetStyleProperty("height", "1px");
            toastScrollViewBottom.SetStyleProperty("background-color", "rgba(0, 0, 0, 0)");
            toastScrollView.Add(toastScrollViewBottom);

            initialized = true;
        }

        private static void UpdateContainerHeight()
        {
            var height = 0;
            foreach (var toast in toastList)
            {
                height += toast.Lines.Count * Toast.LineHeight + Toast.PaddingHeight;
            }
            toastContainer.SetStyleProperty("height", height.ToString() + "px");
        }

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
