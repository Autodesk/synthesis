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

    private void OrientWheelNormals()
    {
        if (GetSkeletalJoint() is RotationalJoint_Base)
        {
            Vector3 com = ((RigidNode)GetParent()).physicalProperties.centerOfMass.AsV3();
            RotationalJoint_Base rJoint = (RotationalJoint_Base)GetSkeletalJoint();
            Vector3 diff = rJoint.basePoint.AsV3() - com;

            double dot = Vector3.Dot(diff, rJoint.axis.AsV3());

            if (dot > 0)
                rJoint.axis = rJoint.axis.Multiply(-1f);
        }
    }

    private void CreateWheel()
    {
        BRigidBody rigidBody = MainObject.GetComponent<BRigidBody>();
        rigidBody.friction = WHEEL_FRICTION;

        WheelDriverMeta wheel = this.GetDriverMeta<WheelDriverMeta>();
        BSphereShape sphereShape = MainObject.AddComponent<BSphereShape>();
        sphereShape.Radius = wheel.radius * 0.01f;

        // Old cylinder method... collision not as smooth.
        ////BCylinderShapeZ cylinderShape = MainObject.AddComponent<BCylinderShapeZ>();
        //BCylinderShapeX cylinderShape = MainObject.AddComponent<BCylinderShapeX>();
        //cylinderShape.HalfExtent = new Vector3(wheel.radius * 1.5f, wheel.width, wheel.width) * 0.01f;
        //cylinderShape.GetCollisionShape().Margin = wheel.radius * 0.005f; // hopefully change later?

        // ... and continuous collision detection
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