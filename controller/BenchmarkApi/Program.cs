using BenchmarkDotNet.Running;

namespace BenchmarkApi
{
    public class Program
    {
        static void Main(string[] args)
        {
            // BenchmarkRunner.Run<BenchmarkVirtualFileSystem>();
            BenchmarkRunner.Run<BenchmarkAssetManager>();
        }
    }
}
