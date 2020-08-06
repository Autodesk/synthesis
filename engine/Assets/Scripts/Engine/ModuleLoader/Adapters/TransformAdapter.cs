using UnityEngine;
using Transform = SynthesisAPI.EnvironmentManager.Components.Transform;
using SynthesisAPI.Utilities;
using System;

using Quaternion = UnityEngine.Quaternion;
using SynQuaternion = MathNet.Spatial.Euclidean.Quaternion;
using MathNet.Spatial.Euclidean;
using Engine.Util;
using System.ComponentModel;

namespace Engine.ModuleLoader.Adapters
{
	public class TransformAdapter : MonoBehaviour, IApiAdapter<Transform>
	{
		public void SetInstance(Transform transform)
		{
			instance = transform;

			instance.PropertyChanged += UpdateProperty;

			unityTransform = gameObject.transform;

			// SynthesisAPI.Utilities.Logger.Log($"Parent: {gameObject.transform.parent.name}");

			unityTransform.localPosition = instance.Position.Map();
			unityTransform.localRotation = instance.Rotation.Map();
			unityTransform.localScale = instance.Scale.Map();
		}

		private void UpdateProperty(object sender, PropertyChangedEventArgs args)
		{
			// SynthesisAPI.Utilities.Logger.Log($"Updating Property: {instance.Entity.Value.Index}");

			switch (args.PropertyName)
			{
				case "Position":
					unityTransform.localPosition = instance.Position.Map();
					break;
				case "Rotation":
					unityTransform.localRotation = instance.Rotation.Map();
					break;
				case "Scale":
					unityTransform.localScale = instance.Scale.Map();
					break;
				default:
					throw new Exception($"Property {args.PropertyName} is not setup");
			}
		}

		public void Update()
		{
			instance.position = instance.PositionValidator(unityTransform.localPosition.Map());
			instance.rotation = instance.RotationValidator(unityTransform.localRotation.Map());
			instance.scale = instance.ScaleValidator(unityTransform.localScale.Map());
		}

		public static Transform NewInstance()
		{
			return new Transform();
		}

		private Transform instance;
		private UnityEngine.Transform unityTransform;
	}
}