using System;
using System.Runtime.InteropServices;
using Inventor;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using InventorAddInBasicGUI2;

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
        public String Name;
        public String RefKey;
        public DriveTypes Driver;
        public WheelType Wheel;
        public FrictionLevel Friction;
        public InternalDiameter Diameter;
        public Pressure Pressure;
        public Stages  Stages;
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
        public double UpperLim;
        public double LowerLim;
        public bool HasLimits;
        public bool Rotating;
        public double JointFrictionLevel;
        public bool HasJointFriction;
        public JointData(AssemblyJoint joint, String name)
        {// set all the default values
            Name = name;
            jointOfType = joint;
            try
            {
                ReferenceKeyManager refKeyMgr = StandardAddInServer.m_inventorApplication.ActiveDocument.ReferenceKeyManager;
                byte[] refKey = new byte[0];
                joint.GetReferenceKey(ref refKey, 0);
                RefKey = refKeyMgr.KeyToString(refKey);
            } catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            Driver = DriveTypes.NoDriver;
            Wheel = WheelType.NotAWheel;
            Friction = FrictionLevel.None;
            Diameter = InternalDiameter.PointFive;
            Pressure = Pressure.psi60;
            Stages = Stages.SingleStageElevator; 
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
            if (joint.Definition.JointType == AssemblyJointTypeEnum.kCylindricalJointType || joint.Definition.JointType == AssemblyJointTypeEnum.kSlideJointType)
            {// if the assembly joint is linear
                Rotating = false;
            }
            else
            {// set the combo box choices to rotating
                Rotating = true;
            }
           
            HasLimits = false;
            JointFrictionLevel = 0;
            HasJointFriction = false;
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
        public bool same(BrowserNodeDefinition f)
        {
            if (f.Label.Equals(Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void copyTo(JointData joint)
        {
            joint.Driver = Driver;
            joint.Wheel = Wheel;
            joint.Friction = Friction;
            joint.Diameter = Diameter;
            joint.Pressure = Pressure;
            joint.Stages = Stages;
            joint.PWMport = PWMport;
            joint.PWMport2 = PWMport2;
            joint.CANport = CANport;
            joint.CANport2 = CANport2;
            joint.DriveWheel = DriveWheel;
            joint.PWM = PWM;
            joint.InputGear = InputGear;
            joint.OutputGear = OutputGear;
            joint.SolenoidPortA = SolenoidPortA;
            joint.SolenoidPortB = SolenoidPortB;
            joint.RelayPort = RelayPort;
            joint.HasBrake = HasBrake;
            joint.BrakePortA = BrakePortA;
            joint.BrakePortB = BrakePortB;
            joint.Rotating = Rotating;
            joint.HasLimits = HasLimits;
            joint.HasJointFriction = HasJointFriction;
            joint.JointFrictionLevel = JointFrictionLevel;
        }
    }
}