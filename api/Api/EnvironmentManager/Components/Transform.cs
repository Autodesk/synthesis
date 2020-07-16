using System;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;
using UnityEngine.Rendering;

namespace SynthesisAPI.EnvironmentManager.Components
{
	[BuiltinComponent]
	public class Transform : Component
	{
		private Vector3D _position;
		private Quaternion _rotation;
		private Vector3D _scale;

		public Vector3D Position
		{
			get => _position;
			set => _position = value;
		}

		public Quaternion Rotation
		{
			get => _rotation;
			set => _rotation = value;
		}

		public Vector3D Scale
		{
			get => _scale;
			set
			{
				_scale = value;
				Changed = true;
			}
		}

		public void Rotate(Vector3D angle)
		{
			angle.Normalize();
			(MathUtil.FromEuler(new EulerAngles(Angle.FromDegrees(angle.X), Angle.FromDegrees(angle.Y),
				Angle.FromDegrees(angle.Z))) - _rotation).RotateRotationQuaternion(_rotation);
		}

		public void Translate(Vector3D v)
		{
			_position += v;
		}

		public bool Changed { get; private set; }
		public void ProcessedChanges() => Changed = false;
	}
}