using BulletSharp;
using BulletUnity;
using Synthesis.RN;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Synthesis.BUExtensions
{
    public class BRaycastRobot : MonoBehaviour
    {
        private const float SuspensionToleranceCm = 5f;
        private const float SuspensionCompressionRatio = 10f;
        private const float SuspensionStiffnessRatio = 2000f;
        private const float RollInfluence = 0.25f;
        private const float DefaultSlidingFriction = 1.0f;
        private const int DefaultNumWheels = 4;

        private VehicleTuning defaultVehicleTuning;
        private RigidNode rootNode;
        private BRigidBody rigidBody;

        /// <summary>
        /// The Bullet Physics RaycastVehicle associated with the BRaycastVehicle.
        /// </summary>
        public RaycastRobot RaycastRobot { get; private set; }

        /// <summary>
        /// The <see cref="RigidNode"/> contianing the wheel information for this <see cref="BRaycastRobot"/>.
        /// </summary>
        public RigidNode RootNode
        {
            get
            {
                return rootNode;
            }
            set
            {
                rootNode = value;

                List<int> driveCoordinates = new List<int>(2);
                int upIndex = GetAxisIdex(Auxiliary.Abs(rootNode.WheelsNormal));

                for (int i = 0; i < 3; i++)
                    if (i != upIndex)
                        driveCoordinates.Add(i);

                RaycastRobot.SetCoordinateSystem(driveCoordinates[0], upIndex, driveCoordinates[1]);

                defaultVehicleTuning.SuspensionStiffness = CalculateStiffness(rootNode.WheelCount);
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
            float slidingFriction = DefaultSlidingFriction;

            switch (wheelType)
            {
                case WheelType.MECANUM:
                    slidingFriction = 0.1f;
                    axle = (Quaternion.AngleAxis((connectionPoint.X > 0 && connectionPoint.Z > 0) || (connectionPoint.X < 0 && connectionPoint.Z < 0) ? -45 : 45,
                        Vector3.up) * axle.ToUnity()).ToBullet();
                    break;
                case WheelType.OMNI:
                    slidingFriction = 0.1f;
                    break;
            }

            RobotWheelInfo wheel = RaycastRobot.AddWheel(connectionPoint,
                rootNode.WheelsNormal.ToBullet(), axle, suspensionRestLength,
                radius, defaultVehicleTuning, false);

            wheel.RollInfluence = RollInfluence;
            wheel.SlidingFriction = slidingFriction;

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

            RaycastRobot = new RaycastRobot(defaultVehicleTuning = new VehicleTuning
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

        /// <summary>
        /// Deregisters this <see cref="BRaycastRobot"/> when this object is destroyed.
        /// </summary>
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

        /// <summary>
        /// Returns the axis index of the given <see cref="Vector3"/>.
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        private int GetAxisIdex(Vector3 axis)
        {
            if (axis.magnitude != 1f)
            {
                Debug.LogError("Vector3 provided is not a unit vector!");
                return -1;
            }

            for (int i = 0; i < 3; i++)
                if (axis[i] == 1f)
                    return i;

            Debug.LogError("Vector3 provided is not aligned with an axis!");
            return -1;
        }
    }
}
