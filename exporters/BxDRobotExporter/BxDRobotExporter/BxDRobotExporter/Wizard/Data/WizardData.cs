﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BxDRobotExporter.Wizard
{
    public class WizardData
    {
        public static WizardData Instance { get; private set; }
        public WizardData()
        {
            Instance = this;
        }
        public int NextFreePort = 3;

        #region Nested enums and classes
        public enum WizardDriveTrain
        {
            WESTERN = 1,
            MECANUM,
            SWERVE,
            H_DRIVE,
            CUSTOM
        }
        public enum WizardMassMode
        {
            SIMPLE_USER,
            COMPLEX_USER,
            MATERIAL_BASED
        } 

        public enum WizardWheelType
        {
            NORMAL = 1,
            OMNI = 2,
            MECANUM = 3
        }
        public enum WizardFrictionLevel
        {
            LOW,
            MEDIUM,
            HIGH
        }

        public struct WheelSetupData
        {
            public WizardWheelType WheelType;
            public WizardFrictionLevel FrictionLevel;
            public byte PWMPort;
            public RigidNode_Base Node;

            public void ApplyToNode()
            {
                Node.GetSkeletalJoint().cDriver = new JointDriver(JointDriverType.MOTOR);
                Node.GetSkeletalJoint().cDriver.SetPort(PWMPort);
                WheelDriverMeta wheelDriver = new WheelDriverMeta();
                switch (FrictionLevel)
                {
                    case WizardFrictionLevel.HIGH:
                        wheelDriver.forwardExtremeSlip = 1; //Speed of max static friction force.
                        wheelDriver.forwardExtremeValue = 10; //Force of max static friction force.
                        wheelDriver.forwardAsympSlip = 1.5f; //Speed of leveled off kinetic friction force.
                        wheelDriver.forwardAsympValue = 8; //Force of leveld off kinetic friction force.

                        if (WheelType == WizardWheelType.OMNI) //Set to relatively low friction, as omni wheels can move sidways.
                        {
                            wheelDriver.sideExtremeSlip = 1; //Same as above, but orthogonal to the movement of the wheel.
                            wheelDriver.sideExtremeValue = .01f;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = .005f;
                        }
                        else
                        {
                            wheelDriver.sideExtremeSlip = 1;
                            wheelDriver.sideExtremeValue = 10;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = 8;
                        }
                        break;
                    case WizardFrictionLevel.MEDIUM:
                        wheelDriver.forwardExtremeSlip = 1f;
                        wheelDriver.forwardExtremeValue = 7;
                        wheelDriver.forwardAsympSlip = 1.5f;
                        wheelDriver.forwardAsympValue = 5;

                        if (WheelType == WizardWheelType.OMNI)
                        {
                            wheelDriver.sideExtremeSlip = 1;
                            wheelDriver.sideExtremeValue = .01f;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = .005f;
                        }
                        else
                        {
                            wheelDriver.sideExtremeSlip = 1;
                            wheelDriver.sideExtremeValue = 7;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = 5;
                        }
                        break;
                    case WizardFrictionLevel.LOW:
                        wheelDriver.forwardExtremeSlip = 1;
                        wheelDriver.forwardExtremeValue = 5;
                        wheelDriver.forwardAsympSlip = 1.5f;
                        wheelDriver.forwardAsympValue = 3;

                        if (WheelType == WizardWheelType.OMNI)
                        {
                            wheelDriver.sideExtremeSlip = 1;
                            wheelDriver.sideExtremeValue = .01f;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = .005f;
                        }
                        else
                        {
                            wheelDriver.sideExtremeSlip = 1;
                            wheelDriver.sideExtremeValue = 5;
                            wheelDriver.sideAsympSlip = 1.5f;
                            wheelDriver.sideAsympValue = 3;
                        }
                        break;
                }
                wheelDriver.type = (global::WheelType)WheelType;
                wheelDriver.isDriveWheel = true;
                Utilities.GUI.LoadMeshes();
                Node.GetSkeletalJoint().cDriver.AddInfo(wheelDriver);

                wheelDriver = (WheelDriverMeta)Node.GetSkeletalJoint().cDriver.GetInfo(typeof(WheelDriverMeta));
                (Node as OGLViewer.OGL_RigidNode).GetWheelInfo(out float radius, out float width, out BXDVector3 center);
                wheelDriver.radius = radius;
                wheelDriver.center = center;
                wheelDriver.width = width;

                Node.GetSkeletalJoint().cDriver.AddInfo(wheelDriver);
            }

        }
        #endregion

        #region BasicRobotInfoPage
        //General Info
        public string RobotName;
        public string Analytics_TeamNumber;
        public string Analytics_TeamLeague;

        //Drive Information
        public WizardDriveTrain DriveTrain;
        public int WheelCount;

        //Mass Info
        public WizardMassMode MassMode;
        public float Mass;
        public float[] Masses;
        #endregion

        #region WheelSetupPage
        public List<WheelSetupData> Wheels;
        public List<RigidNode_Base> WheelNodes
        {
            get
            {
                List<RigidNode_Base> wheelNodes = new List<RigidNode_Base>();

                foreach (WheelSetupData wheel in Wheels)
                {
                    wheelNodes.Add(wheel.Node);
                }

                return wheelNodes;
            }
        }
        #endregion

        #region DefineMovingPartsPage
        public List<RigidNode_Base> MergeQueue = new List<RigidNode_Base>();

        public Dictionary<RigidNode_Base, JointDriver> JointDrivers = new Dictionary<RigidNode_Base, JointDriver>();
        #endregion

        public void Apply()
        {
            switch (MassMode)
            {
                default:
                    break;
                case WizardMassMode.SIMPLE_USER:
                    //Get node volumes
                    List<float> nodeMasses = new List<float>();
                    float totalDefaultMass = 0;
                    foreach(BXDAMesh mesh in Utilities.GUI.Meshes)
                    {
                        nodeMasses.Add(mesh.physics.mass);
                        totalDefaultMass += mesh.physics.mass;
                    }
                    for(int i = 0; i < Utilities.GUI.Meshes.Count; i++)
                    {
                        Utilities.GUI.Meshes[i].physics.mass = this.Mass * (float)(nodeMasses[i] / totalDefaultMass);
                    }
                    break;
                case WizardMassMode.COMPLEX_USER:
                    for (int j = 0; j < Masses.Length; j++)
                    {
                        Utilities.GUI.Meshes[j].physics.mass = Masses[j];
                    }
                    break;
            }

            //WheelSetupPage
            foreach(WheelSetupData data in Wheels)
            {
                data.ApplyToNode();
            }

            //DefineMovingPartsPage
            foreach (KeyValuePair<RigidNode_Base, JointDriver> driver in JointDrivers)
            {
                if (!MergeQueue.Contains(driver.Key) && driver.Value != null)
                {
                    driver.Key.GetSkeletalJoint().cDriver = driver.Value;
                }
            }
            foreach(RigidNode_Base node in MergeQueue)
            {
                Utilities.GUI.MergeNodeIntoParent(node);
            }
        }
    }
}