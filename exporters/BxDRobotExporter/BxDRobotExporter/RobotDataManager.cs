using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BxDRobotExporter.ControlGUI;
using BxDRobotExporter.GUI.Editors;
using BxDRobotExporter.OGLViewer;
using BxDRobotExporter.SkeletalStructure;

namespace BxDRobotExporter
{
    public class RobotDataManager
    {
        //public event Action ExportFinished;
        //public void OnExportFinished()
        //{
        //    ExportFinished.Invoke();
        //}

        public class RuntimeMeta
        {
            public bool UseSettingsDir;
            public string ActiveDir;
            public string ActiveRobotName;
            private float totalWeightKg;
            public float TotalWeightKg
            {
                get => totalWeightKg;
                set => totalWeightKg = (value > 0) ? value : 0; // Prevent negative weight values
            }
            public bool PreferMetric;
            public string FieldName;

            public static RuntimeMeta CreateRuntimeMeta()
            {
                return new RuntimeMeta
                {
                    UseSettingsDir = true,
                    ActiveDir = null,
                    ActiveRobotName = null,
                    TotalWeightKg = 0,
                    PreferMetric = false,
                    FieldName = null
                };
            }
        }

        public RuntimeMeta RMeta = RuntimeMeta.CreateRuntimeMeta();

        public static RobotDataManager Instance;

        public static ExporterSettingsForm.PluginSettingsValues PluginSettings;
    
        private Inventor.AssemblyDocument asmDocument = null; // Set when LoadRobotData is called.
        public RigidNode_Base SkeletonBase = null;
        public List<BXDAMesh> Meshes = null;
        
        private bool meshesAreColored = false;
        private LoadingSkeletonForm loadingSkeleton;
        private LiteExporterForm liteExporter;

        public RobotDataManager()
        {
            Instance = this;

            RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
            {
                return new OglRigidNode(guid);
            };
        }

        /// <summary>
        /// Open Synthesis to a specific robot and field.
        /// </summary>
        /// <param name="node"></param>
        public void OpenSynthesis(string robotName = null, string fieldName = null)
        {
            if (robotName == null)
            {
                // Cancel if no robot name is given
                if (RMeta.ActiveRobotName == null)
                    return;

                robotName = RMeta.ActiveRobotName;
            }

            if (fieldName == null)
            {
                // Cancel if no field name is given
                if (RMeta.FieldName == null)
                    return;

                fieldName = RMeta.FieldName;
            }
        
            Process.Start(InventorDocumentIoUtils.SYNTHESIS_PATH, string.Format("-robot \"{0}\" -field \"{1}\"", PluginSettings.GeneralSaveLocation + "\\" + robotName, fieldName));
        }

        /// <summary>
        /// Build the node tree of the robot from Inventor
        /// </summary>
        public bool LoadRobotSkeleton()
        {
            try
            {
                var exporterThread = new Thread(() =>
                {
                    loadingSkeleton = new LoadingSkeletonForm();
                    loadingSkeleton.ShowDialog();
                });

                exporterThread.SetApartmentState(ApartmentState.STA);
                exporterThread.Start();

                exporterThread.Join();

                GC.Collect();
            }
            catch (InvalidComObjectException)
            {
            }
            catch (TaskCanceledException)
            {
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            if (SkeletonBase == null)
                return false; // Skeleton export failed

            return true;
        }

        /// <summary>
        /// Load meshes of a robot from Inventor
        /// </summary>
        private bool LoadMeshes()
        {
            try
            {
                var exporterThread = new Thread(() =>
                {
                    if (SkeletonBase == null)
                    {
                        loadingSkeleton = new LoadingSkeletonForm();
                        loadingSkeleton.ShowDialog();
                    }

                    if (SkeletonBase != null)
                    {
                        liteExporter = new LiteExporterForm();
                        liteExporter.ShowDialog(); // Remove node building
                    }
                });

                exporterThread.SetApartmentState(ApartmentState.STA);
                exporterThread.Start();

                exporterThread.Join();

                GC.Collect();

                meshesAreColored = PluginSettings.GeneralUseFancyColors;
            }
            catch (InvalidComObjectException)
            {
            }
            catch (TaskCanceledException)
            {
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        
            if (Meshes == null)
                return false; // Meshes were not exported
        
            if (liteExporter.DialogResult != DialogResult.OK)
                return false; // Exporter was canceled
            
            return true;
        }

        /// <summary>
        /// Prompts the user for the name of the robot, as well as other information.
        /// </summary>
        /// <returns>True if user pressed okay, false if they pressed cancel</returns>
        public bool PromptExportSettings()
        {
            if (ExportForm.Prompt(RMeta.ActiveRobotName, out string robotName, out bool colors, out bool openSynthesis, out string field) == DialogResult.OK)
            {
                RMeta.UseSettingsDir = true;
                RMeta.ActiveDir = null;
                RMeta.ActiveRobotName = robotName;
                RMeta.FieldName = field;
            
                PluginSettings.GeneralUseFancyColors = colors;
                PluginSettings.OnSettingsChanged();

                return true;
            }
            return false;
        }
    
        /// <summary>
        /// Iterates over all the joints in the skeleton and writes the corrosponding Inventor limit into the internal joint limit
        /// Necessary to pull the limits into the joint as the exporter exports. Where the joint is actually written to the .bxdj,
        /// we are unable to access RobotExporterAPI or BxDRobotExporter, so writing the limits here is a workaround to that issue.
        /// </summary>
        /// <param name="skeleton">Skeleton to write limits to</param>
        private static void WriteLimits(RigidNode_Base skeleton)
        {
            List<RigidNode_Base> nodes = new List<RigidNode_Base>();
            skeleton.ListAllNodes(nodes);
            int[] parentId = new int[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].GetParent() != null)
                {
                    parentId[i] = nodes.IndexOf(nodes[i].GetParent());

                    if (parentId[i] < 0) throw new Exception("Can't resolve parent ID for " + nodes[i].ToString());
                }
                else
                {
                    parentId[i] = -1;
                }
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                if (parentId[i] >= 0)
                {
                    INventorSkeletalJoint inventorJoint = nodes[i].GetSkeletalJoint() as INventorSkeletalJoint;
                    if (inventorJoint != null)
                        inventorJoint.ReloadInventorJoint();
                }
            }
        }
        /// <summary>
        /// Saves the robot to the directory it was loaded from or the default directory
        /// </summary>
        /// <returns></returns\>
        /// 

  

        public bool ExportRobot()
        {
            try
            {
                WriteLimits(SkeletonBase);// write the limits from Inventor to the skeleton
                // If robot has not been named, prompt user for information
                if (RMeta.ActiveRobotName == null)
                    if (!PromptExportSettings())
                        return false;

                if (!Directory.Exists(PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName))
                    Directory.CreateDirectory(PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName);

                if (Meshes == null || meshesAreColored != PluginSettings.GeneralUseFancyColors) // Re-export if color settings changed
                    LoadMeshes();
                BXDJSkeleton.SetupFileNames(SkeletonBase);


                BXDJSkeleton.WriteSkeleton(
                    (RMeta.UseSettingsDir && RMeta.ActiveDir != null) ? RMeta.ActiveDir : PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName + "\\skeleton.bxdj",
                    SkeletonBase
                );


                //XML EXPORTING
                //BXDJSkeleton.WriteSkeleton((RMeta.UseSettingsDir && RMeta.ActiveDir != null) ? RMeta.ActiveDir : PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName + "\\skeleton.bxdj", SkeletonBase);

                for (int i = 0; i < Meshes.Count; i++)
                {
                    Meshes[i].WriteToFile((RMeta.UseSettingsDir && RMeta.ActiveDir != null) ? RMeta.ActiveDir : PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName + "\\node_" + i + ".bxda");
                }

                return true;
            }
            catch (Exception e)
            {
                //TODO: Create a form that displays a simple error message with an option to expand it and view the exception info
                MessageBox.Show("Error saving robot: " + e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Loads the joint information from the Inventor assembly file. Returns false if fails.
        /// </summary>
        /// <param name="asmDocument">Assembly document to load data from. Data will be saved to this document when <see cref="SaveRobotData"/> is called.</param>
        /// <returns>True if all data was loaded successfully.</returns>
        public bool LoadRobotData(Inventor.AssemblyDocument asmDocument)
        {
            if (asmDocument == null)
                return false;

            if (SkeletonBase == null)
                return false;

            this.asmDocument = asmDocument;
            Inventor.PropertySets propertySets = asmDocument.PropertySets;

            // Load Robot Data
            try
            {
                // Load global robot data
                Inventor.PropertySet propertySet = InventorDocumentIoUtils.GetPropertySet(propertySets, "bxd-robotdata", false);
            
                if (propertySet != null)
                {
                    RMeta.ActiveRobotName = InventorDocumentIoUtils.GetProperty(propertySet, "robot-name", "");
                    RMeta.TotalWeightKg = InventorDocumentIoUtils.GetProperty(propertySet, "robot-weight-kg", 0) / 10.0f; // Stored at x10 for better accuracy
                    RMeta.PreferMetric = InventorDocumentIoUtils.GetProperty(propertySet, "robot-prefer-metric", false);
                    RobotDataManager.Instance.SkeletonBase.driveTrainType = (RigidNode_Base.DriveTrainType)InventorDocumentIoUtils.GetProperty(propertySet, "robot-driveTrainType", (int)RigidNode_Base.DriveTrainType.NONE);
                }

                // Load joint data
                return LoadJointData(propertySets, SkeletonBase) && (propertySet != null);
            }
            catch (Exception e)
            {
                MessageBox.Show("Robot data could not be loaded from the inventor file. The following error occured:\n" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Recursive utility for JointDataLoad.
        /// </summary>
        /// <param name="propertySets">Group of property sets to add any new property sets to.</param>
        /// <param name="currentNode">Current node to save joint data of.</param>
        /// <returns>True if all data was loaded successfully.</returns>
        private static bool LoadJointData(Inventor.PropertySets propertySets, RigidNode_Base currentNode)
        {
            var allSuccessful = true;

            foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> connection in currentNode.Children)
            {
                SkeletalJoint_Base joint = connection.Key;
                RigidNode_Base child = connection.Value;

                // Name of the property set in inventor
                string setName = "bxd-jointdata-" + child.GetModelID();

                // Attempt to open the property set
                Inventor.PropertySet propertySet = InventorDocumentIoUtils.GetPropertySet(propertySets, setName, false);

                // If the property set does not exist, stop loading data
                if (propertySet == null)
                    return false;

                joint.weight = InventorDocumentIoUtils.GetProperty(propertySet, "weight", 10);

                // Get joint properties from set
                // Get driver information
                if (InventorDocumentIoUtils.GetProperty(propertySet, "has-driver", false))
                {
                    if (joint.cDriver == null)
                        joint.cDriver = new JointDriver((JointDriverType)InventorDocumentIoUtils.GetProperty(propertySet, "driver-type", (int)JointDriverType.MOTOR));
                    JointDriver driver = joint.cDriver;

                    joint.cDriver.motor = (MotorType)InventorDocumentIoUtils.GetProperty(propertySet, "motor-type", (int)MotorType.GENERIC);
                    joint.cDriver.port1 = InventorDocumentIoUtils.GetProperty(propertySet, "driver-port1", 0);
                    joint.cDriver.port2 = InventorDocumentIoUtils.GetProperty(propertySet, "driver-port2", -1);
                    joint.cDriver.isCan = InventorDocumentIoUtils.GetProperty(propertySet, "driver-isCan", false);
                    joint.cDriver.lowerLimit = InventorDocumentIoUtils.GetProperty(propertySet, "driver-lowerLimit", 0.0f);
                    joint.cDriver.upperLimit = InventorDocumentIoUtils.GetProperty(propertySet, "driver-upperLimit", 0.0f);
                    joint.cDriver.InputGear = InventorDocumentIoUtils.GetProperty(propertySet, "driver-inputGear", 0.0f);// writes the gearing that the user last had in the exporter to the current gearing value
                    joint.cDriver.OutputGear = InventorDocumentIoUtils.GetProperty(propertySet, "driver-outputGear", 0.0f);// writes the gearing that the user last had in the exporter to the current gearing value
                    joint.cDriver.hasBrake = InventorDocumentIoUtils.GetProperty(propertySet, "driver-hasBrake", false);

                    // Get other properties stored in meta
                    // Wheel information
                    if (InventorDocumentIoUtils.GetProperty(propertySet, "has-wheel", false))
                    {
                        if (driver.GetInfo<WheelDriverMeta>() == null)
                            driver.AddInfo(new WheelDriverMeta());
                        WheelDriverMeta wheel = joint.cDriver.GetInfo<WheelDriverMeta>();

                        wheel.type = (WheelType)InventorDocumentIoUtils.GetProperty(propertySet, "wheel-type", (int)WheelType.NORMAL);
                        wheel.isDriveWheel = InventorDocumentIoUtils.GetProperty(propertySet, "wheel-isDriveWheel", false);
                        wheel.SetFrictionLevel((FrictionLevel)InventorDocumentIoUtils.GetProperty(propertySet, "wheel-frictionLevel", (int)FrictionLevel.MEDIUM));
                    }

                    // Pneumatic information
                    if (InventorDocumentIoUtils.GetProperty(propertySet, "has-pneumatic", false))
                    {
                        if (driver.GetInfo<PneumaticDriverMeta>() == null)
                            driver.AddInfo(new PneumaticDriverMeta());
                        PneumaticDriverMeta pneumatic = joint.cDriver.GetInfo<PneumaticDriverMeta>();

                        pneumatic.width = InventorDocumentIoUtils.GetProperty(propertySet, "pneumatic-diameter", (double)0.5);
                        pneumatic.pressureEnum = (PneumaticPressure)InventorDocumentIoUtils.GetProperty(propertySet, "pneumatic-pressure", (int)PneumaticPressure.MEDIUM);
                    }

                    // Elevator information
                    if (InventorDocumentIoUtils.GetProperty(propertySet, "has-elevator", false))
                    {
                        if (driver.GetInfo<ElevatorDriverMeta>() == null)
                            driver.AddInfo(new ElevatorDriverMeta());
                        ElevatorDriverMeta elevator = joint.cDriver.GetInfo<ElevatorDriverMeta>();

                        elevator.type = (ElevatorType)InventorDocumentIoUtils.GetProperty(propertySet, "elevator-type", (int)ElevatorType.NOT_MULTI);
                        if(((int)elevator.type) > 7)
                        {
                            elevator.type = ElevatorType.NOT_MULTI;
                        }
                    }
                    for(int i = 0; i < InventorDocumentIoUtils.GetProperty(propertySet, "num-sensors", 0); i++)
                    {
                        RobotSensor addedSensor;
                        addedSensor = new RobotSensor((RobotSensorType)InventorDocumentIoUtils.GetProperty(propertySet, "sensorType" + i, (int)RobotSensorType.ENCODER));
                        addedSensor.portA = ((int)InventorDocumentIoUtils.GetProperty(propertySet, "sensorPortA" + i, 0));
                        addedSensor.portB = ((int)InventorDocumentIoUtils.GetProperty(propertySet, "sensorPortB" + i, 0));
                        addedSensor.conTypePortA = ((SensorConnectionType)InventorDocumentIoUtils.GetProperty(propertySet, "sensorPortConA" + i, (int)SensorConnectionType.DIO));
                        addedSensor.conTypePortB = ((SensorConnectionType)InventorDocumentIoUtils.GetProperty(propertySet, "sensorPortConB" + i, (int)SensorConnectionType.DIO));
                        addedSensor.conversionFactor = InventorDocumentIoUtils.GetProperty(propertySet, "sensorConversion" + i, 0.0);
                        joint.attachedSensors.Add(addedSensor);
                    }
                }

                // Recur along this child
                if (!LoadJointData(propertySets, child))
                    allSuccessful = false;
            }

            // Save was successful
            return allSuccessful;
        }

        /// <summary>
        /// Saves the joint information to the most recently loaded assembly file. Returns false if fails.
        /// </summary>
        /// <returns>True if all data was saved successfully.</returns>
        public bool SaveRobotData()
        {
            if (asmDocument == null)
                return false;

            if (SkeletonBase == null)
                return false;

            Inventor.PropertySets propertySets = asmDocument.PropertySets;

            // Save Robot Data
            try
            {
                // Save global robot data
                Inventor.PropertySet propertySet = InventorDocumentIoUtils.GetPropertySet(propertySets, "bxd-robotdata");

                if (RMeta.ActiveRobotName != null)
                    InventorDocumentIoUtils.SetProperty(propertySet, "robot-name", RMeta.ActiveRobotName);
                InventorDocumentIoUtils.SetProperty(propertySet, "robot-weight-kg", RMeta.TotalWeightKg * 10.0f); // x10 for better accuracy
                InventorDocumentIoUtils.SetProperty(propertySet, "robot-prefer-metric", RMeta.PreferMetric);
                InventorDocumentIoUtils.SetProperty(propertySet, "robot-driveTrainType", (int)RobotDataManager.Instance.SkeletonBase.driveTrainType);
          
                // Save joint data
                return SaveJointData(propertySets, SkeletonBase);
            }
            catch (Exception e)
            {
                MessageBox.Show("Robot data could not be save to the inventor file. The following error occured:\n" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Recursive utility for JointDataSave.
        /// </summary>
        /// <returns>True if all data was saved successfully.</returns>
        private static bool SaveJointData(Inventor.PropertySets assemblyPropertySets, RigidNode_Base currentNode)
        {
            bool allSuccessful = true;

            foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> connection in currentNode.Children)
            {
                SkeletalJoint_Base joint = connection.Key;
                RigidNode_Base child = connection.Value;

                // Name of the property set in inventor
                string setName = "bxd-jointdata-" + child.GetModelID();

                // Create the property set if it doesn't exist
                Inventor.PropertySet propertySet = InventorDocumentIoUtils.GetPropertySet(assemblyPropertySets, setName);

                // Add joint properties to set
                // Save driver information
                JointDriver driver = joint.cDriver;
                InventorDocumentIoUtils.SetProperty(propertySet, "has-driver", driver != null);
                InventorDocumentIoUtils.SetProperty(propertySet, "weight", joint.weight);
                if (driver != null)
                {
                    InventorDocumentIoUtils.SetProperty(propertySet, "driver-type", (int)driver.GetDriveType());
                    InventorDocumentIoUtils.SetProperty(propertySet, "motor-type", (int)driver.GetMotorType());
                    InventorDocumentIoUtils.SetProperty(propertySet, "driver-port1", driver.port1);
                    InventorDocumentIoUtils.SetProperty(propertySet, "driver-port2", driver.port2);
                    InventorDocumentIoUtils.SetProperty(propertySet, "driver-isCan", driver.isCan);
                    InventorDocumentIoUtils.SetProperty(propertySet, "driver-lowerLimit", driver.lowerLimit);
                    InventorDocumentIoUtils.SetProperty(propertySet, "driver-upperLimit", driver.upperLimit);
                    InventorDocumentIoUtils.SetProperty(propertySet, "driver-inputGear", driver.InputGear);// writes the input gear to the .IAM file incase the user wants to reexport their robot later
                    InventorDocumentIoUtils.SetProperty(propertySet, "driver-outputGear", driver.OutputGear);// writes the ouotput gear to the .IAM file incase the user wants to reexport their robot later
                    InventorDocumentIoUtils.SetProperty(propertySet, "driver-hasBrake", driver.hasBrake);

                    // Save other properties stored in meta
                    // Wheel information
                    WheelDriverMeta wheel = joint.cDriver.GetInfo<WheelDriverMeta>();
                    InventorDocumentIoUtils.SetProperty(propertySet, "has-wheel", wheel != null);

                    if (wheel != null)
                    {
                        InventorDocumentIoUtils.SetProperty(propertySet, "wheel-type", (int)wheel.type);
                        InventorDocumentIoUtils.SetProperty(propertySet, "wheel-isDriveWheel", wheel.isDriveWheel);
                        InventorDocumentIoUtils.SetProperty(propertySet, "wheel-frictionLevel", (int)wheel.GetFrictionLevel());
                    }

                    // Pneumatic information
                    PneumaticDriverMeta pneumatic = joint.cDriver.GetInfo<PneumaticDriverMeta>();
                    InventorDocumentIoUtils.SetProperty(propertySet, "has-pneumatic", pneumatic != null);

                    if (pneumatic != null)
                    {
                        InventorDocumentIoUtils.SetProperty(propertySet, "pneumatic-diameter", (double)pneumatic.width);
                        InventorDocumentIoUtils.SetProperty(propertySet, "pneumatic-pressure", (int)pneumatic.pressureEnum);
                    }

                    // Elevator information
                    ElevatorDriverMeta elevator = joint.cDriver.GetInfo<ElevatorDriverMeta>();



                    InventorDocumentIoUtils.SetProperty(propertySet, "has-elevator", elevator != null);

                    if (elevator != null)
                    {
                        InventorDocumentIoUtils.SetProperty(propertySet, "elevator-type", (int)elevator.type);
                    }
                }
                for (int i = 0; i < InventorDocumentIoUtils.GetProperty(propertySet, "num-sensors", 0); i++)// delete existing sensors
                {
                    InventorDocumentIoUtils.RemoveProperty(propertySet, "sensorType" + i);
                    InventorDocumentIoUtils.RemoveProperty(propertySet, "sensorPortA" + i);
                    InventorDocumentIoUtils.RemoveProperty(propertySet, "sensorPortConA" + i);
                    InventorDocumentIoUtils.RemoveProperty(propertySet, "sensorPortB" + i);
                    InventorDocumentIoUtils.RemoveProperty(propertySet, "sensorPortConB" + i);
                    InventorDocumentIoUtils.RemoveProperty(propertySet, "sensorConversion" + i);
                }
                InventorDocumentIoUtils.SetProperty(propertySet, "num-sensors", joint.attachedSensors.Count);
                for(int i = 0; i < joint.attachedSensors.Count; i++) {

                    InventorDocumentIoUtils.SetProperty(propertySet, "sensorType" + i, (int)joint.attachedSensors[i].type);
                    InventorDocumentIoUtils.SetProperty(propertySet, "sensorPortA" + i, joint.attachedSensors[i].portA);
                    InventorDocumentIoUtils.SetProperty(propertySet, "sensorPortConA" + i, (int)joint.attachedSensors[i].conTypePortA);
                    InventorDocumentIoUtils.SetProperty(propertySet, "sensorPortB" + i, joint.attachedSensors[i].portB);
                    InventorDocumentIoUtils.SetProperty(propertySet, "sensorPortConB" + i, (int)joint.attachedSensors[i].conTypePortB);
                    InventorDocumentIoUtils.SetProperty(propertySet, "sensorConversion" + i, joint.attachedSensors[i].conversionFactor);
                }

                // Recur along this child
                if (!SaveJointData(assemblyPropertySets, child))
                    allSuccessful = false;
            }

            // Save was successful
            return allSuccessful;
        }

        /// <summary>
        /// Opens the <see cref="DrivetrainWeightForm"/> form
        /// </summary>
        /// <returns>True if robot weight was changed.</returns>
        public bool PromptRobotWeight()
        {
            try
            {
                DrivetrainWeightForm weightForm = new DrivetrainWeightForm();

                weightForm.ShowDialog();

                if (weightForm.DialogResult == DialogResult.OK)
                {
                    RMeta.TotalWeightKg = weightForm.TotalWeightKg;
                    RMeta.PreferMetric = weightForm.PreferMetric;
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }

            return false;
        }

        /// <summary>
        /// Merges a node into the parent. Used during the one click export and the wizard.
        /// </summary>
        /// <param name="node"></param>
        public void MergeNodeIntoParent(RigidNode_Base node)
        {
            if (node.GetParent() == null)
                throw new ArgumentException("ERROR: Root node passed to MergeNodeIntoParent(RigidNode_Base)", "node");

            node.GetParent().ModelFullID += node.ModelFullID;

            //Get meshes for each node
            var childMesh = GetMesh(node);
            var parentMesh = GetMesh(node.GetParent());

            //Merge submeshes and colliders
            parentMesh.meshes.AddRange(childMesh.meshes);
            parentMesh.colliders.AddRange(childMesh.colliders);

            //Merge physics
            parentMesh.physics.Add(childMesh.physics.mass, childMesh.physics.centerOfMass);
        
            //Remove node from the children of its parent
            node.GetParent().Children.Remove(node.GetSkeletalJoint());
            Meshes.Remove(childMesh);
        }

        private BXDAMesh GetMesh(RigidNode_Base node)
        {
            return Meshes[SkeletonBase.ListAllNodes().IndexOf(node)];
        }
    }
}