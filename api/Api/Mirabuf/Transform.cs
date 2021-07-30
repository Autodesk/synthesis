using UnityEngine;

namespace Mirabuf {
    public partial class Transform {
        private Matrix4x4 _unityMatrix;
        public Matrix4x4 UnityMatrix {
            get {
                if (_unityMatrix == default) {
                    _unityMatrix = new Matrix4x4(
                        new Vector4(SpatialMatrix[0], SpatialMatrix[4], SpatialMatrix[8], SpatialMatrix[12]),
                        new Vector4(SpatialMatrix[1], SpatialMatrix[5], SpatialMatrix[9], SpatialMatrix[13]),
                        new Vector4(SpatialMatrix[2], SpatialMatrix[6], SpatialMatrix[10], SpatialMatrix[14]),
                        new Vector4(SpatialMatrix[3], SpatialMatrix[7], SpatialMatrix[11], SpatialMatrix[15])
                        );
                    // _unityMatrix = _unityMatrix.transpose; // I'm tired
                }
                return _unityMatrix;
            }
        }
        
        public static implicit operator Matrix4x4(Transform t)
            => t.UnityMatrix;
        
        // I think?
        public static readonly Transform IDENTITY = new Transform { SpatialMatrix = {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        } }; // (row, column) => (0, 0), (0, 1), (0, 2), (0, 3), (1, 0) ...
    }
}
