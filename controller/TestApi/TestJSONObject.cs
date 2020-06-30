using Newtonsoft.Json;

namespace TestApi
{
    [JsonObject("TestJSONObject")]
    public class TestJsonObject
    {
        [JsonProperty("Text")]
        public string Text { get; set; }
    }
}
