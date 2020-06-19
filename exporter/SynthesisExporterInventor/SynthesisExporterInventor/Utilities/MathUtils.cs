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
    }
}