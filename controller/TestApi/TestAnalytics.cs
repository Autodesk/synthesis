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
        public static void TestLogTime()
        {
            Analytics.SetUnityPrefs("testGUID", true);
            Analytics.LogTimeAsync(Analytics.TimingCategory.Main, Analytics.TimingVariable.Viewing, 1000, Analytics.TimingLabel.MainSimMenu);
            Analytics.UploadDumpAsync();
            Analytics.CleanUp();
        }

        [Test]
        public static void TestLogEvent()
        {
            Analytics.SetUnityPrefs("35009a79-1a05-49d7-b876-2b884d0f825b", true);
            Analytics.LogEventAsync(Analytics.EventCategory.HomeTab, Analytics.EventAction.Clicked, "test", "test");
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Analytics.UploadDumpAsync();
            //var task = Analytics.UploadDump();
            //task.Wait();
            //Thread.Sleep(1000);
            //Analytics.CleanUp();
        }

        [Test]
        public static void TestLogScreenView()
        {
            Analytics.SetUnityPrefs("testGUID", true);
            Analytics.LogScreenViewAsync(Analytics.PageView.MainSimMenu);
            Analytics.UploadDumpAsync();
            Analytics.CleanUp();
        }

        [Test]
        public static void TestLogByEventTypes()
        {

            Analytics.SetUnityPrefs("testGUID", true);
            Analytics.LogAllTypeEvents<TestEvent>(Analytics.EventCategory.HomeTab, Analytics.EventAction.Clicked, "test", "test");
            Assert.IsTrue(EventBus.Push(new TestEvent()));
            Assert.IsTrue(EventBus.Push(new TestEvent()));
            Analytics.UploadDumpAsync();
            Analytics.CleanUp();
        }

        [Test]
        public static void TestLogByEventTags()
        {
            Analytics.SetUnityPrefs("testGUID", true);
            Analytics.LogAllTaggedEvents("tag", Analytics.EventCategory.HomeTab, Analytics.EventAction.Clicked, "test", "test");
            Assert.IsTrue(EventBus.Push("tag", new TestEvent()));
            Assert.IsTrue(EventBus.Push("tag", new TestEvent()));
            Analytics.UploadDumpAsync();
            Analytics.CleanUp();
        }
    }
}
