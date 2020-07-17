using MathNet.Spatial.Euclidean;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Quaternion = MathNet.Spatial.Euclidean.Quaternion;
using Transform = SynthesisAPI.EnvironmentManager.Components.Transform;

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
				unityTransform.localPosition = mapVector3D(instance.Position);
				unityTransform.rotation = mapQuaternion(instance.Rotation);
				unityTransform.localScale = mapVector3D(instance.Scale);
				instance.ProcessedChanges();
			}
			if (instance.lookAtTarget != null) 
			{
				var target = instance.lookAtTarget.Value;
				unityTransform.LookAt(new Vector3((float)target.X, (float)target.Y, (float)target.Z));
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

		private static Vector3 mapVector3D(Vector3D vec) => new Vector3((float) vec.X, (float) vec.Y, (float) vec.Z);
		private static Vector3D mapVector3(Vector3 vec) => new Vector3D(vec.x, vec.y, vec.z);
		private static Quaternion mapUnityQuaternion(UnityEngine.Quaternion q) => new Quaternion(q.w, q.x, q.y, q.z);
		private static UnityEngine.Quaternion mapQuaternion(Quaternion q) =>
			new UnityEngine.Quaternion((float) q.Real, (float) q.ImagX, (float) q.ImagY, (float) q.ImagZ);
		
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