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
        public string ActiveFieldName;
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
                ActiveFieldName = null,
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
    
    private Inventor.AssemblyDocument AsmDocument = null; // Set when LoadFieldData is called.
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
            if (RMeta.ActiveFieldName == null)
                return;

            robotName = RMeta.ActiveFieldName;
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
    /// Build the node tree of the field from Inventor
    /// </summary>
    public bool LoadFieldSkeleton()
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
    /// Load meshes of a field from Inventor
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

    public void writeLimits(RigidNode_Base skeleton)// generally, this class iterates over all the joints in the skeleton and writes the corrosponding Inventor limit into the internal joint limit
        //needed because we want to be able to pull the limits into the joint as the exporter exports, but where the joint is actually written to the .bxdj (the SimulatorAPI) is unable
        //to access InternalFieldExporterAPI or BxDRobotExporter, so writing the limits here is a workaround to that issue
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
                switch (nodes[i].GetSkeletalJoint().GetJointType())
                {
                    case SkeletalJointType.BALL:
                        break;
                    case SkeletalJointType.CYLINDRICAL:
                        ((CylindricalJoint_Base)nodes[i].GetSkeletalJoint()).hasAngularLimit = ((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.HasAngularPositionLimits;//sets whether or not the joint has angular limits based off whether or not the joint has limist
                        if (((CylindricalJoint_Base)nodes[i].GetSkeletalJoint()).hasAngularLimit)// if there are limits, write them to the file
                        {
                            ((CylindricalJoint_Base)nodes[i].GetSkeletalJoint()).angularLimitLow = (float)(((ModelParameter)((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.AngularPositionStartLimit).ModelValue);// get the JointDef from the joint and write the limits to the internal datatype
                            ((CylindricalJoint_Base)nodes[i].GetSkeletalJoint()).angularLimitHigh = (float)(((ModelParameter)((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue);// see above
                        }
                        ((CylindricalJoint_Base)nodes[i].GetSkeletalJoint()).hasLinearStartLimit = ((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.HasLinearPositionStartLimit;// see above
                        if (((CylindricalJoint_Base)nodes[i].GetSkeletalJoint()).hasLinearStartLimit)// see above
                        {
                            ((CylindricalJoint_Base)nodes[i].GetSkeletalJoint()).linearLimitStart = (float)(((ModelParameter)((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.LinearPositionStartLimit).ModelValue);// see above
                        }
                        ((CylindricalJoint_Base)nodes[i].GetSkeletalJoint()).hasLinearEndLimit = ((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.HasLinearPositionEndLimit;// see above
                        if (((CylindricalJoint_Base)nodes[i].GetSkeletalJoint()).hasLinearEndLimit)// see above
                        {
                            ((CylindricalJoint_Base)nodes[i].GetSkeletalJoint()).linearLimitEnd = (float)(((ModelParameter)((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.LinearPositionEndLimit).ModelValue);// see above
                        }
                        break;
                    case SkeletalJointType.LINEAR:
                        ((LinearJoint_Base)nodes[i].GetSkeletalJoint()).hasLowerLimit = ((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.HasLinearPositionStartLimit;// see cylindrical joint above
                        if (((LinearJoint_Base)nodes[i].GetSkeletalJoint()).hasLowerLimit)// see cylindrical joint above
                        {
                            ((LinearJoint_Base)nodes[i].GetSkeletalJoint()).linearLimitLow = (float)(((ModelParameter)((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.LinearPositionStartLimit).ModelValue);// see cylindrical joint above
                            ((ModelParameter)((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.LinearPosition).Value = ((ModelParameter)((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.LinearPositionEndLimit).ModelValue;
                            
                        }
                        ((LinearJoint_Base)nodes[i].GetSkeletalJoint()).hasUpperLimit = ((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.HasLinearPositionEndLimit;// see cylindrical joint above
                        if (((LinearJoint_Base)nodes[i].GetSkeletalJoint()).hasUpperLimit)// see cylindrical joint above
                        {
                            ((LinearJoint_Base)nodes[i].GetSkeletalJoint()).linearLimitHigh = (float)(((ModelParameter)((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.LinearPositionEndLimit).ModelValue);// see cylindrical joint above
                        }
                        break;
                    case SkeletalJointType.PLANAR:
                        break;
                    case SkeletalJointType.ROTATIONAL:
                        ((RotationalJoint_Base)nodes[i].GetSkeletalJoint()).hasAngularLimit = ((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.HasAngularPositionLimits;// see cylindrical joint above
                        if (((RotationalJoint_Base)nodes[i].GetSkeletalJoint()).hasAngularLimit)// see cylindrical joint above
                        {
                            ((RotationalJoint_Base)nodes[i].GetSkeletalJoint()).angularLimitLow = (float)(((ModelParameter)((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.AngularPositionStartLimit).ModelValue);// see cylindrical joint above
                            ((RotationalJoint_Base)nodes[i].GetSkeletalJoint()).angularLimitHigh = (float)(((ModelParameter)((InventorSkeletalJoint)nodes[i].GetSkeletalJoint()).GetWrapped().asmJoint.AngularPositionEndLimit).ModelValue);// see cylindrical joint above
                        }
                        break;
                    default:
                        throw new Exception("Could not determine type of joint");
                }
            }
        }
        foreach (ComponentOccurrence component in InventorManager.Instance.ComponentOccurrences.OfType<ComponentOccurrence>().ToList())
        {
            Exporter.BringJointsToStart(component);
        }
    }
    /// <summary>
    /// Saves the field to the directory it was loaded from or the default directory
    /// </summary>
    /// <returns></returns>
    public bool ExportField()
    {
        try
        {
            writeLimits(SkeletonBase);// write the limits from Inventor to the skeleton
            // If field has not been named, prompt user for information
            if (RMeta.ActiveFieldName == null)
                if (!PromptExportSettings())
                    return false;

            if (!Directory.Exists(PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveFieldName))
                Directory.CreateDirectory(PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveFieldName);

            if (Meshes == null || MeshesAreColored != PluginSettings.GeneralUseFancyColors) // Re-export if color settings changed
                LoadMeshes();
            BXDJSkeleton.SetupFileNames(SkeletonBase);
            BXDJSkeleton.WriteSkeleton((RMeta.UseSettingsDir && RMeta.ActiveDir != null) ? RMeta.ActiveDir : PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveFieldName + "\\skeleton.bxdj", SkeletonBase);

            for (int i = 0; i < Meshes.Count; i++)
            {
                Meshes[i].WriteToFile((RMeta.UseSettingsDir && RMeta.ActiveDir != null) ? RMeta.ActiveDir : PluginSettings.GeneralSaveLocation + "\\" + RMeta.ActiveFieldName + "\\node_" + i + ".bxda");
            }

            return true;
        }
        catch (Exception e)
        {
            //TODO: Create a form that displays a simple error message with an option to expand it and view the exception info
            MessageBox.Show("Error saving field: " + e.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    #region Joint Data Management
    /// <summary>
    /// Loads the joint information from the Inventor assembly file. Returns false if fails.
    /// </summary>
    /// <param name="asmDocument">Assembly document to load data from. Data will be saved to this document when <see cref="SaveFieldData"/> is called.</param>
    /// <returns>True if all data was loaded successfully.</returns>
    public bool LoadFieldData(Inventor.AssemblyDocument asmDocument)
    {
        if (asmDocument == null)
            return false;

        if (SkeletonBase == null)
            return false;

        AsmDocument = asmDocument;
        Inventor.PropertySets propertySets = asmDocument.PropertySets;

        // Load Field Data
        try
        {
            // Load global field data
            Inventor.PropertySet propertySet = Utilities.GetPropertySet(propertySets, "fielddata", false);
            
            if (propertySet != null)
            {
                RMeta.ActiveFieldName = Utilities.GetProperty(propertySet, "field-name", "");
                RMeta.TotalWeightKg = Utilities.GetProperty(propertySet, "field-weight-kg", 0) / 10.0f; // Stored at x10 for better accuracy
                RMeta.PreferMetric = Utilities.GetProperty(propertySet, "field-prefer-metric", false);
            }

            // Load joint data
            return LoadJointData(propertySets, SkeletonBase) && (propertySet != null);
        }
        catch (Exception e)
        {
            MessageBox.Show("Field data could not be loaded from the inventor file. The following error occured:\n" + e.Message);
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
                joint.cDriver.InputGear = Utilities.GetProperty(propertySet, "driver-inputGear", 0.0f);// writes the gearing that the user last had in the exporter to the current gearing value
                joint.cDriver.OutputGear = Utilities.GetProperty(propertySet, "driver-outputGear", 0.0f);// writes the gearing that the user last had in the exporter to the current gearing value
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
    public bool SaveFieldData()
    {
        if (AsmDocument == null)
            return false;

        if (SkeletonBase == null)
            return false;

        Inventor.PropertySets propertySets = AsmDocument.PropertySets;

        // Save Field Data
        try
        {
            // Save global field data
            Inventor.PropertySet propertySet = Utilities.GetPropertySet(propertySets, "fielddata");

            if (RMeta.ActiveFieldName != null)
                Utilities.SetProperty(propertySet, "field-name", RMeta.ActiveFieldName);
            Utilities.SetProperty(propertySet, "field-weight-kg", RMeta.TotalWeightKg * 10.0f); // x10 for better accuracy
            Utilities.SetProperty(propertySet, "field-prefer-metric", RMeta.PreferMetric);

            // Save joint data
            return SaveJointData(propertySets, SkeletonBase);
        }
        catch (Exception e)
        {
            MessageBox.Show("Field data could not be save to the inventor file. The following error occured:\n" + e.Message);
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
                Utilities.SetProperty(propertySet, "driver-inputGear", driver.InputGear);// writes the input gear to the .IAM file incase the user wants to reexport their field later
                Utilities.SetProperty(propertySet, "driver-outputGear", driver.OutputGear);// writes the ouotput gear to the .IAM file incase the user wants to reexport their field later

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
                Description = "Select a Field Folder"
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
        DialogResult overwriteResult = MessageBox.Show("Overwrite existing field?", "Overwrite Warning", MessageBoxButtons.YesNo);

        return overwriteResult == DialogResult.Yes;
    }

    /// <summary>
    /// Reload all panels with newly loaded field data
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
    /// <returns>True if field weight was changed.</returns>
    public bool PromptFieldWeight()
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
    //public void PreviewRobot(string settingsDir = null)
    //{
    //    if(RMeta.ActiveDir != null)
    //    {
    //        Process.Start(Utilities.VIEWER_PATH, "-path \"" + RMeta.ActiveDir + "\"");
    //    }
    //    else
    //    {
    //        Process.Start(Utilities.VIEWER_PATH, "-path \"" + settingsDir + "\\" + RMeta.ActiveFieldName + "\"");
    //    }
    //}

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

    //private void HelpTutorials_Click(object sender, EventArgs e)
    //{
    //    Process.Start("http://bxd.autodesk.com/synthesis/tutorials-robot.html");
    //}

    #endregion

}