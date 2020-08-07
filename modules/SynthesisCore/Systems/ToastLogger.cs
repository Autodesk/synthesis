using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SynthesisCore.Systems
{
    public class ToastLogger : ILogger
    {
        private struct Toast
        {
            public const int CharsPerToastLine = 45;
            private const string Ellipsis = "...";

            public readonly string Content;
            public readonly string AdjustedContent;
            public readonly LogLevel LogLevel;

            public Toast(string content, LogLevel logLevel)
            {
                Content = content;
                LogLevel = logLevel;


                AdjustedContent = Content.Length > CharsPerToastLine ? Content.Substring(0, CharsPerToastLine - Ellipsis.Length) + Ellipsis : Content;

                /*
                // TODO label's don't support new lines in Unity
                for (var i = 0; i < content.Length; i += CharsPerToastLine)
                {
                    AdjustedContent += content.Substring(i, Utilities.Math.Min(CharsPerToastLine, content.Length - i)) + "\n";
                }
                */
            }
        }

        private bool debugLogsEnabled = false;
        private static bool initialized = false;
        private static bool currentlyLogging = false;

        private static ListView toastListView = new ListView();
        private static List<Toast> toastList = new List<Toast>();
        private static int currentToastID = 0;
        private static VisualElement toastListContainer;

        private static void SendToast(Toast toast)
        {
            toastList.Add(toast);
            if (!initialized)
                Setup();
            UpdateContainerHeight();
            toastListView.Refresh();
            toastListView.ScrollToItem(toastList.Count); // TODO does not scroll all the way down for some reason
            toastListView.Refresh();
            currentToastID++;
        }

        private static void UpdateContainerHeight()
        {
            toastListContainer.SetStyleProperty("height", (toastList.Count * toastListView.ItemHeight).ToString() + "px");
        }

		private static void Setup()
        {
            if (initialized)
                return;

            toastListContainer = UIManager.RootElement.Get("toast-notification-container");

            // Position at bottom of container
            toastListView.SetStyleProperty("position", "absolute");
            toastListView.SetStyleProperty("bottom", "0");
            toastListView.SetStyleProperty("width", "100%");
            toastListView.SetStyleProperty("height", "100%");

            toastListView.ItemHeight = 55; // sum of max-height and margin for elements
            
            toastListView.Populate(toastList,
                    () => new Label(),
                    (element, index) =>
                    {
                        var label = element as Label;
                        label.Name = $"sel-toast-{currentToastID}";
                        label.Text = toastList[index].AdjustedContent;

                        label.SetStyleProperty("height", "45");
                        label.SetStyleProperty("max-height", "45");
                        label.SetStyleProperty("font-size", "20px");
                        label.SetStyleProperty("color", "rgb(255, 255, 255)");

                        label.SetStyleProperty("padding-top", "10px");
                        label.SetStyleProperty("padding-bottom", "5px");
                        label.SetStyleProperty("padding-left", "5px");
                        label.SetStyleProperty("padding-right", "5px");

                        label.SetStyleProperty("margin-bottom", "10px");
                        switch (toastList[index].LogLevel)
                        {
                            case LogLevel.Debug:
                                {
                                    label.SetStyleProperty("background-color", "rgba(187, 187, 187, 1)");
                                    break;
                                }
                            case LogLevel.Warning:
                                {
                                    label.SetStyleProperty("background-color", "rgba(255, 165, 0, 1)");
                                    break;
                                }
                            case LogLevel.Error:
                                {
                                    label.SetStyleProperty("background-color", "rgba(255, 24, 66, 1)");
                                    break;
                                }
                            default:
                            case LogLevel.Info:
                                {
                                    label.SetStyleProperty("background-color", "rgba(0, 173, 222, 1)");
                                    break;
                                }
                        }
                    }
                );
            
            toastListView.SubscribeOnSelectionChanged(e =>
            {
                if (e is ListView.SelectionChangedEvent selectionChangedEvent && selectionChangedEvent.SelectedObjects.Count > 0)
                {
                    toastList.RemoveAt(toastListView.SelectedIndex);
                    toastListView.Refresh();
                    toastListView.ClearSelection();
                    UpdateContainerHeight();
                }
            });

            toastListView.SubscribeOnItemChosen(e =>
            {
                if (e is ListView.ItemChosenEvent itemChosenEvent)
                {
                    // TODO show whole Toast.Content maybe
                }
            });

            toastListContainer.Add(toastListView);
            initialized = true;
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
