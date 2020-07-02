using System;

namespace TestApi
{
    public class Program
    {
        public static Guid TestGuid = Guid.Empty;

        public static void Main(string[] args)
        {
            TestVirtualFileSystem.TestMaxDepth();
        }
    }
}
