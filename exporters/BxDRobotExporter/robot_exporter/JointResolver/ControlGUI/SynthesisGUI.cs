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
        public string FieldName;

        public static RuntimeMeta CreateRuntimeMeta()
        {
            return new RuntimeMeta
            {
                UseSettingsDir = true,
                ActiveDir = null,
                ActiveRobotName = null,
                FieldName = null
            };
        }
    }

    public RuntimeMeta RMeta = RuntimeMeta.CreateRuntimeMeta();

    public static SynthesisGUI Instance;

    public static PluginSettingsForm.PluginSettingsValues PluginSettings;

    public Form BXDAViewerPaneForm = new Form
    {
        FormBorderStyle = FormBorderStyle.None
    };
    public Form JointPaneForm = new Form
    {
        FormBorderStyle = FormBorderStyle.None
    };
    
    private Inventor.AssemblyDocument AsmDocument = null; // Set when LoadRobotData is called.
    public RigidNode_Base SkeletonBase = null;
    public List<BXDAMesh> Meshes = null;
    public bool MeshesAreColored = false;
    // TODO: This should be moved to RMeta
    public float TotalWeightKg = -1; // Negative value indicates default mass should be left alone

    private SkeletonExporterForm skeletonExporter;
    private LiteExporterForm liteExporter;

    static SynthesisGUI()
    {
    }

    public SynthesisGUI(bool MakeOwners = false)
    {
        InitializeComponent();

        Instance = this;

        bxdaEditorPane1.Units = "lbs";
        BXDAViewerPaneForm.Controls.Add(bxdaEditorPane1);
        if (MakeOwners) BXDAViewerPaneForm.Owner = this;
        BXDAViewerPaneForm.FormClosing += Generic_FormClosing;

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

        jointEditorPane1.SelectedJoint += bxdaEditorPane1.SelectJoints;

        bxdaEditorPane1.NodeSelected += (BXDAMesh mesh) =>
            {
                List<RigidNode_Base> nodes = new List<RigidNode_Base>();
                SkeletonBase.ListAllNodes(nodes);

                jointEditorPane1.AddSelection(nodes[Meshes.IndexOf(mesh)], true);
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
        BXDAViewerPaneForm.Show();
        JointPaneForm.Show();
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
            return true;
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

        // Export was successful
        List<RigidNode_Base> nodes = SkeletonBase.ListAllNodes();
        for (int i = 0; i < Meshes.Count; i++)
        {
            ((OGL_RigidNode)nodes[i]).loadMeshes(Meshes[i]);
        }
            
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
    /// Saves the robot to the directory it was loaded from or the default directory
    /// </summary>
    /// <returns></returns>
    public bool ExportRobot()
    {
        try
        {
            // If robot has not been named, prompt user for information
            if (RMeta.ActiveRobotName == null)
                if (!PromptExportSettings())
                    return false;

            if (!Directory.Exists(PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName))
                Directory.CreateDirectory(PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName);

            if (Meshes == null || MeshesAreColored != PluginSettings.GeneralUseFancyColors) // Re-export if color settings changed
                LoadMeshes();

            BXDJSkeleton.SetupFileNames(SkeletonBase);
            BXDJSkeleton.WriteSkeleton((RMeta.UseSettingsDir && RMeta.ActiveDir != null) ? RMeta.ActiveDir : PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveRobotName + "\\skeleton.bxdj", SkeletonBase);

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
                TotalWeightKg = Utilities.GetProperty(propertySet, "robot-weight-kg", 0);
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

            // Get joint properties from set
            // Get driver information
            if (Utilities.GetProperty(propertySet, "has-driver", false))
            {
                if (joint.cDriver == null)
                    joint.cDriver = new JointDriver((JointDriverType)Utilities.GetProperty(propertySet, "driver-type", (int)JointDriverType.MOTOR));
                JointDriver driver = joint.cDriver;

                joint.cDriver.portA = Utilities.GetProperty(propertySet, "driver-portA", 0);
                joint.cDriver.portB = Utilities.GetProperty(propertySet, "driver-portB", -1);
                joint.cDriver.isCan = Utilities.GetProperty(propertySet, "driver-isCan", false);
                joint.cDriver.lowerLimit = Utilities.GetProperty(propertySet, "driver-lowerLimit", 0.0f);
                joint.cDriver.upperLimit = Utilities.GetProperty(propertySet, "driver-upperLimit", 0.0f);

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

                    pneumatic.widthEnum = (PneumaticDiameter)Utilities.GetProperty(propertySet, "pneumatic-diameter", (int)PneumaticDiameter.MEDIUM);
                    pneumatic.pressureEnum = (PneumaticPressure)Utilities.GetProperty(propertySet, "pneumatic-pressure", (int)PneumaticPressure.MEDIUM);
                }

                // Elevator information
                if (Utilities.GetProperty(propertySet, "has-elevator", false))
                {
                    if (driver.GetInfo<ElevatorDriverMeta>() == null)
                        driver.AddInfo(new ElevatorDriverMeta());
                    ElevatorDriverMeta elevator = joint.cDriver.GetInfo<ElevatorDriverMeta>();

                    elevator.type = (ElevatorType)Utilities.GetProperty(propertySet, "elevator-type", (int)ElevatorType.NOT_MULTI);
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
            Utilities.SetProperty(propertySet, "robot-weight-kg", TotalWeightKg);

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

            if (driver != null)
            {
                Utilities.SetProperty(propertySet, "driver-type", (int)driver.GetDriveType());
                Utilities.SetProperty(propertySet, "driver-portA", driver.portA);
                Utilities.SetProperty(propertySet, "driver-portB", driver.portB);
                Utilities.SetProperty(propertySet, "driver-isCan", driver.isCan);
                Utilities.SetProperty(propertySet, "driver-lowerLimit", driver.lowerLimit);
                Utilities.SetProperty(propertySet, "driver-upperLimit", driver.upperLimit);

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
                    Utilities.SetProperty(propertySet, "pneumatic-diameter", (int)pneumatic.widthEnum);
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
        bxdaEditorPane1.loadModel(Meshes);
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
                TotalWeightKg = weightForm.TotalWeightKg;
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