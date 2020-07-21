using UnityEngine;
using Transform = SynthesisAPI.EnvironmentManager.Components.Transform;
using Engine.Util;

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
				unityTransform.position = Utilities.MapVector3D(instance.Position);
				unityTransform.rotation = Utilities.MapQuaternion(instance.Rotation);
				unityTransform.localScale = Utilities.MapVector3D(instance.Scale);
				instance._forward = Utilities.MapVector3(unityTransform.forward).Normalize();
				instance.ProcessedChanges();
			}
			if (instance.lookAtTarget != null) 
			{
				unityTransform.LookAt(Utilities.MapVector3D(instance.lookAtTarget.Value));
				instance.Rotation = Utilities.MapUnityQuaternion(unityTransform.localRotation).Normalized;
				instance._forward = Utilities.MapVector3(unityTransform.forward).Normalize();
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