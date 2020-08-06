using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using System;
using System.IO;
using System.Net;

#nullable enable

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

        public override void Teardown()
        {
            if (listener.IsListening)
                listener.Stop(); // close server when Synthesis closes
        }

        private async void Start()
        {
            while (listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();

                    var requestContent = new StreamReader(context.Request.InputStream).ReadToEnd();

                    Result<object, RpcError> result;
                    MethodCallContext? call = null;

                    try
                    {
                        call = MethodCallContext.FromJson(requestContent);

                        if (call.JsonRpcVersion != RpcManager.JsonRpcVersion)
                        {
                            result = new Result<object, RpcError>(
                                new InvalidRequest($"Incompatible RPC versions call {call.JsonRpcVersion} vs current {RpcManager.JsonRpcVersion}"));
                        }
                        else
                        {
                            result = RpcManager.Invoke(call.MethodName, call.Params.ToArray());
                        }
                    }
                    catch (Exception e)
                    {
                        result = new Result<object, RpcError>((ParseError)e);
                    }

                    var response = RpcResponse.ToJson(RpcManager.JsonRpcVersion, result, call == null ? 0 : call.Id);

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(response);

                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.OutputStream.Close(); // Must close output stream
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }
        }
    }
}