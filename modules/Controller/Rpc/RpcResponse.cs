using Newtonsoft.Json;
using System;

#nullable enable

namespace Controller.Rpc
{
    public class RpcResponse
    {
        [JsonProperty("jsonrpc")]
        public string JsonRpcVersion;
        [JsonProperty("result")]
        public object? Result;
        [JsonProperty("has-value")]
        public bool HasResult;
        [JsonProperty("error")]
        public Exception? Error;
        [JsonProperty("id")]
        public int Id;

        public RpcResponse()
        {
            Result = null;
            HasResult = false;
            Error = null;
            JsonRpcVersion = "";
        }

        public RpcResponse(string version, SynthesisAPI.Utilities.Result<object, Exception> result, int id)
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
            JsonRpcVersion = version;
            Id = id;
        }

        public static string ToJson(string version, SynthesisAPI.Utilities.Result<object, Exception> result, int id)
        {
            return new RpcResponse(version, result, id).ToJson();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, RpcManager.JsonSettings);
        }

        public static RpcResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<RpcResponse>(json, RpcManager.JsonSettings);
        }
    }
}
