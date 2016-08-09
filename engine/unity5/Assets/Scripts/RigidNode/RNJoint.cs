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

                BHingedConstraint hc = (BHingedConstraint)(joint = ConfigJoint<BHingedConstraint>(rNode.basePoint.AsV3() - comOffset, rNode.axis.AsV3(), AxisType.X));
                
                hc.localConstraintAxisX = rNode.axis.AsV3().normalized;

                if (hc.setLimit = rNode.hasAngularLimit)
                {
                    hc.lowLimitAngleRadians = rNode.angularLimitLow;
                    hc.highLimitAngleRadians = rNode.angularLimitHigh;
                }

                hc.constraintType = BTypedConstraint.ConstraintType.constrainToAnotherBody;

                if (this.HasDriverMeta<WheelDriverMeta>())
                    ApplyJointMotors();
                
                break;
            case SkeletalJointType.CYLINDRICAL:
                
                CylindricalJoint_Base cNode = (CylindricalJoint_Base)GetSkeletalJoint();

                B6DOFConstraint bc = (B6DOFConstraint)(joint = ConfigJoint<B6DOFConstraint>(cNode.basePoint.AsV3() - comOffset, cNode.axis.AsV3(), AxisType.X));

                bc.linearLimitLower = new Vector3(cNode.linearLimitStart * 0.01f, 0f, 0f);
                bc.linearLimitUpper = new Vector3(cNode.linearLimitEnd * 0.01f, 0f, 0f);

                bc.constraintType = BTypedConstraint.ConstraintType.constrainToAnotherBody;

                break;
            case SkeletalJointType.LINEAR:
                
                LinearJoint_Base lNode = (LinearJoint_Base)GetSkeletalJoint();

                Vector3 axis = lNode.axis.AsV3().normalized;
                // TODO: Figure out how to make a vertical slider?
                BSliderConstraint sc = (BSliderConstraint)(joint = ConfigJoint<BSliderConstraint>(lNode.basePoint.AsV3() - comOffset, lNode.axis.AsV3(), AxisType.X));

                //sc.localConstraintAxisX = new Vector3(0f, 1f, 0f);//lNode.axis.AsV3();
                //sc.localConstraintAxisY = new Vector3(1f, 0f, 0f);//lNode.axis.AsV3();

                if (axis.x < 0) axis.x *= -1f;
                if (axis.y < 0) axis.y *= -1f;
                if (axis.z < 0) axis.z *= -1f;

                sc.localConstraintAxisX = axis;
                sc.localConstraintAxisY = new Vector3(axis.y, axis.z, axis.x);

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