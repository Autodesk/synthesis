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
    public class AxisArrow : ArrowBase
    {
        public Entity ArrowEntity { get; private set; }
        public Transform Transform;
        public UnitVector3D Direction { get; private set; }

        private Entity arrowSpriteEntity;
        private readonly MeshCollider2D collider;
        private bool hasSetSpritePivot = false;
        private readonly Sprite sprite;

        public AxisArrow(UnitVector3D direction)
        {
            Direction = direction;

            ArrowEntity = EnvironmentManager.AddEntity();
            ArrowEntity.GetComponent<Parent>().ParentEntity = MoveArrows.arrowsEntity;
            Transform = ArrowEntity.AddComponent<Transform>();

            arrowSpriteEntity = EnvironmentManager.AddEntity();
            arrowSpriteEntity.GetComponent<Parent>().ParentEntity = ArrowEntity;

            var arrowSpriteAsset = AssetManager.GetAsset<SpriteAsset>("/modules/synthesis_core/sprites/arrow.png");
            var selectedArrowSpriteAsset = AssetManager.GetAsset<SpriteAsset>("/modules/synthesis_core/sprites/arrow-selected.png");
            sprite = arrowSpriteEntity.AddComponent<Sprite>();
            sprite.Visible = false;
            sprite.SetSprite(arrowSpriteAsset);
            arrowSpriteEntity.AddComponent<AlwaysOnTop>();

            collider = arrowSpriteEntity.AddComponent<MeshCollider2D>();
            collider.OnMouseDown = () =>
            {
                if (MoveArrows.selectedArrowDirection == null)
                {
                    CameraController.EnableCameraPan = false;
                    foreach (var i in MoveArrows.arrows)
                    {
                        if (i is AxisArrow axisArrow)
                        {
                            axisArrow.sprite.SetSprite(arrowSpriteAsset);
                            axisArrow.sprite.Color = System.Drawing.Color.FromArgb(175, 255, 255, 255);
                        }
                    }
                    sprite.SetSprite(selectedArrowSpriteAsset);
                    sprite.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
                    MoveArrows.selectedArrowDirection = Direction;
                }
            };
            collider.OnMouseUp = () =>
            {
                if (MoveArrows.selectedArrowDirection != null)
                {
                    CameraController.EnableCameraPan = true;
                    foreach (var i in MoveArrows.arrows)
                    {
                        if (i is AxisArrow axisArrow)
                        {
                            axisArrow.sprite.SetSprite(arrowSpriteAsset);
                            axisArrow.sprite.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
                        }
                    }
                    MoveArrows.selectedArrowDirection = null;
                }
            };
        }

        public override void Update()
        {
            if (!hasSetSpritePivot)
            {
                // Move sprite pivot point from center of image to base of arrow
                var len = sprite.Bounds.Extents.ProjectOn(Direction).Length;
                if (len != 0)
                {
                    var arrowSpriteTransform = arrowSpriteEntity.AddComponent<Transform>();
                    arrowSpriteTransform.Position = new Vector3D(0, len * 2, 0);
                    hasSetSpritePivot = true;
                }
            }
            else
            {
                // Update size of arrows so they always look the same size as they move
                var vectorToCamera = CameraController.Instance.cameraTransform.Position - MoveArrows.arrowsTransform.GlobalPosition;

                var size = vectorToCamera.Length * 0.01;
                MoveArrows.arrowsTransform.Scale = new Vector3D(size, size, size);

                if (!sprite.Visible)
                    sprite.Visible = true;
            }

            // Make arrow face camera
            var forward = CameraController.Instance.cameraTransform.Position - MoveArrows.arrowsTransform.GlobalPosition;
            forward -= forward.ProjectOn(Direction);
            Transform.GlobalRotation = MathUtil.LookAt(forward.Normalize(), Direction);
        }
    }
}
