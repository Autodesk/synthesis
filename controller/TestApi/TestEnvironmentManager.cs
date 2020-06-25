using System;
using SynthesisAPI.EnvironmentManager;
using NUnit.Framework;
using Entity = System.UInt32;


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
            Assert.AreEqual(e1, 1);
            Entity e2 = EnvironmentManager.AddEntity();
            Assert.True(EnvironmentManager.EntityExists(e2));
            Assert.AreNotEqual(e1, e2);
        }
        [Test]
        public static void TestMultiComponents()
        {
            Entity e1 = EnvironmentManager.AddEntity();
            Assert.True(EnvironmentManager.EntityExists(e1));
            Entity e2 = EnvironmentManager.AddEntity();
            Assert.True(EnvironmentManager.EntityExists(e2));
            EnvironmentManager.SetComponent<String>(e2, "test");
            EnvironmentManager.SetComponent<int?>(e2, 3);
            Assert.Null(EnvironmentManager.GetComponent<String>(e1));
            Assert.Null(EnvironmentManager.GetComponent<int?>(e1));
            Assert.AreEqual(EnvironmentManager.GetComponent<String>(e2), "test");
            Assert.AreEqual(EnvironmentManager.GetComponent<int?>(e2), 3);
            EnvironmentManager.SetComponent<String>(e2, "changed");
            Assert.AreEqual(EnvironmentManager.GetComponent<String>(e2), "changed");
        }
        [Test]
        public static void TestRemoveEntity()
        {
            Entity e1 = EnvironmentManager.AddEntity();
            Assert.True(EnvironmentManager.EntityExists(e1));
            Assert.True(EnvironmentManager.RemoveEntity(e1));
            Assert.False(EnvironmentManager.RemoveEntity(e1));
            Assert.False(EnvironmentManager.EntityExists(e1));
        }
        [Test]
        public static void TestRemoveComponent()
        {
            Entity e1 = EnvironmentManager.AddEntity();
            Assert.True(EnvironmentManager.EntityExists(e1));
            EnvironmentManager.SetComponent<String>(e1, "hello");
            Assert.AreEqual(EnvironmentManager.GetComponent<String>(e1), "hello");
            EnvironmentManager.RemoveComponent<String>(e1);
            Assert.Null(EnvironmentManager.GetComponent<String>(e1));
        }
        [Test]
        public static void TestGenerationIndexes()
        {
            Entity e1 = EnvironmentManager.AddEntity();
            Assert.True(EnvironmentManager.EntityExists(e1));
            EnvironmentManager.SetComponent<String>(e1, "test");
            Assert.AreEqual(EnvironmentManager.GetComponent<String>(e1), "test");
            Assert.True(EnvironmentManager.RemoveEntity(e1));
            Assert.False(EnvironmentManager.RemoveEntity(e1));
            Assert.False(EnvironmentManager.EntityExists(e1));
            Entity e2 = EnvironmentManager.AddEntity();
            Assert.True(EnvironmentManager.EntityExists(e2));
            Assert.AreEqual(e1 >> 16, e2 >> 16);
            Assert.AreNotEqual(e1 & 65535, e2 & 65535);
            Assert.Null(EnvironmentManager.GetComponent<String>(e2));
        }
        [Test]
        public static void TestBadEntity()
        {
            Assert.False(EnvironmentManager.EntityExists(1));
            Assert.False(EnvironmentManager.RemoveEntity(1));
            Assert.Null(EnvironmentManager.GetComponent<String>(1));
            EnvironmentManager.SetComponent<String>(1,"test");
            Assert.Null(EnvironmentManager.GetComponent<String>(1));
            EnvironmentManager.AddEntity();
            Assert.False(EnvironmentManager.EntityExists(2));
            Assert.False(EnvironmentManager.RemoveEntity(2));
            Assert.Null(EnvironmentManager.GetComponent<String>(2));
            EnvironmentManager.SetComponent<String>(2, "test");
            Assert.Null(EnvironmentManager.GetComponent<String>(2));
            //bad generation
        }
    }
}
