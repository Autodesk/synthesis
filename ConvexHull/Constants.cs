
namespace MIConvexHull
{
    public static class Constants
    {
        /// <summary>
        /// A value used to determine if a vertex lies on a plane.
        /// </summary>
        public const double PlaneDistanceTolerance = 0.0000001;
        internal const double epsilon = 1e-8;
        internal const double epsilonSquared = 1e-16;

        public static double[] subtract(double[] pt1, double[] pt2, int dimension)
        {
            double[] res = new double[dimension];
            for (int i = 0; i < dimension; i++)
            {
                res[i] = pt1[i] - pt2[i];
            }
            return res;
        }

        public static double normSq(double[] pt, int dim)
        {
            double tot = 0;
            for (int i = 0; i < dim; i++)
            {
                tot += (pt[i] * pt[i]);
            }
            return tot;
        }

        /// <summary>
        /// Checks whether to points are essentially the same position.
        /// </summary>
        /// <param name="pt1">The PT1.</param>
        /// <param name="pt2">The PT2.</param>
        /// <param name="dimension">The dimension.</param>
        /// <returns></returns>
        public static bool spew = false;
        public static bool SamePosition(double[] pt1, double[] pt2, int dimension, double eps = epsilonSquared)
        {
            double dist = normSq(subtract(pt1, pt2, dimension), dimension);
            return dist < eps;
        }
    }
}
