using System;
using NUnit.Framework;
using SynthesisAPI.EventBus;
using SynthesisAPI.Utilities;

namespace TestApi
{
    public class TestEvent : IEvent
    {
        public object[] GetArguments() => new object[] { };
    }

    public class OtherEvent : IEvent
    {
        public object[] GetArguments() => new object[] { };
    }

    public class ParameterizedEvent : IEvent
    {
        public readonly int    NumberArg;
        public readonly string StringArg;

        public ParameterizedEvent(int numberArg, string stringArg)
        {
            NumberArg = numberArg;
            StringArg = stringArg;
        }
    }

    public class Subscriber
    {
        public int Count1;
        public int Count2;

        public int Num;
        public string String;

        public Subscriber()
        {
            Count1 = 0;
            Count2 = 0;
        }

        public void TestMethod(IEvent e)
        {
            Count1 += 1;
        }

        public void TestMethod2(IEvent e)
        {
            Count2 += 1;
        }

        public void TestMethod3(IEvent e)
        {
            Count1 -= 1;
        }

        public void TestMethod4(IEvent e)
        {
            if (e is ParameterizedEvent paramEvent)
            {
                Num = paramEvent.NumberArg;
                String = paramEvent.StringArg;
            }
            else
            {
                throw new Exception();
            }
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
            Assert.IsTrue(EventBus.Push(new TestEvent()));
            Assert.IsTrue(EventBus.Push(new TestEvent()));
            Assert.IsTrue(EventBus.Push("tag", new TestEvent()));
            Assert.AreEqual(3, s.Count2);
            Assert.AreEqual(1, s.Count1);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestMultipleTagEvent()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTagListener("tag1", s.TestMethod);
            EventBus.NewTagListener("tag2", s.TestMethod2);
            Assert.IsTrue(EventBus.Push(new[] { "tag1", "tag2" }, new TestEvent()));
            Assert.AreEqual(1, s.Count2);
            Assert.AreEqual(1, s.Count1);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestMultipleTypeSubscriber()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTypeListener<TestEvent>(s.TestMethod);
            Assert.IsTrue(EventBus.Push(new TestEvent()));
            Assert.AreEqual(1, s.Count1);
            Assert.AreEqual(0, s.Count2);
            EventBus.NewTypeListener<TestEvent>(s.TestMethod2);
            EventBus.NewTypeListener<TestEvent>(s.TestMethod3);
            Assert.IsTrue(EventBus.Push(new TestEvent()));
            Assert.AreEqual(1, s.Count1);
            Assert.AreEqual(1, s.Count2);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestMultipleTagSubscriber()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTagListener("tag", s.TestMethod);
            Assert.IsTrue(EventBus.Push("tag", new TestEvent()));
            Assert.AreEqual(1, s.Count1);
            Assert.AreEqual(0, s.Count2);
            EventBus.NewTagListener("tag", s.TestMethod2);
            EventBus.NewTagListener("tag", s.TestMethod3);
            Assert.IsTrue(EventBus.Push("tag", new TestEvent()));
            Assert.AreEqual(1, s.Count1);
            Assert.AreEqual(1, s.Count2);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestVariedTypes()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTypeListener<TestEvent>(s.TestMethod);
            EventBus.NewTypeListener<OtherEvent>(s.TestMethod2);
            EventBus.NewTagListener("tag", s.TestMethod3);
            Assert.IsTrue(EventBus.Push("tag", new TestEvent()));
            Assert.IsTrue(EventBus.Push("tag", new TestEvent()));
            Assert.IsTrue(EventBus.Push("tag", new OtherEvent()));
            Assert.AreEqual(-1, s.Count1);
            Assert.AreEqual(1, s.Count2);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestPushToNonexistentTag()
        {
            Assert.IsFalse(EventBus.Push("tag", new TestEvent()));
        }

        [Test]
        public static void TestPushToNonexistentType()
        {
            Assert.IsFalse(EventBus.Push(new TestEvent()));
        }

        [Test]
        public static void TestTagListenerRemoval()
        {
            Subscriber s = new Subscriber();
            Subscriber s2 = new Subscriber();
            Assert.IsFalse(EventBus.RemoveTagListener("tag", s.TestMethod));
            EventBus.NewTagListener("tag", s.TestMethod);
            EventBus.NewTagListener("tag", s2.TestMethod);
            Assert.IsTrue(EventBus.Push("tag", new TestEvent()));
            Assert.AreEqual(1, s.Count1);
            Assert.AreEqual(1, s2.Count1);
            Assert.IsTrue(EventBus.RemoveTagListener("tag", s.TestMethod));
            Assert.IsTrue(EventBus.Push("tag", new TestEvent()));
            Assert.AreEqual(1, s.Count1);
            Assert.AreEqual(2, s2.Count1);
            EventBus.RemoveTagListener("tag", s.TestMethod);
            Assert.IsTrue(EventBus.Push("tag", new TestEvent()));
            Assert.AreEqual(1, s.Count1);
            Assert.AreEqual(3, s2.Count1);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestTypeListenerRemoval()
        {
            Subscriber s = new Subscriber();
            Assert.IsFalse(EventBus.RemoveTypeListener<TestEvent>(s.TestMethod));
            EventBus.NewTypeListener<TestEvent>(s.TestMethod);
            Assert.IsTrue(EventBus.Push(new TestEvent()));
            Assert.AreEqual(1, s.Count1);
            Assert.IsTrue(EventBus.RemoveTypeListener<TestEvent>(s.TestMethod));
            Assert.IsFalse(EventBus.Push("tag", new TestEvent()));
            Assert.AreEqual(1, s.Count1);
            Assert.IsFalse(EventBus.RemoveTypeListener<TestEvent>(s.TestMethod));
            Assert.IsFalse(EventBus.Push("tag", new TestEvent()));
            Assert.AreEqual(1, s.Count1);
            EventBus.ResetAllListeners();
        }

        [Test]
        public static void TestParameterizedListener()
        {
            Subscriber s = new Subscriber();
            EventBus.NewTypeListener<ParameterizedEvent>(s.TestMethod4);
            EventBus.Push(new ParameterizedEvent(1, "test"));
            Assert.AreEqual(1, s.Num);
            Assert.AreEqual("test", s.String);
            EventBus.ResetAllListeners();
        }
    }
}
