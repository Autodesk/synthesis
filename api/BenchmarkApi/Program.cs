using BenchmarkDotNet.Running;
using SynthesisAPI.WS;
using System;
using System.Threading;

namespace BenchmarkApi
{
    public class Program
    {
        static void Main(string[] args)
        {
            // BenchmarkRunner.Run<BenchmarkVirtualFileSystem>();
            // BenchmarkRunner.Run<BenchmarkAssetManager>();

            var server = new WebSocketServer("127.0.0.1", 3300);
            // server.OnMessage += (guid, m) => Console.WriteLine(m + "\n");
            server.AddOnConnectListener((guid) => Console.WriteLine($"{guid} - Connected"));
            server.AddOnDisconnectListener((guid) => Console.WriteLine($"{guid} - Disconnected"));

            while (true) { }
        }
    }
}
