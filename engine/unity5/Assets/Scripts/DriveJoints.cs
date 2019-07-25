using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;
using Synthesis.BUExtensions;
using Synthesis.Input;
using Synthesis.RN;
using Synthesis.Utils;

public class DriveJoints
{
    public const int PWM_COUNT = 10;
    public const  int PWM_OFFSET = 2;

    private const float SpeedArrowPwm = 0.5f;
    private const float WheelMaxSpeed = 300f;
    private const float WheelMotorImpulse = 0.1f;
    private const float WheelCoastFriction = 0.025f;

    private const float HingeMaxSpeed = 4f;
    private const float HingeMotorImpulse = 10f;
    private const float HingeCostFriction = 0.1f;

    private const float MaxSliderForce = 1000f;
    private const float MaxSliderSpeed = 2f;

    private static List<RigidNode_Base> listOfSubNodes;

    private static readonly float[] motors;
    private static readonly JoystickSerializer[] joystickSerializers;

    /// <summary>
    /// Represents ports on a mecanum drive.
    /// </summary>
    private enum MecanumPorts
    {
        FrontRight,
        FrontLeft,
        BackRight,
        BackLeft
    };

    /// <summary>
    /// Initializes the <see cref="DriveJoints"/> static fields.
    /// </summary>
    static DriveJoints()
    {
        listOfSubNodes = new List<RigidNode_Base>();
        motors = new float[73];

        joystickSerializers = new JoystickSerializer[3];

        for (int i = 0; i < joystickSerializers.Length; i++)
            joystickSerializers[i] = new JoystickSerializer(i);
    }

    /// <summary>
    /// Updates the motors on the manipulator in mix and match mode. Called every frame. 
    /// </summary>
    /// <param name="skeleton"></param>
    /// <param name="dioModules"></param>
    /// <param name="controlIndex"></param>
    public static void UpdateManipulatorMotors(RigidNode_Base skeleton, float[] pwm)
    {
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
                    if (rigidNode.GetSkeletalJoint().cDriver.port1 == i + 1)
                    {
                        float force = pwm[i];
                        if (rigidNode.GetSkeletalJoint().cDriver.InputGear != 0 && rigidNode.GetSkeletalJoint().cDriver.OutputGear != 0)
                            force *= Convert.ToSingle(rigidNode.GetSkeletalJoint().cDriver.InputGear / rigidNode.GetSkeletalJoint().cDriver.OutputGear);
                        raycastWheel.ApplyForce(force);
                    }
                }

                if (rigidNode.GetSkeletalJoint() != null && rigidNode.GetSkeletalJoint().cDriver != null)
                {
                    if (rigidNode.GetSkeletalJoint().cDriver.GetDriveType().IsMotor() && rigidNode.MainObject.GetComponent<BHingedConstraint>() != null)
                    {
                        if (rigidNode.GetSkeletalJoint().cDriver.port1 == i + 1)
                        {
                            float maxSpeed = 0f;
                            float impulse = 0f;
                            float friction = 0f;
                            if (rigidNode.GetSkeletalJoint().cDriver.InputGear != 0 && rigidNode.GetSkeletalJoint().cDriver.OutputGear != 0)
                                impulse *= Convert.ToSingle(rigidNode.GetSkeletalJoint().cDriver.InputGear / rigidNode.GetSkeletalJoint().cDriver.OutputGear);

                            if (rigidNode.HasDriverMeta<WheelDriverMeta>())
                            {
                                maxSpeed = WheelMaxSpeed;
                                impulse = WheelMotorImpulse;
                                friction = WheelCoastFriction;
                            }
                            else
                            {
                                maxSpeed = HingeMaxSpeed;
                                impulse = HingeMotorImpulse;
                                friction = HingeCostFriction;
                            }

                            BHingedConstraint hingedConstraint = rigidNode.MainObject.GetComponent<BHingedConstraint>();
                            hingedConstraint.enableMotor = true;
                            hingedConstraint.targetMotorAngularVelocity = pwm[i] > 0f ? maxSpeed : pwm[i] < 0f ? -maxSpeed : 0f;
                            hingedConstraint.maxMotorImpulse = rigidNode.GetSkeletalJoint().cDriver.hasBrake ? HingeMotorImpulse : pwm[i] == 0f ? friction : Mathf.Abs(pwm[i] * impulse);
                        }
                    }
                    else if (rigidNode.GetSkeletalJoint().cDriver.GetDriveType().IsElevator())
                    {
                        if (rigidNode.GetSkeletalJoint().cDriver.port1 == i + 1 && rigidNode.HasDriverMeta<ElevatorDriverMeta>())
                        {
                            BSliderConstraint bSliderConstraint = rigidNode.MainObject.GetComponent<BSliderConstraint>();
                            SliderConstraint sc = (SliderConstraint)bSliderConstraint.GetConstraint();
                            sc.PoweredLinearMotor = true;
                            sc.MaxLinearMotorForce = MaxSliderForce;
                            sc.TargetLinearMotorVelocity = pwm[i] * MaxSliderSpeed;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Updates PWM values from joysticks.
    /// </summary>
    /// <param name="controlIndex"></param>
    /// <param name="mecanum"></param>
    /// <returns></returns>
    public static float[] GetPwmValues(int controlIndex, bool isMecanum)
    {
        float[] pwm = new float[PWM_COUNT];
        if (!InputControl.freeze)
        {
            for (int i = PWM_OFFSET; i < PWM_COUNT; i++)
                pwm[i] = InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().pwmAxes[i - PWM_OFFSET]) * SpeedArrowPwm;

            if (isMecanum)
            {
                #region Mecanum Drive
                pwm[(int)MecanumPorts.FrontRight] =
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().vertical) * -SpeedArrowPwm) +
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().horizontal) * -SpeedArrowPwm) +
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().pwmAxes[0]) * -SpeedArrowPwm);

                pwm[(int)MecanumPorts.FrontLeft] =
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().vertical) * SpeedArrowPwm) +
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().horizontal) * SpeedArrowPwm) +
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().pwmAxes[0]) * -SpeedArrowPwm);

                //For some reason, giving the back wheels 0.25 power instead of 0.5 works for strafing
                pwm[(int)MecanumPorts.BackRight] =
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().vertical) * -SpeedArrowPwm) +
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().horizontal) * -SpeedArrowPwm) +
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().pwmAxes[0]) * 0.25f);

                pwm[(int)MecanumPorts.BackLeft] =
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().vertical) * SpeedArrowPwm) +
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().horizontal) * SpeedArrowPwm) +
                    (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().pwmAxes[0]) * 0.25f);

                #endregion
            }
            else
            {
                switch(Controls.Players[controlIndex].GetActiveProfileMode())
                {
                    case Profile.Mode.TankJoystick:
                        pwm[0] =
                           (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().tankRightAxes) * SpeedArrowPwm);

                        pwm[1] =
                           (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().tankLeftAxes) * SpeedArrowPwm);

                        break;
                    case Profile.Mode.ArcadeKeyboard:
                        pwm[0] =
                            (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().vertical) * -SpeedArrowPwm) +
                            (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().horizontal) * SpeedArrowPwm);

                        pwm[1] =
                            (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().vertical) * SpeedArrowPwm) +
                            (InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().horizontal) * SpeedArrowPwm);

                        break;
                    default:
                        throw new Profile.UnhandledControlProfileException();
                }
            }
        }

        return pwm;
    }

    /// <summary>
    /// Updates all motor values from the given <see cref="RigidNode_Base"/>, pwm values, and emulation network info.
    /// </summary>
    /// <param name="skeleton"></param>
    /// <param name="pwm"></param>
    /// <param name="emuList"></param>
    public static void UpdateAllMotors(RigidNode_Base skeleton, float[] pwm, List<Synthesis.Robot.RobotBase.EmuNetworkInfo> emuList)
    {
        listOfSubNodes.Clear();
        skeleton.ListAllNodes(listOfSubNodes);

        if (Synthesis.EmulatorManager.IsRunningRobotCode()) // Use emulator
        {
            if (Synthesis.EmulatorNetworkConnection.Instance.IsConnected())
            {
                UpdateEmulationJoysticks();
                UpdateEmulationMotors();
                UpdateEmulationSensors(emuList);
            }
        } else // Use regular controls
        {
            for (int i = 0; i < pwm.Length; i++)
                motors[i] = pwm[i];
        }

        foreach (RigidNode node in listOfSubNodes.Select(n => n as RigidNode))
        {
            SkeletalJoint_Base joint = node.GetSkeletalJoint();

            if (joint == null || joint.cDriver == null)
                continue;

            BRaycastWheel raycastWheel = node.MainObject.GetComponent<BRaycastWheel>();

            if (raycastWheel != null)
            {
                float output = motors[node.GetSkeletalJoint().cDriver.port1 - 1];

                if (node.GetSkeletalJoint().cDriver.InputGear != 0 && node.GetSkeletalJoint().cDriver.OutputGear != 0)
                    output *= Convert.ToSingle(node.GetSkeletalJoint().cDriver.InputGear / node.GetSkeletalJoint().cDriver.OutputGear);

                raycastWheel.ApplyForce(output);
            }
            else if (joint.cDriver.GetDriveType().IsMotor() && node.MainObject.GetComponent<BHingedConstraint>() != null)
            {
                float maxSpeed = 0f;
                float impulse = 0f;
                float friction = 0f;
                float output = joint.cDriver.isCan ? motors[joint.cDriver.port1 - PWM_COUNT] : motors[joint.cDriver.port1 - 1];

                if (joint.cDriver.InputGear != 0 && joint.cDriver.OutputGear != 0)
                    impulse *= Convert.ToSingle(joint.cDriver.InputGear / joint.cDriver.OutputGear);

                if (node.HasDriverMeta<WheelDriverMeta>())
                {
                    maxSpeed = WheelMaxSpeed;
                    impulse = WheelMotorImpulse;
                    friction = WheelCoastFriction;
                }
                else
                {
                    maxSpeed = HingeMaxSpeed;
                    impulse = HingeMotorImpulse;
                    friction = HingeCostFriction;
                }

                BHingedConstraint hingedConstraint = node.MainObject.GetComponent<BHingedConstraint>();
                hingedConstraint.enableMotor = true;
                hingedConstraint.targetMotorAngularVelocity = output > 0f ? maxSpeed : output < 0f ? -maxSpeed : 0f;
                hingedConstraint.maxMotorImpulse = node.GetSkeletalJoint().cDriver.hasBrake ? HingeMotorImpulse : output == 0f ? friction : Mathf.Abs(output * impulse);
            }
            else if (joint.cDriver.GetDriveType().IsElevator() && node.HasDriverMeta<ElevatorDriverMeta>())
            {
                float output = motors[joint.cDriver.port1 - 1];

                BSliderConstraint bSliderConstraint = node.MainObject.GetComponent<BSliderConstraint>();
                SliderConstraint sc = (SliderConstraint)bSliderConstraint.GetConstraint();
                sc.PoweredLinearMotor = true;
                sc.MaxLinearMotorForce = MaxSliderForce;
                sc.TargetLinearMotorVelocity = output * MaxSliderSpeed;
            }
        }
    }

    /// <summary>
    /// Updates all joystick values for emulation.
    /// </summary>
    private static void UpdateEmulationJoysticks()
    {
        var instance = Synthesis.InputManager.Instance;
        foreach (JoystickSerializer js in joystickSerializers)
        {
            js.SerializeInputs();
            if (js.Id < instance.Joysticks.Count)
            {
                if (js.Id > instance.Joysticks.Count)
                    throw new IndexOutOfRangeException();

                if (js.Axes.Length > instance.Joysticks[js.Id].AxisCount)
                    throw new Exception();
                if (js.Povs.Length > instance.Joysticks[js.Id].PovCount)
                    throw new Exception();

                for (int i = 0; i < js.Axes.Length; i++)
                    instance.Joysticks[js.Id].Axes[i] = ((int)(js.Axes[i] * 128) >= 128 ? 127 : (int)(js.Axes[i] * 128));
                uint buttonValue = 0;
                for (int i = 0; i < js.Buttons.Length && i < 32; i++)
                    buttonValue += (js.Buttons[i] ? 1u : 0u) << i;
                instance.Joysticks[js.Id].Buttons = buttonValue;
                for (int i = 0; i < js.Povs.Length; i++)
                    instance.Joysticks[js.Id].Povs[i] = (int)js.Povs[i];
            }
        }
    }

    /// <summary>
    /// Updates all motor values for emulation.
    /// </summary>
    /// <param name="pwm"></param>
    private static void UpdateEmulationMotors()
    {
        for (int i = 0; i < Synthesis.OutputManager.Instance.PwmHeaders.Count(); i++)
            motors[i] = (float)Synthesis.OutputManager.Instance.PwmHeaders[i];

        foreach (var CAN in Synthesis.OutputManager.Instance.CanMotorControllers)
            motors[CAN.Id + PWM_COUNT] = CAN.Inverted ? -CAN.PercentOutput : CAN.PercentOutput; // first 10 are for PWM outputs
    }

    /// <summary>
    /// Updates all emulation sensor values.
    /// </summary>
    /// <param name="emuList"></param>
    private static void UpdateEmulationSensors(List<Synthesis.Robot.RobotBase.EmuNetworkInfo> emuList)
    {
        int iter = 0;
        foreach (Synthesis.Robot.RobotBase.EmuNetworkInfo a in emuList)
        {
            if (a.RobotSensor.type == RobotSensorType.ENCODER) // TODO revisit this
            {
                RigidNode rigidNode = null;

                try
                {
                    rigidNode = (RigidNode)(a.wheel);
                }
                catch (Exception e)
                {
                    Debug.Log(e.StackTrace);
                }
                BRaycastWheel bRaycastWheel = rigidNode.MainObject.GetComponent<BRaycastWheel>();

                //BRaycastRobot robot = rigidNode.MainObject.GetComponent<BRaycastRobot>();
                double wheelRadius = 3 / 39.3701;// robot.RaycastRobot.GetWheelInfo(0).WheelsRadius
                Vector3 currentPos = bRaycastWheel.transform.position;
                //double displacement = (Math.Sqrt(currentPos.x * currentPos.x + currentPos.z * currentPos.z) - (Math.Sqrt(a.previousPosition.x*a.previousPosition.x + a.previousPosition.z * a.previousPosition.z)));
                double displacement = ((currentPos - a.previousPosition).magnitude) * Math.Sign(bRaycastWheel.GetWheelSpeed());
                double angleDisplacement = (displacement) / (2 * 3.1415 * wheelRadius);

                a.encoderTickCount += angleDisplacement * a.RobotSensor.conversionFactor;

                a.previousPosition = currentPos;

                if (Synthesis.InputManager.Instance.EncoderManagers[iter] == null)
                    Synthesis.InputManager.Instance.EncoderManagers[iter] = new EmulationService.RobotInputs.Types.EncoderManager();

                EmulationService.RobotInputs.Types.EncoderManager.Types.PortType ConvertPortType(SensorConnectionType type)
                {
                    if (type == SensorConnectionType.DIO)
                        return EmulationService.RobotInputs.Types.EncoderManager.Types.PortType.Di;
                    if (type == SensorConnectionType.ANALOG)
                        return EmulationService.RobotInputs.Types.EncoderManager.Types.PortType.Ai;
                    throw new Exception();
                }

                Synthesis.InputManager.Instance.EncoderManagers[iter].AChannel = (uint)a.RobotSensor.portA;
                Synthesis.InputManager.Instance.EncoderManagers[iter].AType = ConvertPortType(a.RobotSensor.conTypePortA); ;
                Synthesis.InputManager.Instance.EncoderManagers[iter].BChannel = (uint)a.RobotSensor.portB;
                Synthesis.InputManager.Instance.EncoderManagers[iter].BType = ConvertPortType(a.RobotSensor.conTypePortB);;
                Synthesis.InputManager.Instance.EncoderManagers[iter].Ticks = (int)a.encoderTickCount;
                
                iter++;
            }
        }
    }
}