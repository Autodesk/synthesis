using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class UnityRigidNode : RigidNode_Base
{

    /// <summary>
    /// Orients drive wheel normals so they face away from the center of mass.
    /// </summary>
    /// <remarks>
    /// Implemented so that the joint's axis is negated when the angle between the joint's axis and the vector from
    /// the wheel to the center of mass is greater than 90 degrees.
    /// </remarks>
    private void OrientWheelNormals()
    {
        if (GetSkeletalJoint() is RotationalJoint_Base && this.HasDriverMeta<WheelDriverMeta>())
        {
            Vector3 com = ((UnityRigidNode) GetParent()).bxdPhysics.centerOfMass.AsV3();
            RotationalJoint_Base rJoint = (RotationalJoint_Base) GetSkeletalJoint();
            Vector3 diff = rJoint.basePoint.AsV3() - com;
            double dot = Vector3.Dot(diff, rJoint.axis.AsV3());
            if (dot < 0)
            {
                rJoint.axis = rJoint.axis.Multiply(-1);
            }
        }
    }
}