using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.VirtualFileSystem;

namespace ApiTest
{
    public class Program
    {
        public static void TestDirectory()
        {
            Directory raw_entry = new Directory("directory", Guid.Empty, Permissions.PublicRead);
            FileSystem.AddResource("/modules", raw_entry);

            Console.WriteLine();
        }

        public static void TestRawEntry()
        {
            string file_path = "/controller/TestApi/test.txt";
            RawEntry raw_entry = new RawEntry("test.txt", Guid.Empty, Permissions.PublicRead, file_path);
            ((RawEntry) FileSystem.AddResource("/modules", raw_entry)).Load();

            string data = Encoding.UTF32.GetString(raw_entry.SharedStream.ReadBytes(30));
            Console.WriteLine(data);

            /*
            raw_entry.SharedStream.WriteBytes("Goodbye World!");

            data = Encoding.UTF32.GetString(raw_entry.SharedStream.ReadBytes(30));
            Console.WriteLine(data);
            */
            Console.WriteLine();
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Tests started");
            FileSystem.Init();

            TestDirectory();
            TestRawEntry();
        }
    }
}
