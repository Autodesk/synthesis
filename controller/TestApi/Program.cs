using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.PreferenceManager;
using SynthesisAPI.VirtualFileSystem;
using TestApi;

namespace TestApi
{
    public class Program
    {
        public static Guid TestGuid = Guid.Empty;

        public static void TestDirectory()
        {
            Directory dir = new Directory("directory", TestGuid, Permissions.PublicRead);
            FileSystem.AddResource("/modules", dir);

            Directory test_dir = (Directory)FileSystem.Traverse("/modules/directory");

            Console.WriteLine(ReferenceEquals(test_dir, dir));

            Directory root = (Directory)test_dir.Traverse("../..");

            Console.WriteLine(ReferenceEquals(root, FileSystem.RootNode));
        }

        public static void TestRawEntry()
        {
            RawEntry raw_entry = new RawEntry("test.txt", TestGuid, Permissions.PublicRead, "/controller/TestApi/test.txt");
            FileSystem.AddResource("/modules", raw_entry);
            raw_entry.Load();

            string str = Encoding.UTF8.GetString(raw_entry.SharedStream.ReadBytes(30));
            Console.WriteLine("\"" + str + "\"");

            raw_entry.SharedStream.WriteBytes("Goodbye World!");
            raw_entry.SharedStream.SetStreamPosition(0);

            str = Encoding.UTF8.GetString(raw_entry.SharedStream.ReadBytes(30));
            Console.WriteLine(str);
        }

        public static void TestSavingPreferences()
        {
            PreferenceManager.SetPreference("test_api", "name", "Hunter Barclay");
            PreferenceManager.SetPreference("test_api", "age", 17);
            PreferenceManager.SetPreference("test_api", "some_float", 1.5837f);
            PreferenceManager.Save("test-prefs-2.json");
        }

        public static void TestLoadingPreferences()
        {
            PreferenceManager.SetPreference("test_api", "name", "Gerald");
            PreferenceManager.Load("test-prefs-2.json", overrideChanges: false);
            string name = PreferenceManager.GetPreference<string>("test_api", "name");
            int age = PreferenceManager.GetPreference<int>("test_api", "age");
            float someFloat = PreferenceManager.GetPreference<float>("test_api", "some_float");

            Console.WriteLine(string.Format("Name: {0}\nAge: {1}\nSome Float: {2}", name, age, someFloat));
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Tests started\n=============================================");
            FileSystem.Init();

            // VFS Tests don't work because they are configured for someones file directory machine
            // TestDirectory();
            // TestRawEntry();
            TestSavingPreferences();
            TestLoadingPreferences();

            Console.ReadKey();
        }
    }
}
