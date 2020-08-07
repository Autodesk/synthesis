using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SynthesisCore.Systems
{
    public class ToastLogger : ILogger
    {
        private struct Toast
        {
            public const int CharsPerToastLine = 45;
            public const int LineHeight = 20;
            public const int PaddingHeight = 20;
            private const string Ellipsis = "...";

            public readonly string Content;
            public readonly string AdjustedContent;
            public readonly int LineCount;
            public readonly LogLevel LogLevel;

            public Toast(string content, LogLevel logLevel)
            {
                Content = content;
                LogLevel = logLevel;


                // AdjustedContent = Content.Length > CharsPerToastLine ? Content.Substring(0, CharsPerToastLine - Ellipsis.Length) + Ellipsis : Content;
                
                AdjustedContent = "";
                LineCount = 0;
                var last_i = -1;
                for (var i = 0; i < content.Length;)
                {
                    if(last_i != -1)
                    {
                        AdjustedContent += string.Concat(Enumerable.Repeat(" ", CharsPerToastLine * 2 - (i - last_i))); // CharsPerToastLine * 2 because adding extra spaces doesn't matter. Unity trims them after it wraps the line
                    }
                    last_i = i;
                    var next = Utilities.Math.Min(CharsPerToastLine, content.Length - i);
                    var newline = content.IndexOf('\n', i, next);
                    if (newline != -1)
                    {
                        AdjustedContent += content.Substring(i, newline - i);
                        i = newline + 1;
                    }
                    else
                    {
                        AdjustedContent += content.Substring(i, next);
                        i += next;
                    }
                    LineCount++;
                }
                Logger.Log(AdjustedContent, LogLevel.Debug);
                Logger.Log(LineCount, LogLevel.Debug);
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
            var height = 0; // toastList.Count * toastListView.ItemHeight
            foreach (var i in toastList)
            {
                height += Toast.PaddingHeight + Toast.LineHeight * i.LineCount;
            }
            toastListContainer.SetStyleProperty("height", height.ToString() + "px");
           
            // toastListContainer.SetStyleProperty("height", (toastList.Count * toastListView.ItemHeight).ToString() + "px");
        }

        private static void BindToast(VisualElement element, int index)
        {
            try
            {
                var textField = element as TextField;

                textField.IsMultiline = true;
                textField.IsReadOnly = true;
                textField.Name = $"sel-toast-{currentToastID}";
                textField.Value = toastList[index].AdjustedContent;

                textField.AddToClassList("toast-notification");
                var height = Toast.PaddingHeight + Toast.LineHeight * toastList[index].LineCount;
                textField.SetStyleProperty("height", height.ToString() + "px");
                textField.SetStyleProperty("max-height", height.ToString() + "px");

                switch (toastList[index].LogLevel)
                {
                    case LogLevel.Debug:
                        {
                            textField.SetStyleProperty("background-color", "rgba(187, 187, 187, 1)");
                            break;
                        }
                    case LogLevel.Warning:
                        {
                            textField.SetStyleProperty("background-color", "rgba(255, 165, 0, 1)");
                            break;
                        }
                    case LogLevel.Error:
                        {
                            textField.SetStyleProperty("background-color", "rgba(255, 24, 66, 1)");
                            break;
                        }
                    default:
                    case LogLevel.Info:
                        {
                            textField.SetStyleProperty("background-color", "rgba(0, 173, 222, 1)");
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Here ex", e);
            }
        }

		private static void Setup()
        {
            try
            {
                if (initialized)
                    return;

                toastListContainer = UIManager.RootElement.Get("toast-notification-container");

                // Position at bottom of container
                toastListView.SetStyleProperty("position", "absolute");
                toastListView.SetStyleProperty("bottom", "0");
                toastListView.SetStyleProperty("width", "100%");
                toastListView.SetStyleProperty("height", "100%");

                toastListView.ItemHeight = 55; // 55 // sum of max-height and margin for elements

                toastListView.Populate(toastList, () => new TextField(), BindToast);

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
            catch (Exception e)
            {
                throw new System.Exception("Toast setup", e);
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
