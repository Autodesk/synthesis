using System;
using SynthesisAPI.VirtualFileSystem;
using NUnit.Framework;

namespace TestApi
{
    [TestFixture]
    public static class TestVirtualFileSystem
    {
        public static string FilePrefix = typeof(TestVirtualFileSystem).Name;

        [Test]
        public static void TestAddEntry()
        {
            string dirLoc = "/temp";
            string dirName = FilePrefix + "directory1";

            var dir = new Directory(dirName, Permissions.PublicReadWrite);
            
            FileSystem.AddEntry(dirLoc, dir);
            var testDir = FileSystem.Traverse<Directory>(dirLoc + Directory.DirectorySeparatorChar + dirName);
            Assert.AreSame(dir, testDir);
        }

        [Test]
        public static void TestParent()
        {
            string dirLoc = "/temp";
            string dirName = FilePrefix + "directory2";
            
            var dir = new Directory(dirName, Permissions.PublicReadWrite);
            FileSystem.AddEntry(dirLoc, dir);

            var parent = (Directory)FileSystem.Traverse(dirLoc);
            var testParent = (Directory)dir?.Traverse("..");
            Assert.AreSame(parent, testParent);
        }

        [Test]
        public static void TestDeleteEntry()
        {
            string dirLoc = "/temp";
            string dirName = FilePrefix + "directory3";

            var dir = new Directory(dirName, Permissions.PublicReadWrite);
            FileSystem.AddEntry(dirLoc, dir);

            Assert.True(FileSystem.EntryExists(dirLoc + Directory.DirectorySeparatorChar + dirName));
            Assert.NotNull(dir.Parent);

            FileSystem.DeleteEntry(dirLoc + Directory.DirectorySeparatorChar + dirName);

            Assert.False(FileSystem.EntryExists(dirLoc + Directory.DirectorySeparatorChar + dirName));
            Assert.Null(dir.Parent);

            string dirName2 = FilePrefix + "directory4";

            var dir2 = new Directory(dirName2, Permissions.PublicReadWrite);
            FileSystem.AddEntry(dirLoc, dir2);

            Assert.True(FileSystem.EntryExists(dirLoc + Directory.DirectorySeparatorChar + dirName2));
            Assert.NotNull(dir2.Parent);

            dir2.Delete();

            Assert.False(FileSystem.EntryExists(dirLoc + Directory.DirectorySeparatorChar + dirName2));
            Assert.Null(dir2.Parent);
        }

        [Test]
        public static void TestDirectoryPermissions()
        {
            string dirLoc1 = "/temp";
            string dirName1 = FilePrefix + "directory4";
            try
            {
                var dir = new Directory(dirName1, Permissions.PublicReadWrite);

                FileSystem.AddEntry(dirLoc1, dir);
            }
            catch (PermissionsExpcetion)
            {
                Assert.Fail();
            }
            Assert.True(FileSystem.EntryExists(dirLoc1, dirName1));


            string dirLoc2 = "/";
            string dirName2 = FilePrefix + "directory5";
            try
            {
                var dir = new Directory(dirName2, Permissions.PublicReadWrite);

                FileSystem.AddEntry(dirLoc2, dir);
                Assert.Fail();
            }
            catch (PermissionsExpcetion)
            {
                Assert.False(FileSystem.EntryExists(dirLoc2, dirName2));
            }
        }

        [Test]
        public static void TestMaxDepth()
        {
            string path = "/temp";
            try
            {
                Console.WriteLine(FileSystem.DepthOfPath(path));
                for (var i = FileSystem.DepthOfPath(path); i < FileSystem.MaxDirectoryDepth + 1; i++)
                {
                    Directory dir = new Directory("TestMaxDepthDir" + i,  Permissions.PublicReadWrite);
                    FileSystem.AddEntry(path, dir);
                    path += "/" + dir.Name;
                }
                Assert.Fail();
            }
            catch (DirectroyDepthExpection)
            {
                Assert.Pass();
            }
        }

        [Test]
        public static void TestCreatePath()
        {
            string path = "/temp/TestCreatePath/new_dir/new_dir/new_dir";
            
            Directory dir = new Directory("new_dir", Permissions.PrivateReadWrite);

            Assert.Null(FileSystem.Traverse(path));

            Assert.NotNull(FileSystem.AddEntry(path, dir));
        }
    }
}
