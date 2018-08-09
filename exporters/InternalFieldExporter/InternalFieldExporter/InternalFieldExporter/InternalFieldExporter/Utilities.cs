using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace InternalFieldExporter
{
    internal static class Utilities
    {
        public const string SYNTHESIS_PATH = @"C:\Program Files (x86)\Autodesk\Synthesis\Synthesis\Synthesis.exe";

        static internal SynthesisGUI GUI;
        static DockableWindow EmbededJointPane;

        /// <summary>
        /// Creates a <see cref="DockableWindow"/> containing all of the components of the SynthesisGUI object
        /// </summary>
        /// <param name="app"></param>
        public static void CreateDockableWindows(Inventor.Application app)
        {
            IntPtr[] children = CreateChildDialog();

            UserInterfaceManager uiMan = app.UserInterfaceManager;
            EmbededJointPane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "InternalFieldExporter:JointEditor", "Field Joint Editor");
            
            #region EmbededJointPane
            EmbededJointPane.DockingState = DockingStateEnum.kDockBottom;
            EmbededJointPane.Height = 250;
            EmbededJointPane.ShowVisibilityCheckBox = false;
            EmbededJointPane.ShowTitleBar = true;
            EmbededJointPane.AddChild(children[0]);
            #endregion
            
            EmbededJointPane.Visible = true;
        }

        private static IntPtr[] CreateChildDialog()
        {
            try
            {
                GUI = new SynthesisGUI(StandardAddInServer.Instance.MainApplication)// pass the main application to the GUI so classes FieldExporter can access Inventor to read the joints
                {
                    Opacity = 0.00d
                };
                GUI.Show();
                GUI.Hide();
                GUI.Opacity = 1.00d;

                return new IntPtr[] { GUI.JointPaneForm.Handle };
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
        }

        /// <summary>
        /// Hides the dockable windows. Used when switching documents. Called in <see cref="StandardAddInServer.ApplicationEvents_OnDeactivateDocument(_Document, EventTimingEnum, NameValueMap, out HandlingCodeEnum)"/>.
        /// </summary>
        public static void HideDockableWindows()
        {
            if (EmbededJointPane != null)
            {
                EmbededJointPane.Visible = false;
            }
        }

        /// <summary>
        /// Shows the dockable windows again when assembly document is switched back to. Called in <see cref="StandardAddInServer.ApplicationEvents_OnActivateDocument(_Document, EventTimingEnum, NameValueMap, out HandlingCodeEnum)"/>.
        /// </summary>
        public static void ShowDockableWindows()
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
            if (Properties.Settings.Default.SaveLocation == "" || Properties.Settings.Default.SaveLocation == "firstRun" || Properties.Settings.Default.ConfigVersion < 1 )
                Properties.Settings.Default.SaveLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\Synthesis\Fields";

            SynthesisGUI.PluginSettings = EditorsLibrary.PluginSettingsForm.Values = new EditorsLibrary.PluginSettingsForm.PluginSettingsValues
            {
                InventorChildColor = Properties.Settings.Default.ChildColor,
                GeneralSaveLocation = Properties.Settings.Default.SaveLocation,
                GeneralUseFancyColors = Properties.Settings.Default.FancyColors
            };
        }

        /// <summary>
        /// Determines the position of a node using BXDVectors
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static BXDVector3 ToBXDVector(dynamic p)
        {
            return new BXDVector3(p.X, p.Y, p.Z);
        }

        public static BXDQuaternion QuaternionFromMatrix(Matrix m)
        {
            BXDQuaternion q = new BXDQuaternion();

            double tr = m.Cell[1, 1] + m.Cell[2, 2] + m.Cell[3, 3];
            double s;

            if (tr > 0)
            {
                s= Math.Sqrt(tr + 1.0) *2;
                q.W = (float)(0.25 * s);
                q.X = (float)((m.Cell[3, 2]) - (m.Cell[2, 3]) / s);
                q.Y = (float)((m.Cell[1, 3]) - (m.Cell[3, 1]) / s);
                q.Z = (float)((m.Cell[2, 1]) - (m.Cell[1, 2]) / s);
            }
            else if ((m.Cell[1, 1] > m.Cell[2, 2]) && (m.Cell[1, 1] > m.Cell[3, 3]))
            {
                s = Math.Sqrt(1.0 + m.Cell[1, 1] - m.Cell[2, 2] - m.Cell[3, 3]) * 2;
                q.W = (float)((m.Cell[3, 2] - m.Cell[2, 3]) / s);
                q.X = (float)(0.25 * s);
                q.Y = (float)((m.Cell[1, 2] + m.Cell[2, 1]) / s);
                q.Z = (float)((m.Cell[1, 3] + m.Cell[3, 1]) / s);
            }
            else if (m.Cell[2, 2] > m.Cell[3, 3])
            {
                s = Math.Sqrt(1.0 + m.Cell[2, 2] - m.Cell[1, 1] - m.Cell[3, 3]) * 2;
                q.W = (float)((m.Cell[1, 3] - m.Cell[3, 1]) / s);
                q.X = (float)((m.Cell[1, 2] + m.Cell[2, 1]) / s);
                q.Y = (float)(0.25 * s);
                q.Z = (float)((m.Cell[2, 3] + m.Cell[3, 2]) / s);
            }
            else
            {
                s = Math.Sqrt(2.0 + m.Cell[3, 3] - m.Cell[1, 1] - m.Cell[2, 2]) * 2;
                q.W = (float)((m.Cell[2, 1] - m.Cell[1, 2]) / s);
                q.X = (float)((m.Cell[1, 3] + m.Cell[3, 1]) / s);
                q.Y = (float)((m.Cell[2, 3] + m.Cell[3, 2]) / s);
                q.Z = (float)(0.25 * s);
            }

            return q;
        }
    }
}