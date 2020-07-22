using UnityEngine;
using Transform = SynthesisAPI.EnvironmentManager.Components.Transform;
using Engine.Util;
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
				instance._forward = MathUtil.MapVector3(unityTransform.forward).Normalize();
				instance.ProcessedChanges();
			}
			if (instance.lookAtTarget != null) 
			{
				unityTransform.LookAt(MathUtil.MapVector3D(instance.lookAtTarget.Value));
				instance.Rotation = MathUtil.MapUnityQuaternion(unityTransform.localRotation).Normalized;
				instance._forward = MathUtil.MapVector3(unityTransform.forward).Normalize();
				instance.ProcessedChanges();
				instance.finishLookAt();
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