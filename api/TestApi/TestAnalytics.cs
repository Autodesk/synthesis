using System;
using SynthesisAPI.EventBus;
using NUnit.Framework;
using System.Threading;

namespace TestApi
{
    [TestFixture]
    public static class TestAnalytics
    {
        public class TestEvent : IEvent
        {
            public object[] GetArguments() => new object[] { };
        }

        [Test]
        public static void TestSetUnityPrefs()
        {
            Analytics.SetUnityPrefs("testGUID", false);
            Assert.AreEqual("testGUID", Analytics.GUID);
            Assert.AreEqual(false, Analytics.DataCollection);
            Analytics.SetUnityPrefs("testGUID", true);
            Assert.AreEqual("testGUID", Analytics.GUID);
            Assert.AreEqual(true, Analytics.DataCollection);
        }

        [Test]
        public static void TestLogTime()
        {
            Analytics.SetUnityPrefs("testGUID", true);
            Analytics.LogTiming(Analytics.TimingCategory.Main, Analytics.TimingVariable.Viewing, Analytics.TimingLabel.MainSimMenu, 1000);
            Analytics.UploadDump();
        }

        [Test]
        public static void TestLogTimeAsync()
        {
            Analytics.SetUnityPrefs("testGUID", true);
            var task = Analytics.LogTimingAsync(Analytics.TimingCategory.Main, Analytics.TimingVariable.Viewing, Analytics.TimingLabel.MainSimMenu, 100);
            task.Wait();
            var task2 = Analytics.UploadDumpAsync();
            task2.Wait();
        }

        [Test]
        public static void TestLogElapsedTime()
        {
            Analytics.SetUnityPrefs("testGUID", true);
            Analytics.StartTime(Analytics.TimingLabel.MainSimMenu, Analytics.TimingVariable.Viewing, 0);
            Analytics.LogElapsedTime(Analytics.TimingCategory.Main, Analytics.TimingVariable.Viewing, Analytics.TimingLabel.MainSimMenu, 100);
            Analytics.UploadDump();
        }

        [Test]
        public static void TestLogElapsedTimeAsync()
        {
            Analytics.SetUnityPrefs("testGUID", true);
            Analytics.StartTime(Analytics.TimingLabel.MainSimMenu, Analytics.TimingVariable.Viewing, 0);
            var task = Analytics.LogElapsedTimeAsync(Analytics.TimingCategory.Main, Analytics.TimingVariable.Viewing, Analytics.TimingLabel.MainSimMenu, 100);
            task.Wait();
            var task2 = Analytics.UploadDumpAsync();
            task2.Wait();
        }

        [Test]
        public static void TestLogEvent()
        {
            Analytics.SetUnityPrefs("testGUID", true);
            Analytics.LogEvent(Analytics.EventCategory.AddRobot, Analytics.EventAction.Clicked, "testlabel", 10);
            Analytics.UploadDump();
        }

        [Test]
        public static void TestLogEventAsync()
        {
            Analytics.SetUnityPrefs("testGUID", true);
            var task = Analytics.LogEventAsync(Analytics.EventCategory.AddRobot, Analytics.EventAction.Clicked, "testlabel", 10);
            task.Wait();
            var task2 = Analytics.UploadDumpAsync();
            task2.Wait();
        }

        [Test]
        public static void TestLogScreenview()
        {
            Analytics.SetUnityPrefs("testGUID", true);
            Analytics.LogScreenView(Analytics.ScreenName.MainSimMenu);
            Analytics.UploadDump();
        }

        [Test]
        public static void TestLogScreenviewAsync()
        {
            Analytics.SetUnityPrefs("testGUID", true);
            var task = Analytics.LogScreenViewAsync(Analytics.ScreenName.MainSimMenu);
            task.Wait();
            var task2 = Analytics.UploadDumpAsync();
            task2.Wait();
        }

    }
}
