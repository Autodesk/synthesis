using System;
using System.Collections.Generic;
using SynthesisAPI.Modules;
using UnityEngine;
using Component = SynthesisAPI.Modules.Component;
using Object = SynthesisAPI.Modules.Object;

namespace SynthesisAPI.Runtime
{
	public interface IApiProvider
	{
		List<IModule> GetModules();
		void RegisterModule(IModule module);

		Transform GetTransformById(Guid id);

		(Guid Id, bool valid) Instantiate(Object o);

		void Log(object o);

		Component AddComponent(Type t, Guid objectId);
		TComponent AddComponent<TComponent>(Guid objectId) where TComponent : Component;

		Component GetComponent(Type t, Guid id);
		TComponent GetComponent<TComponent>(Guid id) where TComponent : Component;
		List<Component> GetComponents(Guid objectId);
		List<TComponent> GetComponents<TComponent>(Guid id) where TComponent : Component;
		Object? GetObject(Guid objectId);
	}
}