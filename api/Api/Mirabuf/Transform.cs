using UnityEngine;

namespace Mirabuf {
    public partial class Transform {
        // TODO: This doesn't adjust for the Fusion to Unity coordinate system. I have it fixed basically everytime I use it, just not here which
        //      of all places for me to fix it. Here would solve basically everything.
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

        /// <summary>
        /// Only reason this exists is because there are multiple points in the Unity project where it expects the data to be wrong.
        /// Dear future developer, I'm sorry. -Hunter
        /// </summary>
        private Matrix4x4? _correctUnityMatrix;
        public Matrix4x4 CorrectUnityMatrix {
            get {
                if (!_correctUnityMatrix.HasValue) {
                    _correctUnityMatrix = UnityMatrix;
                    _correctUnityMatrix = Matrix4x4.TRS(
                        new UnityEngine.Vector3(_correctUnityMatrix.Value.m03 * -0.01f, _correctUnityMatrix.Value.m13 * 0.01f, _correctUnityMatrix.Value.m23 * 0.01f),
                        new Quaternion(-_correctUnityMatrix.Value.rotation.x, _correctUnityMatrix.Value.rotation.y,
                            _correctUnityMatrix.Value.rotation.z, -_correctUnityMatrix.Value.rotation.w
                        ),
                        UnityEngine.Vector3.one
                    );
                }
                return _correctUnityMatrix.Value;
            }
        }

        public static implicit operator Matrix4x4(Transform t)
            => t.UnityMatrix;

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
