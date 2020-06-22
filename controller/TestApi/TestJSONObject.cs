using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestApi
{
    [JsonObject("TestJSONObject")]
    public class TestJSONObject
    {
        [JsonProperty("Text")]
        public string Text { get; set; }
    }
}
