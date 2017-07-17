using System;
using System.Collections.Generic;
using System.Text;
using GopherAPI.Other;

namespace GopherAPI.STL
{
    /// <summary>
    /// Struct containing facet data. Is documentation really neccescary here? No. Why am I doing it? Because I feel like it.
    /// </summary>
    public struct Facet
    {
        /// <summary>
        /// Normal XYZ Coordinates
        /// </summary>
        public readonly Vec3 Normal;
        /// <summary>
        /// XYZ for Point 1
        /// </summary>
        public readonly Vec3 Point1;
        /// <summary>
        /// XYZ for Point 2
        /// </summary>
        public readonly Vec3 Point2;
        /// <summary>
        /// XYZ for Point 3
        /// </summary>
        public readonly Vec3 Point3;


        public Facet(Vec3 normal, Vec3 point1, Vec3 point2, Vec3 point3)
        {
            Normal = normal;
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }
    }
}
