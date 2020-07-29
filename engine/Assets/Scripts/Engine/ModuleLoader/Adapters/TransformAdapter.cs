using UnityEngine;
using Transform = SynthesisAPI.EnvironmentManager.Components.Transform;
using SynthesisAPI.Utilities;
using System;

using Quaternion = UnityEngine.Quaternion;
using SynQuaternion = MathNet.Spatial.Euclidean.Quaternion;
using MathNet.Spatial.Euclidean;
using Engine.Util;

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

			instance.LinkedGetter = Getter;
			instance.LinkedSetter = Setter;
		}

		private object Getter(string n)
		{
			switch (n.ToLower())
			{
				case "position":
					return unityTransform.position.Map();
				case "rotation":
					return unityTransform.rotation.Map().Normalized;
				case "localscale":
					return unityTransform.localScale.Map();
				default:
					throw new Exception($"Property {n} is not setup");
			}
		}

		private void Setter(string n, object o)
		{
			switch (n.ToLower())
			{
				case "position":
					unityTransform.position = ((Vector3D)o).Map();
					break;
				case "rotation":
					unityTransform.rotation = ((SynQuaternion)o).Map();
					break;
				case "localscale":
					unityTransform.localScale = ((Vector3D)o).Map();
					break;
				default:
					throw new Exception($"Property {n} is not setup");
			}
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