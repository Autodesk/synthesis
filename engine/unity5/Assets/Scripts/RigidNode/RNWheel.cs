using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public partial class RigidNode : RigidNode_Base
{
    float WHEEL_MASS_SCALE = 15f;
    float WHEEL_FRICTION = 1f;

    public void OrientWheelNormals()
    {
        if (GetSkeletalJoint() is RotationalJoint_Base)
        {
            Vector3 com = ((RigidNode)GetParent()).PhysicalProperties.centerOfMass.AsV3();
            RotationalJoint_Base rJoint = (RotationalJoint_Base)GetSkeletalJoint();
            Vector3 diff = rJoint.basePoint.AsV3() - com;

            double dot = Vector3.Dot(diff, rJoint.axis.AsV3());

            if (dot > 0)
                rJoint.axis = rJoint.axis.Multiply(-1f);
        }
    }

    private void CreateWheel()
    {
        WheelDriverMeta wheel = this.GetDriverMeta<WheelDriverMeta>();
        BSphereShape sphereShape = MainObject.AddComponent<BSphereShape>();
        sphereShape.Radius = wheel.radius * 0.01f;
    }

    private void UpdateWheelRigidBody()
    {
        BRigidBody rigidBody = MainObject.GetComponent<BRigidBody>();
        rigidBody.friction = WHEEL_FRICTION;

        rigidBody.GetCollisionObject().CcdMotionThreshold = CCD_MOTION_THRESHOLD;
        rigidBody.GetCollisionObject().CcdSweptSphereRadius = CCD_SWEPT_SPHERE_RADIUS;
    }

    private void UpdateWheelRigidBody(float friction)
    {
        BRigidBody rigidBody = MainObject.GetComponent<BRigidBody>();
        rigidBody.friction = friction;

        rigidBody.GetCollisionObject().CcdMotionThreshold = CCD_MOTION_THRESHOLD;
        rigidBody.GetCollisionObject().CcdSweptSphereRadius = CCD_SWEPT_SPHERE_RADIUS;
    }

    private void UpdateWheelMass()
    {
        BRigidBody rigidBody = MainObject.GetComponent<BRigidBody>();
        rigidBody.mass *= WHEEL_MASS_SCALE;
        rigidBody.GetCollisionObject().CollisionShape.CalculateLocalInertia(rigidBody.mass);
    }
}