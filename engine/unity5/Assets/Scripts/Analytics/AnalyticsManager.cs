using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Threading;

public class AnalyticsManager : MonoBehaviour {

    public static AnalyticsManager GlobalInstance { get; set; }

    public UInt16 GUID { get; private set; }
    public const string URL_COLLECT = "https://www.google-analytics.com/collect";
    public const string URL_BATCH = "https://www.google-analytics.com/batch";
    public const string OFFICIAL_TRACKING_ID = "UA-81892961-3";
    public const float DUMP_DELAY = 5;
    public bool DumpData = true;

    public static Mutex mutex;

    private WebClient client;
    private Queue<KeyValuePair<string, string>> loggedData;
    private List<KeyValuePair<string, float>> startTimes;

    private float LastDump = 0;
    private bool lastEnd = false;

    public void Awake()
    {
        GUID = (UInt16)UnityEngine.Random.Range(UInt16.MinValue, UInt16.MaxValue);

        mutex = new Mutex();
        GlobalInstance = this;
        LastDump = Time.time;
        DontDestroyOnLoad(gameObject);
        loggedData = new Queue<KeyValuePair<string, string>>();
        startTimes = new List<KeyValuePair<string, float>>();

        Application.quitting += CleanUp;

        DumpData = PlayerPrefs.GetInt("gatherData", 1) == 1;
    }

    public void LateUpdate()
    {
        if (LastDump + DUMP_DELAY < Time.time)
        {
            UploadDumpAsync();
            LastDump = Time.time;
        }
    }

    public void StartTime(string label, string varible)
    {
        startTimes.Add(new KeyValuePair<string, float>(label + "|" + varible, Time.unscaledTime));
    }

    public float GetElapsedTime(string label, string varible)
    {
        float a = startTimes.Find(x => x.Key.Equals(label + "|" + varible)).Value;
        return (Time.unscaledTime - a) * 1000;
    }

    public void RemoveTime(string label, string varible)
    {
        startTimes.Remove(startTimes.Find(x => x.Key.Equals(label + "|" + varible)));
    }

    #region AsyncMethods

    public async void LogEventAsync(string Catagory, string Action, string Label, string Value)
    {
        if (mutex != null) await LogEvent(Catagory, Action, Label, Value);
    }

    public async void LogScreenViewAsync(string ScreenName)
    {
        if (mutex != null) await LogScreenView(ScreenName);
    }

    public async void LogPageViewAsync(string Title)
    {
        if (mutex != null) await LogPageView(Title);
    }

    public void LogTimingAsync(string Catagory, string Vari, string Label)
    {
        int milli = (int)GetElapsedTime(Label, Vari);
        RemoveTime(Label, Vari);
        LogTimingAsync(Catagory, Vari, milli, Label);
    }

    public async void LogTimingAsync(string Catagory, string Vari, int Time, string Label)
    {
        if (mutex != null) await LogTiming(Catagory, Vari, Time, Label);
    }

    public async void UploadDumpAsync()
    {
        if (mutex != null) await UploadDump();
    }

    #endregion

    #region LogTasks

    public void LogStandardInfo()
    {
        loggedData.Enqueue(new KeyValuePair<string, string>("v", "1"));
        loggedData.Enqueue(new KeyValuePair<string, string>("tid", OFFICIAL_TRACKING_ID));
        loggedData.Enqueue(new KeyValuePair<string, string>("cid", GUID.ToString()));
    }

    private Task LogEvent(string Catagory, string Action, string Label, string Value)
    {
        return Task.Factory.StartNew(() =>
        {
            mutex.WaitOne();
            LogStandardInfo();
            loggedData.Enqueue(new KeyValuePair<string, string>("t", "event"));

            loggedData.Enqueue(new KeyValuePair<string, string>("ec", Catagory));
            loggedData.Enqueue(new KeyValuePair<string, string>("ea", Action));
            if (Label != null) loggedData.Enqueue(new KeyValuePair<string, string>("el", Label));
            if (Label != null) loggedData.Enqueue(new KeyValuePair<string, string>("ev", Value));
            loggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
            mutex.ReleaseMutex();
        });
    }

    private Task LogScreenView(string ScreenName)
    {
        return Task.Factory.StartNew(() =>
        {
            mutex.WaitOne();
            LogStandardInfo();
            loggedData.Enqueue(new KeyValuePair<string, string>("t", "screenname"));

            //ScreenName = ScreenName.Replace(" ", "%20");

            loggedData.Enqueue(new KeyValuePair<string, string>("dl", "https://www.google.com/index.html"));
            loggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
            mutex.ReleaseMutex();
        });
    }

    private Task LogPageView(string Title)
    {
        return Task.Factory.StartNew(() =>
        {
            mutex.WaitOne();
            LogStandardInfo();
            loggedData.Enqueue(new KeyValuePair<string, string>("t", "pageview"));

            loggedData.Enqueue(new KeyValuePair<string, string>("dl", "http://test.com/" + Title));
            loggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
            mutex.ReleaseMutex();
        });
    }

    private Task LogTiming(string Catagory, string Vari, int Time, string Label)
    {
        return Task.Factory.StartNew(() =>
        {
            mutex.WaitOne();
            LogStandardInfo();
            loggedData.Enqueue(new KeyValuePair<string, string>("t", "timing"));

            loggedData.Enqueue(new KeyValuePair<string, string>("utc", Catagory));
            loggedData.Enqueue(new KeyValuePair<string, string>("utv", Vari));
            loggedData.Enqueue(new KeyValuePair<string, string>("utt", Time.ToString()));
            loggedData.Enqueue(new KeyValuePair<string, string>("utl", Label));
            loggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));
            mutex.ReleaseMutex();
        });
    }

    private Task UploadDump()
    {
        return Task.Factory.StartNew(() =>
        {
            mutex.WaitOne();
            if (loggedData.Count < 1 || !DumpData) {
                loggedData = new Queue<KeyValuePair<string, string>>();
                mutex.ReleaseMutex();
                return;
            }

            string data = "";

            Queue<KeyValuePair<string, string>> loggedCopy = new Queue<KeyValuePair<string, string>>(loggedData);
            loggedData.Clear();
            mutex.ReleaseMutex();

            bool batchSend = false;

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

            if (client == null)
            {
                client = new WebClient();
            }

            string result;

            try
            {
                using (var _client = new WebClient())
                {
                    if (batchSend)
                    {
                        result = _client.UploadString(URL_BATCH, "POST", data);
                    }
                    else
                    {
                        result = _client.UploadString(URL_COLLECT, "POST", data);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        });
    }

    #endregion

    public void CleanUp() {
        mutex.Close();
        mutex = null;
    }
}
