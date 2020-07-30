using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using System.IO;
using System.Net;

namespace Controller.Rpc
{
    [ModuleExport]
    public class RpcServer : SystemBase
    {
        private static HttpListener listener = new HttpListener();
        public const string Host = "localhost";
        public const string Port = "5000";
        public const string Prefix = "http://" + Host + ":" + Port + "/";
        public override void Setup()
        {
            RpcManager.Init();
            listener.Prefixes.Add(Prefix); // Must add prefixes
            listener.Start();
            Start();
        }
        public override void OnUpdate() { }
        public override void OnPhysicsUpdate() { }

        private async void Start()
        {
            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();

                var requestContent = new StreamReader(context.Request.InputStream).ReadToEnd();
                MethodCallContext call = MethodCallContext.FromJson(requestContent);

                Result<object, System.Exception> result;

                if (call.JsonRpcVersion != RpcManager.JsonRpcVersion)
                {
                    result = new Result<object, System.Exception>(
                        new System.Exception($"Incompatible RPC versions call {call.JsonRpcVersion} vs current {RpcManager.JsonRpcVersion}"));
                }
                else
                {
                    result = RpcManager.Invoke(call.MethodName, call.Params.ToArray());
                }

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(RpcResponse.ToJson(RpcManager.JsonRpcVersion, result, call.Id));

                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close(); // Must close output stream
            }
            // listener.Stop();
        }
    }
}