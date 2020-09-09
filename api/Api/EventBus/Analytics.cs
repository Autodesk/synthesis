using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SynthesisAPI.EventBus
{
    public static class Analytics
    {
   
        private const string URL_COLLECT = "https://www.google-analytics.com/collect";
        private const string URL_BATCH = "https://www.google-analytics.com/batch";
        private const string OFFICIAL_TRACKING_ID = "UA-81892961-3";
        public static string GUID { get; private set; } = "not-set";
        public static bool DataCollection { get; private set; } = true;


        /// <summary>
        /// Sets client ID and data preferences
        /// </summary>
        /// <param name="unityGuid">Client ID (For Synthesis use Unity GUID)</param>
        /// <param name="dataPref">Boolean value, is true if user has agreed to share data</param>
        public static void SetUnityPrefs(String unityGuid, bool dataPref)
        {
            GUID = unityGuid;
            DataCollection = dataPref;
        }

        /// <summary>
        /// Stores the start time of a timing event
        /// </summary>
        /// <param name="label">Label corresponding to timing event</param>
        /// <param name="variable">Variable corresponding to timing event</param>
        /// <param name="time">Start time in seconds</param>
        public static void StartTime(string label, string variable, float time)
        {
            StartTimes.Add(new KeyValuePair<string, float>(label + "|" + variable, time));
        }

        /// <summary>
        /// Returns time elapsed between start and end of a timing event
        /// </summary>
        /// <param name="label">Label corresponding to timing event</param>
        /// <param name="variable">Variable corresponding to timing event</param>
        /// <param name="time">End time in seconds</param>
        public static float GetElapsedTime(string label, string variable, float time)
        {
            float a = StartTimes.Find(x => x.Key.Equals(label + "|" + variable)).Value;
            return (time - a) * 1000;
        }

        /// <summary>
        /// Removes start time of a timing event
        /// </summary>
        /// <param name="label">Label corresponding to timing event</param>
        /// <param name="variable">Variable corresponding to timing event</param>
        public static void RemoveTime(string label, string variable)
        {
            StartTimes.Remove(StartTimes.Find(x => x.Key.Equals(label + "|" + variable)));
        }

        /// <summary>
        /// Logs an event locally 
        /// </summary>
        /// <param name="category">The category the event should be reported under in Google Analytics</param>
        /// <param name="action">The action the event should be reported under in Google Analytics</param>
        /// <param name="label">The label the event should be reported under in Google Analytics, null if not applicable</param>
        /// <param name="value">The value the event should be reported with in Google Analytics, null if not applicable</param>
        public static void LogEvent(string category, string action, string label, int value)
        {
            LoggedData.Enqueue(new KeyValuePair<string, string>("v", "1"));
            LoggedData.Enqueue(new KeyValuePair<string, string>("tid", OFFICIAL_TRACKING_ID));
            LoggedData.Enqueue(new KeyValuePair<string, string>("cid", GUID));
            LoggedData.Enqueue(new KeyValuePair<string, string>("t", "event"));
            LoggedData.Enqueue(new KeyValuePair<string, string>("ec", category));
            LoggedData.Enqueue(new KeyValuePair<string, string>("ea", action));
            if (label != null) LoggedData.Enqueue(new KeyValuePair<string, string>("el", label));
            LoggedData.Enqueue(new KeyValuePair<string, string>("ev", value.ToString()));
            LoggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
        }

        /// <summary>
        /// Logs a screenview locally 
        /// </summary>
        /// <param name="screenName">The name of the screen as it should be reported in Google Analytics</param>
        public static void LogScreenView(string screenName)
        {
            LoggedData.Enqueue(new KeyValuePair<string, string>("v", "1"));
            LoggedData.Enqueue(new KeyValuePair<string, string>("tid", OFFICIAL_TRACKING_ID));
            LoggedData.Enqueue(new KeyValuePair<string, string>("cid", GUID));
            LoggedData.Enqueue(new KeyValuePair<string, string>("t", "screenview"));
            LoggedData.Enqueue(new KeyValuePair<string, string>("an", "synthesis"));
            LoggedData.Enqueue(new KeyValuePair<string, string>("cd", screenName));
            LoggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
        }

        /// <summary>
        /// Logs a timing event locally given an ending time if the start time was
        /// recorded earlier using the StartTime method
        /// </summary>
        /// <param name="category">The category the event should be reported under in Google Analytics</param>
        /// <param name="variable">The action the event should be reported with in Google Analytics</param>
        /// <param name="label">The label the event should be reported under in Google Analytics, null if not applicable</param>
        /// <param name="time">The time the event ended in seconds</param>
        public static void LogElapsedTime(string category, string variable, string label, float time)
        {
            int milli = (int)GetElapsedTime(label, variable, time);
            RemoveTime(label, variable);
            LogTiming(category, variable, label, milli);
        }

        /// <summary>
        /// Logs a timing event locally given an integer value for the 
        /// duration of the event in milliseconds
        /// </summary>
        /// <param name="category">The category the event should be reported under in Google Analytics</param>
        /// <param name="variable">The variable the event should be reported with in Google Analytics</param>
        /// <param name="label">The label the event should be reported under in Google Analytics, null if not applicable</param>
        /// <param name="time">The duration of the event in milliseconds</param>
        public static void LogTiming(string category, string variable, string label, int time)
        {
            LoggedData.Enqueue(new KeyValuePair<string, string>("v", "1"));
            LoggedData.Enqueue(new KeyValuePair<string, string>("tid", OFFICIAL_TRACKING_ID));
            LoggedData.Enqueue(new KeyValuePair<string, string>("cid", GUID));
            LoggedData.Enqueue(new KeyValuePair<string, string>("t", "timing"));
            LoggedData.Enqueue(new KeyValuePair<string, string>("utc", category));
            LoggedData.Enqueue(new KeyValuePair<string, string>("utv", variable));
            LoggedData.Enqueue(new KeyValuePair<string, string>("utt", time.ToString()));
            LoggedData.Enqueue(new KeyValuePair<string, string>("utl", label));
            LoggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
        }

        /// <summary>
        /// Pushes local data to the Google Analytics server
        /// </summary>
        public static void UploadDump()
        {
            if (LoggedData.Count < 1 || !DataCollection)
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
                    Console.WriteLine("Server response" + result);
                }
                else
                {
                    Console.WriteLine("Collecting");
                    result = client.UploadString(URL_COLLECT, "POST", data);
                    Console.WriteLine(data);
                    Console.WriteLine("Server response" + result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
        }


        #region AsyncMethods

        /// <summary>
        /// Creates an asynchronous task to log an event locally 
        /// </summary>
        /// <param name="category">The category the event should be reported under in Google Analytics</param>
        /// <param name="action">The action the event should be reported under in Google Analytics</param>
        /// <param name="label">The label the event should be reported under in Google Analytics, null if not applicable</param>
        /// <param name="value">The value the event should be reported with in Google Analytics, null if not applicable</param>
        /// <returns>Task that can be run asynchronously to log the event</returns>
        public static Task LogEventAsync(string category, string action, string label, int value)
        {
            return Task.Factory.StartNew(() =>
            {
                mutex.WaitOne();
                LoggedData.Enqueue(new KeyValuePair<string, string>("v", "1"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("tid", OFFICIAL_TRACKING_ID));
                LoggedData.Enqueue(new KeyValuePair<string, string>("cid", GUID));
                LoggedData.Enqueue(new KeyValuePair<string, string>("t", "event"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("ec", category));
                LoggedData.Enqueue(new KeyValuePair<string, string>("ea", action));
                if (label != null) LoggedData.Enqueue(new KeyValuePair<string, string>("el", label));
                LoggedData.Enqueue(new KeyValuePair<string, string>("ev", value.ToString()));
                LoggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
                mutex.ReleaseMutex();
            });
        }

        /// <summary>
        /// Creates an asynchronous task to log a screenview locally 
        /// </summary>
        /// <param name="screenName">The name of the screen as it should be reported in Google Analytics</param>
        /// <returns>Task that can be run asynchronously to log the screenview</returns>
        public static Task LogScreenViewAsync(string screenName)
        {
            return Task.Factory.StartNew(() =>
            {
                mutex.WaitOne();
                LoggedData.Enqueue(new KeyValuePair<string, string>("v", "1"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("tid", OFFICIAL_TRACKING_ID));
                LoggedData.Enqueue(new KeyValuePair<string, string>("cid", GUID));
                LoggedData.Enqueue(new KeyValuePair<string, string>("t", "screenview"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("an", "synthesis"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("cd", screenName));
                LoggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
                mutex.ReleaseMutex();
            });
        }

        /// <summary>
        /// Creates an asynchronous task to log a timing event locally given an ending time 
        /// if the start time was recorded earlier using the StartTime method
        /// </summary>
        /// <param name="category">The category the event should be reported under in Google Analytics</param>
        /// <param name="variable">The action the event should be reported with in Google Analytics</param>
        /// <param name="label">The label the event should be reported under in Google Analytics, null if not applicable</param>
        /// <param name="time">The time the event ended in seconds</param>
        /// <returns>Task that can be run asynchronously to log the timing event</returns>
        public static Task LogElapsedTimeAsync(string category, string variable, string label, float currentTime)
        {
            int milli = (int)GetElapsedTime(label, variable, currentTime);
            RemoveTime(label, variable);
            return LogTimingAsync(category, variable, label, milli);
        }

        /// <summary>
        /// Creates an asynchronous task to log a timing event locally given an integer value 
        /// for the duration of the event in milliseconds
        /// </summary>
        /// <param name="category">The category the event should be reported under in Google Analytics</param>
        /// <param name="variable">The variable the event should be reported with in Google Analytics</param>
        /// <param name="label">The label the event should be reported under in Google Analytics, null if not applicable</param>
        /// <param name="time">The duration of the event in milliseconds</param>
        /// <returns>Task that can be run asynchronously to log the timing event</returns>

        public static Task LogTimingAsync(string category, string variable, string label, int time)
        {
            return Task.Factory.StartNew(() =>
            {
                mutex.WaitOne();
                LoggedData.Enqueue(new KeyValuePair<string, string>("v", "1"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("tid", OFFICIAL_TRACKING_ID));
                LoggedData.Enqueue(new KeyValuePair<string, string>("cid", GUID));
                LoggedData.Enqueue(new KeyValuePair<string, string>("t", "timing"));
                LoggedData.Enqueue(new KeyValuePair<string, string>("utc", category));
                LoggedData.Enqueue(new KeyValuePair<string, string>("utv", variable));
                LoggedData.Enqueue(new KeyValuePair<string, string>("utt", time.ToString()));
                LoggedData.Enqueue(new KeyValuePair<string, string>("utl", label));
                LoggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
                mutex.ReleaseMutex();
            });
        }

        /// <summary>
        /// Creates a task to push local data to the Google Analytics server asynchronously
        /// </summary>
        /// <returns>Task that can be run asynchronously to log the timing event</returns>
        public static Task UploadDumpAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                mutex.WaitOne();
                if (LoggedData.Count < 1 || !DataCollection)
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
                        Console.WriteLine("Server response" + result);
                    }
                    else
                    {
                        Console.WriteLine("Collecting");
                        result = client.UploadString(URL_COLLECT, "POST", data);
                        Console.WriteLine(data);
                        Console.WriteLine("Server response" + result);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.ToString());
                }
            });
        }

        #endregion

        /// <summary>
        /// Closes mutex, kills any async tasks that have not already completed
        /// </summary>
        public static void CleanUp()
        {
            mutex.Close();
        }

        /// <summary>
        /// Categories group multiple objects together. Each main category is grouped by the tabs
        /// in the simulator. Most events will fall into one of the tab categories (e.g. Engine or Entity tab.)
        /// </summary>
        public static class EventCategory
        {
            public const string
                EngineTab = "Engine Tab",
                EntityTab = "Entities Tab",

                ExporterType = "Exported Generator",
                ExporterVersion = "Exporter Version",

                // Start of analytics tracking
                MainSimulator = "Main Simulator",

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
        /// Name of the screen the user is on
        /// </summary>
        public static class ScreenName
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
                EngineTab = "Engine Tab",
                EntityTab = "Entity Tab",
                
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
                Interacting = "Interacting",
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
                EngineTab = "Engine Tab",
                EntityTab = "Entity Tab",
                
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