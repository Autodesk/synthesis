using System;
using System.Linq;
using UnityEngine;
using UMesh = UnityEngine.Mesh;
using UVector3 = UnityEngine.Vector3;

namespace Mirabuf {
    public partial class TriangleMesh {
        private UMesh _unityMesh = null;
        public UMesh UnityMesh {
            get {
                if (_unityMesh == null) {
                    _unityMesh = new UMesh();
                    switch (MeshTypeCase) {
                        case MeshTypeOneofCase.Mesh:
                            _unityMesh.vertices = this.Mesh.Verts.ToVector3Array();
                            _unityMesh.triangles = this.Mesh.Indices.Reverse().ToArray();
                            if (this.Mesh.Normals.Count > 0)
                                _unityMesh.normals = this.Mesh.Normals.ToVector3Array();
                            else
                                _unityMesh.RecalculateNormals();
                            if (this.Mesh.Uv.Count > 0) // You can leave the uvs blank if you don't have any specifications
                                _unityMesh.uv = this.Mesh.Uv.ToVector2Array();
                            break;
                    }
                }
                return _unityMesh;
            }
        }
    }
}
