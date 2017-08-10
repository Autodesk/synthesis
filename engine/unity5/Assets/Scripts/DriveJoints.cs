﻿using System;
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


    public static void UpdateAllMotors(RigidNode_Base skeleton, UnityPacket.OutputStatePacket.DIOModule[] dioModules, int controlIndex, bool mecanum)
    {
        if (dioModules[0] == null)
            return;

        bool IsMecanum = mecanum;
        int reverse = -1;

        float[] pwm = dioModules[0].pwmValues;
        float[] can = dioModules[0].canValues;

        if (IsMecanum)
        {
            #region Mecanum Drive
            pwm[(int)MecanumPorts.FRONT_RIGHT] +=
            (InputControl.GetButton(Controls.buttons[controlIndex].forward) ? reverse * SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].backward) ? reverse * -SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].left) ? reverse * -SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].right) ? reverse * SPEED_ARROW_PWM : 0.0f) +
            (Input.GetKey(KeyCode.O) ? reverse * SPEED_ARROW_PWM : 0.0f) + //Left Rotate
            (Input.GetKey(KeyCode.P) ? reverse * -SPEED_ARROW_PWM : 0.0f); //Right Rotate

            pwm[(int)MecanumPorts.BACK_LEFT] +=
            (InputControl.GetButton(Controls.buttons[controlIndex].forward) ? SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].backward) ? -SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].left) ? -SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].right) ? SPEED_ARROW_PWM : 0.0f) +
            (Input.GetKey(KeyCode.O) ? -SPEED_ARROW_PWM : 0.0f) + //Left Rotate
            (Input.GetKey(KeyCode.P) ? SPEED_ARROW_PWM : 0.0f); //Right Rotate

            pwm[(int)MecanumPorts.FRONT_LEFT] +=
            (InputControl.GetButton(Controls.buttons[controlIndex].forward) ? SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].backward) ? -SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].left) ? SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].right) ? -SPEED_ARROW_PWM : 0.0f) +
            (Input.GetKey(KeyCode.O) ? -SPEED_ARROW_PWM : 0.0f) + //Left Rotate
            (Input.GetKey(KeyCode.P) ? SPEED_ARROW_PWM : 0.0f); //Right Rotate

            pwm[(int)MecanumPorts.BACK_RIGHT] +=
            (InputControl.GetButton(Controls.buttons[controlIndex].forward) ? reverse * SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].backward) ? reverse * -SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].left) ? reverse * SPEED_ARROW_PWM : 0.0f) +
            (InputControl.GetButton(Controls.buttons[controlIndex].right) ? reverse * -SPEED_ARROW_PWM : 0.0f) +
            (Input.GetKey(KeyCode.O) ? reverse * SPEED_ARROW_PWM : 0.0f) + //Left Rotate
            (Input.GetKey(KeyCode.P) ? reverse * -SPEED_ARROW_PWM : 0.0f); //Right Rotate
            #endregion
        }

        if (Controls.IsTankDrive)
        {
            #region Tank Drive
            ////Left motor
            //pwm[0] +=
            //   (InputControl.GetButton(Controls.buttons[controlIndex].tankFrontLeft) ? SPEED_ARROW_PWM : 0.0f) +
            //   (InputControl.GetButton(Controls.buttons[controlIndex].tankBackLeft) ? -SPEED_ARROW_PWM : 0.0f);

            ////Right motor
            //pwm[1] +=
            //   (InputControl.GetButton(Controls.buttons[controlIndex].tankFrontRight) ? -SPEED_ARROW_PWM : 0.0f) +
            //   (InputControl.GetButton(Controls.buttons[controlIndex].tankBackRight) ? SPEED_ARROW_PWM : 0.0f);

            pwm[0] +=
               (InputControl.GetAxis(Controls.axes[controlIndex].tankForward) * SPEED_ARROW_PWM);

            pwm[1] +=
               (InputControl.GetAxis(Controls.axes[controlIndex].tankLeft) * SPEED_ARROW_PWM);

            pwm[2] +=
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm2Plus)) ? SPEED_ARROW_PWM :
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm2Neg)) ? -SPEED_ARROW_PWM : 0f;

            pwm[3] +=
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm3Plus)) ? SPEED_ARROW_PWM :
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm3Neg)) ? -SPEED_ARROW_PWM : 0f;

            pwm[4] +=
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm4Plus)) ? SPEED_ARROW_PWM :
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm4Neg)) ? -SPEED_ARROW_PWM : 0f;

            pwm[5] +=
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm5Plus)) ? SPEED_ARROW_PWM :
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm5Neg)) ? -SPEED_ARROW_PWM : 0f;

            pwm[6] +=
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm6Plus)) ? SPEED_ARROW_PWM :
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm6Neg)) ? -SPEED_ARROW_PWM : 0f;
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
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * SPEED_ARROW_PWM);

            pwm[1] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * -SPEED_ARROW_PWM) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * SPEED_ARROW_PWM);

            pwm[2] +=
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm2Plus)) ? SPEED_ARROW_PWM :
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm2Neg)) ? -SPEED_ARROW_PWM : 0f;

            pwm[3] +=
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm3Plus)) ? SPEED_ARROW_PWM :
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm3Neg)) ? -SPEED_ARROW_PWM : 0f;

            pwm[4] +=
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm4Plus)) ? SPEED_ARROW_PWM :
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm4Neg)) ? -SPEED_ARROW_PWM : 0f;

            pwm[5] +=
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm5Plus)) ? SPEED_ARROW_PWM :
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm5Neg)) ? -SPEED_ARROW_PWM : 0f;

            pwm[6] +=
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm6Plus)) ? SPEED_ARROW_PWM :
                (InputControl.GetButton(Controls.buttons[controlIndex].pwm6Neg)) ? -SPEED_ARROW_PWM : 0f;
            #endregion
        }

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
}