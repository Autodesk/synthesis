using UnityEngine;

public class TestAnalytics : MonoBehaviour
{
    private void Start()
    {
        var data = new AnalyticsEvent(category: "dog", action: "bark", value: "9");
        AnalyticsManager.LogEvent(data);
        AnalyticsManager.PostData(url: "https://www.google-analytics.com/collect");

        var datascreen = new AnalyticsScreenView("Screen Name");
        AnalyticsManager.LogScreenView(datascreen);
        AnalyticsManager.PostData(url: "https://www.google-analytics.com/collect");

        var datapage = new AnalyticsPageView("Page View");
        AnalyticsManager.LogPageView(datapage);
        AnalyticsManager.PostData(url: "https://www.google-analytics.com/collect");
    }
}
