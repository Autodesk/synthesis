// TODO: This code still needed

#if false
using SynthesisAPI.EnvironmentManager.Components;
using UnityEngine;

namespace Engine.ModuleLoader.Adapters
{
    public sealed class LayerAdapter : MonoBehaviour, IApiAdapter<Layer>
	{
		private Layer instance = null;

		public void Awake()
		{
			if (instance == null)
			{
				gameObject.SetActive(false);
			}
		}

		public void Update()
		{
			if (instance.Changed)
			{
				gameObject.layer = instance.LayerMask;
				instance.ProcessedChanges();
			}
		}

		public void SetInstance(Layer layer)
		{
			instance = layer;
			gameObject.SetActive(true);
		}

		public static Layer NewInstance()
		{
			return new Layer();
		}
	}
}
#endif
