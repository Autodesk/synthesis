using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class UnityExtensions
{
    public static Mesh GetScaledCopy(this Mesh mesh, float xScale, float yScale, float zScale)
    {
        Mesh copy = UnityEngine.Object.Instantiate(mesh);

        Vector3[] vertices = new Vector3[copy.vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = copy.vertices[i];
            vertex.x = vertex.x * xScale;
            vertex.y = vertex.y * yScale;
            vertex.z = vertex.z * zScale;
            vertices[i] = vertex;
        }

        copy.vertices = vertices;

        copy.RecalculateNormals();
        copy.RecalculateBounds();

        return copy;
    }
}
