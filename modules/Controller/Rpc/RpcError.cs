using System;

namespace Controller.Rpc
{
    public class RpcError : Exception
    {
        public RpcError() { }
        public RpcError(string message) : base(message) { }
        public RpcError(string message, Exception inner) : base(message, inner) { }
    }

    public class ParseError : RpcError
    {
        public ParseError() { }
        public ParseError(string message) : base(message) { }
        public ParseError(string message, Exception inner) : base(message, inner) { }
    }
    public class InvalidRequest : RpcError
    {
        public InvalidRequest() { }
        public InvalidRequest(string message) : base(message) { }
        public InvalidRequest(string message, Exception inner) : base(message, inner) { }
    }
    public class MethodNotFound : RpcError
    {
        public MethodNotFound() { }
        public MethodNotFound(string message) : base(message) { }
        public MethodNotFound(string message, Exception inner) : base(message, inner) { }
    }
    public class InvalidParams : RpcError
    {
        public InvalidParams() { }
        public InvalidParams(string message) : base(message) { }
        public InvalidParams(string message, Exception inner) : base(message, inner) { }
    }
    public class InternalError : RpcError
    {
        public InternalError() { }
        public InternalError(string message) : base(message) { }
        public InternalError(string message, Exception inner) : base(message, inner) { }
    }
    public class ServerError : RpcError
    {
        public ServerError() { }
        public ServerError(string message) : base(message) { }
        public ServerError(string message, Exception inner) : base(message, inner) { }
    }
}
