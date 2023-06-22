using System;
using Synthesis.PreferenceManager;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public static class AnalyticsManager {
    private const string CLIENT_ID_PREF    = "analytics/client_id";
    public const string USE_ANALYTICS_PREF = "analytics/use_analytics";

    public const string TRACKING_ID = "UA-81892961-7";
    public static string ClientID;

    public const string URL_COLLECT = "https://www.google-analytics.com/collect";

    private static bool _useAnalytics = true;

    private static List<IAnalytics> _pendingEvents;

    public static bool UseAnalytics {
        get => _useAnalytics;
        set {
            _useAnalytics = value;
            PreferenceManager.Load();
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

        _pendingEvents = new List<IAnalytics>();
        if (PreferenceManager.ContainsPreference(USE_ANALYTICS_PREF)) {
            _useAnalytics = PreferenceManager.GetPreference<bool>(USE_ANALYTICS_PREF);
        }
    }

    public static void LogEvent(AnalyticsEvent e) {
        LogAnalytic(e);
    }

    public static void LogScreenView(AnalyticsScreenView e) {
        LogAnalytic(e);
    }

    public static void LogPageView(AnalyticsPageView e) {
        LogAnalytic(e);
    }

    public static void LogTiming(AnalyticsLogTiming e) {
        LogAnalytic(e);
    }

    public static void LogAnalytic(IAnalytics e) {
#if UNITY_EDITOR
        return;
#endif
        _pendingEvents.Add(e);
    }

    // TODO: Determine if still needed as part of codebase cleanup
    // public static void  PostData()
    // {
    //     foreach (var _event in _pendingEvents)
    //     {
    //         if (UseAnalytics)
    //         {
    //             var cli = new WebClient();
    //             try
    //             {
    //                 var reqparm = new System.Collections.Specialized.NameValueCollection();
    //                 reqparm.Add("event_name", System.Net.WebUtility.UrlEncode(_event.GetPostData()));
    //                 var resp = cli.UploadValues(
    //                     $"{ANALYTICS_URL}/analytics", "POST", reqparm);
    //                 Debug.Log(resp);

    //             }
    //             catch (Exception e)
    //             {
    //                 Debug.Log("not pog");
    //             }

    //         }
    //     }
    //     _pendingEvents.Clear();
    // }

    public static void PostData() {
        foreach (var _event in _pendingEvents) {
            if (UseAnalytics) {
                var cli = new WebClient();

                try {
                    var reqparm = new System.Collections.Specialized.NameValueCollection();
                    reqparm.Add("v", "1");
                    reqparm.Add("tid", TRACKING_ID);
                    reqparm.Add("cid", ClientID);
                    reqparm.Add("t", "event");
                    reqparm.Add("ec", ((AnalyticsEvent)_event).Category);
                    reqparm.Add("ea", ((AnalyticsEvent)_event).Action);
                    reqparm.Add("el", ((AnalyticsEvent)_event).Label);

                    var resp = cli.UploadValues(URL_COLLECT, "POST", reqparm);
                } catch (Exception e) {
                    Debug.Log("Failed to post Analytics");
                }
            }
        }

        _pendingEvents.Clear();
    }

    public static string GetActionTypeFromType<T>() =>
        typeof(T).GetCustomAttributes<AnalyticsLabelAttribute>().ToList().Count >
        0?((AnalyticsLabelAttribute)(typeof(T).GetCustomAttributes<AnalyticsLabelAttribute>().First()))
            .Title : typeof(T)
            .Name;

    public static void LogAnalytics(string s) {
    }

    public struct PostResult {
        public bool Batched;
        public string Response;
    }
}

public interface IAnalytics {
    public string GetPostData();
}

public class AnalyticsEvent : IAnalytics {

    public string Category, Value, Action, Label;

    public AnalyticsEvent(string category, string action, string label) {
        Category = category;
        Action   = action;
        Label    = label;
    }

    public string GetPostData() => $"t=event&ec={Category}&ea={Action}&el={Label}";
}

[AttributeUsage(AttributeTargets.Class)]
public class AnalyticsLabelAttribute : Attribute {
    public string Title;
}

public class AnalyticsScreenView : IAnalytics {
    public string ScreenName;
    public AnalyticsScreenView(string screenName) {
        ScreenName = screenName;
    }
    public string GetPostData() => $"t={ScreenName}&dl={"https://www.google.com/index.html"}";
}

public class AnalyticsPageView : IAnalytics {
    public string Title;

    public AnalyticsPageView(string title) {
        Title = title;
    }

    public string GetPostData() => $"t=pageview&dl={"http://test.com/" + Title}";
}

public class AnalyticsLogTiming : IAnalytics {
    public string Category, Vari, Label;
    public int Time;

    public AnalyticsLogTiming(string category, string vari, string label, int time) {
        Category = category;
        Vari     = vari;
        Label    = label;
        Time     = time;
    }

    public string GetPostData() => $"t=timing&utc={Category}&utv={Vari}&utt={Time.ToString()}&utl={Label}";
}
