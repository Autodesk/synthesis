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
        private int _nextFreePort = 2;
        public int NextFreePort
        {
            get => _nextFreePort++;
            private set => _nextFreePort = value;
        }


        #region Nested enums and classes
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
            public int PWMPort;
            public int PWMPort2;
            public RigidNode_Base Node;

            public void ApplyToNode()
            {
                Node.GetSkeletalJoint().cDriver = new JointDriver(JointDriverType.MOTOR);
                //if (WheelType != WizardWheelType.MECANUM)
                //{
                    Node.GetSkeletalJoint().cDriver.SetPort(PWMPort);
                //} else
                //{
                //    Node.GetSkeletalJoint().cDriver.SetPort(PWMPort, 1);
                //}
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
        /// Number of wheels on the robot. TODO: Use this alongside the wheel detection algorithm to add an "Autodetect Wheels" button on <see cref="DefineWheelsPage"/>
        /// </summary>
        public int wheelCount;

        //Weight Info
        /// <summary>
        /// The total weight of the robot in kilograms
        /// </summary>
        public float weightKg;

        /// <summary>
        /// Whether weight units should be expressed in kilograms or pounds.
        /// </summary>
        public bool preferMetric;
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
            Utilities.GUI.ClearConfiguration();

            Utilities.GUI.RMeta.TotalWeightKg = weightKg;
            Utilities.GUI.RMeta.PreferMetric = preferMetric;

            //WheelSetupPage
            foreach (WheelSetupData data in wheels)
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