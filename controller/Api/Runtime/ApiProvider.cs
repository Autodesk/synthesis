using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SynthesisAPI.Modules;
using SynthesisAPI.Modules.Attributes;
using UnityEngine;
using Component = SynthesisAPI.Modules.Component;
using Object = SynthesisAPI.Modules.Object;

namespace SynthesisAPI.Runtime
{
	public static class ApiProvider
	{
		private static IApiProvider? Instance => Inner.Instance;

		public static void RegisterApiProvider(IApiProvider provider)
		{
			if (Inner.Instance != null)
			{
				throw new Exception("Attempt to register multiple API instances");
			}

			Inner.Instance = provider;
		}

		public static void RegisterModule(IModule module)
		{
			Instance?.RegisterModule(module);
		}

		public static List<IModule>? GetModules()
		{
			return Instance?.GetModules();
		}

		private static class Inner
		{
			// ReSharper disable once EmptyConstructor
			static Inner() {}
			// ReSharper disable once MemberHidesStaticFromOuterClass
			internal static IApiProvider? Instance;
		}

		public static void Log(object o)
		{
			Instance?.Log(o);
		}

		public static (Guid, bool) Instantiate(Object o, ref Guid guid)
		{
			var type = o.GetType();
			if (Instance != null)
			{
				var (id, valid) = Instance.Instantiate(o);
				guid = id;
				foreach (var component in type.GetCustomAttributes(typeof(ComponentAttribute))
					.Select((attribute, i) => (ComponentAttribute) attribute))
				{
					Instance?.AddComponent(component.Type, o.GetId());
				}
				return (id, valid);
			}
			throw new Exception("No Api instance defined");
		}

		public static Transform? GetTransformById(Guid objectId)
		{
			return Instance?.GetTransformById(objectId);
		}

		public static Component? GetComponent(Type t, Guid id) => Instance?.GetComponent(t, id);
		public static TComponent GetComponent<TComponent>(Guid id) where TComponent : Component =>
			Instance?.GetComponent<TComponent>(id);

		public static List<Component> GetComponents(Guid objectId)
		{
			if (Instance != null)
				return Instance.GetComponents(objectId);

			throw new Exception("No Api instance defined");
		}

		public static List<TComponent> GetComponents<TComponent>(Guid objectId) where TComponent : Component
		{
			if (Instance != null)
				return Instance.GetComponents<TComponent>(objectId);
			throw new Exception("No Api instance defined");
		}

		public static Component? AddComponent(Type t, Guid objectId)
		{
			return Instance?.AddComponent(t, objectId);
		}

		public static TComponent AddComponent<TComponent>(Guid objectId) where TComponent : Component
		{
			return Instance?.AddComponent<TComponent>(objectId);
		}

		public static Object? GetObjectById(Guid objectId)
		{
			return Instance?.GetObject(objectId);
		}

	}
}