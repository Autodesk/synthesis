using MathNet.Spatial.Euclidean;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SynthesisAPI.EnvironmentManager;
using Transform = SynthesisAPI.EnvironmentManager.Components.Transform;
using Engine.Util;

using Entity = System.UInt32;

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

		private static Transform mapUnityParent(UnityEngine.Transform parent)
		{
			if (parent == null)
				return null;

			foreach (var transform in transforms)
			{
				if (transform.unityTransform == parent)
				{
					return transform.instance.Entity?.GetComponent<Transform>();
				}
			}
			return null; // TODO figure out what to return if unity parent is not in the transforms map
		}

		public void SetInstance(Transform transform)
		{
			instance = transform;
			instance.Parent = mapUnityParent(unityTransform);
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