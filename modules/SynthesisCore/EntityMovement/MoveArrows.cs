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

        internal static MarkerBase[] arrows { get; private set; } = new MarkerBase[7];
        internal static MarkerBase selectedMarker { get; set; } = null;
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
                // Move the arrows to target position
                arrowsTransform.Position = targetTransform.GlobalPosition;

                // Update size of arrows so they always look the same size as they move
                var vectorToCamera = CameraController.Instance.cameraTransform.GlobalPosition - arrowsTransform.GlobalPosition;
                var size = vectorToCamera.Length * 0.01;
                arrowsTransform.Scale = new Vector3D(size, size, size);

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
                arrows[3] = new RotateArrow(UnitVector3D.XAxis, UnitVector3D.YAxis);
                arrows[4] = new RotateArrow(UnitVector3D.YAxis, UnitVector3D.ZAxis);
                arrows[5] = new RotateArrow(UnitVector3D.ZAxis, UnitVector3D.XAxis);
                arrows[6] = new PointMarker();
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
                selectedMarker = null;
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
            if (selectedMarker != null)
            {
                var xMod = InputManager.GetAxisValue("Mouse X");
                var yMod = InputManager.GetAxisValue("Mouse Y");

                if (xMod != 0 || yMod != 0)
                {
                    selectedMarker.MoveEntityTransform(targetTransform, xMod, yMod);
                }
            }
        }
    }
}
