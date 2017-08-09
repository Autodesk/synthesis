using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.BUExtensions
{
    public class BRaycastRobot : MonoBehaviour
    {
        private const float SuspensionToleranceCm = 5f;
        private const float SuspensionCompressionRatio = 0.5f;
        private const float SuspensionStiffnessRatio = 30f;
        private const float RollingFriction = 0.0025f;

        private VehicleTuning vehicleTuning;

        /// <summary>
        /// The Bullet Physics RaycastVehicle associated with the BRaycastVehicle.
        /// </summary>
        public RaycastRobot RaycastRobot { get; private set; }

        /// <summary>
        /// Adds a wheel to the BRaycastVehicle from the given information.
        /// </summary>
        /// <param name="connectionPoint"></param>
        /// <param name="axle"></param>
        /// <param name="suspensionRestLength"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public int AddWheel(WheelType wheelType, BulletSharp.Math.Vector3 connectionPoint, BulletSharp.Math.Vector3 axle, float suspensionRestLength, float radius)
        {
            switch (wheelType)
            {
                case WheelType.MECANUM:
                    RaycastRobot.SlidingFriction = 0.1f;
                    axle = (Quaternion.AngleAxis((connectionPoint.X > 0 && connectionPoint.Z > 0) || (connectionPoint.X < 0 && connectionPoint.Z < 0) ? -45 : 45,
                        Vector3.up) * axle.ToUnity()).ToBullet();
                    break;
                case WheelType.OMNI:
                    RaycastRobot.SlidingFriction = 0.1f;
                    break;
            }

            WheelInfo w = RaycastRobot.AddWheel(connectionPoint,
                -BulletSharp.Math.Vector3.UnitY, axle, suspensionRestLength,
                radius, vehicleTuning, false);

            w.RollInfluence = 0.25f;
            w.Brake = RollingFriction / radius;

            return RaycastRobot.NumWheels - 1;     
        }

        /// <summary>
        /// Initializes the BRaycastVehicle.
        /// </summary>
        void Awake()
        {
            ((DynamicsWorld)BPhysicsWorld.Get().world).SetInternalTickCallback(UpdateVehicle);

            BRigidBody rigidBody = GetComponent<BRigidBody>();

            if (rigidBody == null)
            {
                Destroy(this);
                return;
            }
            
            RaycastRobot = new RaycastRobot(vehicleTuning = new VehicleTuning
            {
                MaxSuspensionForce = 1000f,
                MaxSuspensionTravelCm = SuspensionToleranceCm,
                SuspensionDamping = 10f,
                SuspensionCompression = rigidBody.mass * SuspensionCompressionRatio,
                SuspensionStiffness = rigidBody.mass * SuspensionStiffnessRatio,
                FrictionSlip = 2f
            },
            (RigidBody)rigidBody.GetCollisionObject(),
            new BRobotRaycaster((DynamicsWorld)BPhysicsWorld.Get().world));
            
            RaycastRobot.SetCoordinateSystem(0, 1, 2);
        }
        
        /// <summary>
        /// Updates each wheel's position for proper interpolation.
        /// </summary>
        void Update()
        {
            for (int i = 0; i < RaycastRobot.NumWheels; i++)
                RaycastRobot.UpdateWheelTransform(i, true);
        }

        /// <summary>
        /// Updates the vehicle (synced with the internal physics tick).
        /// </summary>
        /// <param name="world"></param>
        /// <param name="timeStep"></param>
        private void UpdateVehicle(DynamicsWorld world, float timeStep)
        {
            RaycastRobot.UpdateVehicle(timeStep);
        }
    }
}
