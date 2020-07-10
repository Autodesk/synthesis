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
        public const string OFFICIAL_TRACKING_ID = "UA-81892961-6";
        public static string Guid = "not-set";
        public static bool dataCollection = true;

        public static void SetUnityPrefs(String unityGuid, bool dataPref)
        {
            Guid = unityGuid;
            dataCollection = dataPref;
        }

        public static void LogAllTaggedEvents(String tag, string Category, String Action, string Label, string Value)
        {
            EventBus.NewTagListener(tag, (IEvent e) => LogEventAsync(Category, Action, Label, Value));
        }

        public static void LogAllTypeEvents<TEvent>(string Category, String Action, string Label, string Value) where TEvent : IEvent
        {
            EventBus.NewTypeListener<TEvent>((IEvent e) => LogEventAsync(Category, Action, Label, Value));
        }

        public static void StartTime(string label, string variable, float time)
        {
            StartTimes.Add(new KeyValuePair<string, float>(label + "|" + variable, time));
        }

        public static float GetElapsedTime(string label, string variable, float time)
        {
            float a = StartTimes.Find(x => x.Key.Equals(label + "|" + variable)).Value;
            return (time - a) * 1000;
        }

        public static void RemoveTime(string label, string variable)
        {
            StartTimes.Remove(StartTimes.Find(x => x.Key.Equals(label + "|" + variable)));
        }

        public static void UploadDumpTest()
        {
            if (LoggedData.Count < 1 || !dataCollection)
            {
                Console.WriteLine("Clearing log");
                LoggedData.Clear();
                return;
            }

            string data = "";

            Queue<KeyValuePair<string, string>> loggedCopy = new Queue<KeyValuePair<string, string>>(LoggedData);
            LoggedData.Clear();

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
                    Console.WriteLine("Posting");
                    result = client.UploadString(URL_BATCH, data);
                    Console.WriteLine(data);
                    Console.WriteLine(result);
                    Console.WriteLine("This ain't working");
                }
                else
                {
                    Console.WriteLine("Collecting");
                    result = client.UploadString(URL_COLLECT, "POST", data);
                    Console.WriteLine(data);
                    Console.WriteLine(result);
                    Console.WriteLine("This ain't working");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("jsldhfafdh");
                Console.WriteLine(e.ToString());
            }
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

        public static void LogElapsedTimeAsync(string Category, string Variable, string Label, float CurrentTime)
        {
            int milli = (int)GetElapsedTime(Label, Variable, CurrentTime);
            RemoveTime(Label, Variable);
            LogTimeAsync(Category, Variable, milli, Label);
        }

        public static async void LogTimeAsync(string Category, string Variable, int Time, string Label)
        {
            if (mutex != null) await LogTiming(Category, Variable, Time, Label);
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
                LoggedData.Enqueue(new KeyValuePair<string, string>("t", ScreenName));
                LoggedData.Enqueue(new KeyValuePair<string, string>("dl", "https://www.google.com/index.html"));
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
                    Console.WriteLine("Clearing log");
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
                        Console.WriteLine("Posting");
                        result = client.UploadString(URL_BATCH, data);
                        Console.WriteLine(data);
                        Console.WriteLine(result);
                        Console.WriteLine("This ain't working");
                    }
                    else
                    {
                        Console.WriteLine("Collecting");
                        result = client.UploadString(URL_COLLECT, "POST", data);
                        Console.WriteLine(data);
                        Console.WriteLine(result);
                        Console.WriteLine("This ain't working");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("jsldhfafdh");
                    Console.WriteLine(e.ToString());
                }
            });
        }

        #endregion

        public static void CleanUp()
        {
            mutex.Close();
        }

        /// <summary>
        /// Categories group multiple objects together. Each main category is grouped by the tabs
        /// in the simulator. Most events will fall into one of the tab categories (e.g. HomeTab.)
        /// </summary>
        public static class EventCategory
        {
            public const string

                // Main Menu has been deprecated. May consider removing or archiving MainMenu code.
                MainSimMenu = "Main Menu",
                MixAndMatchMenu = "Mix and Match Menu",
                MultiplayerMenu = "LAN Multiplayer Menu",
                MixAndMatchSimulator = "Mix and Match Simulator",

                // Start of analytics tracking
                MainSimulator = "Main Simulator",
                //MultiplayerSimulator = "multiplayerSimulator", // network multiplayer temporarily disabled.

                // Toolbar tabs
                MenuTab = "Menu Tab",
                HomeTab = "Home Tab",
                DPMTab = "Gamepiece Tab",
                ScoringTab = "Scoring Tab",
                SensorTab = "Sensor Tab",
                EmulationTab = "Emulation Tab",
                ExitTab = "Exit Tab",

                // Global categories
                AddRobot = "Add Robot",
                ChangeRobot = "Change Robot",
                LoadRobot = "Load Robot",
                ChangeField = "Change Field",
                Reset = "Reset",
                CameraView = "Camera View",
                Help = "Help Menu",
                Tutorials = "Tutorials";
        }

        /// <summary>
        /// Actions for user behaviors
        /// </summary>
        public static class EventAction
        {
            public const string
                StartSim = "Started Simulator",
                TutorialRequest = "Requested Tutorial",
                BackedOut = "Back",
                Next = "Next",
                Clicked = "Clicked",
                Added = "Added",
                Removed = "Removed",
                Edited = "Edited",
                Toggled = "Toggled",
                Viewed = "Viewed",
                Load = "Load",
                Exit = "Exit",
                Changed = "Changed";
        }

        /// <summary>
        /// Not currently in use but implemented on backend 08/2019
        /// </summary>
        public static class PageView
        {
            public const string
                MainSimMenu = "simMenu",
                MixAndMatchMenu = "mixMenu",
                MultiplayerMenu = "multiplayerMenu",
                MainSimulator = "mainSimulator",
                MixAndMatchSimulator = "mixSimulator",
                MultiplayerSimulator = "multiplayerSimulator";
        }

        /// <summary>
        /// Similar to event categories, timing categories organize objects
        /// into various groups. 
        /// </summary>
        public static class TimingCategory
        {
            public const string
                Main = "Main Menu",
                MixMatch = "Mix and Match",
                Multiplater = "Multiplayer",

                MainSimulator = "In Simulator",
                MenuTab = "Menu Tab",
                HomeTab = "Home Tab",
                DPMTab = "Gamepiece Tab",
                ScoringTab = "Scoring Tab",
                SensorTab = "Sensor Tab",
                EmulationTab = "Emulation Tab",
                Tab = "Toolbar Tab";
        }

        /// <summary>
        /// Actions for timing events
        /// </summary>a
        public static class TimingVariable
        {
            public const string
                Loading = "Loading",
                Playing = "Playing",
                Customizing = "Customizing",
                Viewing = "Viewing",
                Starting = "Starting";
        }

        /// <summary>
        /// Additional information to expand on the timing categories. 
        /// </summary>
        public static class TimingLabel
        {
            public const string
                MixAndMatchMenu = "Mix and Match Menu",
                MainSimMenu = "Main Menu",
                MultiplayerLobbyMenu = "Multiplayer Lobby Menu",

                MainSimulator = "Main Simulator",
                ResetField = "Reset Field",
                ChangeField = "Change Field",
                MixAndMatch = "Mix and Match Mode",
                ReplayMode = "Replay Mode",

                HomeTab = "Home Tab",
                DPMTab = "Gamepiece Tab",
                ScoringTab = "Scoring Tab",
                SensorTab = "Sensor Tab",
                EmulationTab = "Emulation Tab";
        }

        private class Inner
        {


            public Mutex mutex;
            public WebClient client;
            public Queue<KeyValuePair<string, string>> LoggedData;
            public List<KeyValuePair<string, float>> StartTimes;

            private Inner()
            {
                mutex = new Mutex();
                client = new WebClient();
                LoggedData = new Queue<KeyValuePair<string, string>>();
                StartTimes = new List<KeyValuePair<string, float>>();
            }

            private static Inner? _inst;
            public static Inner InnerInstance => _inst ??= new Inner();
        }
        private static Queue<KeyValuePair<string, string>> LoggedData => Instance.LoggedData;
        private static List<KeyValuePair<string, float>> StartTimes => Instance.StartTimes;
        private static WebClient client => Instance.client;
        private static Mutex mutex => Instance.mutex;
        private static Inner Instance => Inner.InnerInstance;
    }
}