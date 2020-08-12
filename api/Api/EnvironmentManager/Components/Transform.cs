using System;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.EnvironmentManager.Components
{
	[BuiltinComponent]
	public class Transform : Component
	{
		private Vector3D _position = new Vector3D();
		private Quaternion _rotation = Quaternion.One;
		private Vector3D _scale = new Vector3D(1, 1, 1);

		public UnitVector3D Forward => MathUtil.QuaternionToForwardVector(Rotation);

		public delegate Vector3D PositionValidatorDelegate(Vector3D position);
		public delegate Quaternion RotationValidatorDelegate(Quaternion rotation);
		public delegate Vector3D ScaleValidatorDelegate(Vector3D scale);

		public PositionValidatorDelegate PositionValidator = (Vector3D position) => position;
		public RotationValidatorDelegate RotationValidator = (Quaternion rotation) => rotation;
		public ScaleValidatorDelegate ScaleValidator = (Vector3D scale) => scale;

		public Vector3D Position
		{
			get => _position;
			set
			{
				_position = PositionValidator(value);
				Changed = true;
			}
		}

		public Quaternion Rotation
		{
			get => _rotation;
			set
			{
				if (!value.IsUnitQuaternion)
					Logger.Log($"Warning: assigning rotation to non-unit quaternion {value}", LogLevel.Warning);
				_rotation = RotationValidator(value);
				Changed = true;
			}
		}

		public Vector3D Scale
		{
			get => _scale;
			set
			{
				_scale = ScaleValidator(value);
				Changed = true;
			}
		}
		public void Rotate(UnitVector3D axis, double angle, bool useWorldAxis = false)
		{
			Rotation = MathUtil.Rotate(Rotation, axis, angle, useWorldAxis);
		}

		public void Rotate(UnitVector3D axis, Angle angle, bool useWorldAxis = false)
		{
			Rotation = MathUtil.Rotate(Rotation, axis, angle, useWorldAxis);
		}

		public void Rotate(Vector3D eulerAngles)
		{
			Rotation = MathUtil.Rotate(Rotation, eulerAngles);
		}

		public void Rotate(EulerAngles eulerAngles)
		{
			Rotation = MathUtil.Rotate(Rotation, eulerAngles);
		}

		public void Rotate(Quaternion rotation)
		{
			Rotation = MathUtil.Rotate(Rotation, rotation);
		}

		public void Translate(Vector3D v)
		{
			Position += v;
		}

		public void LookAt(Vector3D targetPosition)
		{
			Rotation = MathUtil.LookAt((targetPosition - Position).Normalize());
		}
		public void LookAt(Vector3D targetPosition, UnitVector3D upward)
		{
			Rotation = MathUtil.LookAt((targetPosition - Position).Normalize(), upward);
		}

		internal bool Changed { get; private set; } = true;
		internal void ProcessedChanges() => Changed = false;
	}
}