using MathNet.Spatial.Euclidean;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Quaternion = MathNet.Spatial.Euclidean.Quaternion;
using Transform = SynthesisAPI.EnvironmentManager.Components.Transform;
using Engine.Util;

namespace Engine.ModuleLoader.Adapters
{
	public class TransformAdapter : MonoBehaviour, IApiAdapter<Transform>
	{
		private static List<TransformAdapter> transforms = new List<TransformAdapter>(); // TODO manage lifetimes

		public void Awake()
		{
			unityTransform = gameObject.transform;
		}

		public void Update()
		{
			if (instance.Changed)
			{
				unityTransform.parent = mapParent(instance.Parent);
				unityTransform.localPosition = Utilities.MapVector3D(instance.Position);
				unityTransform.localRotation = Utilities.MapQuaternion(instance.Rotation);
				unityTransform.localScale = Utilities.MapVector3D(instance.Scale);
				instance.ProcessedChanges();
			}
			if (instance.lookAtTarget != null) 
			{
				unityTransform.LookAt(Utilities.MapVector3D(instance.lookAtTarget.Value));
				instance.Rotation = Utilities.MapUnityQuaternion(unityTransform.localRotation);
				instance.ProcessedChanges();
				instance.finishLookAt();
			}
		}

		private static UnityEngine.Transform mapParent(Transform parent)
		{
			if (parent == null)
				return null;

			foreach (var transform in transforms)
			{
				if (transform.instance != null && transform.instance.Entity == parent.Entity)
				{
					return transform.unityTransform;
				}
			}
			return null;
		}
		
		public void SetInstance(Transform transform)
		{
			instance = transform;
			transforms.Add(this);
		}

		public static Transform NewInstance()
        {
			return new Transform();
        }

		private Transform instance;
		private UnityEngine.Transform unityTransform;
	}
}