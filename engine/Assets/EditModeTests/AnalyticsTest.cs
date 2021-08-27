using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class AnalyticsTest
{
    [Test]
    public void EventTest() {
        var data = new AnalyticsEvent(category: "dog", action: "bark", label: "9"); 
        AnalyticsManager.LogEvent(data);
        AnalyticsManager.PostData();
        
        var datascreen = new AnalyticsScreenView("Screen Name");
        AnalyticsManager.LogScreenView(datascreen);
        var test2 = AnalyticsManager.PostData();
        
        Debug.Log(test2.result);
        
        var datapage = new AnalyticsPageView("Page View");
        AnalyticsManager.LogPageView(datapage);
        var timing = new AnalyticsLogTiming(category: "test", vari: "test", label: "test", time: 10);
        AnalyticsManager.LogTiming(timing);
        var test3 = AnalyticsManager.PostData();
        
        Assert.IsFalse(test2.usedBatchUrl);
        Assert.IsTrue(test3.usedBatchUrl);
    }
}
