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
        private const float SuspensionCompressionRatio = 10f;
        private const float SuspensionStiffnessRatio = 2000f;
        private const float RollingFriction = 0.0025f;
        private const int DefaultNumWheels = 4;

        private VehicleTuning vehicleTuning;
        private BRigidBody rigidBody;

        /// <summary>
        /// The Bullet Physics RaycastVehicle associated with the BRaycastVehicle.
        /// </summary>
        public RaycastRobot RaycastRobot { get; private set; }

        /// <summary>
        /// Used as a multiplier to calculate the stiffness values for each wheel.
        /// </summary>
        public int NumWheels
        {
            set
            {
                vehicleTuning.SuspensionStiffness = CalculateStiffness(value);
            }
        }

        /// <summary>
        /// Adjusts the friction value of the robot's wheels.
        /// </summary>
        public float Friction
        {
            set
            {
                vehicleTuning.FrictionSlip = value;
            }
            get
            {
                return vehicleTuning.FrictionSlip;
            }
        }

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
        private void Awake()
        {
            rigidBody = GetComponent<BRigidBody>();

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
                SuspensionCompression = SuspensionCompressionRatio,
                SuspensionStiffness = CalculateStiffness(DefaultNumWheels),
                FrictionSlip = 2f
            },
            (RigidBody)rigidBody.GetCollisionObject(),
            new BRobotRaycaster((DynamicsWorld)BPhysicsWorld.Get().world));

            RaycastRobot.SetCoordinateSystem(0, 1, 2);

            BRobotManager.Instance.RegisterRaycastRobot(RaycastRobot);
        }

        /// <summary>
        /// Updates each wheel's position for proper interpolation.
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < RaycastRobot.NumWheels; i++)
                RaycastRobot.UpdateWheelTransform(i, true);
        }

        private void OnDestroy()
        {
            BRobotManager.Instance.DeregisterRaycastRobot(RaycastRobot);
        }

        /// <summary>
        /// Calculates the suspension stiffness with the given number of wheels.
        /// </summary>
        /// <param name="numWheels"></param>
        /// <returns></returns>
        private float CalculateStiffness(int numWheels)
        {
            return SuspensionStiffnessRatio / numWheels;
        }
    }
}
