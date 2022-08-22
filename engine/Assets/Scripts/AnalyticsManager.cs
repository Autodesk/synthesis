using System;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Synthesis.PreferenceManager;
using UnityEngine;

using System.Net.Http;
using System.Threading.Tasks;

public static class AnalyticsManager {

    private const string CLIENT_ID_PREF = "analytics/client_id";
    private const string USE_ANALYTICS_PREF = "analytics/use_analytics";

    private static string AllData = "";

    public const string TRACKING_ID = "UA-81892961-6";
    public static string ClientID;

    private static bool _useAnalytics = true;

    private static HttpClient _httpClient;

    private static List<IAnalytics> _pendingEvents;

    private const string ANALYTICS_URL = "http://192.168.1.7:8080";

    public static bool UseAnalytics {
        get => _useAnalytics;
        set {
            _useAnalytics = value;
            PreferenceManager.SetPreference<bool>(USE_ANALYTICS_PREF, _useAnalytics);
            PreferenceManager.Save();
        }
    }

    static AnalyticsManager() {
        PreferenceManager.Load();
        if (PreferenceManager.ContainsPreference(CLIENT_ID_PREF)) {
            ClientID = PreferenceManager.GetPreference<string>(CLIENT_ID_PREF);
        } else {
            var rand = new System.Random((int)DateTime.Now.ToBinary());
            ClientID = $"CLIENT_{rand.Next(1, int.MaxValue)}";
            PreferenceManager.SetPreference<string>(CLIENT_ID_PREF, ClientID);
            PreferenceManager.Save();
        }

        Debug.Log($"Client ID: {ClientID}");

        _httpClient = new HttpClient();
        _pendingEvents = new List<IAnalytics>();

        if (PreferenceManager.ContainsPreference(USE_ANALYTICS_PREF)) {
            _useAnalytics = PreferenceManager.GetPreference<bool>(USE_ANALYTICS_PREF);
        }
    }

    public static void LogEvent(AnalyticsEvent e)
    {
        LogAnalytic(e);
    }

    public static void LogScreenView(AnalyticsScreenView e)
    {
        LogAnalytic(e);
    }

    public static void LogPageView(AnalyticsPageView e)
    {
        LogAnalytic(e);
    }

    public static void LogTiming(AnalyticsLogTiming e)
    {
        LogAnalytic(e);
    }

    public static void LogAnalytic(IAnalytics e)
    {
        _pendingEvents.Add(e);
    }

    public static void  PostData()
    {
        foreach (var _event in _pendingEvents)
        {
            if (UseAnalytics)
            {
                var cli = new WebClient();
                try
                {
                    var reqparm = new System.Collections.Specialized.NameValueCollection();
                    reqparm.Add("event_name", System.Net.WebUtility.UrlEncode(_event.GetPostData()));
                    var resp = cli.UploadValues(
                        $"{ANALYTICS_URL}/analytics", "POST", reqparm);
                    Debug.Log(resp);

                }
                catch (Exception e)
                {
                    Debug.Log("not pog");
                }

            }
        }
        _pendingEvents.Clear();
    }

    public static void LogAnalytics(string s)
    {
    }

    public struct PostResult {
        public bool   Batched;
        public string Response;
    }
}

public interface IAnalytics
{
    public string GetPostData();
}
public class AnalyticsEvent : IAnalytics
{

    public string Category, Value, Action, Label;

    public AnalyticsEvent(string category, string action, string label)
    {
        Category = category;
        Action = action;
        Label = label;
    }

    /*
    public AnalyticsEvent(string category, string action, string value)
    {
        Category = category;
        Action = action;
        Value = value;
    }
    */

    public string GetPostData()
        => $"{Category}_{Action}_{Label}";
    //=> $"t=event&ec={Category}&ea={Action}&el={Label}";
}

public class AnalyticsScreenView : IAnalytics
{
    public string ScreenName;
    public AnalyticsScreenView(string screenName)
    {
        ScreenName = screenName;
    }
    public string GetPostData()
        => $"t={ScreenName}&dl={"https://www.google.com/index.html"}";
}

public class AnalyticsPageView : IAnalytics
{
    public string Title;
    public AnalyticsPageView(string title)
    {
        Title = title;
    }
    public string GetPostData() => $"t=pageview&dl={"http://test.com/" + Title}";
}

public class AnalyticsLogTiming : IAnalytics
{
    public string Category, Vari, Label;
    public int Time;

    public AnalyticsLogTiming(string category, string vari, string label, int time)
    {
        Category = category;
        Vari = vari;
        Label = label;
        Time = time;
    }

    public string GetPostData() => $"t=timing&utc={Category}&utv={Vari}&utt={Time.ToString()}&utl={Label}";
}

