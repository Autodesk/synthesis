#if true
using MathNet.Spatial.Euclidean;
using SynthesisAPI.Runtime;

namespace Controller.Rpc
{
    public static class ExampleApi
    {
        [RpcMethod]
        public static long Add(long a, long b)
        {
            return a + b;
        }

        [RpcMethod]
        public static void PrintMessage(string msg, LogLevel logLevel = LogLevel.Info)
        {
            ApiProvider.Log(msg, logLevel);
        }

        [RpcMethod]
        public static void PrintArray(string[] msg)
        {
            foreach (var m in msg)
            {
                ApiProvider.Log(m);
            }
        }

        [RpcMethod]
        public static string ReturnString(string msg)
        {
            return msg;
        }

        [RpcMethod]
        public static void ThrowException(string msg)
        {
            throw new System.Exception(msg);
        }
    }
}
#endif