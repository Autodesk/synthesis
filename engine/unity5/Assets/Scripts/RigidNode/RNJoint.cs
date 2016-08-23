using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;

public partial class RigidNode : RigidNode_Base
{
    private const float CCD_MOTION_THRESHOLD = 5f;
    private const float CCD_SWEPT_SPHERE_RADIUS = 0.1f;

    private enum AxisType
    {
        X,
        Y
    }
    
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

                BHingedConstraint hc = (BHingedConstraint)(joint = ConfigJoint<BHingedConstraint>(rNode.basePoint.AsV3() - ComOffset, rNode.axis.AsV3(), AxisType.X));
                Vector3 rAxis = rNode.axis.AsV3().normalized;

                //if (rAxis.x < 0) rAxis.x *= -1f;
                //if (rAxis.y < 0) rAxis.y *= -1f;
                //if (rAxis.z < 0) rAxis.z *= -1f;

                hc.localConstraintAxisX = rAxis;
                hc.localConstraintAxisY = new Vector3(rAxis.y, rAxis.z, rAxis.x); // This is the closeset thing to working, so keep it until better solution found.
                //hc.localConstraintAxisY = new Vector3(Math.Abs(rAxis.y), rAxis.x, rAxis.z); // So very close...
                if (hc.setLimit = rNode.hasAngularLimit)
                {
                    hc.lowLimitAngleRadians = rNode.currentAngularPosition - rNode.angularLimitHigh;
                    hc.highLimitAngleRadians = rNode.currentAngularPosition - rNode.angularLimitLow;
                }

                hc.constraintType = BTypedConstraint.ConstraintType.constrainToAnotherBody;
                
                break;
            case SkeletalJointType.CYLINDRICAL:
                
                CylindricalJoint_Base cNode = (CylindricalJoint_Base)GetSkeletalJoint();

                B6DOFConstraint bc = (B6DOFConstraint)(joint = ConfigJoint<B6DOFConstraint>(cNode.basePoint.AsV3() - ComOffset, cNode.axis.AsV3(), AxisType.X));

                bc.linearLimitLower = new Vector3(cNode.linearLimitStart * 0.01f, 0f, 0f);
                bc.linearLimitUpper = new Vector3(cNode.linearLimitEnd * 0.01f, 0f, 0f);

                bc.constraintType = BTypedConstraint.ConstraintType.constrainToAnotherBody;

                break;
            case SkeletalJointType.LINEAR:
                
                LinearJoint_Base lNode = (LinearJoint_Base)GetSkeletalJoint();

                Vector3 lAxis = lNode.axis.AsV3().normalized;
                // TODO: Figure out how to make a vertical slider?
                BSliderConstraint sc = (BSliderConstraint)(joint = ConfigJoint<BSliderConstraint>(lNode.basePoint.AsV3() - ComOffset, lNode.axis.AsV3(), AxisType.X));

                if (lAxis.x < 0) lAxis.x *= -1f;
                if (lAxis.y < 0) lAxis.y *= -1f;
                if (lAxis.z < 0) lAxis.z *= -1f;

                sc.localConstraintAxisX = lAxis;
                sc.localConstraintAxisY = new Vector3(lAxis.y, lAxis.z, lAxis.x);

                sc.lowerLinearLimit = lNode.linearLimitLow * 0.01f;
                sc.upperLinearLimit = lNode.linearLimitHigh * 0.01f;

                sc.lowerAngularLimitRadians = 0f;
                sc.upperAngularLimitRadians = 0f;

                sc.constraintType = BTypedConstraint.ConstraintType.constrainToAnotherBody;

                bool b = this.HasDriverMeta<ElevatorDriverMeta>();

                if (GetSkeletalJoint().cDriver != null)
                {
                    if (GetSkeletalJoint().cDriver.GetDriveType().IsElevator())
                    {
                        MainObject.GetComponent<BRigidBody>().mass *= 2f;
                    }
                }
                
                break;
        }
    }

    public T GetJoint<T>() where T : BTypedConstraint
    {
        return (T)joint;
    }

    private T ConfigJoint<T>(Vector3 position, Vector3 axis, AxisType axisType) where T : BTypedConstraint
    {
        GameObject parent = ((RigidNode)GetParent()).MainObject;

        T joint = MainObject.AddComponent<T>();
        joint.otherRigidBody = parent.GetComponent<BRigidBody>();
        joint.localConstraintPoint = position;

        joint.debugDrawSize = 0.1f;

        return joint;
    }
}