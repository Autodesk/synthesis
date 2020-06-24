using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.VirtualFileSystem;

namespace TestApi
{
    public static class TestVirtualFileSystem
    {
        public static void TestDirectory()
        {
            Directory dir = new Directory("directory", Program.TestGuid, Permissions.PublicRead);
            FileSystem.AddResource("/modules", dir);

            Directory test_dir = (Directory)FileSystem.Traverse("/modules/directory");

            Console.WriteLine(ReferenceEquals(test_dir, dir));

            Directory parent = (Directory)FileSystem.Traverse("/modules");

            Directory test_parent = (Directory)test_dir.Traverse("..");

            Console.WriteLine(ReferenceEquals(parent, test_parent));
        }

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
                Console.WriteLine("Failure");
            }
            catch (Exception)
            {
                Console.WriteLine("Success");
            }
        }

        public static void Test()
        {
            TestDirectory();
            TestMaxDepth();
        }
    }
}
