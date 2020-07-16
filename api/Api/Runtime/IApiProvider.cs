using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SynthesisAPI.Modules;
using UnityEngine;
using Component = SynthesisAPI.EnvironmentManager.Component;

namespace SynthesisAPI.Runtime
{
	public interface IApiProvider
	{

		void Log(object o, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);

		void AddEntityToScene(uint entity);

		void RemoveEntityFromScene(uint entity);

		#nullable enable
		Component? AddComponentToScene(uint entity, Type t);

		void RemoveComponentFromScene(uint entity, Type t);
	}
}