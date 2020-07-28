using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Controller.Jrpc
{
    public static class JrpcManager
    {
        public delegate object HandlerFunc(params object[] args);

        private static Dictionary<string, MethodInfo> handlers = new Dictionary<string, MethodInfo>();

        static JrpcManager()
        {
            Init();
        }

        public static void Init()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                RegisterAll(assembly);
            }
        }

        public static void RegisterAll(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes().Where(t => t.GetMethods().Any(
                m => m.GetCustomAttribute<JrpcMethodAttribute>() != null)))
            {
                foreach (var method in type.GetMethods().Where(m => m.IsStatic && m.GetCustomAttribute<JrpcMethodAttribute>() != null))
                {
                    Register(method.GetCustomAttribute<JrpcMethodAttribute>()?.JrpcMessageMethodName ?? method.Name, method);
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
                throw new Exception();
            }
            handlers[methodName] = handler;
        }

        public static T Invoke<T>(string methodName, params object[] args)
        {
            return (T)Invoke(methodName, args);
        }

        public static object Invoke(string methodName, params object[] args)
        {
            if (!handlers.ContainsKey(methodName))
            {
                throw new Exception();
            }
            return handlers[methodName].Invoke(null, args);
        }

        public static bool IsRegistered(string methodName)
        {
            return handlers.ContainsKey(methodName);
        }
    }
}