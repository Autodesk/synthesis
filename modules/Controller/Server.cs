using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using System.Net;

namespace Controller
{
    [ModuleExport]
    public class Server : SystemBase
    {
        HttpListener listener = new HttpListener();
        public override void Setup()
        {
            listener.Prefixes.Add("http://localhost:5000/"); // Must add prefixes
            listener.Start();
            Test();
        }
        public override void OnUpdate() { }
        public override void OnPhysicsUpdate() { }

        private async void Test()
        {
            ApiProvider.Log("Server: Waiting for connection");
            HttpListenerContext context = await listener.GetContextAsync();
            // HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Hello world!");
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close(); // Must close output stream
            listener.Stop();
        }
    }
}
