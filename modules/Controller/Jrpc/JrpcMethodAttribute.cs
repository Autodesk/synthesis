using System;

namespace Controller.Jrpc
{
    [AttributeUsage(AttributeTargets.Method)]
    public class JrpcMethodAttribute : Attribute
    {
        internal string JrpcMessageMethodName { get; private set; }

        public JrpcMethodAttribute() { }

        public JrpcMethodAttribute(string JrpcMessageMethodName)
        {
            this.JrpcMessageMethodName = JrpcMessageMethodName;
        }
    }
}
