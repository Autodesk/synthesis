using System;
using SynthesisAPI.VirtualFileSystem;

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

            Console.WriteLine("=============================================\nTests finished");
        }
    }
}
