using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Controller
{
    [ModuleExport]
    public class ExampleClient : SystemBase
    {
        private HttpClient client = new HttpClient();
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
            ApiProvider.Log("Client: sending get request");
            HttpResponseMessage response = await client.GetAsync("");
            if (response.IsSuccessStatusCode)
            {
                ApiProvider.Log(await response.Content.ReadAsStringAsync());
            }
            else
            {
                ApiProvider.Log("Client: bad response");
            }
        }
    }
}
