using System;
using SynthesisAPI.EnvironmentManager;
using NUnit.Framework;
using Entity = System.UInt32;
using SynthesisAPI.Modules.Components;
using SynthesisAPI.Modules;

namespace TestApi
{
    [TestFixture]
    public static class TestEnivornmentManager
    {
        [Test]
        public static void TestAddEntity()
        {
            Entity e1 = EnvironmentManager.AddEntity();
            Assert.True(EnvironmentManager.EntityExists(e1));
            Assert.True(e1.EntityExists());
            Assert.AreEqual(e1, 1);
            Entity e2 = EnvironmentManager.AddEntity();
            Assert.True(EnvironmentManager.EntityExists(e2));
            Assert.True(e2.EntityExists());
            Assert.AreNotEqual(e1, e2);
        }
        
        [Test]
        public static void TestMultiComponents()
        {
            Entity e1 = EnvironmentManager.AddEntity();
            Assert.True(e1.EntityExists());
            Entity e2 = EnvironmentManager.AddEntity();
            Assert.True(e2.EntityExists());
            TestComponent t = new TestComponent();
            AnotherTestComponent at = new AnotherTestComponent();
            e2.SetComponent(t); //EnvironmentManager.SetComponent(e2,t)
            e2.SetComponent(at);
            Assert.Null(e1.GetComponent(typeof(TestComponent)));
            Assert.Null(e1.GetComponent(typeof(AnotherTestComponent)));
            Assert.Null(e1.GetComponent<TestComponent>());
            Assert.Null(e1.GetComponent<AnotherTestComponent>());
            Assert.AreSame(e2.GetComponent(typeof(TestComponent)), t);
            Assert.AreSame(e2.GetComponent(typeof(AnotherTestComponent)), at);
            Assert.AreSame(e2.GetComponent<TestComponent>(), t);
            Assert.AreSame(e2.GetComponent<AnotherTestComponent>(), at);
            TestComponent t2 = new TestComponent();
            e2.SetComponent(t2);
            Assert.AreSame(e2.GetComponent<TestComponent>(), t2);
        }
        [Test]
        public static void TestRemoveEntity()
        {
            Entity e1 = EnvironmentManager.AddEntity();
            Assert.True(e1.EntityExists());
            Assert.True(e1.RemoveEntity());
            Assert.False(e1.RemoveEntity());
            Assert.False(e1.EntityExists());
        }

        [Test]
        public static void TestRemoveComponent()
        {
            Entity e1 = EnvironmentManager.AddEntity();
            Assert.True(e1.EntityExists());
            TestComponent t = new TestComponent();
            e1.SetComponent(t);
            Assert.AreSame(e1.GetComponent<TestComponent>(), t);
            e1.RemoveComponent<TestComponent>();
            Assert.Null(e1.GetComponent<TestComponent>());
        }

        [Test]
        public static void TestGenerationIndexes()
        {
            Entity e1 = EnvironmentManager.AddEntity();
            Assert.True(e1.EntityExists());
            TestComponent t = new TestComponent();
            e1.SetComponent(t);
            Assert.AreEqual(e1.GetComponent<TestComponent>(), t);
            Assert.True(e1.RemoveEntity());
            Assert.False(e1.RemoveEntity());
            Assert.False(e1.EntityExists());
            Entity e2 = EnvironmentManager.AddEntity();
            Assert.True(e2.EntityExists());
            Assert.AreEqual(e1 >> 16, e2 >> 16);
            Assert.AreNotEqual(e1 & 65535, e2 & 65535);
            Assert.Null(e2.GetComponent<TestComponent>());
        }

        [Test]
        public static void TestBadEntity()
        {
            Entity e1 = 1;
            Assert.False(e1.EntityExists());
            Assert.False(e1.RemoveEntity());
            Assert.Null(e1.GetComponent<TestComponent>());
            TestComponent t = new TestComponent();
            e1.SetComponent(t);
            Assert.Null(e1.GetComponent<TestComponent>());
            EnvironmentManager.AddEntity();
            Entity e2 = 2;
            Assert.False(e2.EntityExists());
            Assert.False(e2.RemoveEntity());
            Assert.Null(e2.GetComponent<TestComponent>());
            e2.SetComponent(t);
            Assert.Null(e2.GetComponent<TestComponent>());
        }
    }

    /// <summary>
    /// Empty Component class for testing
    /// </summary>
    class TestComponent : Component
    {
        
    }
    /// <summary>
    /// Another empty Component class for testing
    /// </summary>
    class AnotherTestComponent : Component
    {

    }
}
