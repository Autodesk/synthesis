using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;

namespace SynthesisCore.Systems
{
    [ModuleExport]
    public class MoveArrows : SystemBase
    {
        private Entity arrows;
        private Transform transform;
        private UnitVector3D direction;
        public override void OnPhysicsUpdate() { }

        public override void OnUpdate()
        {
            var forward = CameraController.Instance.cameraTransform.Position - transform.Position;
            forward -= forward.ProjectOn(direction);
            transform.Rotation = MathUtil.LookAt(forward.Normalize(), direction);
        }

        public override void Setup()
        {
            direction = UnitVector3D.XAxis;
            arrows = EnvironmentManager.AddEntity();
            transform = arrows.AddComponent<Transform>();
            var arrowSpriteAsset = AssetManager.GetAsset<SpriteAsset>("/modules/synthesis_core/sprites/arrow.png");
            var selectedArrowSpriteAsset = AssetManager.GetAsset<SpriteAsset>("/modules/synthesis_core/sprites/arrow-selected.png");
            var sprite = arrows.AddComponent<Sprite>();
            sprite.SetSprite(arrowSpriteAsset);
            sprite.AlwaysOnTop = true;
            arrows.AddComponent<MeshCollider2D>().OnClick = () =>
            {
                Logger.Log("Here mod");
                sprite.SetSprite(selectedArrowSpriteAsset);
            };
        }

        public override void Teardown() { }
    }
}
