using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.InputManager;
using SynthesisAPI.Utilities;
using SynthesisCore.Systems;

namespace SynthesisCore.EntityMovement
{
    /// <summary>
    /// System that controls arrows to move entities around
    /// </summary>
    public class MoveArrows : SystemBase
    {
        internal static Entity arrowsEntity { get; private set; }
        internal static Transform arrowsTransform;
        private static Entity? targetEntity = null;
        private static Transform targetTransform = null;

        internal static ArrowBase[] arrows { get; private set; } = new ArrowBase[3];
        internal static UnitVector3D? selectedArrowDirection { get; set; } = null;
        private static bool IsMovingEntity => targetEntity != null;

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate()
        {
            if (!arrowsEntity.EntityExists())
            {
                targetEntity = null;
            }
            if (IsMovingEntity)
            {
                arrowsTransform.Position = targetTransform.GlobalPosition;
                foreach (var arrow in arrows)
                {
                    arrow.Update();
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
                arrows[0] = new AxisArrow(UnitVector3D.XAxis);
                arrows[1] = new AxisArrow(UnitVector3D.YAxis);
                arrows[2] = new AxisArrow(UnitVector3D.ZAxis);
            }
            
            targetEntity = entity;
            foreach(var selectedRigidBody in EnvironmentManager.GetComponentsWhere<Rigidbody>(_ => true))
            {
                // Disable all physics
                selectedRigidBody.IsKinematic = true;
                selectedRigidBody.DetectCollisions = false;
            }
            targetTransform = targetEntity?.GetComponent<Transform>(); // TODO need to move the root parent that is jointed to this
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
                selectedArrowDirection = null;
                foreach (var selectedRigidBody in EnvironmentManager.GetComponentsWhere<Rigidbody>(_ => true))
                {
                    // Re-enable all physics
                    selectedRigidBody.IsKinematic = false;
                    selectedRigidBody.DetectCollisions = true;
                }
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
                        targetTransform.GlobalPosition += unitDeltaDir.ScaleBy(magnitude);
                    }
                }
            }
        }
    }
}
