using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using Synthesis.FSM;
using Synthesis.BUExtensions;
using Synthesis.States;
using Synthesis.Utils;
using Synthesis.Robot;

namespace Synthesis.RN
{
    public partial class RigidNode : RigidNode_Base
    {
        private const float CCD_MOTION_THRESHOLD = 5f;
        private const float CCD_SWEPT_SPHERE_RADIUS = 0.1f;

        private enum AxisType
        {
            X,
            Y
        }

        public void CreateJoint(RobotBase robotBase, float wheelFriction = 1f, float lateralFriction = 1f)
        {
            if (joint != null || GetSkeletalJoint() == null)
            {
                return;
            }

            switch (GetSkeletalJoint().GetJointType())
            {
                case SkeletalJointType.ROTATIONAL:

                    if (this.HasDriverMeta<WheelDriverMeta>() && this.GetDriverMeta<WheelDriverMeta>().type != WheelType.NOT_A_WHEEL)
                    {
                        RigidNode parent = (RigidNode)GetParent();

                        if (parent.MainObject.GetComponent<BRaycastRobot>() == null)
                            parent.MainObject.AddComponent<BRaycastRobot>().RootNode = RootNode;

                        WheelType wheelType = this.GetDriverMeta<WheelDriverMeta>().type;

                        BRaycastWheel wheel = MainObject.AddComponent<BRaycastWheel>();
                        wheel.CreateWheel(this);

                        if (robotBase is MaMRobot)
                        {
                            wheel.Friction = wheelFriction;
                            wheel.SlidingFriction = lateralFriction;
                        }

                        MainObject.transform.parent = parent.MainObject.transform;

                        robotBase.Weight += (float)GetSkeletalJoint().weight;

                    }
                    else
                    {
                        RotationalJoint_Base rNode = (RotationalJoint_Base)GetSkeletalJoint();

                        BHingedConstraintEx hc = (BHingedConstraintEx)(joint = ConfigJoint<BHingedConstraintEx>(rNode.basePoint.AsV3() - ComOffset, rNode.axis.AsV3(), AxisType.X));
                        Vector3 rAxis = rNode.axis.AsV3().normalized;

                        hc.axisInA = rAxis;
                        hc.axisInB = rAxis;

                        if (hc.setLimit = rNode.hasAngularLimit)
                        {
                            hc.lowLimitAngleRadians = rNode.currentAngularPosition - rNode.angularLimitHigh;
                            hc.highLimitAngleRadians = rNode.currentAngularPosition - rNode.angularLimitLow;
                        }

                        hc.constraintType = BTypedConstraint.ConstraintType.constrainToAnotherBody;
                    }
                    break;
                case SkeletalJointType.CYLINDRICAL:

                    CylindricalJoint_Base cNode = (CylindricalJoint_Base)GetSkeletalJoint();

                    Vector3 cAxis = cNode.axis.AsV3().normalized;
                    B6DOFConstraint bc = (B6DOFConstraint)(joint = ConfigJoint<B6DOFConstraint>(cNode.basePoint.AsV3() - ComOffset, cAxis, AxisType.X));

                    bc.localConstraintAxisX = cAxis;
                    bc.localConstraintAxisY = new Vector3(cAxis.y, cAxis.z, cAxis.x);

                    bc.linearLimitLower = new Vector3((cNode.linearLimitStart - cNode.currentLinearPosition) * 0.01f, 0f, 0f);
                    bc.linearLimitUpper = new Vector3((cNode.linearLimitEnd - cNode.currentLinearPosition) * 0.01f, 0f, 0f);

                    // TODO: Implement angular cylinder limits

                    bc.constraintType = BTypedConstraint.ConstraintType.constrainToAnotherBody;

                    break;
                case SkeletalJointType.LINEAR:

                    LinearJoint_Base lNode = (LinearJoint_Base)GetSkeletalJoint();

                    Vector3 lAxis = lNode.axis.AsV3().normalized;
                    BSliderConstraint sc = (BSliderConstraint)(joint = ConfigJoint<BSliderConstraint>(lNode.basePoint.AsV3() - ComOffset, lAxis, AxisType.X));

                    sc.localConstraintAxisX = lAxis;
                    sc.localConstraintAxisY = new Vector3(lAxis.y, lAxis.z, lAxis.x);

                    sc.lowerLinearLimit = (lNode.linearLimitLow - lNode.currentLinearPosition) * 0.01f;
                    sc.upperLinearLimit = (lNode.linearLimitHigh - lNode.currentLinearPosition) * 0.01f;

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

            float weight = (float)GetSkeletalJoint().weight;
            if (MainObject.GetComponent<BRigidBody>() != null)
            {
                
                MainObject.GetComponent<BRigidBody>().mass = weight;
            }
            else
            {
                BRigidBody br = robotBase.RootNode.MainObject.GetComponent<BRigidBody>();
                robotBase.Weight += (float)weight;
                br.mass += weight;
                RigidBody r = (RigidBody)br.GetCollisionObject();

            }
            
            
        }

        private MainState mainState;


        /// <summary>
        /// Creates node_0 of a manipulator for QuickSwap mode. Node_0 is used to attach the manipulator to the robot.
        /// </summary>
        public void CreateManipulatorJoint(GameObject robot)
        {
            //Ignore physics/collisions between the manipulator and the robot. Currently not working. 
            foreach (BRigidBody rb in robot.GetComponentsInChildren<BRigidBody>())
            {
                MainObject.GetComponent<BRigidBody>().GetCollisionObject().SetIgnoreCollisionCheck(rb.GetCollisionObject(), true);
            }

            if (joint != null || GetSkeletalJoint() == null)
            {
                BHingedConstraintEx hc = MainObject.AddComponent<BHingedConstraintEx>();

                hc.thisRigidBody = MainObject.GetComponent<BRigidBody>();
                hc.otherRigidBody = robot.GetComponentInChildren<BRigidBody>();
                hc.axisInA = new Vector3(0, 1, 0);
                hc.axisInB = new Vector3(0, 1, 0);
                hc.setLimit = true;

                hc.localConstraintPoint = new Vector3(0, 0, 0);

                hc.lowLimitAngleRadians = 0;
                hc.highLimitAngleRadians = 0;

                hc.constraintType = BTypedConstraint.ConstraintType.constrainToAnotherBody;
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
}