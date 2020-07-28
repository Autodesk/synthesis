using Newtonsoft.Json;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using System.IO;
using System.Net;

namespace Controller.Jrpc
{
    [ModuleExport]
    public class JrpcServer : SystemBase
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
            HttpListenerContext context = await listener.GetContextAsync();

            var requestContent = new StreamReader(context.Request.InputStream).ReadToEnd();
            MethodCallContext call = MethodCallContext.FromJson(requestContent);

            var responseContent = JsonConvert.SerializeObject(JrpcManager.Invoke(call.MethodName, call.Params.ToArray()));
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseContent);

            HttpListenerResponse response = context.Response;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close(); // Must close output stream

            listener.Stop();
        }
    }
}