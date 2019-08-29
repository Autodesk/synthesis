﻿using System;
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
    public const int PWM_HDR_COUNT = 10;
    public const int PWM_MXP_COUNT = 10;

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

    // Output percent of motor controller
    private static readonly float[] pwm_motor_controllers;
    private static readonly Dictionary<int, float> can_motor_controllers;

    private static readonly JoystickSerializer[] joystickSerializers;

    /// <summary>
    /// Initializes the <see cref="DriveJoints"/> static fields.
    /// </summary>
    static DriveJoints()
    {
        listOfSubNodes = new List<RigidNode_Base>();
        pwm_motor_controllers = new float[PWM_HDR_COUNT + PWM_MXP_COUNT];
        can_motor_controllers = new Dictionary<int, float>();

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
    public static void UpdateManipulatorMotors(RigidNode_Base skeleton, int controlIndex, List<Synthesis.Robot.RobotBase.EmuNetworkInfo> emuList)
    {
        UpdateAllOutputs(controlIndex, emuList);
        listOfSubNodes.Clear();
        skeleton.ListAllNodes(listOfSubNodes);

        foreach (RigidNode_Base node in listOfSubNodes)
        {
            RigidNode rigidNode = (RigidNode)node;

            BRaycastWheel raycastWheel = rigidNode.MainObject.GetComponent<BRaycastWheel>();

            if (raycastWheel != null)
            {
                float force = GetOutput(rigidNode.GetSkeletalJoint().cDriver);
                if (rigidNode.GetSkeletalJoint().cDriver.InputGear != 0 && rigidNode.GetSkeletalJoint().cDriver.OutputGear != 0)
                    force *= Convert.ToSingle(rigidNode.GetSkeletalJoint().cDriver.InputGear / rigidNode.GetSkeletalJoint().cDriver.OutputGear);
                raycastWheel.ApplyForce(force);
            }

            if (rigidNode.GetSkeletalJoint() != null && rigidNode.GetSkeletalJoint().cDriver != null)
            {
                if (rigidNode.GetSkeletalJoint().cDriver.GetDriveType().IsMotor() && rigidNode.MainObject.GetComponent<BHingedConstraint>() != null)
                {
                    float output = GetOutput(rigidNode.GetSkeletalJoint().cDriver);
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
                    hingedConstraint.targetMotorAngularVelocity = output > 0f ? maxSpeed : output < 0f ? -maxSpeed : 0f;
                    hingedConstraint.maxMotorImpulse = rigidNode.GetSkeletalJoint().cDriver.hasBrake ? HingeMotorImpulse : output == 0f ? friction : Mathf.Abs(output * impulse);
                }
                else if (rigidNode.GetSkeletalJoint().cDriver.GetDriveType().IsElevator())
                {
                    if (rigidNode.HasDriverMeta<ElevatorDriverMeta>())
                    {
                        float output = GetOutput(rigidNode.GetSkeletalJoint().cDriver);
                        BSliderConstraint bSliderConstraint = rigidNode.MainObject.GetComponent<BSliderConstraint>();
                        SliderConstraint sc = (SliderConstraint)bSliderConstraint.GetConstraint();
                        sc.PoweredLinearMotor = true;
                        sc.MaxLinearMotorForce = MaxSliderForce;
                        sc.TargetLinearMotorVelocity = output * MaxSliderSpeed;
                    }
                }
            }
        }
    }

    private static void UpdateAllOutputs(int controlIndex, List<Synthesis.Robot.RobotBase.EmuNetworkInfo> emuList)
    {
        if (Synthesis.EmulatorManager.UseEmulation) // Use emulator
        {
            if (Synthesis.EmulatorNetworkConnection.Instance.IsConnected())
            {
                UpdateEmulationJoysticks();
                UpdateEmulationMotorControllers();
                UpdateEmulationSensors(emuList);
            }
            else // Disable outputs
            {
                can_motor_controllers.Clear();
                for (int i = 0; i < pwm_motor_controllers.Length; i++)
                {
                    pwm_motor_controllers[i] = 0;
                }
            }
        }
        else // Use regular controls
        {
            UpdateEngineMotorControllers(controlIndex);
        }
    }

    private static float GetOutput(JointDriver cDriver)
    {
        if(!cDriver.GetDriveType().IsMotor())
            throw new Exception("Motor controller output is not motor");
        int motor_controller_address = cDriver.port1 - 1;
        if (cDriver.isCan)
            return can_motor_controllers.ContainsKey(motor_controller_address) ? can_motor_controllers[motor_controller_address] : 0f;
        return pwm_motor_controllers[motor_controller_address];
    }

    /// <summary>
    /// Updates PWM values from joysticks.
    /// </summary>
    /// <param name="controlIndex"></param>
    /// <param name="mecanum"></param>
    /// <returns></returns>
    private static void UpdateEngineMotorControllers(int controlIndex)
    {
        can_motor_controllers.Clear(); // Engine controls do not currently support CAN
        for (int i = 0; i < PWM_HDR_COUNT; i++)
        {
            pwm_motor_controllers[i] = InputControl.GetAxis(Controls.Players[controlIndex].GetAxes().pwmAxes[i], true) * SpeedArrowPwm;
        }
        for (int i = 0; i < PWM_MXP_COUNT; i++)
        {
            pwm_motor_controllers[i + PWM_HDR_COUNT] = 0;
        }
    }

    /// <summary>
    /// Updates all motor values from the given <see cref="RigidNode_Base"/>, pwm values, and emulation network info.
    /// </summary>
    /// <param name="skeleton"></param>
    /// <param name="pwm"></param>
    /// <param name="emuList"></param>
    public static void UpdateAllMotors(RigidNode_Base skeleton, int controlIndex, List<Synthesis.Robot.RobotBase.EmuNetworkInfo> emuList)
    {
        listOfSubNodes.Clear();
        skeleton.ListAllNodes(listOfSubNodes);

        UpdateAllOutputs(controlIndex, emuList);

        foreach (RigidNode node in listOfSubNodes.Select(n => n as RigidNode))
        {
            SkeletalJoint_Base joint = node.GetSkeletalJoint();

            if (joint == null || joint.cDriver == null)
                continue;

            BRaycastWheel raycastWheel = node.MainObject.GetComponent<BRaycastWheel>();

            if (raycastWheel != null)
            {
                float output = pwm_motor_controllers[node.GetSkeletalJoint().cDriver.port1 - 1];

                if (node.GetSkeletalJoint().cDriver.InputGear != 0 && node.GetSkeletalJoint().cDriver.OutputGear != 0)
                    output *= Convert.ToSingle(node.GetSkeletalJoint().cDriver.InputGear / node.GetSkeletalJoint().cDriver.OutputGear);

                raycastWheel.ApplyForce(output);
            }
            else if (joint.cDriver.GetDriveType().IsMotor() && node.MainObject.GetComponent<BHingedConstraint>() != null)
            {
                float maxSpeed = 0f;
                float impulse = 0f;
                float friction = 0f;
                float output = GetOutput(joint.cDriver);

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
                float output = pwm_motor_controllers[joint.cDriver.port1 - 1];

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
        var instance = Synthesis.EmulatedRoboRIO.RobotInputs;
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
    private static void UpdateEmulationMotorControllers()
    {
        for (int i = 0; i < Synthesis.EmulatedRoboRIO.RobotOutputs.PwmHeaders.Count(); i++)
        {
            pwm_motor_controllers[i] = (float)Synthesis.EmulatedRoboRIO.RobotOutputs.PwmHeaders[i];
        }
        for (int i = 0; i < Synthesis.EmulatedRoboRIO.RobotOutputs.MxpData.Count(); i++)
        {
            if (Synthesis.EmulatedRoboRIO.RobotOutputs.MxpData[i].Config == EmulationService.MXPData.Types.Config.Pwm)
            {
                pwm_motor_controllers[PWM_HDR_COUNT + Synthesis.EmulatedRoboRIO.MXPDigitalToPWMIndex(i)] = (float)Synthesis.EmulatedRoboRIO.RobotOutputs.MxpData[i].Value;
            }
        }

        can_motor_controllers.Clear();
        foreach (var CAN in Synthesis.EmulatedRoboRIO.RobotOutputs.CanMotorControllers)
        {
            can_motor_controllers.Add(CAN.Id, CAN.PercentOutput);
        }
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

                if (Synthesis.EmulatedRoboRIO.RobotInputs.EncoderManagers[iter] == null)
                    Synthesis.EmulatedRoboRIO.RobotInputs.EncoderManagers[iter] = new EmulationService.RobotInputs.Types.EncoderManager();

                EmulationService.RobotInputs.Types.EncoderManager.Types.PortType ConvertPortType(SensorConnectionType type)
                {
                    if (type == SensorConnectionType.DIO)
                        return EmulationService.RobotInputs.Types.EncoderManager.Types.PortType.Di;
                    if (type == SensorConnectionType.ANALOG)
                        return EmulationService.RobotInputs.Types.EncoderManager.Types.PortType.Ai;
                    throw new Exception();
                }

                Synthesis.EmulatedRoboRIO.RobotInputs.EncoderManagers[iter].AChannel = (uint)a.RobotSensor.portA;
                Synthesis.EmulatedRoboRIO.RobotInputs.EncoderManagers[iter].AType = ConvertPortType(a.RobotSensor.conTypePortA); ;
                Synthesis.EmulatedRoboRIO.RobotInputs.EncoderManagers[iter].BChannel = (uint)a.RobotSensor.portB;
                Synthesis.EmulatedRoboRIO.RobotInputs.EncoderManagers[iter].BType = ConvertPortType(a.RobotSensor.conTypePortB);;
                Synthesis.EmulatedRoboRIO.RobotInputs.EncoderManagers[iter].Ticks = (int)a.encoderTickCount;
                
                iter++;
            }
        }
    }
}
