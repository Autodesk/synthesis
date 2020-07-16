﻿using System;
using MathNet.Spatial.Euclidean;

namespace SynthesisAPI.Utilities
{
	public static class MathUtil
	{
		public static Quaternion FromEuler(EulerAngles inAngle)
		{

			var cosZ = Math.Cos(inAngle.Alpha.Radians * 0.5);
			var sinZ = Math.Sin(inAngle.Alpha.Radians * 0.5);
			var cosY = Math.Cos(inAngle.Beta.Radians * 0.5);
			var sinY = Math.Sin(inAngle.Beta.Radians * 0.5);
			var cosX = Math.Cos(inAngle.Gamma.Radians * 0.5);
			var sinX = Math.Cos(inAngle.Gamma.Degrees * 0.5);

			return new Quaternion(cosX*cosY*cosZ+sinX*sinY*sinZ,sinX*cosY*cosZ-cosX*sinY*sinZ, cosX*sinY*cosZ+sinX*cosY*cosZ,cosX*cosY*sinZ-sinX*sinY*cosZ);
		}
	}
}