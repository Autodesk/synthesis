using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.InputManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;

namespace SynthesisCore.Systems
{
    /// <summary>
    /// System that controls arrows to move entities around
    /// </summary>
    public class MoveArrows : SystemBase
    {
        private static Entity arrowsEntity;
        private static Transform arrowsTransform;
        private static Entity? targetEntity = null;
        private static Transform targetTransform = null;

        private static readonly Arrow[] arrows = new Arrow[3];
        private static UnitVector3D? selectedArrowDirection = null;
        private static bool IsMovingEntity => targetEntity != null;

        /// <summary>
        /// Represents an arrow for movement along an axis
        /// </summary>
        public class Arrow
        {
            public Entity ArrowEntity { get; private set; }
            public Transform Transform;
            public UnitVector3D Direction { get; private set; }

            private Entity arrowSpriteEntity;
            private readonly MeshCollider2D collider;
            private bool hasSetSpritePivot = false;
            private readonly Sprite sprite;

            private bool hasUpdatedScalingOnce = false;

            public Arrow(UnitVector3D direction)
            {
                Direction = direction;

                ArrowEntity = EnvironmentManager.AddEntity();
                ArrowEntity.GetComponent<Parent>().Set(arrowsEntity);
                Transform = ArrowEntity.AddComponent<Transform>();

                arrowSpriteEntity = EnvironmentManager.AddEntity();
                arrowSpriteEntity.GetComponent<Parent>().Set(ArrowEntity);

                var arrowSpriteAsset = AssetManager.GetAsset<SpriteAsset>("/modules/synthesis_core/sprites/arrow.png");
                var selectedArrowSpriteAsset = AssetManager.GetAsset<SpriteAsset>("/modules/synthesis_core/sprites/arrow-selected.png");
                sprite = arrowSpriteEntity.AddComponent<Sprite>();
                sprite.Visible = false;
                sprite.SetSprite(arrowSpriteAsset);
                arrowSpriteEntity.AddComponent<AlwaysOnTop>();

                collider = arrowSpriteEntity.AddComponent<MeshCollider2D>();
                collider.OnMouseDown = () =>
                {
                    if (selectedArrowDirection == null)
                    {
                        CameraController.EnableCameraPan = false;
                        foreach (var i in arrows)
                        {
                            i.sprite.SetSprite(arrowSpriteAsset);
                            i.sprite.Color = System.Drawing.Color.FromArgb(175, 255, 255, 255);
                        }
                        sprite.SetSprite(selectedArrowSpriteAsset);
                        sprite.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
                        selectedArrowDirection = Direction;
                    }
                };
                collider.OnMouseUp = () =>
                {
                    if (selectedArrowDirection != null)
                    {
                        CameraController.EnableCameraPan = true;
                        foreach (var i in arrows)
                        {
                            i.sprite.SetSprite(arrowSpriteAsset);
                            i.sprite.Color = System.Drawing.Color.FromArgb(255, 255, 255, 255);
                        }
                        selectedArrowDirection = null;
                    }
                };
            }

            public void UpdateSpritePivot()
            {
                // Move sprite pivot point from center of image to base of arrow
                sprite.Visible = hasSetSpritePivot && hasUpdatedScalingOnce;
                if (!hasSetSpritePivot && collider.Bounds.Extents.Y != 0)
                {
                    var arrowSpriteTransform = arrowSpriteEntity.AddComponent<Transform>();
                    arrowSpriteTransform.Position = new Vector3D(0, collider.Bounds.Extents.Y * 2, 0);
                    hasSetSpritePivot = true;
                }
            }

            public void UpdateScaling()
            {
                // Update size of arrows so they always look the same size as they move
                var vectorToCamera = CameraController.Instance.cameraTransform.Position - targetTransform.Position;

                var size = vectorToCamera.Length * 0.01;
                arrowsTransform.Scale = new Vector3D(size, size, size);

                hasUpdatedScalingOnce = true;
            }
        }

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate()
        {
            if (IsMovingEntity)
            {
                foreach (var arrow in arrows)
                {
                    // Make arrow face camera
                    var forward = CameraController.Instance.cameraTransform.Position - targetTransform.Position;
                    forward -= forward.ProjectOn(arrow.Direction);
                    arrow.Transform.Rotation = MathUtil.LookAt(forward.Normalize(), arrow.Direction);
                    arrow.UpdateScaling();
                    arrow.UpdateSpritePivot();
                }

                MoveEntityTransform();
            }
        }

        public override void Setup() { }

        public override void Teardown() { }

        /// <summary>
        /// Add move arrows to an entity
        /// </summary>
        /// <param name="entity"></param>
        public static void MoveEntity(Entity entity)
        {
            if (entity.GetComponent<Transform>() == null)
            {
                Logger.Log("Move arrows cannot move an entity that has no transform", LogLevel.Error);
                return;
            }
            if (!IsMovingEntity)
            {
                // Create arrows
                arrowsEntity = EnvironmentManager.AddEntity();
                arrowsTransform = arrowsEntity.AddComponent<Transform>();
                arrows[0] = new Arrow(UnitVector3D.XAxis);
                arrows[1] = new Arrow(UnitVector3D.YAxis);
                arrows[2] = new Arrow(UnitVector3D.ZAxis);
            }
            
            targetEntity = entity;
            arrowsEntity.GetComponent<Parent>().Set(targetEntity.Value);
            targetTransform = targetEntity?.GetComponent<Transform>();
        }

        /// <summary>
        /// Remove move arrows from an entity
        /// </summary>
        public static void StopMovingEntity()
        {
            if (IsMovingEntity)
            {
                EnvironmentManager.RemoveEntity(arrowsEntity);
                targetEntity = null;
            }
        }

        /// <summary>
        /// Use mouse input to move the entity
        /// </summary>
        private void MoveEntityTransform()
        {
            if (selectedArrowDirection.HasValue)
            {
                var xMod = InputManager.GetAxisValue("Mouse X");
                var yMod = InputManager.GetAxisValue("Mouse Y");

                if (xMod != 0 || yMod != 0)
                {
                    var magnitude = (System.Math.Abs(xMod) + System.Math.Abs(yMod)) * 0.2;

                    var horizontalDir = UnitVector3D.YAxis.CrossProduct(CameraController.Instance.cameraTransform.Forward); // Side to side direction of mouse movement
                    var mouseDir = new Vector3D(0, yMod, 0) + horizontalDir.ScaleBy(xMod); // yMod is always up and down, and xMod is side to side
                    var deltaDir = mouseDir.ProjectOn(selectedArrowDirection.Value);
                    if (deltaDir.Length > float.Epsilon)
                    {
                        var unitDeltaDir = deltaDir.Normalize();
                        targetTransform.Position += unitDeltaDir.ScaleBy(magnitude);
                    }
                }
            }
        }
    }
}
