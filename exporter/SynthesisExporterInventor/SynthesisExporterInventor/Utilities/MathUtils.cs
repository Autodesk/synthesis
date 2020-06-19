using Inventor;

namespace SynthesisExporterInventor.Utilities
{
    static internal class MathUtils
    {
        public static double BoxVolume(Box b)
        {
            var dx = b.MaxPoint.X - b.MinPoint.X;
            var dy = b.MaxPoint.Y - b.MinPoint.Y;
            var dz = b.MaxPoint.Z - b.MinPoint.Z;
            return dx * dy * dz;
        }

        public static BXDVector3 ToBXDVector(dynamic p)
        {
            return new BXDVector3(p.X, p.Y, p.Z);
        }

        public static Vector ToInventorVector(BXDVector3 v)
        {
            return RobotExporterAddInServer.Instance.Application.TransientGeometry.CreateVector(v.x, v.y, v.z);
        }
    }
}