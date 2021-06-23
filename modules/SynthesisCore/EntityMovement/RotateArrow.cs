using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;
using SynthesisCore.Systems;

namespace SynthesisCore.EntityMovement
{
    public class RotateArrow : MarkerBase
    {
        private Entity ArrowEntity;
        private Transform Transform;
        public UnitVector3D RotationAxisDirection { get; private set; }
        public UnitVector3D SpriteUpDirection { get; private set; }

        private readonly MeshCollider2D collider;

        public RotateArrow(UnitVector3D rotationAxisDirection, UnitVector3D spriteUpDirection)
        {
            RotationAxisDirection = rotationAxisDirection;
            SpriteUpDirection = spriteUpDirection;

            ArrowEntity = EnvironmentManager.AddEntity();
            ArrowEntity.GetComponent<Parent>().ParentEntity = MoveArrows.arrowsEntity;
            Transform = ArrowEntity.AddComponent<Transform>();

            var arrowSpriteAsset = AssetManager.GetAsset<SpriteAsset>("/modules/synthesis_core/sprites/rotate-arrow.png");
            var selectedArrowSpriteAsset = AssetManager.GetAsset<SpriteAsset>("/modules/synthesis_core/sprites/rotate-arrow-selected.png");
            Sprite = ArrowEntity.AddComponent<Sprite>();
            Sprite.SetSprite(arrowSpriteAsset);
            ArrowEntity.AddComponent<AlwaysOnTop>();

            // Make arrow face axis
            Transform.GlobalRotation = MathUtil.LookAt(RotationAxisDirection, SpriteUpDirection);

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
                            i.Sprite.Visible = false;
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
                            i.Sprite.Visible = true;
                        }
                    }
                    Sprite.SetSprite(arrowSpriteAsset);
                    Transform.GlobalRotation = MathUtil.LookAt(RotationAxisDirection, SpriteUpDirection);
                    MoveArrows.selectedMarker = null;
                }
            };
        }

        public override void Update() { }

        public override void MoveEntityTransform(Transform targetTransform, float xMod, float yMod)
        {
            
            var horizontalDir = UnitVector3D.YAxis.CrossProduct(CameraController.Instance.cameraTransform.Forward); // Side to side direction of mouse movement
            var mouseDir = new Vector3D(0, yMod, 0) + horizontalDir.ScaleBy(xMod); // yMod is always up and down, and xMod is side to side

            var rotateDirVector = RotationAxisDirection.CrossProduct(SpriteUpDirection); // Get vector perpendicular to forward and sprite up

            var deltaDir = mouseDir.ProjectOn(rotateDirVector);
            if (deltaDir.Length > float.Epsilon)
            {
                var magnitude = (System.Math.Abs(xMod) + System.Math.Abs(yMod)) * 3;

                var sign = deltaDir.AngleTo(rotateDirVector).Degrees < 180 ? 1 : -1; // Get rotation direction

                targetTransform.Rotate(RotationAxisDirection, sign* magnitude, true);
                Transform.Rotate(RotationAxisDirection, sign * magnitude, true);
            }
        }
    }
}
