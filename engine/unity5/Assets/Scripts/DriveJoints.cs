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
using Synthesis.StatePacket;
using Synthesis.Utils;

public class DriveJoints
{
    private const float SpeedArrowPwm = 0.5f;

    private const float HingeCostFriction = 0.1f;

    private const float MaxSliderForce = 1000f;
    private const float MaxSliderSpeed = 2f;

    private static List<RigidNode_Base> listOfSubNodes;

    private static readonly float[] motors;
    private static readonly JoystickSerializer[] joystickSerializers;

    public struct Motor
    {
        public float baseTorque;
        public float maxSpeed;
        public float slope;
        public Motor(float baseTorque, float maxSpeed)
        {
            this.baseTorque = baseTorque / 60;
            this.maxSpeed = maxSpeed * 0.104719755f;
            this.slope = 0f;
        }
    }

    private static Dictionary<MotorType, Motor> motorDefinition = new Dictionary<MotorType, Motor>();

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

        DefineMotors();
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
             (InputControl.GetAxis(Controls.axes[controlIndex].pwm4Axes) * SpeedArrowPwm);
        pwm[5] +=
             (InputControl.GetAxis(Controls.axes[controlIndex].pwm5Axes) * SpeedArrowPwm);

        pwm[6] +=
             (InputControl.GetAxis(Controls.axes[controlIndex].pwm6Axes) * SpeedArrowPwm);

        listOfSubNodes.Clear();
        skeleton.ListAllNodes(listOfSubNodes);

        for (int i = 0; i < pwm.Length; i++)
        {
            foreach (RigidNode_Base node in listOfSubNodes)
            {
                RigidNode rigidNode = (RigidNode)node;

                BRaycastWheel raycastWheel = rigidNode.MainObject.GetComponent<BRaycastWheel>();

                SkeletalJoint_Base joint = rigidNode.GetSkeletalJoint();

                if (raycastWheel != null)
                {
                    if (joint.cDriver.port1 == i + 1)
                    {
                        float output = pwm[i];

                        MotorType motorType = joint.cDriver.GetMotorType();

                        float torque = motorType == MotorType.GENERIC ? 2.42f : 60 * motorDefinition[motorType].baseTorque - motorDefinition[motorType].slope * raycastWheel.GetWheelSpeed() / 9.549297f;

                        if (joint.cDriver.InputGear != 0 && joint.cDriver.OutputGear != 0)
                            torque /= Convert.ToSingle(joint.cDriver.InputGear / joint.cDriver.OutputGear);

                        raycastWheel.ApplyForce(output, torque, motorType == MotorType.GENERIC);
                    }
                }

                if (joint != null && joint.cDriver != null)
                {
                    if (joint.cDriver.GetDriveType().IsMotor() && rigidNode.MainObject.GetComponent<BHingedConstraint>() != null)
                    {
                        if (joint.cDriver.port1 == i + 1)
                        {
                            float maxSpeed = 0f;
                            float impulse = 0f;
                            float friction = 0f;

                            friction = HingeCostFriction;

                            MotorType motorType = joint.cDriver.GetMotorType();
                            Motor motor = motorType == MotorType.GENERIC ? new Motor(10f, 4f) : motorDefinition[motorType];

                            maxSpeed = motor.maxSpeed;
                            impulse = motor.baseTorque - motor.slope * ((RigidBody)(rigidNode.MainObject.GetComponent<BRigidBody>().GetCollisionObject())).AngularVelocity.Length / 9.549297f;


                            if (joint.cDriver.InputGear != 0 && joint.cDriver.OutputGear != 0)
                            {
                                float gearRatio = Convert.ToSingle(joint.cDriver.InputGear / joint.cDriver.OutputGear);
                                impulse /= gearRatio;
                                maxSpeed *= gearRatio;
                            }

                            BHingedConstraint hingedConstraint = rigidNode.MainObject.GetComponent<BHingedConstraint>();
                            hingedConstraint.enableMotor = true;
                            hingedConstraint.targetMotorAngularVelocity = pwm[i] > 0f ? maxSpeed : pwm[i] < 0f ? -maxSpeed : 0f;
                            hingedConstraint.maxMotorImpulse = joint.cDriver.hasBrake ? motor.baseTorque : pwm[i] == 0f ? friction : Mathf.Abs(pwm[i] * impulse);
                        }
                    }
                    else if (joint.cDriver.GetDriveType().IsElevator())
                    {
                        if (joint.cDriver.port1 == i + 1 && rigidNode.HasDriverMeta<ElevatorDriverMeta>())
                        {
                            BSliderConstraint bSliderConstraint = rigidNode.MainObject.GetComponent<BSliderConstraint>();
                            SliderConstraint sc = (SliderConstraint)bSliderConstraint.GetConstraint();
                            sc.PoweredLinearMotor = true;
                            sc.MaxLinearMotorForce = MaxSliderForce;
                            sc.TargetLinearMotorVelocity = pwm[i] * MaxSliderSpeed;
                        }
                    }
                    else if (joint.cDriver.GetDriveType().IsPneumatic() && rigidNode.HasDriverMeta<PneumaticDriverMeta>())
                    {
                        BSliderConstraint bSliderConstraint = rigidNode.MainObject.GetComponent<BSliderConstraint>();
                        SliderConstraint sc = (SliderConstraint)bSliderConstraint.GetConstraint();

                        float output = motors[joint.cDriver.port1 - 1];

                        float psi = node.GetDriverMeta<PneumaticDriverMeta>().pressurePSI * 6894.76f;
                        float width = node.GetDriverMeta<PneumaticDriverMeta>().widthMM * 0.001f;
                        float stroke = (sc.UpperLinearLimit - sc.LowerLinearLimit) / 0.01f;

                        float force = psi * ((float)Math.PI) * width * width / 4f;
                        float speed = stroke / 60f;

                        sc.PoweredLinearMotor = true;
                        sc.MaxLinearMotorForce = force;
                        sc.TargetLinearMotorVelocity = sc.TargetLinearMotorVelocity != 0 && output == 0 ? sc.TargetLinearMotorVelocity : output * speed;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Updates PWM values from the given <see cref="UnityPacket.OutputStatePacket.DIOModule"/>s, and control index.
    /// </summary>
    /// <param name="dioModules"></param>
    /// <param name="controlIndex"></param>
    /// <param name="mecanum"></param>
    /// <returns></returns>
    public static float[] GetPwmValues(UnityPacket.OutputStatePacket.DIOModule[] dioModules, int controlIndex, bool mecanum)
    {
        bool IsMecanum = mecanum;

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
            pwm[(int)MecanumPorts.FrontRight] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * -SpeedArrowPwm) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * -SpeedArrowPwm) +
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * -SpeedArrowPwm);

            pwm[(int)MecanumPorts.FrontLeft] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * SpeedArrowPwm) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * SpeedArrowPwm) +
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * -SpeedArrowPwm);

            //For some reason, giving the back wheels 0.25 power instead of 0.5 works for strafing
            pwm[(int)MecanumPorts.BackRight] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * -SpeedArrowPwm) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * -SpeedArrowPwm) +
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * 0.25f);

            pwm[(int)MecanumPorts.BackLeft] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * SpeedArrowPwm) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * SpeedArrowPwm) +
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * 0.25f);

            pwm[4] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm4Axes) * SpeedArrowPwm);

            pwm[5] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm5Axes) * SpeedArrowPwm);

            pwm[6] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm6Axes) * SpeedArrowPwm);

            pwm[7] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm7Axes) * SpeedArrowPwm);

            pwm[8] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm8Axes) * SpeedArrowPwm);

            pwm[9] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm9Axes) * SpeedArrowPwm);
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
               (InputControl.GetAxis(Controls.axes[controlIndex].tankRightAxes) * SpeedArrowPwm);

            pwm[1] +=
               (InputControl.GetAxis(Controls.axes[controlIndex].tankLeftAxes) * SpeedArrowPwm);

            pwm[2] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * SpeedArrowPwm);

            pwm[3] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm3Axes) * SpeedArrowPwm);

            pwm[4] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm4Axes) * SpeedArrowPwm);

            pwm[5] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm5Axes) * SpeedArrowPwm);

            pwm[6] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm6Axes) * SpeedArrowPwm);

            pwm[7] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm7Axes) * SpeedArrowPwm);

            pwm[8] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm8Axes) * SpeedArrowPwm);

            pwm[9] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm9Axes) * SpeedArrowPwm);
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
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * -SpeedArrowPwm) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * SpeedArrowPwm);

            pwm[1] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].vertical) * SpeedArrowPwm) +
                (InputControl.GetAxis(Controls.axes[controlIndex].horizontal) * SpeedArrowPwm);

            pwm[2] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm2Axes) * SpeedArrowPwm);

            pwm[3] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm3Axes) * SpeedArrowPwm);

            pwm[4] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm4Axes) * SpeedArrowPwm);

            pwm[5] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm5Axes) * SpeedArrowPwm);

            pwm[6] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm6Axes) * SpeedArrowPwm);

            pwm[7] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm7Axes) * SpeedArrowPwm);

            pwm[8] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm8Axes) * SpeedArrowPwm);

            pwm[9] +=
                (InputControl.GetAxis(Controls.axes[controlIndex].pwm9Axes) * SpeedArrowPwm);
            #endregion
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

        UpdateEmulationJoysticks();

        UpdateEmulationMotors(pwm);

        UpdateEmulationSensors(emuList);

        foreach (RigidNode node in listOfSubNodes.Select(n => n as RigidNode))
        {
            SkeletalJoint_Base joint = node.GetSkeletalJoint();

            if (joint == null || joint.cDriver == null)
                continue;

            BRaycastWheel raycastWheel = node.MainObject.GetComponent<BRaycastWheel>();

            if (raycastWheel != null)
            {
                float output = motors[node.GetSkeletalJoint().cDriver.port1 - 1];

                MotorType motorType = joint.cDriver.GetMotorType();

                float torque = motorType == MotorType.GENERIC ? 2.42f : 60 * motorDefinition[motorType].baseTorque - motorDefinition[motorType].slope * raycastWheel.GetWheelSpeed() / 9.549297f;

                if (joint.cDriver.InputGear != 0 && joint.cDriver.OutputGear != 0)
                    torque /= Convert.ToSingle(joint.cDriver.InputGear / joint.cDriver.OutputGear);

                raycastWheel.ApplyForce(output, torque, motorType == MotorType.GENERIC);
            }
            else if (joint.cDriver.GetDriveType().IsMotor() && node.MainObject.GetComponent<BHingedConstraint>() != null)
            {
                float maxSpeed = 0f;
                float impulse = 0f;
                float friction = 0f;
                float output = !joint.cDriver.isCan ? motors[joint.cDriver.port1 - 1] : motors[joint.cDriver.port1 - 10];

                friction = HingeCostFriction;

                MotorType motorType = joint.cDriver.GetMotorType();
                Motor motor = motorType == MotorType.GENERIC ? new Motor(10f, 4f) : motorDefinition[motorType];

                maxSpeed = motor.maxSpeed;
                impulse = motor.baseTorque - motor.slope * ((RigidBody)(node.MainObject.GetComponent<BRigidBody>().GetCollisionObject())).AngularVelocity.Length / 9.549297f;


                if (joint.cDriver.InputGear != 0 && joint.cDriver.OutputGear != 0)
                {
                    float gearRatio = Convert.ToSingle(joint.cDriver.InputGear / joint.cDriver.OutputGear);
                    impulse /= gearRatio;
                    maxSpeed *= gearRatio;
                }

                BHingedConstraint hingedConstraint = node.MainObject.GetComponent<BHingedConstraint>();
                hingedConstraint.enableMotor = true;
                hingedConstraint.targetMotorAngularVelocity = output > 0f ? maxSpeed : output < 0f ? -maxSpeed : 0f;
                hingedConstraint.maxMotorImpulse = node.GetSkeletalJoint().cDriver.hasBrake ? motor.baseTorque : output == 0f ? friction : Mathf.Abs(output * impulse);
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
            else if (joint.cDriver.GetDriveType().IsPneumatic() && node.HasDriverMeta<PneumaticDriverMeta>())
            {
                BSliderConstraint bSliderConstraint = node.MainObject.GetComponent<BSliderConstraint>();
                SliderConstraint sc = (SliderConstraint)bSliderConstraint.GetConstraint();

                float output = motors[joint.cDriver.port1 - 1];

                float psi = node.GetDriverMeta<PneumaticDriverMeta>().pressurePSI * 6894.76f;
                float width = node.GetDriverMeta<PneumaticDriverMeta>().widthMM * 0.001f;
                float stroke = (sc.UpperLinearLimit - sc.LowerLinearLimit) / 0.01f;

                float force = psi * ((float)Math.PI) * width * width / 4f;
                float speed = stroke / 60f;

                sc.PoweredLinearMotor = true;
                sc.MaxLinearMotorForce = force;
                sc.TargetLinearMotorVelocity = sc.TargetLinearMotorVelocity != 0 && output == 0 ? sc.TargetLinearMotorVelocity : output * speed;
            }
        }
    }

    /// <summary>
    /// Updates all joystick values for emulation.
    /// </summary>
    private static void UpdateEmulationJoysticks()
    {
        foreach (JoystickSerializer js in joystickSerializers)
        {
            js.SerializeInputs();
            Serialization.updateJoystick(js.Id, js.Axes, js.Buttons, js.Povs);
        }
    }

    /// <summary>
    /// Updates all motor values for emulation.
    /// </summary>
    /// <param name="pwm"></param>
    private static void UpdateEmulationMotors(float[] pwm)
    {
        for (int i = 0; i < pwm.Length; i++)
            motors[i] = pwm[i];

        if (Synthesis.GUI.EmulationDriverStation.Instance.isRunCode)
        {
            for (int i = 0; i < pwm.Length; i++)
                motors[i] = (float)Serialization.getPWM(i);

            foreach (var CAN in OutputManager.Instance.Roborio.CANDevices)
                motors[CAN.id + 10] = CAN.inverted == 0 ? CAN.speed : -CAN.speed;
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

            if (a.RobotSensor.type == RobotSensorType.ENCODER)
            {
                //BRaycastRobot robot = rigidNode.MainObject.GetComponent<BRaycastRobot>();
                double wheelRadius = 3 / 39.3701;// robot.RaycastRobot.GetWheelInfo(0).WheelsRadius;
                Vector3 currentPos = bRaycastWheel.transform.position;
                //double displacement = (Math.Sqrt(currentPos.x * currentPos.x + currentPos.z * currentPos.z) - (Math.Sqrt(a.previousPosition.x*a.previousPosition.x + a.previousPosition.z * a.previousPosition.z)));
                double displacement = ((currentPos - a.previousPosition).magnitude) * Math.Sign(bRaycastWheel.GetWheelSpeed());
                double angleDisplacement = (displacement) / (2 * 3.1415 * wheelRadius);

                a.encoderTickCount += angleDisplacement * a.RobotSensor.conversionFactor;

                a.previousPosition = currentPos;

                var portAType = a.RobotSensor.conTypePortA.ToString() == "DIO" ? "DI" : a.RobotSensor.conTypePortA.ToString();
                var portBType = a.RobotSensor.conTypePortB.ToString() == "DIO" ? "DI" : a.RobotSensor.conTypePortB.ToString();

                if (InputManager.Instance.Roborio.Encoders[iter] == null)
                    InputManager.Instance.Roborio.Encoders[iter] = new EncoderData();
                if (Synthesis.GUI.EmulationDriverStation.Instance.isRunCode)
                    InputManager.Instance.Roborio.Encoders[iter].updateEncoder((int)a.RobotSensor.portA, portAType, (int)a.RobotSensor.portB, portBType, (int)a.encoderTickCount);
                iter++;
            }
        }
    }
    #region add to dictionary
    private static void DefineMotors()
    {
        //TO DO - add motor slopes and vex/ftc compatibality
        motorDefinition.Add(MotorType.CIM, new Motor(2.41f, 5330f));
        motorDefinition.Add(MotorType.MINI_CIM, new Motor(1.41f, 5840f));
        motorDefinition.Add(MotorType.BAG_MOTOR, new Motor(0.43f, 13180f));
        motorDefinition.Add(MotorType.REDLINE_775_PRO, new Motor(0.71f, 18730f));
        motorDefinition.Add(MotorType.ANDYMARK_9015, new Motor(0.36f, 14270f));
        motorDefinition.Add(MotorType.BANEBOTS_775_18v, new Motor(0.72f, 13050f));
        motorDefinition.Add(MotorType.BANEBOTS_775_12v, new Motor(0, 0));
        motorDefinition.Add(MotorType.BANEBOTS_550_12v, new Motor(0.38f, 19000f));
        motorDefinition.Add(MotorType.ANDYMARK_775_125, new Motor(0.28f, 5800f));
        motorDefinition.Add(MotorType.SNOW_BLOWER, new Motor(7.9f, 100f));
        motorDefinition.Add(MotorType.NIDEC_BLDC, new Motor(0.32f, 2700f));
        motorDefinition.Add(MotorType.THROTTLE_MOTOR, new Motor(0.13f, 5300f));
        motorDefinition.Add(MotorType.WINDOW_MOTOR, new Motor(0.154f, 90f));
        motorDefinition.Add(MotorType.NEVEREST, new Motor(0.17f, 5480f));
        motorDefinition.Add(MotorType.TETRIX_MOTOR, new Motor(0.38f, 150f));
        motorDefinition.Add(MotorType.MODERN_ROBOTICS_MATRIX, new Motor(3.27f, 190f));
        motorDefinition.Add(MotorType.REV_ROBOTICS_HD_HEX_20_TO_1, new Motor(0f, 0f));
        motorDefinition.Add(MotorType.REV_ROBOTICS_HD_HEX_40_TO_1, new Motor(0f, 0f));
        motorDefinition.Add(MotorType.REV_ROBOTICS_CORE_HEX, new Motor(0f, 0f));
        motorDefinition.Add(MotorType.VEX_V5_Smart_Motor_600_RPM, new Motor(0f, 0f));
        motorDefinition.Add(MotorType.VEX_V5_Smart_Motor_200_RPM, new Motor(0f, 0f));
        motorDefinition.Add(MotorType.VEX_V5_Smart_Motor_100_RPM, new Motor(0f, 0f));
        motorDefinition.Add(MotorType.VEX_393_NORMAL_SPEED, new Motor(0f, 0f));
        motorDefinition.Add(MotorType.VEX_393_HIGH_SPEED, new Motor(0f, 0f));
        motorDefinition.Add(MotorType.VEX_393_TURBO_GEAR_SET, new Motor(0f, 0f));
    }
    #endregion
}