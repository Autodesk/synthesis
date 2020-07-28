using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Controller.Rpc
{
    public static class RpcManager
    {
        public static string Version = "1.0.0";

        public delegate object HandlerFunc(params object[] args);

        private static Dictionary<string, MethodInfo> handlers = new Dictionary<string, MethodInfo>();

        static RpcManager()
        {
            Init();
        }

        public static void Init()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Controller");
            RegisterAll(assembly);
        }

        public static void RegisterAll(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods().Where(m => m.IsStatic && m.GetCustomAttribute<RpcMethodAttribute>() != null))
                {
                    Register(method.GetCustomAttribute<RpcMethodAttribute>()?.RpcMessageMethodName ?? method.Name, method);
                }
            }
        }

        public static void Register(HandlerFunc handler)
        {
            Register(handler.Method);
        }

        public static void Register(string methodName, HandlerFunc handler)
        {
            Register(methodName, handler.Method);
        }

        public static void Register(MethodInfo handler)
        {
            Register(handler.Name, handler);
        }

        public static void Register(string methodName, MethodInfo handler)
        {
            if (handlers.ContainsKey(methodName))
            {
                throw new Exception("Registering existing method name");
            }
            handlers[methodName] = handler;
        }

        public static Result<T, Exception> Invoke<T>(string methodName, params object[] args)
        {
            return Invoke(methodName, args).MapResult<T>((res) => (T)res);
        }

        public static Result<object, Exception> Invoke(string methodName, params object[] args)
        {
            if (!handlers.ContainsKey(methodName))
            {
                return new Result<object, Exception>(new Exception($"Missing function: {methodName}"));
            }
            if(handlers[methodName].ReturnType == typeof(void))
            {
                try
                {
                    handlers[methodName].Invoke(null, args);
                    return new Result<object, Exception>(new Void());
                }
                catch(Exception e)
                {
                    return new Result<object, Exception>(e);
                }
            }
            try
            {
                return new Result<object, Exception>(handlers[methodName].Invoke(null, args));
            }
            catch (Exception e)
            {
                return new Result<object, Exception>(e);
            }
        }

        public static bool IsRegistered(string methodName)
        {
            return handlers.ContainsKey(methodName);
        }
    }
}