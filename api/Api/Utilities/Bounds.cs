using MathNet.Spatial.Euclidean;

namespace SynthesisAPI.Utilities
{
    public class Bounds
    {
        internal UnityEngine.Bounds _bounds;

        public Bounds()
        {
            _bounds = new UnityEngine.Bounds();
        }

        public Vector3D Size => MathUtil.MapVector3(_bounds.size);
        public Vector3D Extents => MathUtil.MapVector3(_bounds.extents);
        public Vector3D Center => MathUtil.MapVector3(_bounds.center);
    }
}
