using Controller.Rpc;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Controller
{
    [ModuleExport]
    public class ExampleClient : SystemBase
    {
        private static HttpClient client = new HttpClient();
        private static string MyVersion = "1.0.0";
        public override void Setup()
        {
            client.BaseAddress = new Uri("http://localhost:5000/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Test();
        }
        public override void OnUpdate() { }
        public override void OnPhysicsUpdate() { }

        private async void Test()
        {
            var a = await InvokeAsync<long>("Add", 1, 5);
            ApiProvider.Log($"Client: result = {a}");

            await InvokeAsync("PrintMessage", "Hello world!");

            var b = await InvokeAsync<string>("ReturnString", "Hello world!");
            ApiProvider.Log($"Client: result = {b}");

            var c = await InvokeAsync<string>("ReturnString", "");
            ApiProvider.Log($"Client: result = {c}");

            await InvokeAsync("ThrowException", "Test error");
        }

        public static async Task<T> InvokeAsync<T>(string methodName, params object[] args)
        {
            var result = await MakeRequestAsync(methodName, args);
            if (!result.HasResult)
            {
                throw new Exception($"Method {methodName} did not return result");
            }
            return (T)result.Result;
        }

        public static async Task InvokeAsync(string methodName, params object[] args)
        {
            var result = await MakeRequestAsync(methodName, args);

            if (result.HasResult)
            {
                ApiProvider.Log($"Discarding result value from call to {methodName}", LogLevel.Warning);
            }
            return;
        }

        private static async Task<string> ReadContent(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            throw new Exception($"HTTP response status code {response.StatusCode}");
        }

        private static async Task<RpcResponse> MakeRequestAsync(string methodName, params object[] args)
        {
            var response = await client.PostAsync("", new StringContent(MethodCallContext.ToJson(MyVersion, methodName, args)));
            var result = RpcResponse.FromJson(await ReadContent(response));
            if(result.Version != MyVersion)
            {
                throw new Exception($"Incompatible RPC versions result {result.Version} vs current {MyVersion}");
            }
            if (result.Error != null)
            {
                throw result.Error;
            }
            return result;
        }
    }
}
