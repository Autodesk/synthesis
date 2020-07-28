using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;
using System.IO;
using System.Net;

namespace Controller.Jrpc
{
    [ModuleExport]
    public class JrpcServer : SystemBase
    {
        private static HttpListener listener = new HttpListener();
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
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();

                var requestContent = new StreamReader(context.Request.InputStream).ReadToEnd();
                MethodCallContext call = MethodCallContext.FromJson(requestContent);

                Result<object, System.Exception> result;

                if (call.Version != JrpcManager.Version)
                {
                    result = new Result<object, System.Exception>(
                        new System.Exception($"Incompatible Jrpc versions call {call.Version} vs current {JrpcManager.Version}"));
                }
                else
                {
                    result = JrpcManager.Invoke(call.MethodName, call.Params.ToArray());
                }

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(JrpcResponse.ToJson(JrpcManager.Version, result));

                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close(); // Must close output stream
            }
            // listener.Stop();
        }
    }
}