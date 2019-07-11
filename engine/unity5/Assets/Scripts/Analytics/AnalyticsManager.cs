using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using System;

public class AnalyticsManager : MonoBehaviour {

    public static AnalyticsManager GlobalInstance { get; set; }

    public const string URL_COLLECT = "https://www.google-analytics.com/collect";
    public const string URL_BATCH = "https://www.google-analytics.com/batch";
    public const string TESTING_TRACKING_ID = "UA-142391571-1";
    public const string OFFICIAL_TRACKING_ID = "UA-81892961-3";
    public const float DUMP_DELAY = 5;
    public bool DumpData = true;

    private WebClient client;
    private Queue<KeyValuePair<string, string>> loggedData;
    private List<KeyValuePair<string, float>> startTimes;

    private float LastDump = 0;

    public void Awake()
    {
        GlobalInstance = this;
        LastDump = Time.time;
        DontDestroyOnLoad(gameObject);
        loggedData = new Queue<KeyValuePair<string, string>>();
        startTimes = new List<KeyValuePair<string, float>>();
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
        await LogEvent(Catagory, Action, Label, Value);
    }

    public async void LogScreenViewAsync(string ScreenName)
    {
        await LogScreenView(ScreenName);
    }

    public async void LogPageViewAsync(string Title)
    {
        await LogPageView(Title);
    }

    public void LogTimingAsync(string Catagory, string Vari, string Label)
    {
        int milli = (int)GetElapsedTime(Label, Vari);
        RemoveTime(Label, Vari);
        LogTimingAsync(Catagory, Vari, milli, Label);
    }

    public async void LogTimingAsync(string Catagory, string Vari, int Time, string Label)
    {
        await LogTiming(Catagory, Vari, Time, Label);
    }

    public async void UploadDumpAsync()
    {
        await UploadDump();
    }

    #endregion

    #region LogTasks

    public void LogStandardInfo()
    {
        loggedData.Enqueue(new KeyValuePair<string, string>("v", "1"));
        loggedData.Enqueue(new KeyValuePair<string, string>("tid", OFFICIAL_TRACKING_ID));
        loggedData.Enqueue(new KeyValuePair<string, string>("cid", "555"));
    }

    private Task LogEvent(string Catagory, string Action, string Label, string Value)
    {
        return Task.Factory.StartNew(() =>
        {
            LogStandardInfo();
            loggedData.Enqueue(new KeyValuePair<string, string>("t", "event"));

            loggedData.Enqueue(new KeyValuePair<string, string>("ec", Catagory));
            loggedData.Enqueue(new KeyValuePair<string, string>("ea", Action));
            if (Label != null) loggedData.Enqueue(new KeyValuePair<string, string>("el", Label));
            if (Label != null) loggedData.Enqueue(new KeyValuePair<string, string>("ev", Value));
            loggedData.Enqueue(new KeyValuePair<string, string>("NEW", ""));

            Debug.Log("Event Logged");
        });
    }

    private Task LogScreenView(string ScreenName)
    {
        return Task.Factory.StartNew(() =>
        {
            LogStandardInfo();
            loggedData.Enqueue(new KeyValuePair<string, string>("t", "screenname"));

            //ScreenName = ScreenName.Replace(" ", "%20");

            loggedData.Enqueue(new KeyValuePair<string, string>("dl", "https://www.google.com/index.html"));

            Debug.Log("Screenname Logged");
        });
    }

    private Task LogPageView(string Title)
    {
        return Task.Factory.StartNew(() =>
        {
            LogStandardInfo();
            loggedData.Enqueue(new KeyValuePair<string, string>("t", "pageview"));

            loggedData.Enqueue(new KeyValuePair<string, string>("dl", "http://test.com/" + Title));

            Debug.Log("Pageview Logged");
        });
    }

    private Task LogTiming(string Catagory, string Vari, int Time, string Label)
    {
        return Task.Factory.StartNew(() =>
        {
            LogStandardInfo();
            loggedData.Enqueue(new KeyValuePair<string, string>("t", "timing"));

            loggedData.Enqueue(new KeyValuePair<string, string>("utc", Catagory));
            loggedData.Enqueue(new KeyValuePair<string, string>("utv", Vari));
            loggedData.Enqueue(new KeyValuePair<string, string>("utt", Time.ToString()));
            loggedData.Enqueue(new KeyValuePair<string, string>("utl", Label));

            Debug.Log("Timing Logged");
        });
    }

    private Task UploadDump()
    {
        return Task.Factory.StartNew(() =>
        {

            if (!DumpData || loggedData.Count < 1) { Debug.Log("Not Dumping"); return; }

            Debug.Log("Starting Data Dump");

            string data = "";

            Queue<KeyValuePair<string, string>> loggedCopy = new Queue<KeyValuePair<string, string>>(loggedData);
            loggedData.Clear();

            bool batchSend = false;

            while (loggedCopy.Count > 0)
            {
                KeyValuePair<string, string> pair = loggedCopy.Dequeue();
                //Debug.Log(pair.Key + " " + pair.Value);
                if (pair.Key.Equals("NEW"))
                {
                    data += "\n";
                    batchSend = true;
                }
                else
                {
                    if ((data != "") && (data != "\n")) data += "&";

                    data += pair.Key + "=" + pair.Value;
                }


            }

            if (client == null)
            {
                client = new WebClient();
            }

            string result;

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

            Debug.Log(result);

            Debug.Log("Data Dumped\n'" + data + "'");
        });
    }

    #endregion
}
