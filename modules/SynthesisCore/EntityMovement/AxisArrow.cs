using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;
using SynthesisCore.Systems;

namespace SynthesisCore.EntityMovement
{
    /// <summary>
    /// Represents an arrow for movement along an axis
    /// </summary>
    public class AxisArrow : MarkerBase
    {
        private Entity ArrowEntity;
        private Transform Transform;
        public UnitVector3D AxisDirection { get; private set; }

        private readonly MeshCollider2D collider;

        public AxisArrow(UnitVector3D axisDirection)
        {
            AxisDirection = axisDirection;

            ArrowEntity = EnvironmentManager.AddEntity();
            ArrowEntity.GetComponent<Parent>().ParentEntity = MoveArrows.arrowsEntity;
            Transform = ArrowEntity.AddComponent<Transform>();

            var arrowSpriteAsset = AssetManager.GetAsset<SpriteAsset>("/modules/synthesis_core/sprites/arrow.png");
            var selectedArrowSpriteAsset = AssetManager.GetAsset<SpriteAsset>("/modules/synthesis_core/sprites/arrow-selected.png");
            Sprite = ArrowEntity.AddComponent<Sprite>();
            Sprite.SetSprite(arrowSpriteAsset);
            ArrowEntity.AddComponent<AlwaysOnTop>();

            collider = ArrowEntity.AddComponent<MeshCollider2D>();
            collider.OnMouseDown = () =>
            {
                if (MoveArrows.selectedMarker == null)
                {
                    CameraController.EnableCameraPan = false;
                    foreach (var i in MoveArrows.arrows)
                    {
                        if (i != this)
                        {
                            i.Sprite.Color = System.Drawing.Color.FromArgb(175, 255, 255, 255);
                        }
                    }
                    Sprite.SetSprite(selectedArrowSpriteAsset);
                    Sprite.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
                    MoveArrows.selectedMarker = this;
                }
            };
            collider.OnMouseUp = () =>
            {
                if (MoveArrows.selectedMarker == this)
                {
                    CameraController.EnableCameraPan = true;
                    foreach (var i in MoveArrows.arrows)
                    {
                        if (i != this)
                        {
                            i.Sprite.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
                        }
                    }
                    Sprite.SetSprite(arrowSpriteAsset);
                    MoveArrows.selectedMarker = null;
                }
            };
        }

        public override void Update()
        {
            // Make arrow face camera
            var forward = CameraController.Instance.cameraTransform.Position - MoveArrows.arrowsTransform.GlobalPosition;
            forward -= forward.ProjectOn(AxisDirection);
            Transform.GlobalRotation = MathUtil.LookAt(forward.Normalize(), AxisDirection);
        }

        public override void MoveEntityTransform(Transform targetTransform, float xMod, float yMod)
        {
            var horizontalDir = UnitVector3D.YAxis.CrossProduct(CameraController.Instance.cameraTransform.Forward); // Side to side direction of mouse movement
            var mouseDir = new Vector3D(0, yMod, 0) + horizontalDir.ScaleBy(xMod); // yMod is always up and down, and xMod is side to side
            var deltaDir = mouseDir.ProjectOn(AxisDirection);
            if (deltaDir.Length > float.Epsilon)
            {
                var magnitude = (System.Math.Abs(xMod) + System.Math.Abs(yMod)) * 0.2;

                var unitDeltaDir = deltaDir.Normalize();
                targetTransform.GlobalPosition += unitDeltaDir.ScaleBy(magnitude);
            }
        }
    }
}
