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
		public UnitVector3D Forward => MathUtil.QuaternionToForwardVector(Rotation);

		public delegate Vector3D PositionValidatorDelegate(Vector3D position);
		public delegate Quaternion RotationValidatorDelegate(Quaternion rotation);
		public delegate Vector3D ScaleValidatorDelegate(Vector3D scale);

		public PositionValidatorDelegate PositionValidator = (Vector3D position) => position;
		public RotationValidatorDelegate RotationValidator = (Quaternion rotation) => rotation;
		public ScaleValidatorDelegate ScaleValidator = (Vector3D scale) => scale;

		// These delegates will be setup by the Adapter
		internal Action<string, object> LinkedSetter = (n, o) => throw new Exception("Setter not assigned");
		internal Func<string, object> LinkedGetter = n => throw new Exception("Getter not assigned");

		public Vector3D Position
		{
			get => (Vector3D)LinkedGetter("position");
			set => LinkedSetter("position", PositionValidator(value));
		}

		public Quaternion Rotation
		{
			get => (Quaternion)LinkedGetter("rotation");
			set
			{
				if (!value.IsUnitQuaternion)
					ApiProvider.Log($"Warning: assigning rotation to non-unit quaternion {value}", LogLevel.Warning);
				LinkedSetter("rotation", RotationValidator(value));
			}
		}

		public Vector3D Scale
		{
			get => (Vector3D)LinkedGetter("localScale");
			set => LinkedSetter("localScale", ScaleValidator(value));
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

		public void LookAt(Vector3D target)
		{
			Rotation = MathUtil.LookAt((target - Position).Normalize());
		}

		internal bool Changed { get; private set; } = true;
		internal void ProcessedChanges() => Changed = false;
	}
}