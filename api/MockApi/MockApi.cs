using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using SynthesisAPI.VirtualFileSystem;
using UnityEngine.UIElements;

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
                var entity = EnvironmentManager.AddEntity();
                entity.AddComponent(type);
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
            private bool debugLogsEnabled = false;

            private void LogAction(string function, string msg = "")
            {
                var m = $"MockApiProvider.{function}";
                if (msg != "")
                {
                    m += $": {msg}";
                }
                Log(m);
            }

            public void AddEntityToScene(Entity entity)
            {
                LogAction($"Add Entity {entity}");
            }

            public void RemoveEntityFromScene(Entity entity)
            {
                LogAction($"Remove Entity {entity}");
            }

#nullable enable
            public Component? AddComponentToScene(Entity entity, Type t)
            {
                LogAction("Add Component", $"Adding {t} to {entity}");
                return (Component?)Activator.CreateInstance(t);
            }

            public void RemoveComponentFromScene(Entity entity, Type t)
            {
                LogAction("Remove Component", $"Adding {t} to {entity}");
            }

            public void Log(object o, LogLevel logLevel = LogLevel.Info, string memberName = "", string filePath = "", int lineNumber = 0)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if (!debugLogsEnabled)
                        {
                            return;
                        }
                        {
                            Console.WriteLine(o);
                            break;
                        }
                    case LogLevel.Warning:
                    case LogLevel.Error:
                    case LogLevel.Info:
                        {
                            Console.WriteLine(o);
                            break;
                        }
                    default:
                        throw new SynthesisException("Unhandled log level");
                }
            }

            public void SetEnableDebugLogs(bool enable)
            {
                debugLogsEnabled = enable;
            }


            public T CreateUnityType<T>(params object[] args) where T : class
            {
                throw new NotImplementedException();
            }

            public VisualTreeAsset GetDefaultUIAsset(string assetName)
            {
                throw new NotImplementedException();
            }

            // public TUnityType InstantiateFocusable<TUnityType>() where TUnityType : Focusable
            // {
            //     throw new NotImplementedException();
            // }

            public VisualElement GetRootVisualElement()
            {
                throw new NotImplementedException();
            }
        }
    }
}