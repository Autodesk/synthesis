using System;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.EnvironmentManager.Components
{
	[BuiltinComponent]
	public class Transform : Component
	{
		private Vector3D _position = new Vector3D();
		private Quaternion _rotation = Quaternion.One;
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

		public void Rotate(Angle angle, Vector3D axis)
		{
			// Math from https://www.euclideanspace.com/maths/geometry/rotations/conversions/angleToQuaternion/index.htm
			var real = Math.Cos(angle.Radians / 2d);

			var factor = Math.Sin(angle.Radians / 2d);
			var imagX = axis.X * factor;
			var imagY = axis.Y * factor;
			var imagZ = axis.Z * factor;
			try
			{
				Rotation = new Quaternion(real, imagX, imagY, imagZ).Normalized.RotateRotationQuaternion(Rotation).Normalized;
			}
			catch (Exception e)
			{
				throw new Exception($"Transform: Rotation failed: angle: {angle} around axis {axis}", e);
			}
		}

		public void Rotate(Vector3D eulerAngles)
		{
			Rotate(new EulerAngles(Angle.FromDegrees(eulerAngles.X), Angle.FromDegrees(eulerAngles.Y),
				Angle.FromDegrees(eulerAngles.Z)));
		}

		public void Rotate(EulerAngles eulerAngles)
		{
			Rotate(MathUtil.FromEuler(eulerAngles));
		}

		public void Rotate(Quaternion rotation)
		{
			try
			{
				Rotation = rotation.Normalized.RotateRotationQuaternion(Rotation).Normalized;
			}
			catch (Exception e)
			{
				throw new Exception($"Transform: rotation failed with quaternion {rotation}", e);
			}
		}

		public void Translate(Vector3D v)
		{
			Position += v;
		}

		internal Vector3D? lookAtTarget { get; private set; } = null;

		internal void finishLookAt() => lookAtTarget = null;

		public void LookAt(Vector3D target)
		{
			lookAtTarget = target;
		}

		public bool Changed { get; private set; } = true;
		public void ProcessedChanges() => Changed = false;
	}
}