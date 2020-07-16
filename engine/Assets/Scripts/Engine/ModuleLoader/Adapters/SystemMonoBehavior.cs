using SynthesisAPI.EnvironmentManager;
using UnityEngine;

namespace Engine.ModuleLoader.Adapters
{
	public class SystemMonoBehavior : MonoBehaviour, IApiAdapter<SystemBase>
	{
		public string Name;

		public SystemBase _system;

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

		public void SetInstance(SystemBase instance)
		{
			_system = instance;
			Name = _system.GetType().FullName;
			gameObject.SetActive(true);
		}
	}
}