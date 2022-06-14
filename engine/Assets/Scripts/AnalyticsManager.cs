using System.Net;
using UnityEngine;

public static class AnalyticsManager
{
    private static string AllData = "";

    public const string TRACKING_ID = "UA-81892961-6";
    public const string CLIENT_ID = "667";

    public const string URL_COLLECT = "https://www.google-analytics.com/collect";
    public const string URL_BATCH = "https://www.google-analytics.com/batch";

    public static bool useAnalytics = true;
    public static void LogEvent(AnalyticsEvent e)
    {
        LogAnalytic(e);
        //AllData += $"v=1&tid={TRACKING_ID}&cid={CLIENT_ID}&{e.GetPostData()}\n";
    }

    public static void LogScreenView(AnalyticsScreenView e)
    {
        LogAnalytic(e);
        //AllData += $"v=1&tid={TRACKING_ID}&cid={CLIENT_ID}&{e.GetPostData()}\n";
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
        AllData += $"v=1&tid={TRACKING_ID}&cid={CLIENT_ID}&{e.GetPostData()}\n";
    }

    public static PostResult PostData() {
        if(useAnalytics){
            bool useBatch = AllData.Split('\n').Length > 2;
            WebClient cli = new WebClient();
            string res = cli.UploadString(useBatch ? URL_BATCH : URL_COLLECT, "POST", AllData);
            AllData = string.Empty;
            // Debug.Log(res);
            return new PostResult() { usedBatchUrl = useBatch, result = res };
        }
        else{
            return new PostResult(){ usedBatchUrl = false, result = ""};
        }
    }
    
    public struct PostResult {
        public bool usedBatchUrl;
        public string result;
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
        => $"t=event&ec={Category}&ea={Action}&el={Label}";
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

