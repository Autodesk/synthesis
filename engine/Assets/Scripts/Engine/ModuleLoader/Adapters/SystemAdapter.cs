using SynthesisAPI.EnvironmentManager;
using UnityEngine;

namespace Engine.ModuleLoader.Adapters
{
	public class SystemAdapter : MonoBehaviour, IApiAdapter<SystemBase>
	{
		public string Name;

		private SystemBase _system;

		public void Awake()
		{
			if (_system == null)
			{
				gameObject.SetActive(false);
			}
		}

		public void Start() => _system.Setup();

		public void Update() => _system.OnUpdate();

		public void FixedUpdate() => _system.OnPhysicsUpdate();

		public void OnDestroy() => _system.Teardown();

		public void SetInstance(SystemBase instance)
		{
			_system = instance;
			Name = _system.GetType().FullName;
			gameObject.SetActive(true);
		}
	}
}