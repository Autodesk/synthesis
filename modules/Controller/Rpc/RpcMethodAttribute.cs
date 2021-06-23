using System;

namespace Controller.Rpc
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class RpcMethodAttribute : Attribute
    {
        internal string RpcMessageMethodName { get; private set; }

        public RpcMethodAttribute() { }

        public RpcMethodAttribute(string rpcMessageMethodName)
        {
            RpcMessageMethodName = rpcMessageMethodName;
        }
    }
}
