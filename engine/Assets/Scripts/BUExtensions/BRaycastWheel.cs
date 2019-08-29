using BulletSharp;
using BulletUnity;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Synthesis.BUExtensions
{
    public class BRaycastWheel : MonoBehaviour
    {
        private RN.RigidNode node;
        private Vector3 axis;
        private BRaycastRobot robot;
        private BulletSharp.Math.Vector3 basePoint;
        private float radius;
        private int wheelIndex;
        private bool updatePosition;

        private const float VerticalOffset = 0.1f;
        private const float SimTorque = 2.42f;
        private const float MaxAngularSpeed = 40f;
        private const float RollingFriction = 0.01f;

        public float MassTorqueScalar = 0.1f;

        /// <summary>
        /// Sets or gets the radius of the wheel.
        /// </summary>
        public float Radius
        {
            get
            {
                return robot.RaycastRobot.GetWheelInfo(wheelIndex).WheelsRadius;
            }
            set
            {
                robot.RaycastRobot.GetWheelInfo(wheelIndex).WheelsRadius = value;
            }
        }

        /// <summary>
        /// Sets or gets the friction of the wheel.
        /// </summary>
        public float Friction
        {
            get
            {
                return robot.RaycastRobot.GetWheelInfo(wheelIndex).FrictionSlip;
            }
            set
            {
                robot.RaycastRobot.GetWheelInfo(wheelIndex).FrictionSlip = value;
            }
        }

        /// <summary>
        /// Sets or gets the sliding friction of the wheel.
        /// </summary>
        public float SlidingFriction
        {
            get
            {
                return robot.RaycastRobot.GetWheelInfo(wheelIndex).SlidingFriction;
            }
            set
            {
                robot.RaycastRobot.GetWheelInfo(wheelIndex).SlidingFriction = value;
            }
        }

        /// <summary>
        /// Creates a wheel and attaches it to the parent BRaycastVehicle.
        /// </summary>
        /// <param name="node"></param>
        public void CreateWheel(RN.RigidNode node, bool updatePosition = false)
        {
            this.node = node;
            this.updatePosition = updatePosition;

            RN.RigidNode parent = (RN.RigidNode)node.GetParent();
            robot = parent.MainObject.GetComponent<BRaycastRobot>();

            if (robot == null)
            {
                Debug.LogError("Could not add BRaycastWheel because its parent does not have a BRaycastVehicle!");
                Destroy(this);
            }

            RotationalJoint_Base joint = (RotationalJoint_Base)node.GetSkeletalJoint();

            node.OrientWheelNormals();

            axis = joint.axis.AsV3();

            WheelDriverMeta driverMeta = node.GetDriverMeta<WheelDriverMeta>();

            radius = driverMeta.radius * 0.01f;

            Vector3 localPosition = parent.MainObject.transform.InverseTransformPoint(node.MainObject.transform.position);

            basePoint = localPosition.ToBullet() - robot.RootNode.WheelsNormal.ToBullet() * VerticalOffset;

            wheelIndex = robot.AddWheel(driverMeta.type, basePoint, axis.normalized.ToBullet(), VerticalOffset, radius);
        }

        /// <summary>
        /// Applies the given force to the wheel.
        /// </summary>
        /// <param name="force"></param>
        public void ApplyForce(float force)
        {
            robot.RaycastRobot.ApplyEngineForce(-force * (SimTorque / radius) * robot.RaycastRobot.OverrideMass * MassTorqueScalar, wheelIndex);
        }

        /// <summary>
        /// Initializes the BRaycastWheel.
        /// </summary>
        private void Awake()
        {
            wheelIndex = -1;
        }

        /// <summary>
        /// Updates the position of the wheel according to the BRaycastVehicle's position and speed.
        /// </summary>
        private void Update()
        {
            if (robot == null)
                return;

            RobotWheelInfo wheel = robot.RaycastRobot.GetWheelInfo(wheelIndex);

            if (wheel.Brake == 0f)
                wheel.Brake = (RollingFriction / radius) * robot.RaycastRobot.OverrideMass * MassTorqueScalar;

            if (updatePosition)
                transform.position = wheel.WorldTransform.Origin.ToUnity();

            transform.localRotation *= Quaternion.AngleAxis(-wheel.Speed, axis);
        }

        /// <summary>
        /// Get the wheel speed to be used for encoder calculations
        /// </summary>
        /// <returns></returns>
        public float GetWheelSpeed()
        {
            RobotWheelInfo wheel = robot.RaycastRobot.GetWheelInfo(wheelIndex);
            return wheel.Speed;
        }
    }
}