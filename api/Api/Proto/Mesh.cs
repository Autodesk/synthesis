using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

using UMesh = UnityEngine.Mesh;

namespace SynthesisAPI.Proto {
    
    /// <summary>
    /// Partial class to add utility functions and properties to Protobuf types
    /// </summary>
    public partial class Mesh {
        public static implicit operator UMesh(Mesh mesh) {
            UMesh m = new UMesh();
            m.indexFormat = IndexFormat.UInt32;
            List<Vector3> verts = new List<Vector3>();
            int[] tris = new int[mesh.Triangles.Count];
            SubMeshDescriptor[] descriptors = new SubMeshDescriptor[mesh.SubMeshes.Count];
            for (int j = 0; j < mesh.Vertices.Count; j++) {
                verts.Add(mesh.Vertices[j]);
            }
            for (int i = 0; i < mesh.Triangles.Count; i++) {
                tris[i] = mesh.Triangles[i];
            }
            for (int i = 0; i < mesh.SubMeshes.Count; i++) {
                descriptors[i] = new SubMeshDescriptor(mesh.SubMeshes[i].Start, mesh.SubMeshes[i].Count);
            }
            m.vertices = verts.ToArray();
            m.triangles = tris;
            m.SetSubMeshes(descriptors);
            m.RecalculateNormals();

            // Debug.Log($"{verts.Count} -> {m.vertices.Length}");
            // Debug.Log($"{tris.Length} -> {m.triangles.Length}");
            // Debug.Log($"Polygons: {m.triangles.Length / 3}");
            
            return m;
        }
    }
}
