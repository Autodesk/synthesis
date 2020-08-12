using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.InputManager;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Utilities;

namespace SynthesisCore.Systems
{
    [ModuleExport]
    public class MoveArrows : SystemBase
    {
        private static Entity arrowsEntity;
        private static Transform arrowsTransform;

        public class Arrow
        {
            public Entity ArrowEntity { get; private set; }
            public Transform Transform;
            public UnitVector3D Direction { get; private set; }

            private Entity arrowSpriteEntity;
            private MeshCollider2D collider;
            private bool hasSetSpritePivot = false;
            private Sprite sprite;

            private const double sizeEpsilon = 0.001;
            private double lastSize = 0;

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
                if (!hasSetSpritePivot && collider.Bounds.Extents.Y != 0)
                {
                    var arrowSpriteTransform = arrowSpriteEntity.AddComponent<Transform>();
                    arrowSpriteTransform.Position = new Vector3D(0, collider.Bounds.Extents.Y * 2, 0);
                    hasSetSpritePivot = true;
                }
            }

            public void UpdateScaling()
            {
                var vectorToCamera = CameraController.Instance.cameraTransform.Position - arrowsTransform.Position;

                var size = vectorToCamera.Length * 0.01;
                if (System.Math.Abs(size - lastSize) > sizeEpsilon)
                {
                    Logger.Log(size);
                    arrowsTransform.Scale = new Vector3D(size, size, size);
                    lastSize = size;
                }
            }
        }
        private static Arrow[] arrows = new Arrow[3];
        private static UnitVector3D? selectedArrowDirection = null;

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate()
        {
            foreach (var arrow in arrows)
            {
                arrow.UpdateSpritePivot();
                var forward = CameraController.Instance.cameraTransform.Position - arrow.Transform.Position;
                forward -= forward.ProjectOn(arrow.Direction);
                arrow.Transform.Rotation = MathUtil.LookAt(forward.Normalize(), arrow.Direction);
                arrow.UpdateScaling();
            }

            MoveArrowsTransform();
        }

        public override void Setup()
        {
            arrowsEntity = EnvironmentManager.AddEntity();
            arrowsTransform = arrowsEntity.AddComponent<Transform>();
            arrows[0] = new Arrow(UnitVector3D.XAxis);
            arrows[1] = new Arrow(UnitVector3D.YAxis);
            arrows[2] = new Arrow(UnitVector3D.ZAxis);
        }

        public override void Teardown() { }

        private void MoveArrowsTransform()
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
                        arrowsTransform.Position += unitDeltaDir.ScaleBy(magnitude);
                    }
                }
            }
        }
    }
}
