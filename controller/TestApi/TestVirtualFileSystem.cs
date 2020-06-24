using System;
using SynthesisAPI.VirtualFileSystem;
using NUnit.Framework;

namespace TestApi
{
    [TestFixture]
    public static class TestVirtualFileSystem
    {
        [Test]
        public static void TestDirectory()
        {
            Directory dir = new Directory("directory", Program.TestGuid, Permissions.PublicRead);
            FileSystem.AddResource("/modules", dir);

            Directory test_dir = (Directory)FileSystem.Traverse("/modules/directory");

            Assert.AreSame(dir, test_dir);

            Directory parent = (Directory)FileSystem.Traverse("/modules");

            Directory test_parent = (Directory)test_dir.Traverse("..");

            Assert.AreSame(parent, test_parent);
        }

        [Test]
        public static void TestMaxDepth()
        {
            string path = "";
            try
            {
                for (var i = 0; i < FileSystem.MaxDirectoryDepth; i++)
                {
                    Directory dir = new Directory("directory" + i, Program.TestGuid, Permissions.PublicRead);
                    FileSystem.AddResource(path, dir);
                    path += "/" + dir.Name;
                }
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
        }
    }
}
