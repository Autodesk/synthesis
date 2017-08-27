using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BxDRobotExporter.Wizard
{
    /// <summary>
    /// Class storing all of the data gathered during the Guided Export process. 
    /// </summary>
    public class WizardData
    {
        /// <summary>
        /// Active instance of <see cref="WizardData"/>. TODO: Put a <see cref="WizardData"/> property in <see cref="IWizardPage"/>
        /// </summary>
        public static WizardData Instance { get; private set; }

        public WizardData()
        {
            Instance = this;
        }

        /// <summary>
        /// The next free PWM port to assign a driver to.
        /// </summary>
        public int nextFreePort = 3;

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

        /// <summary>
        /// Struct used for storing data from <see cref="WheelSetupPanel"/>s
        /// </summary>
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
        /// <summary>
        /// The name of the robot. Does not do anything now.
        /// </summary>
        public string robotName;
        /// <summary>
        /// Analytics currently not implemented
        /// </summary>
        public string analytics_TeamNumber;
        /// <summary>
        /// Analytics currently not implemented
        /// </summary>
        public string analytics_TeamLeague;

        //Drive Information
        /// <summary>
        /// Drive train set in <see cref="BasicRobotInfoPage"/>
        /// </summary>
        public WizardDriveTrain driveTrain;
        /// <summary>
        /// Number of wheels on the robot. TODO: Use this alongside the wheel detection algorithm to add an "Autodetect Wheels" button on <see cref="DefineWheelsPage"/>
        /// </summary>
        public int wheelCount;

        //Mass Info
        /// <summary>
        /// The mode by which the mass is calculated.
        /// </summary>
        public WizardMassMode massMode;
        /// <summary>
        /// Only assigned if <see cref="massMode"/> is set to <see cref="WizardMassMode.SIMPLE_USER"/>
        /// </summary>
        public float mass;
        /// <summary>
        /// Only assigned if <see cref="massMode"/> is set to <see cref="WizardMassMode.COMPLEX_USER"/>
        /// </summary>
        public float[] masses;
        #endregion

        #region DefineWheelsPage
        /// <summary>
        /// A list of all the <see cref="WheelSetupData"/> gotten from <see cref="DefineWheelsPage"/>
        /// </summary>
        public List<WheelSetupData> wheels;
        /// <summary>
        /// A property that gets the <see cref="RigidNode_Base"/> from each <see cref="WheelSetupData"/> in <see cref="wheels"/>
        /// </summary>
        public List<RigidNode_Base> WheelNodes
        {
            get
            {
                List<RigidNode_Base> wheelNodes = new List<RigidNode_Base>();

                foreach (WheelSetupData wheel in wheels)
                {
                    wheelNodes.Add(wheel.Node);
                }

                return wheelNodes;
            }
        }
        #endregion

        #region DefineMovingPartsPage
        /// <summary>
        /// List of all the <see cref="RigidNode_Base"/> objects to be merged into their parent node.
        /// </summary>
        public List<RigidNode_Base> MergeQueue = new List<RigidNode_Base>();

        /// <summary>
        /// Dictionary associating <see cref="RigidNode_Base"/> instances with <see cref="JointDriver"/> instances
        /// </summary>
        public Dictionary<RigidNode_Base, JointDriver> JointDrivers = new Dictionary<RigidNode_Base, JointDriver>();
        #endregion

        /// <summary>
        /// Applies the gathered data to the nodes and meshes.
        /// </summary>
        public void Apply()
        {
            switch (massMode)
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
                        Utilities.GUI.Meshes[i].physics.mass = this.mass * (float)(nodeMasses[i] / totalDefaultMass);
                    }
                    break;
                case WizardMassMode.COMPLEX_USER:
                    for (int j = 0; j < masses.Length; j++)
                    {
                        Utilities.GUI.Meshes[j].physics.mass = masses[j];
                    }
                    break;
            }

            //WheelSetupPage
            foreach(WheelSetupData data in wheels)
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