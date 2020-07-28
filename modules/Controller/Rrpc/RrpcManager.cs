using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Controller.Rpc
{
    public static class RpcManager
    {
        public static string JsonRpcVersion = "2.0";

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
                foreach (var method in type.GetMethods().Where(m => m.GetCustomAttribute<RpcMethodAttribute>() != null))
                {
                    if (method.IsStatic)
                    {
                        Register(method.GetCustomAttribute<RpcMethodAttribute>()?.RpcMessageMethodName ?? method.Name, method);
                    }
                    else
                    {
                        throw new Exception("Non-static RPC method");
                    }
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

        private static Type[] IntegerTypes = new[] { typeof(int), typeof(uint) };

        private static object[] FixArguments(MethodInfo method, params object[] args)
        {
            var args_i = 0;
            foreach (var param in method.GetParameters())
            {
                if (param.HasDefaultValue && args_i >= args.Length)
                {
                    args = args.Concat(new[] { Type.Missing }).ToArray();
                }
                if (param.ParameterType.IsEnum && args[args_i] is long)
                {
                    args[args_i] = Enum.ToObject(param.ParameterType, args[args_i]);
                }
                else if (args[args_i] is long && param.ParameterType.IsPrimitive && IntegerTypes.Contains(param.ParameterType))
                {
                    args[args_i] = Convert.ChangeType(args[args_i], param.ParameterType);
                }
                args_i++;
            }
            return args;
        }

        public static Result<object, Exception> Invoke(string methodName, params object[] args)
        {
            try
            {
                if (!handlers.ContainsKey(methodName))
                {
                    throw new Exception($"Missing function: {methodName}");
                }
                var handler = handlers[methodName];
                args = FixArguments(handler, args);

                if (handler.ReturnType == typeof(void))
                {
                    handler.Invoke(null, args);
                    return new Result<object, Exception>(new Void());
                }
                return new Result<object, Exception>(handler.Invoke(null, args));
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