using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Api.GUI;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Utilities;
using UnityEngine.UIElements;
using Component = SynthesisAPI.EnvironmentManager.Component;

#nullable enable

namespace SynthesisAPI.Runtime
{
	internal static class ApiProvider
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

		public static void AddEntityToScene(Entity entity) => Instance?.AddEntityToScene(entity);

		public static void RemoveEntityFromScene(Entity entity) => Instance?.RemoveEntityFromScene(entity);

		public static Component? AddComponentToScene(Entity entity, Type t) => Instance?.AddComponentToScene(entity,t);

		public static void AddComponentToScene(Entity entity, Component component) => Instance?.AddComponentToScene(entity, component);

		public static void RemoveComponentFromScene(Entity entity, Type t) => Instance?.RemoveComponentFromScene(entity, t);

		public static void EnqueueTaskForMainThread(Action task) => Instance?.EnqueueTaskForMainThread(task);

		public static UnityEngine.Coroutine? StartCoroutine(IEnumerator routine) => Instance?.StartCoroutine(routine);
		public static void StopCoroutine(IEnumerator routine) => Instance?.StopCoroutine(routine);

		public static T? CreateUnityType<T>(params object[] args) where T : class => Instance?.CreateUnityType<T>(args);

		// public static TUnityType? InstantiateFocusable<TUnityType>()
		// 	where TUnityType : UnityEngine.UIElements.Focusable =>
		// 	Instance?.InstantiateFocusable<TUnityType>();

		public static UnityEngine.UIElements.VisualElement? GetRootVisualElement() =>
			Instance?.GetRootVisualElement();

		internal static GUIManager GetGUIManager() => Instance?.GetGUIManager();
	}
}
