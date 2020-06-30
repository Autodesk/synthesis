using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SynthesisAPI.EventBus;

namespace TestApi
{
    public class TestEvent : IEvent
    {
        public TestEvent()
        {
        }
        string IEvent.EventType
        {
            get
            {
                return "test";
            }

        }

        byte[] IEvent.getData()
        {
            return null; ;
        }
    }

    public class OtherEvent : IEvent
    {
        public OtherEvent()
        {
        }
        string IEvent.EventType
        {
            get
            {
                return "other";
            }

        }

        byte[] IEvent.getData()
        {
            return null; ;
        }
    }

    public class Subscriber
    {
        public int count1;
        public int count2;
        public Subscriber()
        {
            count1 = 0;
            count2 = 0;
        }

        public void TestMethod(IEvent e)
        {
            count1 += 1;
        }

        public void TestMethod2(IEvent e)
        {
            count2 += 1;
        }

        public void TestMethod3(IEvent e)
        {
            count1 -= 1;
        }
    }

    [TestFixture]
    public static class TestEventBus
    {
        [Test]
        public static void TestSingleSubscriber()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTagListener("tag", s.TestMethod);
            EventBus.NewTypeListener("test", s.TestMethod2);
            EventBus.Push<TestEvent>(new TestEvent());
            EventBus.Push<TestEvent>(new TestEvent());
            EventBus.Push<TestEvent>("tag", new TestEvent());
            Assert.AreEqual(s.count2, 3);
            Assert.AreEqual(s.count1, 1);
            EventBus.resetAllListeners();
        }

        [Test]
        public static void TestMultipleTypeSubscriber()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTypeListener("test", s.TestMethod);
            EventBus.Push<TestEvent>(new TestEvent());
            Assert.AreEqual(s.count1, 1);
            Assert.AreEqual(s.count2, 0);
            EventBus.NewTypeListener("test", s.TestMethod2);
            EventBus.NewTypeListener("test", s.TestMethod3);
            EventBus.Push<TestEvent>(new TestEvent());
            Assert.AreEqual(s.count1, 1);
            Assert.AreEqual(s.count2, 1);
            EventBus.resetAllListeners();
        }

        [Test]
        public static void TestMultipleTagSubscriber()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTagListener("tag", s.TestMethod);
            EventBus.Push<TestEvent>("tag", new TestEvent());
            Assert.AreEqual(s.count1, 1);
            Assert.AreEqual(s.count2, 0);
            EventBus.NewTagListener("tag", s.TestMethod2);
            EventBus.NewTagListener("tag", s.TestMethod3);
            EventBus.Push<TestEvent>("tag", new TestEvent());
            Assert.AreEqual(s.count1, 1);
            Assert.AreEqual(s.count2, 1);
            EventBus.resetAllListeners();
        }

        [Test]
        public static void TestVariedTypes()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTypeListener("test", s.TestMethod);
            EventBus.NewTypeListener("other", s.TestMethod2);
            EventBus.NewTagListener("tag", s.TestMethod3);
            EventBus.Push<TestEvent>("tag", new TestEvent());
            EventBus.Push<TestEvent>("tag", new TestEvent());
            EventBus.Push<OtherEvent>("tag", new OtherEvent());
            Assert.AreEqual(s.count1, -1);
            Assert.AreEqual(s.count2, 1);
            EventBus.resetAllListeners();
        }

        [Test]
        public static void TestPushToNonexistentTag()
        {
            var ex = Assert.Throws<Exception>(() => EventBus.Push<TestEvent>("tag", new TestEvent()));
            Assert.That(ex.Message == "No subscribers found");
        }

        [Test]
        public static void TestPushToNonexistentType()
        {
            var ex = Assert.Throws<Exception>(() => EventBus.Push<TestEvent>(new TestEvent()));
            Assert.That(ex.Message == "No subscribers found");
        }

    }
}
