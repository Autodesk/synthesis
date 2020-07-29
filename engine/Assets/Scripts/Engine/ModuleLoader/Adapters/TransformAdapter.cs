using UnityEngine;
using Transform = SynthesisAPI.EnvironmentManager.Components.Transform;
using SynthesisAPI.Utilities;
using System;

using Quaternion = UnityEngine.Quaternion;
using SynQuaternion = MathNet.Spatial.Euclidean.Quaternion;
using MathNet.Spatial.Euclidean;

namespace Engine.ModuleLoader.Adapters
{
	public class TransformAdapter : MonoBehaviour, IApiAdapter<Transform>
	{
		public void OnEnable()
		{
			if (instance == null)
			{
				gameObject.SetActive(false);
				return;
			}

			unityTransform = gameObject.transform;

			instance.LinkedGetter = n => ParseFromUnity(typeof(UnityEngine.Transform).GetProperty(n).GetGetMethod().Invoke(unityTransform, null));
			instance.LinkedSetter = (n, o) => typeof(UnityEngine.Transform).GetProperty(n).GetSetMethod().Invoke(unityTransform, new object[] { ParseToUnity(o) });
		}

		private object ParseFromUnity(object obj)
		{
			Type type = obj.GetType();
			switch (type.Name)
			{
				case "Vector3":
					return ((Vector3)obj).Map();
				case "Quaternion":
					return ((Quaternion)obj).Map();
			}
			return obj;
		}

		private object ParseToUnity(object obj) // TODO: fix the lazy dynamic
		{
			Type type = obj.GetType();
			switch (type.Name)
			{
				case "Vector3D":
					return ((Vector3D)obj).Map();
				case "Quaternion":
					return ((SynQuaternion)obj).Map();
			}
			return obj; // If we don't need to parse (int, double, long, float, etc.)
		}

		public void Update()
		{
			if (instance.Changed)
			{
				// unityTransform.position = MathUtil.MapVector3D(instance.Position);
				// unityTransform.rotation = MathUtil.MapQuaternion(instance.Rotation);
				// unityTransform.localScale = MathUtil.MapVector3D(instance.Scale);
				instance.ProcessedChanges();
			}
		}

		public void SetInstance(Transform transform)
		{
			instance = transform;
			gameObject.SetActive(true);
		}

		public static Transform NewInstance()
		{
			return new Transform();
		}

		private Transform instance;
		private UnityEngine.Transform unityTransform;
	}
}