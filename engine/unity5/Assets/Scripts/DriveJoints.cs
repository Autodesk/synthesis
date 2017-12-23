using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;
using Assets.Scripts.BUExtensions;

public class DriveJoints
{
    private const float SPEED_ARROW_PWM = 0.5f;
    private const float WHEEL_MAX_SPEED = 300f;
    private const float WHEEL_MOTOR_IMPULSE = 0.1f;
    private const float WHEEL_COAST_FRICTION = 0.025f;

    private const float HINGE_MAX_SPEED = 4f;
    private const float HINGE_MOTOR_IMPULSE = 10f;
    private const float HINGE_COAST_FRICTION = 0.1f;

    private const float MAX_SLIDER_FORCE = 1000f;
    private const float MAX_SLIDER_SPEED = 2f;

    private static List<RigidNode_Base> listOfSubNodes = new List<RigidNode_Base>();

    enum MecanumPorts { FRONT_RIGHT, FRONT_LEFT, BACK_RIGHT, BACK_LEFT };

    public static void SetSolenoid(RigidNode node, bool forward)
    {
        float acceleration = 0;

        B6DOFConstraint b6DOFConstraint = node.GetJoint<B6DOFConstraint>();

        if (b6DOFConstraint == null)
            return;

        // TODO: This code is untested - test it.
        if (b6DOFConstraint.motorLinearMaxMotorForce.x > 0)
        {
            acceleration = b6DOFConstraint.motorLinearMaxMotorForce.x / b6DOFConstraint.thisRigidBody.mass * (forward ? 1 : -1);
        }
        else
        {
            // TODO: Wth are all these arbitrary numbers??? Make constants.
            float psiToNMm2 = 0.00689475728f;
            float maximumForce = (psiToNMm2 * 60f) * (Mathf.PI * Mathf.Pow(6.35f, 2f));
            acceleration = (maximumForce / b6DOFConstraint.thisRigidBody.mass) * (forward ? 1 : -1);
            return;
        }

        // This is sketchy as heck, could be the cause of any issues that might occur.
        float velocity = acceleration * (Time.deltaTime) - Vector3.Dot(b6DOFConstraint.thisRigidBody.velocity,
            ((RigidBody)node.MainObject.GetComponent<BRigidBody>().GetCollisionObject()).WorldTransform.ToUnity().MultiplyVector(b6DOFConstraint.localConstraintAxisX));

        b6DOFConstraint.motorLinearTargetVelocity = new Vector3(velocity, 0f, 0f);
    }

    public static float GetLinearPositionRelativeToParent(RigidNode baseNode)
    {
        RigidBody baseRB = (RigidBody)baseNode.MainObject.GetComponent<BRigidBody>().GetCollisionObject();

        Vector3 baseDirection = BulletSharp.Math.Quaternion.RotationMatrix(baseRB.WorldTransform).ToUnity() * baseNode.GetJoint<BTypedConstraint>().localConstraintAxisX;
        baseDirection.Normalize();

        RigidBody parentRB = (RigidBody)((RigidNode)baseNode.GetParent()).MainObject.GetComponent<BRigidBody>().GetCollisionObject();

        Vector3 difference = baseRB.WorldTransform.Origin.ToUnity() - parentRB.WorldTransform.Origin.ToUnity();

        return -Vector3.Dot(baseDirection, difference);
    }

    public static float GetAngleBetweenChildAndParent(RigidNode child)
    {
        BHingedConstraint hinge = child.GetJoint<BHingedConstraint>();

        if (hinge != null)
            return ((HingeConstraint)hinge.GetConstraint()).HingeAngle;

        RigidBody childRB = (RigidBody)child.MainObject.GetComponent<BRigidBody>().GetCollisionObject();
        RigidBody parentRB = (RigidBody)((RigidNode)child.GetParent()).MainObject.GetComponent<BRigidBody>().GetCollisionObject();

        Vector3 childUp = BulletSharp.Math.Quaternion.RotationMatrix(childRB.WorldTransform).ToUnity() * Vector3.up;
        Vector3 parentUp = BulletSharp.Math.Quaternion.RotationMatrix(parentRB.WorldTransform).ToUnity() * Vector3.up;

        return MathfExt.ToDegrees(Mathf.Acos(Vector3.Dot(childUp, parentUp) / (childUp.magnitude * parentUp.magnitude)));
    }

    /// <summary>
    /// Updates the motors on the manipulator in mix and match mode. Called every frame. 
    /// </summary>
    /// <param name="skeleton"></param>
    /// <param name="dioModules"></param>
    /// <param name="controlIndex"></param>
    public static void UpdateManipulatorMotors(RigidNode_Base skeleton, UnityPacket.OutputStatePacket.DIOModule[] dioModules, int controlIndex)
    {
        float[] pwm;
        float[] can;

        if (dioModules[0] != null)
        {
            pwm = dioModules[0].pwmValues;
            can = dioModules[0].canValues;
        }
        else
        {
            pwm = new float[10];
            can = new float[10];
        }

        pwm[4] +=
             (InputControl.GetAxis(Controls.axes[controlIndex].pwm4Axes) * SPEED_ARROW_PWM);
        pwm[5] +=
             (InputControl.GetAxis(Controls.axes[controlIndex].pwm5Axes) * SPEED_ARROW_PWM);

        pwm[6] +=
             (InputControl.GetAxis(Controls.axes[controlIndex].pwm6Axes) * SPEED_ARROW_PWM);

        listOfSubNodes.Clear();
        skeleton.ListAllNodes(listOfSubNodes);

        for (int i = 0; i < pwm.Length; i++)
        {
            foreach (RigidNode_Base node in listOfSubNodes)
            {
                RigidNode rigidNode = (RigidNode)node;

                BRaycastWheel raycastWheel = rigidNode.MainObject.GetComponent<BRaycastWheel>();

                if (raycastWheel != null)
                {
                    if (rigidNode.GetSkeletalJoint().cDriver.portA == i + 1)
                    {
                        raycastWheel.ApplyForce(pwm[i]);
                    }
                }

                if (rigidNode.GetSkeletalJoint() != null && rigidNode.GetSkeletalJoint().cDriver != null)
                {
                    if (rigidNode.GetSkeletalJoint().cDriver.GetDriveType().IsMotor() && rigidNode.MainObject.GetComponent<BHingedConstraint>() != null)
                    {
                        if (rigidNode.GetSkeletalJoint().cDriver.portA == i + 1)
                        {
                            float maxSpeed = 0f;
                            float impulse = 0f;
                            float friction = 0f;

                            if (rigidNode.HasDriverMeta<WheelDriverMeta>())
                            {
                                maxSpeed = WHEEL_MAX_SPEED;
                                impulse = WHEEL_MOTOR_IMPULSE;
                                friction = WHEEL_COAST_FRICTION;
                            }
                            else
                            {
                                maxSpeed = HINGE_MAX_SPEED;
                                impulse = HINGE_MOTOR_IMPULSE;
                                friction = HINGE_COAST_FRICTION;
                            }

                            BHingedConstraint hingedConstraint = rigidNode.MainObject.GetComponent<BHingedConstraint>();
                            hingedConstraint.enableMotor = true;
                            hingedConstraint.targetMotorAngularVelocity = pwm[i] > 0f ? maxSpeed : pwm[i] < 0f ? -maxSpeed : 0f;
                            hingedConstraint.maxMotorImpulse = pwm[i] == 0f ? friction : Mathf.Abs(pwm[i] * impulse);
                        }
                    }
                    else if (rigidNode.GetSkeletalJoint().cDriver.GetDriveType().IsElevator())
                    {
                        if (rigidNode.GetSkeletalJoint().cDriver.portA == i + 1 && rigidNode.HasDriverMeta<ElevatorDriverMeta>())
                        {
                            BSliderConstraint bSliderConstraint = rigidNode.MainObject.GetComponent<BSliderConstraint>();
                            SliderConstraint sc = (SliderConstraint)bSliderConstraint.GetConstraint();
                            sc.PoweredLinearMotor = true;
                            sc.MaxLinearMotorForce = MAX_SLIDER_FORCE;
                            sc.TargetLinearMotorVelocity = pwm[i] * MAX_SLIDER_SPEED;
                        }
                    }
                }
            }
        }
    }
    public static void UpdateAllMotors(RigidNode_Base skeleton, UnityPacket.OutputStatePacket.DIOModule[] dioModules, int controlIndex, bool mecanum)
    {
        bool IsMecanum = mecanum;
        int reverse = -1;

        float[] pwm;
        float[] can;

        if (dioModules[0] != null)
        {
            pwm = dioModules[0].pwmValues;
            can = dioModules[0].canValues;
        }
        else
        {
            pwm = new float[10];
            can = new float[10];
        }

        if (IsMecanum)
        {
            #region Mecanum Drive
            pwm[(int)MecanumPorts.FRONT_RIGHT] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * -SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * -SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * -SPEED_ARROW_PWM);

            pwm[(int)MecanumPorts.FRONT_LEFT] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * -SPEED_ARROW_PWM);

            //For some reason, giving the back wheels 0.25 power instead of 0.5 works for strafing
            pwm[(int)MecanumPorts.BACK_RIGHT] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * -SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * -SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * 0.25f);

            pwm[(int)MecanumPorts.BACK_LEFT] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * 0.25f);
            pwm[4] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm4Axes) * SPEED_ARROW_PWM);

            pwm[5] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm5Axes) * SPEED_ARROW_PWM);

            pwm[6] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm6Axes) * SPEED_ARROW_PWM);

            pwm[7] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm7Axes) * SPEED_ARROW_PWM);

            pwm[8] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm8Axes) * SPEED_ARROW_PWM);

            pwm[9] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm9Axes) * SPEED_ARROW_PWM);
            #endregion
        }

        if (Controls.TankDriveEnabled)
        {
            #region Tank Drive
            //pwm[0] +=
            //   (InputControl.GetButton(Controls.buttons[controlIndex].tankFrontLeft) ? SPEED_ARROW_PWM : 0.0f) +
            //   (InputControl.GetButton(Controls.buttons[controlIndex].tankBackLeft) ? -SPEED_ARROW_PWM : 0.0f);

            //pwm[1] +=
            //   (InputControl.GetButton(Controls.buttons[controlIndex].tankFrontRight) ? -SPEED_ARROW_PWM : 0.0f) +
            //   (InputControl.GetButton(Controls.buttons[controlIndex].tankBackRight) ? SPEED_ARROW_PWM : 0.0f);

            pwm[0] +=
               (InputControl.GetAxis(Controls.axes[controlIndex].tankRightAxes) * SPEED_ARROW_PWM);

            pwm[1] +=
               (InputControl.GetAxis(Controls.axes[controlIndex].tankLeftAxes) * SPEED_ARROW_PWM);

            pwm[2] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * SPEED_ARROW_PWM);

            pwm[3] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm3Axes) * SPEED_ARROW_PWM);

            pwm[4] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm4Axes) * SPEED_ARROW_PWM);

            pwm[5] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm5Axes) * SPEED_ARROW_PWM);

            pwm[6] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm6Axes) * SPEED_ARROW_PWM);

            pwm[7] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm7Axes) * SPEED_ARROW_PWM);

            pwm[8] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm8Axes) * SPEED_ARROW_PWM);

            pwm[9] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm9Axes) * SPEED_ARROW_PWM);
            #endregion
        }
        else
        {
            #region Arcade Drive
            //pwm[0] +=
            //    (InputControl.GetButton(Controls.buttons[controlIndex].forward) ? SPEED_ARROW_PWM : 0.0f) +
            //    (InputControl.GetButton(Controls.buttons[controlIndex].backward) ? -SPEED_ARROW_PWM : 0.0f) +
            //    (InputControl.GetButton(Controls.buttons[controlIndex].left) ? -SPEED_ARROW_PWM : 0.0f) +
            //    (InputControl.GetButton(Controls.buttons[controlIndex].right) ? SPEED_ARROW_PWM : 0.0f);

            //pwm[1] +=
            //    (InputControl.GetButton(Controls.buttons[controlIndex].forward) ? -SPEED_ARROW_PWM : 0.0f) +
            //    (InputControl.GetButton(Controls.buttons[controlIndex].backward) ? SPEED_ARROW_PWM : 0.0f) +
            //    (InputControl.GetButton(Controls.buttons[controlIndex].left) ? -SPEED_ARROW_PWM : 0.0f) +
            //    (InputControl.GetButton(Controls.buttons[controlIndex].right) ? SPEED_ARROW_PWM : 0.0f);

            pwm[0] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * -SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * SPEED_ARROW_PWM);

            pwm[1] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * SPEED_ARROW_PWM);

            pwm[2] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * SPEED_ARROW_PWM);

            pwm[3] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm3Axes) * SPEED_ARROW_PWM);

            pwm[4] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm4Axes) * SPEED_ARROW_PWM);

            pwm[5] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm5Axes) * SPEED_ARROW_PWM);

            pwm[6] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm6Axes) * SPEED_ARROW_PWM);

            pwm[7] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm7Axes) * SPEED_ARROW_PWM);

            pwm[8] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm8Axes) * SPEED_ARROW_PWM);

            pwm[9] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm9Axes) * SPEED_ARROW_PWM);
            #endregion
        }

        listOfSubNodes.Clear();
        skeleton.ListAllNodes(listOfSubNodes);

        for (int i = 0; i < pwm.Length; i++)
        {
            foreach (RigidNode_Base node in listOfSubNodes)
            {
                RigidNode rigidNode = (RigidNode)node;

                if (pwm[i] != 0f)
                {
                    BRigidBody rigidBody = rigidNode.MainObject.GetComponent<BRigidBody>();

                    if (rigidBody != null && !rigidBody.GetCollisionObject().IsActive)
                        rigidBody.GetCollisionObject().Activate();
                }

                BRaycastWheel raycastWheel = rigidNode.MainObject.GetComponent<BRaycastWheel>();

                if (raycastWheel != null)
                {
                    if (rigidNode.GetSkeletalJoint().cDriver.portA == i + 1)
                    {
                        raycastWheel.ApplyForce(pwm[i]);
                    }
                }

                if (rigidNode.GetSkeletalJoint() != null && rigidNode.GetSkeletalJoint().cDriver != null)
                {
                    if (rigidNode.GetSkeletalJoint().cDriver.GetDriveType().IsMotor() && rigidNode.MainObject.GetComponent<BHingedConstraint>() != null)
                    {
                        if (rigidNode.GetSkeletalJoint().cDriver.portA == i + 1)
                        {
                            float maxSpeed = 0f;
                            float impulse = 0f;
                            float friction = 0f;

                            if (rigidNode.HasDriverMeta<WheelDriverMeta>())
                            {
                                maxSpeed = WHEEL_MAX_SPEED;
                                impulse = WHEEL_MOTOR_IMPULSE;
                                friction = WHEEL_COAST_FRICTION;
                            }
                            else
                            {
                                maxSpeed = HINGE_MAX_SPEED;
                                impulse = HINGE_MOTOR_IMPULSE;
                                friction = HINGE_COAST_FRICTION;
                            }

                            BHingedConstraint hingedConstraint = rigidNode.MainObject.GetComponent<BHingedConstraint>();
                            hingedConstraint.enableMotor = true;
                            hingedConstraint.targetMotorAngularVelocity = pwm[i] > 0f ? maxSpeed : pwm[i] < 0f ? -maxSpeed : 0f;
                            hingedConstraint.maxMotorImpulse = pwm[i] == 0f ? friction : Mathf.Abs(pwm[i] * impulse);
                        }
                    }
                    else if (rigidNode.GetSkeletalJoint().cDriver.GetDriveType().IsElevator())
                    {
                        if (rigidNode.GetSkeletalJoint().cDriver.portA == i + 1 && rigidNode.HasDriverMeta<ElevatorDriverMeta>())
                        {
                            BSliderConstraint bSliderConstraint = rigidNode.MainObject.GetComponent<BSliderConstraint>();
                            SliderConstraint sc = (SliderConstraint)bSliderConstraint.GetConstraint();
                            sc.PoweredLinearMotor = true;
                            sc.MaxLinearMotorForce = MAX_SLIDER_FORCE;
                            sc.TargetLinearMotorVelocity = pwm[i] * MAX_SLIDER_SPEED;
                        }
                    }
                }
            }
        }
    }
}