using Core.ModuleLoader;
using SynthesisAPI.Modules;
using UnityEngine;

namespace Engine.ModuleLoader
{
	public class BehaviorAdapter : MonoBehaviour, IApiAdapter<Behavior>
	{
		public void Start() => behavior.Start();
		public void Update() => behavior.Update();
		public void FixedUpdate() => behavior.FixedUpdate();
		public void LateUpdate() => behavior.LateUpdate();
		public void OnGUI() => behavior.OnGUI();
		public void OnDisable() => behavior.OnDisable();
		public void OnEnable() => behavior.OnEnable();

		private Behavior behavior = new Behavior();
		public void SetInstance(Behavior instance)
		{
			behavior = instance;
		}
	}
}