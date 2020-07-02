using Newtonsoft.Json;

namespace TestApi
{
    [JsonObject("TestJsonObject")]
    public class TestJsonObject
    {
        [JsonProperty("Text")]
        public string Text { get; set; }
    }
}
