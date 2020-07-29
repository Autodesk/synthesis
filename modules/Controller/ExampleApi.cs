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
        public static string ReturnString(string msg)
        {
            return msg;
        }

        [RpcMethod]
        public static void ThrowException(string msg)
        {
            throw new System.Exception(msg);
        }

        [RpcMethod]
        public static void PrintCompound(Vector3D vec)
        {
            ApiProvider.Log(vec.ToString());
        }

        [RpcMethod]
        public static Vector3D ReturnCompound(Vector3D vec)
        {
            return vec;
        }
    }
}
#endif