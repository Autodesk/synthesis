using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

		public static void Log(object o, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
		{
			Instance?.Log(o, memberName, filePath, lineNumber);
		}

		public static void AddEntityToScene(uint entity) => Instance?.AddEntityToScene(entity);

		public static void RemoveEntityFromScene(uint entity) => Instance?.RemoveEntityFromScene(entity);

		public static Component? AddComponentToScene(uint entity, Type t) => Instance?.AddComponentToScene(entity,t);

		public static void RemoveComponentFromScene(uint entity, Type t) => Instance?.RemoveComponentFromScene(entity, t);
	}
}