using Newtonsoft.Json;

#nullable enable

namespace Controller.Rpc
{
    public class RpcResponse
    {
        public class ErrorObject
        {
            [JsonProperty("code")]
            public int Code;
            [JsonProperty("message")]
            public string Message;
            [JsonProperty("data")]
            public object? Data;

            public ErrorObject() : this(0, "") { }

            public ErrorObject(int code, string message, object? data = null)
            {
                Code = code;
                Message = message;
                Data = data;
            }

            public static ErrorObject FromException(RpcError e)
            {
                // Error codes and names from https://www.jsonrpc.org/specification
                var eo = new ErrorObject();
                if (e is ParseError)
                    eo = new ErrorObject(-32700, "Parse error");
                else if (e is InvalidRequest)
                    eo = new ErrorObject(-32600, "Invalid request");
                else if (e is MethodNotFound)
                    eo = new ErrorObject(-32601, "Method not found");
                else if (e is InvalidParams)
                    eo = new ErrorObject(-32602, "Invalid params");
                else if (e is InternalError)
                    eo = new ErrorObject(-32603, "Internal error");
                else if (e is ServerError)
                    eo = new ErrorObject(-32000, "Server error");
                eo.Data = e.ToString();
                return eo;
            }
        }

        [JsonProperty("jsonrpc")]
        public string JsonRpcVersion;
        
        [JsonProperty("result")]
        public object? Result;
        
        //[JsonIgnore]
        //public bool HasResult { get; } // TODO consider adding this back in somehow
        
        [JsonProperty("error")]
        public ErrorObject? Error;
        
        [JsonProperty("id")]
        public int Id;

        public RpcResponse()
        {
            Result = null;
            //HasResult = false;
            Error = null;
            JsonRpcVersion = "";
        }

        public RpcResponse(string version, SynthesisAPI.Utilities.Result<object, RpcError> result, int id)
        {
            if (result.isError)
            {
                Error = ErrorObject.FromException(result.GetError());
            }
            else if(!(result.GetResult() is Void))
            {
                Result = result.GetResult();
                // HasResult = true;
            }
            JsonRpcVersion = version;
            Id = id;
        }

        public static string ToJson(string version, SynthesisAPI.Utilities.Result<object, RpcError> result, int id)
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
