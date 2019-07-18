using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EditorsLibrary;
using JointResolver.ControlGUI;
using OGLViewer;
using Inventor;

public delegate bool ValidationAction(RigidNode_Base baseNode, out string message);

[Guid("ec18f8d4-c13e-4c86-8148-7414efb6e1e2")]
public partial class SynthesisGUI : Form
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
        private float _totalWeightKg;
        public float TotalWeightKg
        {
            get => _totalWeightKg;
            set => _totalWeightKg = (value > 0) ? value : 0; // Prevent negative weight values
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

    public static SynthesisGUI Instance;

    public static PluginSettingsForm.PluginSettingsValues PluginSettings;

    public Form JointPaneForm = new Form
    {
        FormBorderStyle = FormBorderStyle.None
    };
    
    private Inventor.AssemblyDocument AsmDocument = null; // Set when LoadRobotData is called.
    public RigidNode_Base SkeletonBase = null;
    public List<BXDAMesh> Meshes = null;
    public bool MeshesAreColored = false;
    public Inventor.Application MainApplication;
    private SkeletonExporterForm skeletonExporter;
    private LiteExporterForm liteExporter;

    public SynthesisGUI(Inventor.Application MainApplication, bool MakeOwners = false)
    {
        InitializeComponent();
        this.MainApplication = MainApplication;
        Instance = this;
        JointPaneForm.Controls.Add(jointEditorPane1);
        if (MakeOwners) JointPaneForm.Owner = this;
        JointPaneForm.FormClosing += Generic_FormClosing;
        
        RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
        {
            return new OGL_RigidNode(guid);
        };

        settingsExporter.Click += SettingsExporter_OnClick;

        Shown += SynthesisGUI_Shown;

        FormClosing += new FormClosingEventHandler(delegate (object sender, FormClosingEventArgs e)
        {
            InventorManager.ReleaseInventor();
        });

        jointEditorPane1.ModifiedJoint += delegate (List<RigidNode_Base> nodes)
        {

            if (nodes == null || nodes.Count == 0) return;

            foreach (RigidNode_Base node in nodes)
            {
                if (node.GetSkeletalJoint() != null && node.GetSkeletalJoint().cDriver != null &&
                    node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>() != null &&
                    node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().radius == 0 &&
                    node is OGL_RigidNode)
                {
                    (node as OGL_RigidNode).GetWheelInfo(out float radius, out float width, out BXDVector3 center);

                    WheelDriverMeta wheelDriver = node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>();
                    wheelDriver.center = center;
                    wheelDriver.radius = radius;
                    wheelDriver.width = width;
                    node.GetSkeletalJoint().cDriver.AddInfo(wheelDriver);

                }
            }
        };
    }

    private void Generic_FormClosing(object sender, FormClosingEventArgs e)
    {
        foreach (Form f in OwnedForms)
        {
            if(f.Visible)
                f.Close();
        }
        Close();
    }

    private void SynthesisGUI_Shown(object sender, EventArgs e)
    {
        Hide();
        JointPaneForm.Show();
    }

    /// <summary>
    /// Removes all configuration from the current skeleton.
    /// </summary>
    public void ClearConfiguration()
    {
        ClearConfiguration(SkeletonBase);
    }
    /// <summary>
    /// Removes all configuration from the current skeleton (recursive utility).
    /// </summary>
    private void ClearConfiguration(RigidNode_Base baseNode)
    {
        SkeletalJoint_Base joint = baseNode.GetSkeletalJoint();

        if (joint != null)
            joint.ClearConfiguration();

        foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> child in baseNode.Children)
            ClearConfiguration(child.Value);
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
        
        Process.Start(Utilities.SYNTHESIS_PATH, string.Format("-robot \"{0}\" -field \"{1}\"", PluginSettings.GeneralSaveLocation + "\\" + robotName, fieldName));
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
                skeletonExporter = new SkeletonExporterForm();
                skeletonExporter.ShowDialog();
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
    public bool LoadMeshes()
    {
        try
        {
            var exporterThread = new Thread(() =>
            {
                if (SkeletonBase == null)
                {
                    skeletonExporter = new SkeletonExporterForm();
                    skeletonExporter.ShowDialog();
                }

                if (SkeletonBase != null)
                {
#if LITEMODE
                    liteExporter = new LiteExporterForm();
                    liteExporter.ShowDialog(); // Remove node building
#else
                    exporter = new ExporterForm(PluginSettings);
                    exporter.ShowDialog();
#endif
                }
            });

            exporterThread.SetApartmentState(ApartmentState.STA);
            exporterThread.Start();

            exporterThread.Join();

            GC.Collect();

            MeshesAreColored = PluginSettings.GeneralUseFancyColors;
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
        if (ExportRobotForm.Prompt(RMeta.ActiveRobotName, out string robotName, out bool colors, out bool openSynthesis, out string field) == DialogResult.OK)
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
    public void WriteLimits(RigidNode_Base skeleton)
    {
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        skeleton.ListAllNodes(nodes);
        int[] parentID = new int[nodes.Count];

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].GetParent() != null)
            {
                parentID[i] = nodes.IndexOf(nodes[i].GetParent());

                if (parentID[i] < 0) throw new Exception("Can't resolve parent ID for " + nodes[i].ToString());
            }
            else
            {
                parentID[i] = -1;
            }
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            if (parentID[i] >= 0)
            {
                InventorSkeletalJoint inventorJoint = nodes[i].GetSkeletalJoint() as InventorSkeletalJoint;
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

            if (Meshes == null || MeshesAreColored != PluginSettings.GeneralUseFancyColors) // Re-export if color settings changed
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

    #region Joint Data Management
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

        AsmDocument = asmDocument;
        Inventor.PropertySets propertySets = asmDocument.PropertySets;

        // Load Robot Data
        try
        {
            // Load global robot data
            Inventor.PropertySet propertySet = Utilities.GetPropertySet(propertySets, "bxd-robotdata", false);
            
            if (propertySet != null)
            {
                RMeta.ActiveRobotName = Utilities.GetProperty(propertySet, "robot-name", "");
                RMeta.TotalWeightKg = Utilities.GetProperty(propertySet, "robot-weight-kg", 0) / 10.0f; // Stored at x10 for better accuracy
                RMeta.PreferMetric = Utilities.GetProperty(propertySet, "robot-prefer-metric", false);
                SynthesisGUI.Instance.SkeletonBase.driveTrainType = (RigidNode_Base.DriveTrainType)Utilities.GetProperty(propertySet, "robot-driveTrainType", (int)RigidNode_Base.DriveTrainType.NONE);
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
    public bool LoadJointData(Inventor.PropertySets propertySets, RigidNode_Base currentNode)
    {
        bool allSuccessful = true;

        foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> connection in currentNode.Children)
        {
            SkeletalJoint_Base joint = connection.Key;
            RigidNode_Base child = connection.Value;

            // Name of the property set in inventor
            string setName = "bxd-jointdata-" + child.GetModelID();

            // Attempt to open the property set
            Inventor.PropertySet propertySet = Utilities.GetPropertySet(propertySets, setName, false);

            // If the property set does not exist, stop loading data
            if (propertySet == null)
                return false;

            joint.weight = Utilities.GetProperty(propertySet, "weight", 10);

            // Get joint properties from set
            // Get driver information
            if (Utilities.GetProperty(propertySet, "has-driver", false))
            {
                if (joint.cDriver == null)
                    joint.cDriver = new JointDriver((JointDriverType)Utilities.GetProperty(propertySet, "driver-type", (int)JointDriverType.MOTOR));
                JointDriver driver = joint.cDriver;

                joint.cDriver.motor = (MotorType)Utilities.GetProperty(propertySet, "motor-type", (int)MotorType.GENERIC);
                joint.cDriver.port1 = Utilities.GetProperty(propertySet, "driver-port1", 0);
                joint.cDriver.port2 = Utilities.GetProperty(propertySet, "driver-port2", -1);
                joint.cDriver.isCan = Utilities.GetProperty(propertySet, "driver-isCan", false);
                joint.cDriver.lowerLimit = Utilities.GetProperty(propertySet, "driver-lowerLimit", 0.0f);
                joint.cDriver.upperLimit = Utilities.GetProperty(propertySet, "driver-upperLimit", 0.0f);
                joint.cDriver.InputGear = Utilities.GetProperty(propertySet, "driver-inputGear", 0.0f);// writes the gearing that the user last had in the exporter to the current gearing value
                joint.cDriver.OutputGear = Utilities.GetProperty(propertySet, "driver-outputGear", 0.0f);// writes the gearing that the user last had in the exporter to the current gearing value
                joint.cDriver.hasBrake = Utilities.GetProperty(propertySet, "driver-hasBrake", false);

                // Get other properties stored in meta
                // Wheel information
                if (Utilities.GetProperty(propertySet, "has-wheel", false))
                {
                    if (driver.GetInfo<WheelDriverMeta>() == null)
                        driver.AddInfo(new WheelDriverMeta());
                    WheelDriverMeta wheel = joint.cDriver.GetInfo<WheelDriverMeta>();

                    wheel.type = (WheelType)Utilities.GetProperty(propertySet, "wheel-type", (int)WheelType.NORMAL);
                    wheel.isDriveWheel = Utilities.GetProperty(propertySet, "wheel-isDriveWheel", false);
                    wheel.SetFrictionLevel((FrictionLevel)Utilities.GetProperty(propertySet, "wheel-frictionLevel", (int)FrictionLevel.MEDIUM));
                }

                // Pneumatic information
                if (Utilities.GetProperty(propertySet, "has-pneumatic", false))
                {
                    if (driver.GetInfo<PneumaticDriverMeta>() == null)
                        driver.AddInfo(new PneumaticDriverMeta());
                    PneumaticDriverMeta pneumatic = joint.cDriver.GetInfo<PneumaticDriverMeta>();

                    pneumatic.width = Utilities.GetProperty(propertySet, "pneumatic-diameter", (double)0.5);
                    pneumatic.pressureEnum = (PneumaticPressure)Utilities.GetProperty(propertySet, "pneumatic-pressure", (int)PneumaticPressure.MEDIUM);
                }

                // Elevator information
                if (Utilities.GetProperty(propertySet, "has-elevator", false))
                {
                    if (driver.GetInfo<ElevatorDriverMeta>() == null)
                        driver.AddInfo(new ElevatorDriverMeta());
                    ElevatorDriverMeta elevator = joint.cDriver.GetInfo<ElevatorDriverMeta>();

                    elevator.type = (ElevatorType)Utilities.GetProperty(propertySet, "elevator-type", (int)ElevatorType.NOT_MULTI);
                    if(((int)elevator.type) > 7)
                    {
                        elevator.type = ElevatorType.NOT_MULTI;
                    }
                }
                for(int i = 0; i < Utilities.GetProperty(propertySet, "num-sensors", 0); i++)
                {
                    RobotSensor addedSensor;
                    addedSensor = new RobotSensor((RobotSensorType)Utilities.GetProperty(propertySet, "sensorType" + i, (int)RobotSensorType.ENCODER));
                    addedSensor.portA = ((int)Utilities.GetProperty(propertySet, "sensorPortA" + i, 0));
                    addedSensor.portB = ((int)Utilities.GetProperty(propertySet, "sensorPortB" + i, 0));
                    addedSensor.conTypePortA = ((SensorConnectionType)Utilities.GetProperty(propertySet, "sensorPortConA" + i, (int)SensorConnectionType.DIO));
                    addedSensor.conTypePortB = ((SensorConnectionType)Utilities.GetProperty(propertySet, "sensorPortConB" + i, (int)SensorConnectionType.DIO));
                    addedSensor.conversionFactor = Utilities.GetProperty(propertySet, "sensorConversion" + i, 0.0);
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
        if (AsmDocument == null)
            return false;

        if (SkeletonBase == null)
            return false;

        Inventor.PropertySets propertySets = AsmDocument.PropertySets;

        // Save Robot Data
        try
        {
            // Save global robot data
            Inventor.PropertySet propertySet = Utilities.GetPropertySet(propertySets, "bxd-robotdata");

            if (RMeta.ActiveRobotName != null)
                Utilities.SetProperty(propertySet, "robot-name", RMeta.ActiveRobotName);
            Utilities.SetProperty(propertySet, "robot-weight-kg", RMeta.TotalWeightKg * 10.0f); // x10 for better accuracy
            Utilities.SetProperty(propertySet, "robot-prefer-metric", RMeta.PreferMetric);
            Utilities.SetProperty(propertySet, "robot-driveTrainType", (int)SynthesisGUI.Instance.SkeletonBase.driveTrainType);
          
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
    private bool SaveJointData(Inventor.PropertySets assemblyPropertySets, RigidNode_Base currentNode)
    {
        bool allSuccessful = true;

        foreach (KeyValuePair<SkeletalJoint_Base, RigidNode_Base> connection in currentNode.Children)
        {
            SkeletalJoint_Base joint = connection.Key;
            RigidNode_Base child = connection.Value;

            // Name of the property set in inventor
            string setName = "bxd-jointdata-" + child.GetModelID();

            // Create the property set if it doesn't exist
            Inventor.PropertySet propertySet = Utilities.GetPropertySet(assemblyPropertySets, setName);

            // Add joint properties to set
            // Save driver information
            JointDriver driver = joint.cDriver;
            Utilities.SetProperty(propertySet, "has-driver", driver != null);
            Utilities.SetProperty(propertySet, "weight", joint.weight);
            if (driver != null)
            {
                Utilities.SetProperty(propertySet, "driver-type", (int)driver.GetDriveType());
                Utilities.SetProperty(propertySet, "motor-type", (int)driver.GetMotorType());
                Utilities.SetProperty(propertySet, "driver-port1", driver.port1);
                Utilities.SetProperty(propertySet, "driver-port2", driver.port2);
                Utilities.SetProperty(propertySet, "driver-isCan", driver.isCan);
                Utilities.SetProperty(propertySet, "driver-lowerLimit", driver.lowerLimit);
                Utilities.SetProperty(propertySet, "driver-upperLimit", driver.upperLimit);
                Utilities.SetProperty(propertySet, "driver-inputGear", driver.InputGear);// writes the input gear to the .IAM file incase the user wants to reexport their robot later
                Utilities.SetProperty(propertySet, "driver-outputGear", driver.OutputGear);// writes the ouotput gear to the .IAM file incase the user wants to reexport their robot later
                Utilities.SetProperty(propertySet, "driver-hasBrake", driver.hasBrake);

                // Save other properties stored in meta
                // Wheel information
                WheelDriverMeta wheel = joint.cDriver.GetInfo<WheelDriverMeta>();
                Utilities.SetProperty(propertySet, "has-wheel", wheel != null);

                if (wheel != null)
                {
                    Utilities.SetProperty(propertySet, "wheel-type", (int)wheel.type);
                    Utilities.SetProperty(propertySet, "wheel-isDriveWheel", wheel.isDriveWheel);
                    Utilities.SetProperty(propertySet, "wheel-frictionLevel", (int)wheel.GetFrictionLevel());
                }

                // Pneumatic information
                PneumaticDriverMeta pneumatic = joint.cDriver.GetInfo<PneumaticDriverMeta>();
                Utilities.SetProperty(propertySet, "has-pneumatic", pneumatic != null);

                if (pneumatic != null)
                {
                    Utilities.SetProperty(propertySet, "pneumatic-diameter", (double)pneumatic.width);
                    Utilities.SetProperty(propertySet, "pneumatic-pressure", (int)pneumatic.pressureEnum);
                }

                // Elevator information
                ElevatorDriverMeta elevator = joint.cDriver.GetInfo<ElevatorDriverMeta>();



                Utilities.SetProperty(propertySet, "has-elevator", elevator != null);

                if (elevator != null)
                {
                    Utilities.SetProperty(propertySet, "elevator-type", (int)elevator.type);
                }
            }
            for (int i = 0; i < Utilities.GetProperty(propertySet, "num-sensors", 0); i++)// delete existing sensors
            {
                Utilities.RemoveProperty(propertySet, "sensorType" + i);
                Utilities.RemoveProperty(propertySet, "sensorPortA" + i);
                Utilities.RemoveProperty(propertySet, "sensorPortConA" + i);
                Utilities.RemoveProperty(propertySet, "sensorPortB" + i);
                Utilities.RemoveProperty(propertySet, "sensorPortConB" + i);
                Utilities.RemoveProperty(propertySet, "sensorConversion" + i);
            }
            Utilities.SetProperty(propertySet, "num-sensors", joint.attachedSensors.Count);
            for(int i = 0; i < joint.attachedSensors.Count; i++) {

                Utilities.SetProperty(propertySet, "sensorType" + i, (int)joint.attachedSensors[i].type);
                Utilities.SetProperty(propertySet, "sensorPortA" + i, joint.attachedSensors[i].portA);
                Utilities.SetProperty(propertySet, "sensorPortConA" + i, (int)joint.attachedSensors[i].conTypePortA);
                Utilities.SetProperty(propertySet, "sensorPortB" + i, joint.attachedSensors[i].portB);
                Utilities.SetProperty(propertySet, "sensorPortConB" + i, (int)joint.attachedSensors[i].conTypePortB);
                Utilities.SetProperty(propertySet, "sensorConversion" + i, joint.attachedSensors[i].conversionFactor);
            }

            // Recur along this child
            if (!SaveJointData(assemblyPropertySets, child))
                allSuccessful = false;
        }

        // Save was successful
        return allSuccessful;
    }
    #endregion

    /// <summary>
    /// Get the desired folder to open from or save to
    /// </summary>
    /// <returns>The full path of the selected folder</returns>
    private string OpenFolderPath()
    {
        string dirPath = null;

        var dialogThread = new Thread(() =>
        {
            FolderBrowserDialog openDialog = new FolderBrowserDialog()
            {
                Description = "Select a Robot Folder"
            };
            DialogResult openResult = openDialog.ShowDialog();

            if (openResult == DialogResult.OK) dirPath = openDialog.SelectedPath;
        });

        dialogThread.SetApartmentState(ApartmentState.STA);
        dialogThread.Start();
        dialogThread.Join();

        return dirPath;
    }

    /// <summary>
    /// Warn the user that they are about to overwrite existing data
    /// </summary>
    /// <returns>Whether the user wishes to overwrite the data</returns>
    private bool WarnOverwrite()
    {
        DialogResult overwriteResult = MessageBox.Show("Overwrite existing robot?", "Overwrite Warning", MessageBoxButtons.YesNo);

        return overwriteResult == DialogResult.Yes;
    }

    /// <summary>
    /// Reload all panels with newly loaded robot data
    /// </summary>
    public void ReloadPanels()
    {
        jointEditorPane1.SetSkeleton(SkeletonBase);
    }

    protected override void OnResize(EventArgs e)
    {
        SuspendLayout();

        base.OnResize(e);
        splitContainer1.Height = ClientSize.Height - 27;

        ResumeLayout();
    }

    /// <summary>
    /// Opens the <see cref="SetWeightForm"/> form
    /// </summary>
    /// <returns>True if robot weight was changed.</returns>
    public bool PromptRobotWeight()
    {
        try
        {
            SetWeightForm weightForm = new SetWeightForm();

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
    /// Opens the <see cref="PluginSettingsForm"/> form
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void SettingsExporter_OnClick(object sender, System.EventArgs e)
    {
        try
        {
            //TODO: Implement Value saving and loading
            PluginSettingsForm eSettingsForm = new PluginSettingsForm();
    
            eSettingsForm.ShowDialog();
    
            //BXDSettings.Instance.AddSettingsObject("Plugin Settings", ExporterSettingsForm.values);
            PluginSettings = PluginSettingsForm.Values;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
            throw;
        }
    }

    /// <summary>
    /// Runs the standalone Robot Viewer with and tells it to view the current robot
    /// </summary>
    /// <param name="settingsDir"></param>
    public void PreviewRobot(string settingsDir = null)
    {
        if(RMeta.ActiveDir != null)
        {
            Process.Start(Utilities.VIEWER_PATH, "-path \"" + RMeta.ActiveDir + "\"");
        }
        else
        {
            Process.Start(Utilities.VIEWER_PATH, "-path \"" + settingsDir + "\\" + RMeta.ActiveRobotName + "\"");
        }
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

    #region OUTDATED EXPORTER METHODS
    /// <summary>
    /// Reset the <see cref="ExporterProgressWindow"/> progress bar
    /// </summary>
    public void ExporterReset()
    {
        exporter.ResetProgress();
    }

    public void ExporterOverallReset()
    {
        exporter.ResetOverall();
    }

    /// <summary>
    /// Set the length of the <see cref="ExporterProgressWindow"/> progress bar
    /// </summary>
    /// <param name="percentLength">The length of the bar in percentage points (0%-100%)</param>
    public void ExporterSetProgress(double percentLength)
    {
        exporter.AddProgress((int)Math.Floor(percentLength) - exporter.GetProgress());
    }

    /// <summary>
    /// Set the <see cref="ExporterProgressWindow"/> text after "Progress:"
    /// </summary>
    /// <param name="text">The text to add</param>
    public void ExporterSetSubText(string text)
    {
        exporter.SetProgressText(text);
    }

    public void ExporterSetMeshes(int num)
    {
        exporter.SetNumMeshes(num);
    }

    public void ExporterStepOverall()
    {
        exporter.AddOverallStep();
    }

    public void ExporterSetOverallText(string text)
    {
        exporter.SetOverallText(text);
    }

    private ExporterForm exporter = null;

    private void HelpTutorials_Click(object sender, EventArgs e)
    {
        Process.Start("http://bxd.autodesk.com/synthesis/tutorials-robot.html");
    }

    #endregion

}