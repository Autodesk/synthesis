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
            Directory dir = new Directory("directory", Program.TestGuid, Permissions.PublicReadWrite);
            FileSystem.AddResource("/temp", dir);

            Directory test_dir = (Directory)FileSystem.Traverse("/temp/directory");

            Assert.AreSame(dir, test_dir);

            Directory parent = (Directory)FileSystem.Traverse("/temp");

            Directory test_parent = (Directory)test_dir.Traverse("..");

            Assert.AreSame(parent, test_parent);
        }

        [Test]
        public static void TestDirectoryPermissions()
        {
            try
            {
                Directory dir = new Directory("directory", Program.TestGuid, Permissions.PublicReadWrite);

                FileSystem.AddResource("", dir);
                Assert.Fail();
            }
            catch (Exception)
            {
                Assert.Pass();
            }
        }

        [Test]
        public static void TestMaxDepth()
        {
            string path = "/temp";
            try
            {
                for (var i = 1; i < FileSystem.MaxDirectoryDepth; i++)
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
