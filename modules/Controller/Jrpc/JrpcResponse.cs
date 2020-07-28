#nullable enable

using Newtonsoft.Json;
using System;

namespace Controller.Jrpc
{
    public class JrpcResponse
    {
        public object? Result;
        public bool HasResult;
        public Exception? Error;
        public string Version;

        public JrpcResponse()
        {
            Result = null;
            HasResult = false;
            Error = null;
            Version = "";
        }

        public JrpcResponse(string version, object result, bool hasResult, Exception error) // TODO make internal?
        {
            Result = result;
            HasResult = hasResult;
            Error = error;
            Version = version;
        }

        public JrpcResponse(string version, SynthesisAPI.Utilities.Result<object, Exception> result)
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
            return new JrpcResponse(version, result).ToJson();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static JrpcResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<JrpcResponse>(json);
        }
    }
}
