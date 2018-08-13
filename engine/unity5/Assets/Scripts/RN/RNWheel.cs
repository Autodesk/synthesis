using BulletSharp;
using BulletUnity;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Synthesis.RN
{
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

        public void GenerateWheelInfo()
        {
            if (GetParent() != null)
            {
                Debug.LogWarning("Generating wheel info should only be called on the root node!");
                return;
            }
            
            Vector3 centroid;

            IEnumerable<RigidNode> wheelNodes = ListAllNodes()
                .Where(n => n.HasDriverMeta<WheelDriverMeta>() && n.GetDriverMeta<WheelDriverMeta>().type != WheelType.NOT_A_WHEEL)
                .Select(n => n as RigidNode);

            WheelCount = wheelNodes.Count();

            WheelsNormal = Auxiliary.BestFitUnitNormal(wheelNodes
                .Select(n => n.MainObject.transform.position)
                .ToArray(), out centroid);

            if (Vector3.Dot(centroid - MainObject.transform.position, WheelsNormal) < 0)
                WheelsNormal *= -1f;
        }
    }
}