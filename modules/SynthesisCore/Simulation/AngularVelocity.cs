using System;

namespace SynthesisCore.Simulation
{
    /// <summary>
    /// A class that pairs an angular velocity with units
    /// </summary>
    public class AngularVelocity
    {
        private double radiansPerSec;

        public double RadiansPerSec
        {
            get => radiansPerSec;
            set => radiansPerSec = value;
        }

        public double DegreesPerSec
        {
            get => radiansPerSec * (180d / Math.PI);
            set => radiansPerSec = value * (Math.PI / 180d);
        }

        public double RotationsPerMin
        {
            get => radiansPerSec * (30d / Math.PI);
            set => radiansPerSec = value * (Math.PI / 30d);
        }

        public static AngularVelocity FromRadiansPerSec(double radiansPerSec)
        {
            return new AngularVelocity
            {
                radiansPerSec = radiansPerSec
            };
        }

        public static AngularVelocity FromDegreesPerSec(double degreesPerSec)
        {
            return new AngularVelocity
            {
                DegreesPerSec = degreesPerSec
            };
        }

        public static AngularVelocity FromRotationsPerMin(double rotationsPerMin)
        {
            return new AngularVelocity
            {
                RotationsPerMin = rotationsPerMin
            };
        }

        public override string ToString()
        {
            return radiansPerSec.ToString() + " rad/sec";
        }

        private AngularVelocity()
        {
            radiansPerSec = 0;
        }
    }
}
