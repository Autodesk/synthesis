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
        private AssemblyJoint jointOfType;
        public DriveTypes driver;
        public int port;
        public bool DriveWheel;
        public bool PWM;
        public double InputGear;
        public double OutputGear;
        public int SolenoidPortA;
        public int SolenoidPortB;
        public int RelayPort;
        public int PWMport2;
        public bool HasBrake;
        public int BrakePortA;
        public int BrakePortB;
        public double upperLim;
        public double lowerLim;
        public JointData(AssemblyJoint joint)
        {
            jointOfType = joint;
            driver = DriveTypes.NoDriver; 
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