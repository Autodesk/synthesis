using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;

namespace MockApi
{
    public static class MockApi
    {
        public static void Init()
        {
            SynthesisAPI.Runtime.ApiProvider.RegisterApiProvider(new MockApiProvider());
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                 a.GetTypes()).Where(e => e.IsSubclassOf(typeof(SystemBase))))
            {
                var entity = SynthesisAPI.Runtime.ApiProvider.AddEntity();
                if (entity != null)
                    SynthesisAPI.Runtime.ApiProvider.AddComponent(type, entity.Value);
                else
                    throw new Exception("Entity is null");
            }
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
                a.GetTypes()).Where(e => e.GetMethods().Any(
                    m => m.GetCustomAttribute<CallbackAttribute>() != null || m.GetCustomAttribute<TaggedCallbackAttribute>() != null)))
            {
                var instance = Activator.CreateInstance(type);
                foreach (var callback in type.GetMethods()
                    .Where(m => m.GetCustomAttribute<CallbackAttribute>() != null))
                {
                    RegisterTypeCallbackByMethodInfo(callback, instance);
                }
                foreach (var callback in type.GetMethods()
                    .Where(m => m.GetCustomAttribute<TaggedCallbackAttribute>() != null))
                {
                    RegisterTagCallbackByMethodInfo(callback, instance);
                }
            }
        }

        private static void RegisterTagCallbackByMethodInfo(MethodInfo callback, object instance)
        {
            if (instance.GetType() != callback.DeclaringType)
            {
                throw new Exception(
                    $"Type of instance variable \"{instance.GetType()}\" does not match declaring type of callback \"{callback.Name}\" (expected \"{callback.DeclaringType}\"");
            }
            var eventType = callback.GetParameters().First().ParameterType;
            var tag = callback.GetCustomAttribute<TaggedCallbackAttribute>().Tag;
            typeof(EventBus).GetMethod("NewTagListener").Invoke(null, new object[]
                {
                    tag,
                    CreateEventCallback(callback, instance, eventType)
                });
        }

        private static void RegisterTypeCallbackByMethodInfo(MethodInfo callback, object instance)
        {
            if (instance.GetType() != callback.DeclaringType)
            {
                throw new Exception(
                    $"Type of instance variable \"{instance.GetType()}\" does not match declaring type of callback \"{callback.Name}\" (expected \"{callback.DeclaringType}\"");
            }
            var eventType = callback.GetParameters().First().ParameterType;
            typeof(EventBus).GetMethod("NewTypeListener")
                ?.MakeGenericMethod(eventType).Invoke(null, new object[]
                {
                    CreateEventCallback(callback, instance, eventType)
                });
        }

        static EventBus.EventCallback CreateEventCallback(MethodInfo m, object instance, Type eventType)
        {
            return (e) => m.Invoke(instance,
                new[]
                {
                    typeof(ReflectHelper).GetMethod("CastObject")
                        ?.MakeGenericMethod(eventType)
                        .Invoke(null, new object[] {e})
                });
        }


        private class MockApiProvider : IApiProvider
        {
            private void LogAction(string function, string msg = "")
            {
                var m = $"MockApiProvider.{function}";
                if (msg != "")
                {
                    m += ": {msg}";
                }
                Log(m);
            }

            public Component AddComponent(Type t, uint entity)
            {
                LogAction("AddComponent", $"Adding {t} to {entity}");
                var component = (Component)Activator.CreateInstance(t);
                EnvironmentManager.AddComponent(entity, component);
                return component;
            }

            public TComponent AddComponent<TComponent>(uint entity) where TComponent : Component
            {
                LogAction("AddComponent", $"Adding <{typeof(TComponent)}> to {entity}");
                return (TComponent)AddComponent(typeof(TComponent), entity) ;
            }

            public uint AddEntity()
            {
                LogAction("AddEntity");
                return EnvironmentManager.AddEntity();
            }

            public Component GetComponent(Type t, uint entity)
            {
                LogAction("GetComponent", $"Get {t} from {entity}");
                return entity.GetComponent(t);
            }

            public TComponent GetComponent<TComponent>(uint entity) where TComponent : Component
            {
                LogAction("GetComponent", $"Get <{typeof(TComponent)}> from {entity}");
                return entity.GetComponent<TComponent>();
            }

            public List<Component> GetComponents(uint entity)
            {
                LogAction("GetComponents", $"Get from {entity}");
                return entity.GetComponents();
            }

            public void Log(object o)
            {
                Console.WriteLine(o);
            }
        }
    }
}
