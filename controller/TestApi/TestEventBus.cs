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
        public TestEvent() { }
        string IEvent.EventType => "test";
        byte[] IEvent.getData() => null;
    }

    public class OtherEvent : IEvent
    {
        public OtherEvent() { }
        string IEvent.EventType => "other";
        byte[] IEvent.getData() => null;
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
            EventBus.NewTypeListener<TestEvent>(s.TestMethod2);
            Assert.IsTrue(EventBus.Push<TestEvent>(new TestEvent()));
            Assert.IsTrue(EventBus.Push<TestEvent>(new TestEvent()));
            Assert.IsTrue(EventBus.Push<TestEvent>("tag", new TestEvent()));
            Assert.AreEqual(s.count2, 3);
            Assert.AreEqual(s.count1, 1);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestMultipleTypeSubscriber()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTypeListener<TestEvent>(s.TestMethod);
            Assert.IsTrue(EventBus.Push<TestEvent>(new TestEvent()));
            Assert.AreEqual(s.count1, 1);
            Assert.AreEqual(s.count2, 0);
            EventBus.NewTypeListener<TestEvent>(s.TestMethod2);
            EventBus.NewTypeListener<TestEvent>(s.TestMethod3);
            Assert.IsTrue(EventBus.Push<TestEvent>(new TestEvent()));
            Assert.AreEqual(s.count1, 1);
            Assert.AreEqual(s.count2, 1);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestMultipleTagSubscriber()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTagListener("tag", s.TestMethod);
            Assert.IsTrue(EventBus.Push<TestEvent>("tag", new TestEvent()));
            Assert.AreEqual(s.count1, 1);
            Assert.AreEqual(s.count2, 0);
            EventBus.NewTagListener("tag", s.TestMethod2);
            EventBus.NewTagListener("tag", s.TestMethod3);
            Assert.IsTrue(EventBus.Push<TestEvent>("tag", new TestEvent()));
            Assert.AreEqual(s.count1, 1);
            Assert.AreEqual(s.count2, 1);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestVariedTypes()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTypeListener<TestEvent>(s.TestMethod);
            EventBus.NewTypeListener<OtherEvent>(s.TestMethod2);
            EventBus.NewTagListener("tag", s.TestMethod3);
            Assert.IsTrue(EventBus.Push<TestEvent>("tag", new TestEvent()));
            Assert.IsTrue(EventBus.Push<TestEvent>("tag", new TestEvent()));
            Assert.IsTrue(EventBus.Push<OtherEvent>("tag", new OtherEvent()));
            Assert.AreEqual(s.count1, -1);
            Assert.AreEqual(s.count2, 1);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestPushToNonexistentTag()
        {
            Assert.IsFalse(EventBus.Push<TestEvent>("tag", new TestEvent()));
        }

        [Test]
        public static void TestPushToNonexistentType()
        {
            Assert.IsFalse(EventBus.Push<TestEvent>(new TestEvent()));
        }

        [Test]
        public static void TestTagListenerRemoval()
        {
            Subscriber s = new Subscriber();
            Subscriber s2 = new Subscriber();
            Assert.IsFalse(EventBus.RemoveTagListener("tag", s.TestMethod));
            EventBus.NewTagListener("tag", s.TestMethod);
            EventBus.NewTagListener("tag", s2.TestMethod);
            Assert.IsTrue(EventBus.Push<TestEvent>("tag", new TestEvent()));
            Assert.AreEqual(s.count1, 1);
            Assert.AreEqual(s2.count1, 1);
            Assert.IsTrue(EventBus.RemoveTagListener("tag", s.TestMethod));
            Assert.IsTrue(EventBus.Push<TestEvent>("tag", new TestEvent()));
            Assert.AreEqual(s.count1, 1);
            Assert.AreEqual(s2.count1, 2);
            EventBus.RemoveTagListener("tag", s.TestMethod);
            Assert.IsTrue(EventBus.Push<TestEvent>("tag", new TestEvent()));
            Assert.AreEqual(s.count1, 1);
            Assert.AreEqual(s2.count1, 3);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestTypeListenerRemoval()
        {
            Subscriber s = new Subscriber();
            Assert.IsFalse(EventBus.RemoveTypeListener<TestEvent>(s.TestMethod));
            EventBus.NewTypeListener<TestEvent>(s.TestMethod);
            Assert.IsTrue(EventBus.Push<TestEvent>(new TestEvent()));
            Assert.AreEqual(s.count1, 1);
            Assert.IsTrue(EventBus.RemoveTypeListener<TestEvent>(s.TestMethod));
            Assert.IsFalse(EventBus.Push<TestEvent>("tag", new TestEvent()));
            Assert.AreEqual(s.count1, 1);
            Assert.IsFalse(EventBus.RemoveTypeListener<TestEvent>(s.TestMethod));
            Assert.IsFalse(EventBus.Push<TestEvent>("tag", new TestEvent()));
            Assert.AreEqual(s.count1, 1);
        }

    }
}
