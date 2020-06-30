using Core.ModuleLoader;
using SynthesisAPI.Modules;
using UnityEngine;

namespace Engine.ModuleLoader
{
	public class BehaviorAdapter : MonoBehaviour, IApiAdapter<SystemBase>
	{
		public void Start() => _system.Start();
		public void Update() => _system.Update();
		public void FixedUpdate() => _system.FixedUpdate();
		public void LateUpdate() => _system.LateUpdate();
		public void OnGUI() => _system.OnGUI();
		public void OnDisable() => _system.OnDisable();
		public void OnEnable() => _system.OnEnable();

		private SystemBase _system = new SystemBase();
		public void SetInstance(SystemBase instance)
		{
			_system = instance;
		}
	}
}