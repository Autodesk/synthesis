using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.VirtualFileSystem;
using TestApi;

namespace TestApi
{
    public class Program
    {
        public static Guid TestGuid = Guid.Empty;

        public static void Main(string[] args)
        {
            Console.WriteLine("Tests started\n=============================================");

            FileSystem.Init();
            TestVirtualFileSystem.Test();

            TestAssetManager.Test();

            TestPreferenceManager.Test();

            Console.WriteLine("=============================================\nTests finished");
        }
    }
}
