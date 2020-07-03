using System;
using System.Collections.Generic;
using SynthesisAPI.Modules;
using UnityEngine;
using Component = SynthesisAPI.Modules.Component;

namespace SynthesisAPI.Runtime
{
	public interface IApiProvider
	{

		void Log(object o);

		uint AddEntity();

		Component AddComponent(Type t, uint entity);
		TComponent AddComponent<TComponent>(uint entity) where TComponent : Component;

		Component GetComponent(Type t, uint entity);
		TComponent GetComponent<TComponent>(uint entity) where TComponent : Component;
		List<Component> GetComponents(uint entity);
	}
}