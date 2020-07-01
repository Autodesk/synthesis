using System;
using System.Collections.Generic;
using SynthesisAPI.Modules.Attributes;
using UnityEngine;
using Component = SynthesisAPI.EnvironmentManager.Component;

#nullable enable

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

		public static uint? AddEntity() => Instance?.AddEntity();

		public static Component? GetComponent(Type t, uint id) => Instance?.GetComponent(t, id);
		public static TComponent? GetComponent<TComponent>(uint id) where TComponent : Component =>
			Instance?.GetComponent<TComponent>(id);

		public static List<Component> GetComponents(uint entity)
		{
			if (Instance != null)
				return Instance.GetComponents(entity);

			throw new Exception("No Api instance defined");
		}

		}
			return Instance?.InstantiateFocusable<TUnityType>();
		public static TUnityType InstantiateFocusable<TUnityType>() where TUnityType : UnityEngine.UIElements.Focusable
		{

		public static UnityEngine.UIElements.VisualElement GetRootVisualElement() => Instance?.GetRootVisualElement();

		public static Component? AddComponent(Type t, uint entity) => Instance?.AddComponent(t, entity);
	}
}