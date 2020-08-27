using System.Numerics;
using Inventor;
using SharpGLTF.Transforms;

namespace SynthesisInventorGltfExporter.Unused
{
    public class UnusedUtilities
    {
        // Someone might find these useful later...
        
        private Matrix4x4 InvToGltfMatrix4X4(Matrix invTransform)
        {
            var transform = new Matrix4x4(
                (float) invTransform.Cell[1, 1],
                (float) invTransform.Cell[2, 1],
                (float) invTransform.Cell[3, 1],
                (float) invTransform.Cell[4, 1],
                (float) invTransform.Cell[1, 2],
                (float) invTransform.Cell[2, 2],
                (float) invTransform.Cell[3, 2],
                (float) invTransform.Cell[4, 2],
                (float) invTransform.Cell[1, 3],
                (float) invTransform.Cell[2, 3],
                (float) invTransform.Cell[3, 3],
                (float) invTransform.Cell[4, 3],
                (float) invTransform.Cell[1, 4],
                (float) invTransform.Cell[2, 4],
                (float) invTransform.Cell[3, 4],
                (float) invTransform.Cell[4, 4]
            );
            return AffineTransform.Evaluate(transform, null, null, null);
        }
    }
}