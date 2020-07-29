using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SynthesisAPI.Runtime;
using System;
using System.Collections.Generic;

namespace Controller.Rpc
{
    public class CompoundTypeConverter
    {
        public delegate object TypeConverter(params object[] args);

        public string TypeName;
        public object[] Args;
        private static Dictionary<string, TypeConverter> typeConverters = new Dictionary<string, TypeConverter>();

        public CompoundTypeConverter(Type type, params object[] args) : this(type.Name, args) { }

        [JsonConstructor]
        public CompoundTypeConverter(string typeName, params object[] args)
        {
            TypeName = typeName;
            Args = args;
        }

        public static CompoundTypeConverter Create<T>(params object[] args)
        {
            return new CompoundTypeConverter(typeof(T).Name, args);
        }

        public static void Register(string typeName, TypeConverter typeConverter)
        {
            typeConverters.Add(typeName, typeConverter);
        }

        public static void Register<T>(TypeConverter typeConverter)
        {
            typeConverters.Add(typeof(T).Name, typeConverter);
        }

        public static object FromJObject(JObject value)
        {
            return value.ToObject<CompoundTypeConverter>().Convert();
        }

        public static object Convert(string typeName, params object[] args)
        {
            if (typeName == null || !typeConverters.ContainsKey(typeName))
            {
                throw new Exception($"No registered converter for type {(typeName ?? "null")}");
            }
            for(var i = 0; i < args.Length; i++)
            {
                if(args[i] is JObject jObject)
                {
                    args[i] = FromJObject(jObject);
                }
            }
            return typeConverters[typeName](args);
        }

        public object Convert()
        {
            return Convert(TypeName, Args);
        }
    }
}
