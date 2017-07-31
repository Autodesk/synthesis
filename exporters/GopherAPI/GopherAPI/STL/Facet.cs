using System;
using System.Collections.Generic;
using System.Text;
using GopherAPI.Other;
using System.IO;
using System.Drawing;

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

        /// <summary>
        /// Color of the facet
        /// </summary>
        public readonly Color FacetColor;

        /// <summary>
        /// if this is false, the color will revert to the default color of the bumper
        /// </summary>
        public readonly bool IsDefault;

        /// <summary>
        /// returns Point1, Point2, and Point3 in an array
        /// </summary>
        public Vec3[] Verteces
        { get { return new Vec3[] { Point1, Point2, Point3 }; } }

        internal byte[] Binary
        {
            get
            {
                var stream = new MemoryStream();
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Normal[0]);
                    writer.Write(Normal[1]);
                    writer.Write(Normal[2]);
                    writer.Write(Point1[0]);
                    writer.Write(Point1[1]);
                    writer.Write(Point1[2]);
                    writer.Write(Point2[0]);
                    writer.Write(Point2[1]);
                    writer.Write(Point2[2]);
                    writer.Write(Point3[0]);
                    writer.Write(Point3[1]);
                    writer.Write(Point3[2]);
                }
                var ret = stream.ToArray();
                stream.Dispose();
                return ret;
            }
        }

        public Facet(Vec3 normal, Vec3 point1, Vec3 point2, Vec3 point3, Color color, bool isDefault)
        {
            Normal = normal;
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
            FacetColor = color;
            IsDefault = isDefault;
        }
    }
}
