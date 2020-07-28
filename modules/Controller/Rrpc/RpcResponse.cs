#nullable enable

using Newtonsoft.Json;
using System;

namespace Controller.Rpc
{
    public class RpcResponse
    {
        public object? Result;
        public bool HasResult;
        public Exception? Error;
        public string Version;

        public RpcResponse()
        {
            Result = null;
            HasResult = false;
            Error = null;
            Version = "";
        }

        public RpcResponse(string version, object result, bool hasResult, Exception error) // TODO make internal?
        {
            Result = result;
            HasResult = hasResult;
            Error = error;
            Version = version;
        }

        public RpcResponse(string version, SynthesisAPI.Utilities.Result<object, Exception> result)
        {
            if (result.isError)
            {
                Error = result.GetError();
            }
            else if(!(result.GetResult() is Void))
            {
                Result = result.GetResult();
                HasResult = true;
            }
            Version = version;
        }

        public static string ToJson(string version, SynthesisAPI.Utilities.Result<object, Exception> result)
        {
            return new RpcResponse(version, result).ToJson();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static RpcResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<RpcResponse>(json);
        }
    }
}
