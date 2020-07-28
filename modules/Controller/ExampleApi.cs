namespace Controller.Jrpc
{
    public static class ExampleApi
    {
        [JrpcMethod]
        public static long Add(long a, long b)
        {
            return a + b;
        }

        [JrpcMethod]
        public static void PrintMessage(string msg)
        {
            SynthesisAPI.Runtime.ApiProvider.Log(msg);
        }

        [JrpcMethod]
        public static string ReturnString(string msg)
        {
            return msg;
        }

        [JrpcMethod]
        public static void ThrowException(string msg)
        {
            throw new System.Exception(msg);
        }
    }
}
