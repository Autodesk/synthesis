#if ENABLE_EXAMPLE_API
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
        public static void PrintMessage(string msg)
        {
            SynthesisAPI.Runtime.ApiProvider.Log(msg);
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