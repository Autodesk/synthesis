using Core.ModuleLoader;
using SynthesisAPI.EnvironmentManager;
using UnityEngine;

namespace Engine.ModuleLoader
{
	public class SystemMonoBehavior : MonoBehaviour, IApiAdapter<SystemBase>
	{
		public void Update() => _system.OnUpdate();

		private SystemBase _system;
		public void SetInstance(SystemBase instance)
		{
			_system = instance;
		}
	}
}