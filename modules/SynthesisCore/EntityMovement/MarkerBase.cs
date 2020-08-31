using SynthesisAPI.EnvironmentManager.Components;

namespace SynthesisCore.EntityMovement
{
    public abstract class MarkerBase
    {
        public Sprite Sprite { get; protected set; }

        public abstract void Update();

        public abstract void MoveEntityTransform(Transform targetTransform, float xMod, float yMod);
    }
}
