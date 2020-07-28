namespace Controller.Jrpc
{
    public static class ExampleApi
    {
        [JrpcMethod]
        public static long Add(long a, long b)
        {
            return a + b;
        }
    }
}
