using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class UnityRigidNode : RigidNode_Base
{
    //converts inventor's limit information to the modular system unity uses (180/-180)
    private void AngularLimit(float currentAngle, float lowLimit, float highLimit)
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

    //finds the difference between the current position, which is always one of the two end points, then finds the difference between the two. 
    //this is then divided by 2 to find the limit for unity
    private void LinearLimit(float currentPosition, float minPosition, float maxPosition)
    {
        // This assumes that it is pre centered.  Kind of sad but is there a choice?

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
        //this is the conditional for Identified wheels
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
                    AngularLimit(nodeR.currentAngularPosition * (180.0f / Mathf.PI),
                                 nodeR.angularLimitLow * (180.0f / Mathf.PI),
                                 nodeR.angularLimitHigh * (180.0f / Mathf.PI));
                }
            });
            //don't worry, I'm a doctor

            if (this.HasDriverMeta<WheelDriverMeta>())
            {
                CreateWheel(nodeR);
            }
        }
        else if (GetSkeletalJoint().GetJointType() == SkeletalJointType.CYLINDRICAL)
        {
            CylindricalJoint_Base nodeC = (CylindricalJoint_Base) GetSkeletalJoint();

            joint = ConfigJointInternal<ConfigurableJoint>(nodeC.basePoint.AsV3(), nodeC.axis.AsV3(), delegate(ConfigurableJoint jointSub)
            {
                jointSub.xMotion = ConfigurableJointMotion.Limited;
                jointSub.angularXMotion = !nodeC.hasAngularLimit ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited;
                LinearLimit(nodeC.currentLinearPosition, nodeC.linearLimitStart, nodeC.linearLimitEnd);
                if (GetSkeletalJoint().cDriver != null && GetSkeletalJoint().cDriver.GetDriveType().IsPneumatic())
                {
                    JointDrive drMode = new JointDrive();
                    drMode.mode = JointDriveMode.Velocity;
                    drMode.maximumForce = 100.0f;
                    jointSub.xDrive = drMode;
                }
                if (jointSub.angularXMotion == ConfigurableJointMotion.Limited)
                {
                    AngularLimit(nodeC.currentAngularPosition * (180.0f / Mathf.PI),
                                 nodeC.angularLimitLow * (180.0f / Mathf.PI),
                                 nodeC.angularLimitHigh * (180.0f / Mathf.PI));
                }
            });
        }
        else if (GetSkeletalJoint().GetJointType() == SkeletalJointType.LINEAR)
        {
            LinearJoint_Base nodeL = (LinearJoint_Base) GetSkeletalJoint();

            joint = ConfigJointInternal<ConfigurableJoint>(nodeL.basePoint.AsV3(), nodeL.axis.AsV3(), delegate(ConfigurableJoint jointSub)
            {
                jointSub.xMotion = ConfigurableJointMotion.Limited;
                LinearLimit(nodeL.currentLinearPosition, nodeL.linearLimitLow, nodeL.linearLimitHigh);
            });

        }
        SetXDrives();
    }
}