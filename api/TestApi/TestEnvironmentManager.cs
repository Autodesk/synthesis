using SynthesisAPI.EnvironmentManager;
using NUnit.Framework;

namespace TestApi
{
    [TestFixture]
    public static class TestEnivornmentManager
    {

        [SetUp]
        public static void SetUp()
        {
            EnvironmentManager.Clear();
        }

        [Test]
        public static void TestEntity()
        {
            uint aActual = 5;
            uint bActual = 6;
            Entity a = aActual;
            Entity b = bActual;

            Assert.True(aActual == a);
            Assert.True(a.Equals(aActual));
            Assert.True(aActual.Equals(a));

            Assert.AreNotEqual(a, b);

            b = aActual;
            Assert.AreEqual(a, b);
            Assert.AreEqual(aActual.ToString(), a.ToString());
        }

        [Test]
        public static void TestAddEntity()
        {
            Entity e1 = EnvironmentManager.AddEntity();
            Assert.True(EnvironmentManager.EntityExists(e1));
            Assert.True(e1.EntityExists());
            Assert.AreEqual(e1, (Entity)1);
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
            TestComponent t = e2.AddComponent<TestComponent>(); //EnvironmentManager.SetComponent(e2,t)
            AnotherTestComponent at = e2.AddComponent<AnotherTestComponent>();
            Assert.Null(e1.GetComponent(typeof(TestComponent)));
            Assert.Null(e1.GetComponent(typeof(AnotherTestComponent)));
            Assert.Null(e1.GetComponent<TestComponent>());
            Assert.Null(e1.GetComponent<AnotherTestComponent>());
            Assert.AreSame(e2.GetComponent(typeof(TestComponent)), t);
            Assert.AreSame(e2.GetComponent(typeof(AnotherTestComponent)), at);
            Assert.AreSame(e2.GetComponent<TestComponent>(), t);
            Assert.AreSame(e2.GetComponent<AnotherTestComponent>(), at);
            TestComponent t2 = e2.AddComponent<TestComponent>();
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
            TestComponent t = e1.AddComponent<TestComponent>();
            Assert.AreSame(e1.GetComponent<TestComponent>(), t);
            e1.RemoveComponent<TestComponent>();
            Assert.Null(e1.GetComponent<TestComponent>());
        }

        [Test]
        public static void TestGenerationIndexes()
        {
            Entity e1 = EnvironmentManager.AddEntity();
            Assert.True(e1.EntityExists());
            TestComponent t = e1.AddComponent<TestComponent>();
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
            TestComponent t = e1.AddComponent<TestComponent>();
            Assert.Null(e1.GetComponent<TestComponent>());
            EnvironmentManager.AddEntity();
            Entity e2 = 2;
            Assert.False(e2.EntityExists());
            Assert.False(e2.RemoveEntity());
            Assert.Null(e2.GetComponent<TestComponent>());
            e2.AddComponent<TestComponent>();
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
