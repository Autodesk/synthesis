using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;

public partial class RigidNode : RigidNode_Base
{
    private const float CCD_MOTION_THRESHOLD = 5f;
    private const float CCD_SWEPT_SPHERE_RADIUS = 0.1f;
    
    public void CreateJoint()
    {
        if (joint != null || GetSkeletalJoint() == null)
            return;

        switch (GetSkeletalJoint().GetJointType())
        {
            case SkeletalJointType.ROTATIONAL:

                if (this.HasDriverMeta<WheelDriverMeta>())
                    OrientWheelNormals();

                RotationalJoint_Base rNode = (RotationalJoint_Base)GetSkeletalJoint();

                BHingedConstraint hc = (BHingedConstraint)(joint = ConfigJoint<BHingedConstraint>(rNode.basePoint.AsV3(), rNode.axis.AsV3()));
                
                if (hc.setLimit = rNode.hasAngularLimit)
                {
                    hc.lowLimitAngleRadians = rNode.angularLimitLow;
                    hc.highLimitAngleRadians = rNode.angularLimitHigh;
                }

                // Create wheel?
                hc.constraintType = BTypedConstraint.ConstraintType.constrainToAnotherBody;

                BRigidBody rigidBody = gameObject.GetComponent<BRigidBody>();

                // 'tis a wheel, so it likely needs more mass...
                //rigidBody.mass *= 50f;
                
                // ... and continuous collision detection
                rigidBody.GetCollisionObject().CcdMotionThreshold = CCD_MOTION_THRESHOLD;
                rigidBody.GetCollisionObject().CcdSweptSphereRadius = CCD_SWEPT_SPHERE_RADIUS;

                // TODO: Spinning wheels have gone to crap. Fix it.

                break;
        }
    }

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

    private T ConfigJoint<T>(Vector3 position, Vector3 axis) where T : BTypedConstraint
    {
        GameObject parent = ((RigidNode)GetParent()).gameObject;

        T joint = gameObject.AddComponent<T>();
        joint.otherRigidBody = parent.GetComponent<BRigidBody>();
        //joint.thisRigidBody = parent.GetComponent<BRigidBody>();
        joint.localConstraintPoint = position;

        axis.Normalize();
        joint.localConstraintAxisX = axis;
        joint.debugDrawSize = 0.1f; // For debugging.

        return joint;
    }
}