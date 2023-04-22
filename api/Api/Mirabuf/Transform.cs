using UnityEngine;

namespace Mirabuf {
    public partial class Transform {
        /// <summary>
        /// Transform data in the Fusion360 coordinate system.
        /// 
        /// It's used a lot because I forgot to translate it at first and its now translated
        /// everywhere else except for right here. Hence, why UnityMatrix is now a thing
        /// </summary>
        private Matrix4x4 _mirabufMatrix;
        public Matrix4x4 MirabufMatrix {
            get {
                if (_mirabufMatrix == default) {
                    _mirabufMatrix = new Matrix4x4(
                        new Vector4(SpatialMatrix[0], SpatialMatrix[4], SpatialMatrix[8], SpatialMatrix[12]),
                        new Vector4(SpatialMatrix[1], SpatialMatrix[5], SpatialMatrix[9], SpatialMatrix[13]),
                        new Vector4(SpatialMatrix[2], SpatialMatrix[6], SpatialMatrix[10], SpatialMatrix[14]),
                        new Vector4(SpatialMatrix[3], SpatialMatrix[7], SpatialMatrix[11], SpatialMatrix[15])
                        );
                }
                return _mirabufMatrix;
            }
        }

        /// <summary>
        /// Puts the transform data into the Unity coordinate system
        /// </summary>
        private Matrix4x4? _unityMatrix;
        public Matrix4x4 UnityMatrix {
            get {
                if (!_unityMatrix.HasValue) {
                    _unityMatrix = MirabufMatrix;
                    _unityMatrix = Matrix4x4.TRS(
                        new UnityEngine.Vector3(_unityMatrix.Value.m03 * -0.01f, _unityMatrix.Value.m13 * 0.01f, _unityMatrix.Value.m23 * 0.01f),
                        new Quaternion(-_unityMatrix.Value.rotation.x, _unityMatrix.Value.rotation.y,
                            _unityMatrix.Value.rotation.z, -_unityMatrix.Value.rotation.w
                        ),
                        UnityEngine.Vector3.one
                    );
                }
                return _unityMatrix.Value;
            }
        }

        public static implicit operator Matrix4x4(Transform t)
            => t.MirabufMatrix;

        public static implicit operator Transform(Matrix4x4 m)
            => new Transform {SpatialMatrix = {
                m[0, 0], m[0, 1], m[0, 2], m[0, 3],
                m[1, 0], m[1, 1], m[1, 2], m[1, 3],
                m[2, 0], m[2, 1], m[2, 2], m[2, 3],
                m[3, 0], m[3, 1], m[3, 2], m[3, 3]
            }};
        
        // I think?
        public static readonly Transform IDENTITY = new Transform { SpatialMatrix = {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        } }; // (row, column) => (0, 0), (0, 1), (0, 2), (0, 3), (1, 0) ...
    }
}
