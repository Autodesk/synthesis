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
		private Vector3D _position = new Vector3D();
		private Quaternion _rotation = new Quaternion(0, 0, 0, 1);
		private Vector3D _scale = new Vector3D(1, 1, 1);
		private Transform _parent = null;

		public Transform Parent
        {
			get => _parent;
			set
			{
				_parent = value;
				Changed = true;
			}

		}

		public Vector3D Position
		{
			get => _position;
			set
			{
				_position = value;
				Changed = true;
			}
		}

		public Quaternion Rotation
		{
			get => _rotation;
			set
			{
				_rotation = value;
				Changed = true;
			}
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

		public void Rotate(Vector3D angles)
		{
			angles = angles.Normalize().ToVector3D();
			Rotate(MathUtil.FromEuler(new EulerAngles(Angle.FromDegrees(angles.X), Angle.FromDegrees(angles.Y),
				Angle.FromDegrees(angles.Z))) - Rotation);
		}

		public void Rotate(Quaternion rotation)
		{
			Rotation = rotation.Normalized.RotateRotationQuaternion(Rotation); // TODO these rotation functions may not work as intended
		}

		public void Translate(Vector3D v)
		{
			Position += v;
		}

		internal Vector3D? lookAtTarget { get; private set; }  = null;

		internal void finishLookAt() => lookAtTarget = null;

		public void LookAt(Vector3D target)
        {
			lookAtTarget = target;
		}

		public bool Changed { get; private set; }
		public void ProcessedChanges() => Changed = false;
	}
}