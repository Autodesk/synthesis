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
				ToUnity();
				instance.ProcessedChanges();
			}
		}

		private void ToUnity()
        {
			unityTransform.position = MathUtil.MapVector3D(instance.Position);
			unityTransform.rotation = MathUtil.MapQuaternion(instance.Rotation);
			unityTransform.localScale = MathUtil.MapVector3D(instance.Scale);
		}

		public void SetInstance(Transform transform)
		{
			instance = transform;
			ToUnity();
		}

		public static Transform NewInstance()
		{
			return new Transform();
		}

		private Transform instance;
		private UnityEngine.Transform unityTransform;
	}
}