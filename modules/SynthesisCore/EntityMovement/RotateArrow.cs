using MathNet.Spatial.Euclidean;

namespace SynthesisCore.EntityMovement
{
    public class RotateArrow : ArrowBase
    {
        public UnitVector3D RotationAxisDirection { get; private set; }

        public RotateArrow(UnitVector3D rotationAxisDirection)
        {
            RotationAxisDirection = rotationAxisDirection;
            // TODO
        }

        public override void Update()
        {
            //TODO
        }
    }
}
