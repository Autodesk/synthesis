using UnityEngine;
using Transform = SynthesisAPI.EnvironmentManager.Components.Transform;
using SynthesisAPI.Utilities;

namespace Engine.ModuleLoader.Adapters
{
	public class TransformAdapter : MonoBehaviour, IApiAdapter<Transform>
	{
		public void Awake()
		{
			unityTransform = gameObject.transform;
		}

		public void Update()
		{
			if (instance.Changed)
			{
				unityTransform.position = MathUtil.MapVector3D(instance.Position);
				unityTransform.rotation = MathUtil.MapQuaternion(instance.Rotation);
				unityTransform.localScale = MathUtil.MapVector3D(instance.Scale);
				instance.ProcessedChanges();
			}
		}

		public void SetInstance(Transform transform)
		{
			instance = transform;
		}

		public static Transform NewInstance()
		{
			return new Transform();
		}

		private Transform instance;
		private UnityEngine.Transform unityTransform;
	}
}