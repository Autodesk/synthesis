using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;

namespace InventorAddInBasicGUI2
{
    public enum DriveTypes {  NoDriver, Motor, Servo, BumperPnuematic, RelayPneumatic, WormScrew, DualMotor, Elevator};
    public enum WheelType { NotAWheel, Normal, Omni, Mecanum};
    public enum FrictionLevel { None, High, Medium, Low };
    public enum InternalDiameter { One, PointFive, PointTwoFive};
    public enum Pressure { psi60, psi20, psi10};
    public enum Stages {  SingleStageElevator, CascadingStageOne, CascadingStageTwo, ContinuousStage1, ContinuousStage2 };
    //this has got all dat dater
    public class JointData
    {
        public AssemblyJoint jointOfType;
        public DriveTypes driver;
        public WheelType wheel;
        public FrictionLevel friction;
        public InternalDiameter diameter;
        public Pressure pressure;
        public Stages  stages;
        public double PWMport;
        public double PWMport2;
        public double CANport;
        public double CANport2;
        public bool DriveWheel;
        public bool PWM;
        public double InputGear;
        public double OutputGear;
        public double SolenoidPortA;
        public double SolenoidPortB;
        public double RelayPort;
        public bool HasBrake;
        public double BrakePortA;
        public double BrakePortB;
        public double upperLim;
        public double lowerLim;
        public bool HasLimits;
        public JointData(AssemblyJoint joint)
        {
            jointOfType = joint;
            driver = DriveTypes.NoDriver;
            wheel = WheelType.NotAWheel;
            friction = FrictionLevel.None;
            diameter = InternalDiameter.PointFive;
            pressure = Pressure.psi60;
            stages = Stages.SingleStageElevator; 
            PWMport = 1;
            PWMport2 = 1;
            CANport = 1;
            CANport2 = 1;
            DriveWheel = false;
            PWM = true;
            InputGear = 1;
            OutputGear = 1;
            SolenoidPortA = 1;
            SolenoidPortB = 1;
            RelayPort = 1;
            HasBrake = false;
            BrakePortA = 1;
            BrakePortB = 1;
            upperLim = 0;
            lowerLim = 0;
            HasLimits = false;
        }
        public bool equals(AssemblyJoint j)
        {
            if (j.Equals(jointOfType))
            {
                return true;
            } else
            {
                return false;
            }
        }
    }
}