using Core.ModuleLoader;
using SynthesisAPI.EnvironmentManager;
using UnityEngine;

namespace Engine.ModuleLoader
{
	public class SystemMonoBehavior : MonoBehaviour, IApiAdapter<SystemBase>
	{
		public string Name;

		public void Update() => _system.OnUpdate();

		public void FixedUpdate() => _system.OnPhysicsUpdate();

		public SystemBase _system;
		public void SetInstance(SystemBase instance)
		{
			_system = instance;
			Name = _system.GetType().FullName;
		}
	}
}