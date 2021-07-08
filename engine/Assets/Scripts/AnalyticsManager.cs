using System.Net;
using UnityEngine;

public static class AnalyticsManager
{

    private static string AllData = "";

    public const string TRACKING_ID = "UA-81892961-6";
    public const string CLIENT_ID = "667";



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

    public static void PostData(string url)
    {
        WebClient cli = new WebClient();
        string res = cli.UploadString(url, "POST", AllData);
        Debug.Log(res);
    }

}

public interface IAnalytics
{
    public string GetPostData();
}
public class AnalyticsEvent : IAnalytics
{

    public string Category, Value, Action;

    public AnalyticsEvent(string category, string action, string value)
    {
        Category = category;
        Action = action;
        Value = value;
    }

    public string GetPostData()
        => $"t=event&ec={Category}&ea={Action}&ev={Value}";
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

