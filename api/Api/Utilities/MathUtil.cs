using System;
using MathNet.Spatial.Euclidean;

namespace SynthesisAPI.Utilities
{
	public static class MathUtil
	{
		public static Quaternion FromEuler(EulerAngles inAngle)
		{
			// Math from https://math.stackexchange.com/questions/2975109/how-to-convert-euler-angles-to-quaternions-and-get-the-same-euler-angles-back-fr

			var yaw = inAngle.Alpha.Radians;
			var pitch = inAngle.Beta.Radians;
			var roll = inAngle.Gamma.Radians;

			var sinYaw = Math.Sin(yaw / 2);
			var cosYaw = Math.Cos(yaw / 2);
			var sinPitch = Math.Sin(pitch / 2);
			var cosPitch = Math.Cos(pitch / 2);
			var sinRoll = Math.Sin(roll / 2);
			var cosRoll = Math.Cos(roll / 2);

			var qx = sinRoll * cosPitch * cosYaw - cosRoll * sinPitch * sinYaw;

			var qy = cosRoll * sinPitch * cosYaw + sinRoll * cosPitch * sinYaw;

			var qz = cosRoll * cosPitch * sinYaw - sinRoll * sinPitch * cosYaw;

			var qw = cosRoll * cosPitch * cosYaw + sinRoll * sinPitch * sinYaw;

			// This z, y, x order works correctly with interchanging Quaternions and EulerAngles
			// using Quaternion.ToEulerAngles()
			return new Quaternion(qw, qz, qy, qx);
		}

		public static Vector3D ToVector(EulerAngles eulerAngles)
		{
			return new Vector3D(eulerAngles.Alpha.Degrees, eulerAngles.Beta.Degrees, eulerAngles.Gamma.Degrees);
		}

		public static UnitVector3D ToWorldVector(UnitVector3D vector, Quaternion rotation)
		{
			return ToWorldVector(vector.ToVector3D(), rotation).Normalize();
		}

		public static Vector3D ToWorldVector(Vector3D vector, Quaternion rotation)
		{
			var rot = rotation.Inversed.Normalized;

			// Math from https://gamedev.stackexchange.com/questions/28395/rotating-vector3-by-a-quaternion

			Vector3D u = new Vector3D(rot.ImagX, rot.ImagY, rot.ImagZ);
			double s = rot.Real;

			return 2d * u.DotProduct(vector) * u
				+ (s * s - u.DotProduct(u)) * vector
				+ 2d * s * u.CrossProduct(vector);
		}

		/// <summary>
		/// Linear interpolation
		/// </summary>
		public static double Lerp(double a, double b, double by)
		{
			return a * (1 - by) + b * by;
		}

		/// <summary>
		/// Linear interpolation
		/// </summary>
		public static Vector3D Lerp(Vector3D a, Vector3D b, double by)
		{
			return new Vector3D(Lerp(a.X, b.X, by), Lerp(a.Y, b.Y, by), Lerp(a.Z, b.Z, by));
		}

		public static Quaternion Lerp(Quaternion a, Quaternion b, double by)
		{
			return new Quaternion(Lerp(a.Real, b.Real, by), Lerp(a.ImagX, b.ImagX, by), Lerp(a.ImagY, b.ImagY, by), Lerp(a.ImagZ, b.ImagZ, by));
		}

		public static Quaternion LookAt(UnitVector3D forward)
		{
			return LookAt(forward, UnitVector3D.YAxis);
		}

		public static Quaternion LookAt(UnitVector3D forward, UnitVector3D upward)
		{
			return MapUnityQuaternion(UnityEngine.Quaternion.LookRotation(
				MapVector3D(forward.ToVector3D()),
				MapVector3D(upward.ToVector3D())));
		}

		public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta)
		{
			return MapUnityQuaternion(UnityEngine.Quaternion.RotateTowards(MapQuaternion(from), MapQuaternion(to), maxDegreesDelta));
		}

		internal static UnityEngine.Vector3 MapVector3D(Vector3D vec) =>
			new UnityEngine.Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
		internal static Vector3D MapVector3(UnityEngine.Vector3 vec) =>
			new Vector3D(vec.x, vec.y, vec.z);
		internal static Quaternion MapUnityQuaternion(UnityEngine.Quaternion q) =>
			new Quaternion(q.w, q.x, q.y, q.z);
		internal static UnityEngine.Quaternion MapQuaternion(Quaternion q) =>
			new UnityEngine.Quaternion((float)q.ImagX, (float)q.ImagY, (float)q.ImagZ, (float)q.Real);
	}
}