using Controller.Jrpc;
using Newtonsoft.Json;
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
        }

        public static async Task<T> InvokeAsync<T>(string methodName, params object[] args)
        {
            var s = MethodCallContext.ToJson(methodName, args);
            HttpResponseMessage response = await client.PostAsync("", new StringContent(s));
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            }
            throw new Exception();
        }
    }
}
