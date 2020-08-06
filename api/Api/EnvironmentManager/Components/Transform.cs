using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
		public RotationValidatorDelegate RotationValidator = (Quaternion rotation) => rotation.Normalized;
		public ScaleValidatorDelegate ScaleValidator = (Vector3D scale) => scale;

		public event PropertyChangedEventHandler PropertyChanged;

		internal string name = string.Empty;
		public string Name {
			get => name;
			set {
				name = value;
				OnPropertyChanged();
			}
		}

		internal Vector3D position = new Vector3D(0, 0, 0);
		public Vector3D Position
		{
			get => position;
			set {
				position = PositionValidator(value);
				OnPropertyChanged();
			}
		}

		internal Quaternion rotation = new Quaternion(1, 0, 0, 0);
		public Quaternion Rotation
		{
			get => rotation;
			set
			{
				if (!value.IsUnitQuaternion)
					Logger.Log($"Warning: assigning rotation to non-unit quaternion {value}", LogLevel.Warning);
				rotation = RotationValidator(value);
				OnPropertyChanged();
			}
		}

		internal Vector3D scale = new Vector3D(1, 1, 1);
		public Vector3D Scale
		{
			get => scale;
			set {
				scale = ScaleValidator(value);
				OnPropertyChanged();
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

		public void LookAt(Vector3D target)
		{
			Rotation = MathUtil.LookAt((target - Position).Normalize());
		}

		protected void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		internal bool Changed { get; private set; } = true;
		internal void ProcessedChanges() => Changed = false;
	}
}