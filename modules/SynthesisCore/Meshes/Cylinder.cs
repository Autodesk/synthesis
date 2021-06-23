using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager.Components;
using System.Collections.Generic;

namespace SynthesisCore.Meshes
{
    public static class Cylinder
    {
        /// <summary>
        /// Create or bind a cylinder mesh
        /// </summary>
        /// <param name="m"></param>
        /// <param name="granularity">The number of steps around the circle</param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Mesh Make(Mesh m, uint granularity, double radius, double height)
        {
            if (m == null)
                m = new Mesh();
            
            double halfHeight = height / 2;
            var vertices = new List<Vector3D>();
            var triangles = new List<int>();

            double dRad = 2 * System.Math.PI / (granularity - 2);
            double rad = dRad;

            var top_circle_vertices = new List<Vector3D> { new Vector3D(0, halfHeight, 0) };
            var bottom_circle_vertices = new List<Vector3D> { new Vector3D(0, -halfHeight, 0) };

            // Create vertices around circumference
            for (var i = 0; i < granularity; i++)
            {
                var x = radius * System.Math.Cos(rad);
                var z = radius * System.Math.Sin(rad);
                top_circle_vertices.Add(new Vector3D(x, halfHeight, z));
                bottom_circle_vertices.Add(new Vector3D(x, -halfHeight, z));

                rad += dRad;
            }

            top_circle_vertices.Reverse();
            vertices.AddRange(top_circle_vertices); // Note the order here matters
            vertices.AddRange(bottom_circle_vertices);

            for (var i = 0; i < granularity; i++)
            {
                // Top triangle
                var top_center = top_circle_vertices.Count - 1;
                var top_edge_a = top_center - (i + 1) % (int)granularity;
                var top_edge_b = top_center - (i + 2) % (int)granularity;
                triangles.Add(top_center + 0);
                triangles.Add(top_edge_b);
                triangles.Add(top_edge_a);

                // Bottom triangle
                var bottom_center = top_circle_vertices.Count;
                var bottom_edge_a = bottom_center + (i + 1) % (int)granularity;
                var bottom_edge_b = bottom_center + (i + 2) % (int)granularity;
                triangles.Add(bottom_center + 0);
                triangles.Add(bottom_edge_a);
                triangles.Add(bottom_edge_b);

                // Top to bottom triangle
                triangles.Add(top_edge_a);
                triangles.Add(top_edge_b);
                triangles.Add(bottom_edge_a);

                // Bottom to top triangle
                triangles.Add(bottom_edge_a);
                triangles.Add(top_edge_b);
                triangles.Add(bottom_edge_b);

            }

            m.Vertices = vertices;
            m.Triangles = triangles;

            return m;
        }
    }
}
