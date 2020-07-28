using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Controller.Jrpc
{
    public class MethodCallContext
    {
        public string Version;
        public string MethodName;
        public List<object> Params;

        public MethodCallContext()
        {
            Params = new List<object>();
        }

        public MethodCallContext(string version, string methodName, IEnumerable<object> args = null)
        {
            Version = version;
            MethodName = methodName;
            Params = args?.ToList() ?? new List<object>();
        }

        public static string ToJson(string version, string methodName, params object[] args)
        {
            return new MethodCallContext(version, methodName, args).ToJson();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static MethodCallContext FromJson(string json)
        {
            return JsonConvert.DeserializeObject<MethodCallContext>(json);
        }
    }
}
