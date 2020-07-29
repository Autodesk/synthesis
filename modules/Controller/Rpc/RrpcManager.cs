using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        internal static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

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
                        throw new Exception($"Non-static RPC method {method.Name}");
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

        private static readonly Type[] IntegerTypes = new[] {
            typeof(ulong),
            typeof(uint),
            typeof(ushort),
            typeof(sbyte),
            typeof(long),
            typeof(int),
            typeof(byte),
            typeof(short)
        };

        private static object[] FixArguments(MethodInfo method, params object[] args)
        {
            var args_i = 0;
            foreach (var param in method.GetParameters())
            {
                if (args_i >= args.Length)
                {
                    if (param.HasDefaultValue)
                    {
                        args = args.Concat(new[] { Type.Missing }).ToArray();
                    }
                    else
                    {
                        throw new Exception("RPC method call missing arguments");
                    }
                }
                else if (param.ParameterType != args[args_i].GetType())
                {
                    args[args_i] = FixType(param.ParameterType, args[args_i]);
                }
                args_i++;
            }
            return args;
        }

        internal static object FixType(Type type, object value)
        {
            if (value is JObject jObject)
            {
                value = type.IsPrimitive ? jObject.ToObject(type) : CompoundTypeConverter.FromJObject(jObject);
            }
            if (type.IsEnum)
            {
                if (value is long)
                {
                    value = Enum.ToObject(type, value);
                }
                else if (value is string)
                {
                    value = Enum.Parse(type, (string)value);
                }
            }
            else if (value is long && type.IsPrimitive && IntegerTypes.Contains(type))
            {
                value = Convert.ChangeType(value, type);
            }
            return value;
        }

        public static Result<object, Exception> Invoke(string methodName, params object[] args)
        {
            try
            {
                if (!handlers.ContainsKey(methodName))
                {
                    throw new Exception($"Missing RPC function: {methodName}");
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
                return new Result<object, Exception>(new Exception($"RPC server failed to invoke {methodName}", e));
            }
        }

        public static bool IsRegistered(string methodName)
        {
            return handlers.ContainsKey(methodName);
        }
    }
}