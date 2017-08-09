using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.BUExtensions
{
    public class BRaycastWheel : MonoBehaviour
    {
        private RigidNode node;
        private Vector3 axis;
        private BRaycastRobot vehicle;
        private BulletSharp.Math.Vector3 basePoint;
        private float radius;

        private int wheelIndex;

        private const float VerticalOffset = 0.1f;
        private const float SimTorque = 2.42f;

        /// <summary>
        /// Creates a wheel and attaches it to the parent BRaycastVehicle.
        /// </summary>
        /// <param name="node"></param>
        public void CreateWheel(RigidNode node)
        {
            this.node = node;

            RigidNode parent = (RigidNode)node.GetParent();
            vehicle = parent.MainObject.GetComponent<BRaycastRobot>();

            if (vehicle == null)
            {
                Debug.LogError("Could not add BRaycastWheel because its parent does not have a BRaycastVehicle!");
                Destroy(this);
            }

            node.OrientWheelNormals();

            RotationalJoint_Base joint = (RotationalJoint_Base)node.GetSkeletalJoint();
            axis = joint.axis.AsV3();

            WheelDriverMeta driverMeta = node.GetDriverMeta<WheelDriverMeta>();

            radius = driverMeta.radius * 0.01f;

            basePoint = (node.MainObject.transform.localPosition - parent.ComOffset).ToBullet() + new BulletSharp.Math.Vector3(0f, VerticalOffset, 0f);

            wheelIndex = vehicle.AddWheel(driverMeta.type, basePoint, axis.normalized.ToBullet(), VerticalOffset, radius);
        }

        /// <summary>
        /// Applies the given force to the wheel.
        /// </summary>
        /// <param name="force"></param>
        public void ApplyForce(float force)
        {
            vehicle.RaycastRobot.ApplyEngineForce(-force * (SimTorque / radius), wheelIndex);
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
            if (vehicle == null)
                return;

            Vector3 velocity = ((RigidBody)transform.parent.GetComponent<BRigidBody>().GetCollisionObject()).GetVelocityInLocalPoint(basePoint).ToUnity();
            Vector3 localVelocity = transform.parent.InverseTransformDirection(velocity);

            WheelInfo wheelInfo = vehicle.RaycastRobot.GetWheelInfo(wheelIndex);
            Vector3 wheelAxle = wheelInfo.WheelAxleCS.ToUnity();

            transform.position = wheelInfo.WorldTransform.Origin.ToUnity();
            transform.localRotation *= Quaternion.AngleAxis(-Vector3.Dot(localVelocity,
                Quaternion.AngleAxis(90f, Vector3.up) * wheelAxle / (Mathf.PI * radius)), axis);

            Debug.DrawLine(transform.position, transform.position + transform.parent.TransformDirection(wheelInfo.WheelAxleCS.ToUnity()) * 0.1f, Color.red);
        }
    }
}
