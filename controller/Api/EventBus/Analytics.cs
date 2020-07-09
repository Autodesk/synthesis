using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SynthesisAPI.EventBus
{
    public static class Analytics
    {
   
        public const string URL_COLLECT = "https://www.google-analytics.com/collect";
        public const string URL_BATCH = "https://www.google-analytics.com/batch";
        public const string OFFICIAL_TRACKING_ID = "UA-81892961-3";
        public const float DUMP_DELAY = 5;
        public static string Guid = "not set";
        public static bool dataCollection = true;

        public static void SetUnityPrefs(String unityGuid, bool dataPref)
        {
            Guid = unityGuid;
            dataCollection = dataPref;
        }

        public static void LogAllTagggedEvents(String tag, string Category, String Action, string Label, string Value)
        {
            EventBus.NewTagListener(tag, (IEvent e) => LogEventAsync(Category, Action, Label, Value));
        }

        public static void LogAllTypeEvents<TEvent>(string Category, String Action, string Label, string Value) where TEvent : IEvent
        {
            EventBus.NewTypeListener<TEvent>((IEvent e) => LogEventAsync(Category, Action, Label, Value));
        }

        #region AsyncMethods
        public static async void LogEventAsync(string Category, string Action, string Label, string Value)
        {
            if (mutex != null) await LogEvent(Category, Action, Label, Value);
        }

        public static async void LogScreenViewAsync(string ScreenName)
        {
            if (mutex != null) await LogScreenView(ScreenName);
        }

        public static async void LogPageViewAsync(string Title)
        {
            if (mutex != null) await LogPageView(Title);
        }

        public static async void UploadDumpAsync()
        {
            if (mutex != null) await UploadDump();
        }

        #endregion

        #region LogTasks

        public static void LogStandardInfo()
        {
            LoggedData.Enqueue(new KeyValuePair<string, string>("v", "1"));
            LoggedData.Enqueue(new KeyValuePair<string, string>("tid", OFFICIAL_TRACKING_ID));
            LoggedData.Enqueue(new KeyValuePair<string, string>("cid", Guid));
        }

        private static Task LogEvent(string Category, string Action, string Label, string Value)
        {
            return Task.Factory.StartNew(() =>
            {
                mutex.WaitOne();
                LogStandardInfo();
                LoggedData.Enqueue(new KeyValuePair<string, string>("t", "event"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("ec", Category));
                LoggedData.Enqueue(new KeyValuePair<string, string>("ea", Action));
                if (Label != null) LoggedData.Enqueue(new KeyValuePair<string, string>("el", Label));
                if (Value != null) LoggedData.Enqueue(new KeyValuePair<string, string>("ev", Value));
                LoggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
                mutex.ReleaseMutex();
            });
        }

        private static Task LogScreenView(string ScreenName)
        {
            return Task.Factory.StartNew(() =>
            {
                mutex.WaitOne();
                LogStandardInfo();
                LoggedData.Enqueue(new KeyValuePair<string, string>("t", "screenname"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("dl", "https://www.google.com/index.html"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
                mutex.ReleaseMutex();
            });
        }

        private static Task LogPageView(string Title)
        {
            return Task.Factory.StartNew(() =>
            {
                mutex.WaitOne();
                LogStandardInfo();
                LoggedData.Enqueue(new KeyValuePair<string, string>("t", "pageview"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("dl", "http://test.com/" + Title));
                LoggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
                mutex.ReleaseMutex();
            });
        }

        private static Task LogTiming(string Category, string Var, int Time, string Label)
        {
            return Task.Factory.StartNew(() =>
            {
                mutex.WaitOne();
                LogStandardInfo();
                LoggedData.Enqueue(new KeyValuePair<string, string>("t", "timing"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("utc", Category));
                LoggedData.Enqueue(new KeyValuePair<string, string>("utv", Var));
                LoggedData.Enqueue(new KeyValuePair<string, string>("utt", Time.ToString()));
                LoggedData.Enqueue(new KeyValuePair<string, string>("utl", Label));
                LoggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
                mutex.ReleaseMutex();
            });
        }

        private static Task UploadDump()
        {
            return Task.Factory.StartNew(() =>
            {
                mutex.WaitOne();
                if (LoggedData.Count < 1 || !dataCollection)
                {
                    LoggedData.Clear();
                    mutex.ReleaseMutex();
                    return;
                }

                string data = "";

                Queue<KeyValuePair<string, string>> loggedCopy = new Queue<KeyValuePair<string, string>>(LoggedData);
                LoggedData.Clear();
                mutex.ReleaseMutex();

                bool batchSend = false;
                bool lastEnd = false;
                while (loggedCopy.Count > 0)
                {
                    KeyValuePair<string, string> pair = loggedCopy.Dequeue();
                    if (pair.Key != null)
                    {
                        if (pair.Key.Equals("NEW"))
                        {
                            data += "\n";
                            batchSend = true;
                            lastEnd = true;
                        }
                        else
                        {
                            if ((data != "") && !lastEnd) data += "&";

                            data += pair.Key + "=" + pair.Value;
                            lastEnd = false;
                        }
                    }

                }

                string result;

                try
                {
                    if (batchSend)
                    {
                        result = client.UploadString(URL_BATCH, "POST", data);
                    }
                    else
                    {
                        result = client.UploadString(URL_COLLECT, "POST", data);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    //Debug.Log(e.ToString());
                }
            });
        }

        #endregion

        public static void CleanUp()
        {
            mutex.Close();
        }

        private class Inner
        {


            public Mutex mutex;

            public WebClient client;
            public Queue<KeyValuePair<string, string>> LoggedData;


            private Inner()
            {
                mutex = new Mutex();
                LoggedData = new Queue<KeyValuePair<string, string>>();
            }

            private static Inner? _inst;
            public static Inner InnerInstance => _inst ??= new Inner();
        }

        private static Queue<KeyValuePair<string, string>> LoggedData => Instance.LoggedData;
        private static WebClient client => Instance.client;
        private static Mutex mutex => Instance.mutex;
        private static Inner Instance => Inner.InnerInstance;
    }
}