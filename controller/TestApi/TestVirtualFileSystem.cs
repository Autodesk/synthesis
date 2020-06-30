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

            Directory testDir = (Directory)FileSystem.Traverse("/temp/directory");

            Assert.AreSame(dir, testDir);

            Directory parent = (Directory)FileSystem.Traverse("/temp");

            Directory testParent = (Directory)testDir.Traverse("..");

            Assert.AreSame(parent, testParent);
        }

        [Test]
        public static void TestDirectoryPermissions()
        {
            try
            {
                var dir = new Directory("directoryperms", Program.TestGuid, Permissions.PublicReadWrite);

                FileSystem.AddResource("temp/", dir);
            }
            catch (PermissionsExpcetion)
            {
                Assert.Fail();
            }
            try
            {
                var dir = new Directory("directoryperms2", Program.TestGuid, Permissions.PublicReadWrite);

                FileSystem.AddResource("", dir);
                Assert.Fail();
            }
            catch (PermissionsExpcetion) { }
            Assert.Pass();
        }

        [Test]
        public static void TestMaxDepth()
        {
            string path = "/temp";
            try
            {
                for (var i = 1; i < FileSystem.MaxDirectoryDepth; i++)
                {
                    Directory dir = new Directory("directory" + i, Program.TestGuid, Permissions.PublicReadOnly);
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
