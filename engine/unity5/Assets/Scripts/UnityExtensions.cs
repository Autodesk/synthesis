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
            vertex.x = vertex.x * xScale;//- copy.bounds.max.x * 0.5f * xScale;//(vertex.x /*+ copy.bounds.center.x * 0.25f*/) * xScale;
            vertex.y = vertex.y * yScale;//- copy.bounds.max.y * 0.5f * yScale;//(vertex.y /*+ copy.bounds.center.y * 0.25f*/) * yScale;
            vertex.z = vertex.z * zScale;//- copy.bounds.max.z * 0.5f * zScale;//(vertex.z /*+ copy.bounds.center.z * 0.25f*/) * zScale;
            vertices[i] = vertex;
        }

        copy.vertices = vertices;

        copy.RecalculateNormals();
        copy.RecalculateBounds();

        //mesh.bounds.center.Set(mesh.bounds.center.x * xScale, mesh.bounds.center.y * yScale, mesh.bounds.center.z * zScale);

        return copy;
    }
}
