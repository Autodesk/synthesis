using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class UnityRigidNode : RigidNode_Base
{
    //converts inventor's limit information to the modular system unity uses (180/-180)
    private static void AngularLimit(Joint joint, float currentAngle, float lowLimit, float highLimit)
    {
        // This assumes it is precentered.  Sad but acceptable.
        lowLimit = (highLimit - lowLimit) / 2f;
        highLimit = -lowLimit;

        if (joint is ConfigurableJoint)
        {
            ConfigurableJoint cj = (ConfigurableJoint) joint;
            if (Mathf.Abs(highLimit - lowLimit) >= 360.0f)
            {
                cj.angularXMotion = ConfigurableJointMotion.Free;
            }
            else
            {
                SoftJointLimit low = new SoftJointLimit(), high = new SoftJointLimit();
                cj.angularXMotion = ConfigurableJointMotion.Limited;
                low.limit = lowLimit;
                high.limit = highLimit;
                cj.lowAngularXLimit = low;
                cj.highAngularXLimit = high;
            }
        }
        else if (joint is HingeJoint)
        {
            HingeJoint hj = (HingeJoint) joint;
            JointLimits limits = new JointLimits();
            limits.min = lowLimit;
            limits.max = highLimit;
            hj.limits = limits;
        }
    }

    /// <summary>
    /// Configures the linear limit for this node's joint according to the given values.
    /// </summary>
    /// <remarks>For this to work the current position MUST BE in the middle.  </remarks>
    /// <param name="currentPosition">The current linear position</param>
    /// <param name="minPosition">The minimum linear position</param>
    /// <param name="maxPosition">The maximum linear position</param>
    private static void LinearLimit(Joint joint, float currentPosition, float minPosition, float maxPosition)
    {
        float center = (maxPosition - minPosition) / 2.0f;
        //also sets limit properties to eliminate any shaking and twitching from the joint when it hit sthe limit
        SoftJointLimit linear = new SoftJointLimit();
        linear.limit = Mathf.Abs(center) * 0.01f;
        linear.bounciness = 1e-05f;
        linear.spring = 0f;
        linear.damper = 1e30f;
        if (joint is ConfigurableJoint)
            ((ConfigurableJoint) joint).linearLimit = linear;
    }

    /// <summary>
    /// Configures the drivers/motors for this joint.
    /// </summary>
    private void SetXDrives()
    {
        //if the node has a joint and driver
        if (GetSkeletalJoint() != null && GetSkeletalJoint().cDriver != null)
        {
			if(GetSkeletalJoint().cDriver.GetDriveType().IsElevator())
			{
				Debug.Log("swag money");
			}
            if (GetSkeletalJoint().cDriver.GetDriveType().IsPneumatic())
            {
                PneumaticDriverMeta pneum = GetSkeletalJoint().cDriver.GetInfo<PneumaticDriverMeta>();
                if (pneum != null)
                {
                    const float psiToNMm2 = 0.00689475728f * Init.PHYSICS_MASS_MULTIPLIER;
                    if (joint is ConfigurableJoint)
                    {
                        JointDrive drMode = new JointDrive();
                        drMode.mode = JointDriveMode.Velocity;
                        drMode.maximumForce = (psiToNMm2 * pneum.pressurePSI) * (Mathf.PI * Mathf.Pow((pneum.widthMM / 2), 2));
                        ((ConfigurableJoint) joint).xDrive = drMode;
                    }
                }
            }

            if (GetSkeletalJoint().cDriver.GetDriveType().IsMotor())
            {
                if (joint is ConfigurableJoint)
                {
                    JointDrive drMode = new JointDrive();
                    drMode.mode = JointDriveMode.Velocity;
                    drMode.maximumForce = 100.0f;
                    ((ConfigurableJoint) joint).angularXDrive = drMode;
                }
                else if (joint is HingeJoint)
                {
                    JointMotor motor = new JointMotor();
                    motor.force = 100.0f;
                    motor.freeSpin = false;
                    ((HingeJoint) joint).motor = motor;
                    ((HingeJoint) joint).useMotor = true;
                }
            }

            if (this.HasDriverMeta<WheelDriverMeta>())
            {
                JointMotor motor = new JointMotor();
                motor.force = 0f;
                motor.freeSpin = true;
                ((HingeJoint) joint).motor = motor;
                ((HingeJoint) joint).useMotor = false;
            }

        }
    }

    /// <summary>
    /// Crenates the proper joint type for this node.
    /// </summary>
    public void CreateJoint()
    {
        if (joint != null || GetSkeletalJoint() == null)
        {
            return;
        }
        //this is the conditional for Identifying wheels
        if (GetSkeletalJoint().GetJointType() == SkeletalJointType.ROTATIONAL)
        {
            if (this.HasDriverMeta<WheelDriverMeta>())
                OrientWheelNormals();

            RotationalJoint_Base nodeR = (RotationalJoint_Base) GetSkeletalJoint();

            //takes the x, y, and z axis information from a custom vector class to unity's vector class
            joint = ConfigJointInternal<HingeJoint>(nodeR.basePoint.AsV3(), nodeR.axis.AsV3(), delegate(HingeJoint jointSub)
            {
                jointSub.useLimits = nodeR.hasAngularLimit;
                if (nodeR.hasAngularLimit)
                {
                    AngularLimit(jointSub,
                                 MathfExt.ToDegrees(nodeR.currentAngularPosition),
                                 MathfExt.ToDegrees(nodeR.angularLimitLow),
                                 MathfExt.ToDegrees(nodeR.angularLimitHigh));
                }
            });
            //don't worry, I'm a doctor

            if (this.HasDriverMeta<WheelDriverMeta>())
            {
                CreateWheel();
            }

        }
        else if (GetSkeletalJoint().GetJointType() == SkeletalJointType.CYLINDRICAL)
        {
            CylindricalJoint_Base nodeC = (CylindricalJoint_Base) GetSkeletalJoint();

            joint = ConfigJointInternal<ConfigurableJoint>(nodeC.basePoint.AsV3(), nodeC.axis.AsV3(), delegate(ConfigurableJoint jointSub)
            {
                jointSub.xMotion = ConfigurableJointMotion.Limited;
                jointSub.angularXMotion = !nodeC.hasAngularLimit ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited;
                LinearLimit(jointSub,
                            nodeC.currentLinearPosition,
                            nodeC.linearLimitStart,
                            nodeC.linearLimitEnd);
                if (GetSkeletalJoint().cDriver != null && GetSkeletalJoint().cDriver.GetDriveType().IsPneumatic())
                {
                    JointDrive drMode = new JointDrive();
                    drMode.mode = JointDriveMode.Velocity;
                    drMode.maximumForce = 100.0f;
                    jointSub.xDrive = drMode;
                }
                if (jointSub.angularXMotion == ConfigurableJointMotion.Limited)
                {
                    AngularLimit(jointSub,
                                 MathfExt.ToDegrees(nodeC.currentAngularPosition),
                                 MathfExt.ToDegrees(nodeC.angularLimitLow),
                                 MathfExt.ToDegrees(nodeC.angularLimitHigh));
                }
            });
        }
        else if (GetSkeletalJoint().GetJointType() == SkeletalJointType.LINEAR)
        {
            LinearJoint_Base nodeL = (LinearJoint_Base) GetSkeletalJoint();

            joint = ConfigJointInternal<ConfigurableJoint>(nodeL.basePoint.AsV3(), nodeL.axis.AsV3(), delegate(ConfigurableJoint jointSub)
            {
                jointSub.xMotion = ConfigurableJointMotion.Limited;
                LinearLimit(jointSub,
                            nodeL.currentLinearPosition,
                            nodeL.linearLimitLow,
                            nodeL.linearLimitHigh);
            });
			//TODO make code good
			if(GetSkeletalJoint().cDriver.GetDriveType().IsElevator())
			{
				Debug.Log(GetSkeletalJoint().cDriver.portA);
				unityObject.AddComponent<ElevatorScript>();
				Debug.Log("added");
			}
        }
        SetXDrives();

    }

    /// <summary>
    /// Creates a joint at the given position, aligned to the given axis, with the given type.
    /// </summary>
    /// <typeparam name="T">The joint type</typeparam>
    /// <param name="pos">The base position</param>
    /// <param name="axis">The axis</param>
    /// <param name="jointType">The joint callback for additional configuration</param>
    /// <returns>The joint that was created</returns>
    private T ConfigJointInternal<T>(Vector3 pos, Vector3 axis, Action<T> jointType) where T : Joint
    {
        GameObject parentObject = ((UnityRigidNode) GetParent()).unityObject;
        if (!parentObject.gameObject.GetComponent<Rigidbody>())
        {
            parentObject.gameObject.AddComponent<Rigidbody>();
        }

        T joint = unityObject.gameObject.AddComponent<T>();

        joint.connectedBody = parentObject.GetComponent<Rigidbody>();

        joint.anchor = pos;
        joint.connectedAnchor = pos;

        axis.Normalize();
        joint.axis = axis;

        if (joint is ConfigurableJoint)
        {
            ConfigurableJoint cj = joint as ConfigurableJoint;
            cj.angularXMotion = ConfigurableJointMotion.Locked;
            cj.angularYMotion = ConfigurableJointMotion.Locked;
            cj.angularZMotion = ConfigurableJointMotion.Locked;
            cj.xMotion = ConfigurableJointMotion.Locked;
            cj.yMotion = ConfigurableJointMotion.Locked;
            cj.zMotion = ConfigurableJointMotion.Locked;
        }
        jointType(joint);
        return joint;
    }

    /// <summary>
    /// Gets the joint for this node as the given joint type, or null if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The joint type</typeparam>
    /// <returns>The joint, or null if no such joint exists</returns>
    public T GetJoint<T>() where T : Joint
    {
        return joint as T;
    }
}