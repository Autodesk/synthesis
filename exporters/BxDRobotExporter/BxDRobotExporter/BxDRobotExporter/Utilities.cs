using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EditorsLibrary;
using Inventor;
using OGLViewer;

namespace BxDRobotExporter
{
    internal static class Utilities
    {
        public const string SYNTHESIS_PATH = @"C:\Program Files\Autodesk\Synthesis\Synthesis\Synthesis.exe";

        static internal SynthesisGUI GUI;
        static DockableWindow EmbededJointPane;
        public static DockableWindow EmbededPrecheckPane;

                
        /// <summary>
        /// Creates a <see cref="DockableWindow"/> containing all of the components of the SynthesisGUI object
        /// </summary>
        /// <param name="app"></param>
        public static void CreateDockableWindows(Inventor.Application app)
        {

            UserInterfaceManager uiMan = app.UserInterfaceManager;
            EmbededJointPane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:JointEditor", "Advanced Robot Joint Editor");
            
            #region EmbededJointPane
            EmbededJointPane.DockingState = DockingStateEnum.kDockBottom;
            EmbededJointPane.Height = 250;
            EmbededJointPane.ShowVisibilityCheckBox = false;
            EmbededJointPane.ShowTitleBar = true;
            StandardAddInServer.Instance.advancedJointEditor = new JointEditorPane();

            StandardAddInServer.Instance.advancedJointEditor.SetSkeleton(GUI.SkeletonBase);
            StandardAddInServer.Instance.advancedJointEditor.SelectedJoint += nodes => InventorUtils.FocusAndHighlightNodes(nodes, StandardAddInServer.Instance.MainApplication.ActiveView.Camera,  1);
            StandardAddInServer.Instance.advancedJointEditor.ModifiedJoint += delegate (List<RigidNode_Base> nodes)
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
            EmbededJointPane.AddChild(StandardAddInServer.Instance.advancedJointEditor.Handle);
            #endregion
            
            EmbededJointPane.Visible = true;
            
            EmbededPrecheckPane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:PrecheckPane", "Robot Export Guide");
            
            EmbededPrecheckPane.DockingState = DockingStateEnum.kDockRight;
            EmbededPrecheckPane.Width = 600;
            EmbededPrecheckPane.ShowVisibilityCheckBox = false;
            EmbededPrecheckPane.ShowTitleBar = true;
            var precheckPanel = new PrecheckPanel.PrecheckPanel();
            EmbededPrecheckPane.AddChild(precheckPanel.Handle);
            
            EmbededPrecheckPane.Visible = true;
        }

        public static void CreateChildDialog()
        {
            try
            {
                GUI = new SynthesisGUI(StandardAddInServer.Instance.MainApplication)// pass the main application to the GUI so classes RobotExporter can access Inventor to read the joints
                {
                    Opacity = 0.00d
                };
                GUI.Show();
                GUI.Hide();
                GUI.Opacity = 1.00d;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Disposes of the dockable windows.
        /// </summary>
        public static void DisposeDockableWindows()
        {
            if (EmbededJointPane != null)
            {
                EmbededJointPane.Visible = false;
                EmbededJointPane.Delete();
            }
            if (EmbededPrecheckPane != null)
            {
                EmbededPrecheckPane.Visible = false;
                EmbededPrecheckPane.Delete();
            }
        }

        public static bool IsAdvancedJointEditorVisible()
        {
            if (EmbededJointPane == null) return false;
            return EmbededJointPane.Visible;
        }

        public static void ToggleAdvancedJointEditor()
        {
            if (EmbededJointPane != null)
            {
                if (IsAdvancedJointEditorVisible())
                {
                    HideAdvancedJointEditor();
                }
                else
                {
                    ShowAdvancedJointEditor();
                }
            }
        }
        /// <summary>
        /// Hides the dockable windows. Used when switching documents. Called in <see cref="StandardAddInServer.ApplicationEvents_OnDeactivateDocument(_Document, EventTimingEnum, NameValueMap, out HandlingCodeEnum)"/>.
        /// </summary>
        public static void HideAdvancedJointEditor() // TODO: Figure out how to call this when the advanced editor tab is closed manually (Inventor API)
        {
            if (EmbededJointPane != null)
            {
                EmbededJointPane.Visible = false;
                InventorUtils.FocusAndHighlightNodes(null, StandardAddInServer.Instance.MainApplication.ActiveView.Camera, 1);
            }
        }

        /// <summary>
        /// Shows the dockable windows again when assembly document is switched back to. Called in <see cref="StandardAddInServer.ApplicationEvents_OnActivateDocument(_Document, EventTimingEnum, NameValueMap, out HandlingCodeEnum)"/>.
        /// </summary>
        public static void ShowAdvancedJointEditor()
        {
            if (EmbededJointPane != null)
            {
                EmbededJointPane.Visible = true;
            }
        }

        /// <summary>
        /// Converts from a <see cref="System.Drawing.Color"/> to an <see cref="Inventor.Color"/>
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Inventor.Color GetInventorColor(System.Drawing.Color color)
        {
            return StandardAddInServer.Instance.MainApplication.TransientObjects.CreateColor(color.R, color.G, color.B);
        }

        /// <summary>
        /// Initializes all of the <see cref="SynthesisGUI"/> settings to the proper values. Should be called once in the Activate class
        /// </summary>
        public static void LoadSettings()
        {
            // Old configurations get overriden (version numbers below 1)
            if (Properties.Settings.Default.SaveLocation == "" || Properties.Settings.Default.SaveLocation == "firstRun" || Properties.Settings.Default.ConfigVersion < 2)
                Properties.Settings.Default.SaveLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Robots";

            if (Properties.Settings.Default.ConfigVersion < 3)
            {
                SynthesisGUI.PluginSettings = EditorsLibrary.PluginSettingsForm.Values = new EditorsLibrary.PluginSettingsForm.PluginSettingsValues
                {
                    InventorChildColor = Properties.Settings.Default.ChildColor,
                    GeneralSaveLocation = Properties.Settings.Default.SaveLocation,
                    GeneralUseFancyColors = Properties.Settings.Default.FancyColors,
                    openSynthesis = Properties.Settings.Default.ExportToField,
                    fieldName = Properties.Settings.Default.SelectedField,
                    defaultRobotCompetition = "GENERIC",
                    useAnalytics = true
                };
            }
            else
            {
                SynthesisGUI.PluginSettings = EditorsLibrary.PluginSettingsForm.Values = new EditorsLibrary.PluginSettingsForm.PluginSettingsValues
                {
                    InventorChildColor = Properties.Settings.Default.ChildColor,
                    GeneralSaveLocation = Properties.Settings.Default.SaveLocation,
                    GeneralUseFancyColors = Properties.Settings.Default.FancyColors,
                    openSynthesis = Properties.Settings.Default.ExportToField,
                    fieldName = Properties.Settings.Default.SelectedField,
                    defaultRobotCompetition = Properties.Settings.Default.DefaultRobotCompetition,
                    useAnalytics = Properties.Settings.Default.UseAnalytics
                };
            }
        }


        // Analytics

        
    }
}