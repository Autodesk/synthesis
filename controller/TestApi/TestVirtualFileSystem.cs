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

            Directory root = (Directory)test_dir.Traverse("../..");

            Console.WriteLine(ReferenceEquals(root, FileSystem.RootNode));
        }

        public static void TestRawEntry()
        {
            RawEntry raw_entry = new RawEntry("test.txt", Program.TestGuid, Permissions.PublicRead, "/controller/TestApi/files/test.txt");
            FileSystem.AddResource("/modules", raw_entry);
            raw_entry.Load();

            string str = Encoding.UTF8.GetString(raw_entry.SharedStream.ReadBytes(30));
            Console.WriteLine("\"" + str + "\"");

            raw_entry.SharedStream.WriteBytes("Goodbye World!");
            raw_entry.SharedStream.SetStreamPosition(0);

            str = Encoding.UTF8.GetString(raw_entry.SharedStream.ReadBytes(30));
            Console.WriteLine(str);
        }

        public static void Test()
        {
            TestDirectory();
            TestRawEntry();
        }
    }
}
