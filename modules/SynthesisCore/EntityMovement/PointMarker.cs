using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;
using SynthesisCore.Systems;

namespace SynthesisCore.EntityMovement
{
    public class PointMarker : MarkerBase
    {
        private Entity MarkerEntity;
        private Transform Transform;

        public PointMarker()
        {
            MarkerEntity = EnvironmentManager.AddEntity();
            MarkerEntity.GetComponent<Parent>().ParentEntity = MoveArrows.arrowsEntity;
            Transform = MarkerEntity.AddComponent<Transform>();

            SpriteAsset markerSpriteAsset = AssetManager.GetAsset<SpriteAsset>("/modules/synthesis_core/sprites/point-marker.png");
            Sprite = MarkerEntity.AddComponent<Sprite>();
            Sprite.SetSprite(markerSpriteAsset);
            MarkerEntity.AddComponent<AlwaysOnTop>();
        }

        public override void Update()
        {
            // Make arrow face camera
            var forward = CameraController.Instance.cameraTransform.Position - MoveArrows.arrowsTransform.GlobalPosition;
            Transform.GlobalRotation = MathUtil.LookAt(forward.Normalize());
        }

        public override void MoveEntityTransform(Transform targetTransform, float xMod, float yMod) { }
    }
}
