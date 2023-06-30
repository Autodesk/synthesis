using System;
using System.Linq;
using UnityEngine;
using UMesh = UnityEngine.Mesh;

namespace Mirabuf {
    public partial class TriangleMesh {
        private UMesh _unityMesh = null;
        private UMesh _colliderMesh = null;
        public UMesh UnityMesh {
            get {
                if (_unityMesh == null) {
                    _unityMesh = MakeMesh();
                }
                return _unityMesh;
            }
        }
        public UMesh ColliderMesh {
            get {
                if (_colliderMesh == null) {
                    try {
                        _colliderMesh = MakeMesh();
                        Physics.BakeMesh(_colliderMesh.GetInstanceID(), true);
                    } catch (Exception) {
                        // TODO: Maybe don't silently fail
                        _colliderMesh = new UMesh();
                        Debug.LogError("Failed to bake physics mesh!");
                    }
                }
                return _colliderMesh;
            }
        }

        private UMesh MakeMesh() {
            var m = new UMesh();
            switch (MeshTypeCase) {
                case MeshTypeOneofCase.Mesh:
                    m.vertices = this.Mesh.Verts.ToVector3Array();
                    m.triangles = this.Mesh.Indices.Reverse().ToArray();
                    if (this.Mesh.Normals.Count > 0)
                        m.normals = this.Mesh.Normals.ToVector3Array();
                    else
                        m.RecalculateNormals();
                    if (this.Mesh.Uv.Count > 0) // You can leave the uvs blank if you don't have any specifications
                        m.uv = this.Mesh.Uv.ToVector2Array();
                    break;
            }
            return m;
        }
    }
}
