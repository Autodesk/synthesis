using MathNet.Spatial.Euclidean;
using UnityEditor;
using UnityEngine;
using Quaternion = MathNet.Spatial.Euclidean.Quaternion;
using Transform = SynthesisAPI.EnvironmentManager.Components.Transform;

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
			if (instance.Changed) // TODO compare against float.Epsilon?
			{
				unityTransform.position = mapVector3D(instance.Position);
				// unityTransform.rotation = mapQuaternion(instance.Rotation);
				unityTransform.localScale = mapVector3D(instance.Scale);
			}
			if (instance.lookAtTarget != null) 
			{
				var target = instance.lookAtTarget.Value;
				unityTransform.LookAt(new Vector3((float)target.X, (float)target.Y, (float)target.Z));
				instance.finishLookAt();
			}
		}

		private static Vector3 mapVector3D(Vector3D vec) => new Vector3((float) vec.X, (float) vec.Y, (float) vec.Z);
		private static Vector3D mapVector3(Vector3 vec) => new Vector3D(vec.x, vec.y, vec.z);
		private static Quaternion mapUnityQuaternion(UnityEngine.Quaternion q) => new Quaternion(q.w, q.x, q.y, q.z);
		private static UnityEngine.Quaternion mapQuaternion(Quaternion q) =>
			new UnityEngine.Quaternion((float) q.Real, (float) q.ImagX, (float) q.ImagY, (float) q.ImagZ);
		
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