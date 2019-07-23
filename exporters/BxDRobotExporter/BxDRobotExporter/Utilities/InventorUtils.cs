using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BxDRobotExporter.ControlGUI;
using BxDRobotExporter.ExportGuide;
using BxDRobotExporter.GUI.Editors;
using BxDRobotExporter.GUI.Editors.AdvancedJointEditor;
using BxDRobotExporter.GUI.Editors.DegreesOfFreedomViewer;
using BxDRobotExporter.OGLViewer;
using BxDRobotExporter.Properties;
using Inventor;
using Application = Inventor.Application;
using Environment = System.Environment;

namespace BxDRobotExporter
{
    internal static class InventorUtils
    {
        static internal SynthesisGUI GUI;
        static DockableWindow EmbededJointPane;
        public static DockableWindow EmbededPrecheckPane;
        public static DockableWindow EmbededKeyPane;

                
        /// <summary>
        /// Creates a <see cref="DockableWindow"/> containing all of the components of the SynthesisGUI object
        /// </summary>
        /// <param name="app"></param>
        public static void CreateDockableWindows(Application app)
        {

            UserInterfaceManager uiMan = app.UserInterfaceManager;
            EmbededJointPane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:JointEditor", "Advanced Robot Joint Editor");
            
            #region EmbededJointPane
            EmbededJointPane.DockingState = DockingStateEnum.kDockBottom;
            EmbededJointPane.Height = 250;
            EmbededJointPane.ShowVisibilityCheckBox = false;
            EmbededJointPane.ShowTitleBar = true;
            StandardAddInServer.Instance.AdvancedAdvancedJointEditor = new AdvancedJointEditorUserControl();

            StandardAddInServer.Instance.AdvancedAdvancedJointEditor.SetSkeleton(GUI.SkeletonBase);
            StandardAddInServer.Instance.AdvancedAdvancedJointEditor.SelectedJoint += nodes => FocusAndHighlightNodes(nodes, StandardAddInServer.Instance.MainApplication.ActiveView.Camera,  1);
            StandardAddInServer.Instance.AdvancedAdvancedJointEditor.ModifiedJoint += delegate (List<RigidNode_Base> nodes)
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
            EmbededJointPane.AddChild(StandardAddInServer.Instance.AdvancedAdvancedJointEditor.Handle);
            #endregion
            
            EmbededJointPane.Visible = true;

            EmbededKeyPane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:KeyPane", "Degrees of Freedom Key");
            EmbededKeyPane.DockingState = DockingStateEnum.kFloat;
            EmbededKeyPane.Width = 220;
            EmbededKeyPane.Height = 130;
            EmbededKeyPane.SetMinimumSize(120, 220);
            EmbededKeyPane.ShowVisibilityCheckBox = false;
            EmbededKeyPane.ShowTitleBar = true;
            var keyPanel = new DOFKeyPane();
            EmbededKeyPane.AddChild(keyPanel.Handle);
            EmbededKeyPane.Visible = false;

            EmbededPrecheckPane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:PrecheckPane", "Robot Export Guide");
            EmbededPrecheckPane.DockingState = DockingStateEnum.kDockRight;
            EmbededPrecheckPane.Width = 600;
            EmbededPrecheckPane.ShowVisibilityCheckBox = false;
            EmbededPrecheckPane.ShowTitleBar = true;
            var precheckPanel = new ExportGuidePanel();
            EmbededPrecheckPane.AddChild(precheckPanel.Handle);
            EmbededPrecheckPane.Visible = true;
        }

        public static void CreateChildDialog()
        {
            try
            {
                GUI = new SynthesisGUI(StandardAddInServer.Instance.MainApplication);  // pass the main application to the GUI so classes RobotExporter can access Inventor to read the joints
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

            if (EmbededKeyPane != null)
            {
                EmbededKeyPane.Visible = false;
                EmbededKeyPane.Delete();
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
                FocusAndHighlightNodes(null, StandardAddInServer.Instance.MainApplication.ActiveView.Camera, 1);
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
        public static Color GetInventorColor(System.Drawing.Color color)
        {
            return StandardAddInServer.Instance.MainApplication.TransientObjects.CreateColor(color.R, color.G, color.B);
        }

        /// <summary>
        /// Initializes all of the <see cref="SynthesisGUI"/> settings to the proper values. Should be called once in the Activate class
        /// </summary>
        public static void LoadSettings()
        {
            // Old configurations get overriden (version numbers below 1)
            if (Settings.Default.SaveLocation == "" || Settings.Default.SaveLocation == "firstRun" || Settings.Default.ConfigVersion < 2)
                Settings.Default.SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Robots";

            if (Settings.Default.ConfigVersion < 3)
            {
                SynthesisGUI.PluginSettings = ExporterSettingsForm.Values = new ExporterSettingsForm.PluginSettingsValues
                {
                    InventorChildColor = Settings.Default.ChildColor,
                    GeneralSaveLocation = Settings.Default.SaveLocation,
                    GeneralUseFancyColors = Settings.Default.FancyColors,
                    openSynthesis = Settings.Default.ExportToField,
                    fieldName = Settings.Default.SelectedField,
                    defaultRobotCompetition = "GENERIC",
                    useAnalytics = true
                };
            }
            else
            {
                SynthesisGUI.PluginSettings = ExporterSettingsForm.Values = new ExporterSettingsForm.PluginSettingsValues
                {
                    InventorChildColor = Settings.Default.ChildColor,
                    GeneralSaveLocation = Settings.Default.SaveLocation,
                    GeneralUseFancyColors = Settings.Default.FancyColors,
                    openSynthesis = Settings.Default.ExportToField,
                    fieldName = Settings.Default.SelectedField,
                    defaultRobotCompetition = Settings.Default.DefaultRobotCompetition,
                    useAnalytics = Settings.Default.UseAnalytics
                };
            }
        }


        // Analytics


        /// <summary>
        /// Selects a list of nodes in Inventor.
        /// </summary>
        /// <param name="nodes">List of nodes to select.</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        public static void FocusAndHighlightNodes(List<RigidNode_Base> nodes, Camera camera, double zoom)
        {
            if (nodes == null)
            {
                ClearHighlight();
                camera.Fit();
                camera.ApplyWithoutTransition();
                return;
            }
            var occurrences = GetComponentOccurrencesFromNodes(nodes);
            if (occurrences == null)
            {
                return;
            }
            
            StandardAddInServer.Instance.ChildHighlight.Clear();
            // Highlighting must occur after the camera is moved, as inventor clears highlight objects when the camera is moved
            FocusCameraOnOccurrences(occurrences, 15, camera, zoom, StandardAddInServer.ViewDirection.Y);
            HighlightOccurrences(occurrences);
        }

        /// <summary>
        /// Selects a list of nodes in Inventor.
        /// </summary>
        /// <param name="nodes">List of nodes to select.</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        public static void FocusAndHighlightNodes(List<RigidNode_Base> nodes, Camera camera)
        {
            if (nodes == null)
            {
                return;
            }
            var occurrences = GetComponentOccurrencesFromNodes(nodes);
            if (occurrences == null)
            {
                return;
            }
            
            StandardAddInServer.Instance.ChildHighlight.Clear();
            // Highlighting must occur after the camera is moved, as inventor clears highlight objects when the camera is moved
            FocusCameraOnOccurrences(occurrences, 15, camera, StandardAddInServer.ViewDirection.Y);
            HighlightOccurrences(occurrences);
        }

        public static void HighlightOccurrences(List<ComponentOccurrence> occurrences)
        {
            StandardAddInServer.Instance.ClearDOFHighlight();
            StandardAddInServer.Instance.ChildHighlight.Clear();

            foreach (var componentOccurrence in occurrences)
            {
                StandardAddInServer.Instance.ChildHighlight.AddItem(componentOccurrence);
            }
        }

        public static void ClearHighlight()
        {
            StandardAddInServer.Instance.ClearDOFHighlight();
            StandardAddInServer.Instance.ChildHighlight.Clear();
        }

        public static List<ComponentOccurrence> GetComponentOccurrencesFromNodes(List<RigidNode_Base> nodes)
        {
            if (nodes == null)
            {
                return null;
            }

            // Get all node ID's
            List<string> nodeIDs = new List<string>();
            ;
            foreach (RigidNode_Base node in nodes)
                nodeIDs.AddRange(node.GetModelID().Split(new String[] {"-_-"}, StringSplitOptions.RemoveEmptyEntries));

            // Select all nodes
            List<ComponentOccurrence> occurrences = new List<ComponentOccurrence>();
            foreach (string id in nodeIDs)
            {
                ComponentOccurrence occurrence = GetOccurrence(id);

                if (occurrence != null)
                {
                    occurrences.Add(occurrence);
                }
            }

            return occurrences;
        }

        /// <summary>
        /// Public method used to select a node.
        /// </summary>
        /// <param name="node">Node to select.</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        public static void FocusAndHighlightNode(RigidNode_Base node, Camera camera, double zoom)
        {
            FocusAndHighlightNodes(new List<RigidNode_Base> {node}, camera, zoom);
        }

        /// <summary>
        /// Public method used to select a node.
        /// </summary>
        /// <param name="node">Node to select.</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        public static void FocusAndHighlightNode(RigidNode_Base node, Camera camera)
        {
            FocusAndHighlightNodes(new List<RigidNode_Base> {node}, camera);
        }

        /// <summary>
        /// Gets the <see cref="ComponentOccurrence"/> of the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ComponentOccurrence GetOccurrence(string name)
        {
            foreach (ComponentOccurrence component in StandardAddInServer.Instance.AsmDocument.ComponentDefinition.Occurrences)
            {
                if (component.Name == name)
                    return component;
            }

            return null;
        }

        /// <summary>
        /// Sets the position and target of the camera.
        /// </summary>
        /// <param name="focus">Point that camera should look at.</param>
        /// <param name="viewDistance">Distance the camera should be from that point</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        /// <param name="viewDirection">Direction to view the point from.</param>
        /// <param name="animate">True to animate movement of camera.</param>
        public static void SetCameraView(Vector focus, double viewDistance, Camera camera, double zoom, StandardAddInServer.ViewDirection viewDirection = StandardAddInServer.ViewDirection.Y, bool animate = true)
        {
            camera.Fit(); // TODO: Determine model size properly
            camera.GetExtents(out var width, out var height);

            SetCameraView(focus, viewDistance, height * zoom, height * zoom, camera, viewDirection, animate);
        }

        /// <summary>
        /// Sets the position and target of the camera.
        /// </summary> // TODO: Add support for other directions
        /// <param name="focus">Point that camera should look at.</param>
        /// <param name="viewDistance">Distance the camera should be from that point</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        /// <param name="viewDirection">Direction to view the point from.</param>
        /// <param name="animate">True to animate movement of camera.</param>
        public static void SetCameraView(Vector focus, Box boundingBox, double viewDistance, Camera camera, bool animate = true)
        {
            var boxVector = boundingBox.MinPoint.VectorTo(boundingBox.MaxPoint);
            SetCameraView(focus, viewDistance, boxVector.X, boxVector.Z, camera, StandardAddInServer.ViewDirection.Y, animate);
        }

        /// <summary>
        /// Sets the position and target of the camera.
        /// </summary>
        /// <param name="focus">Point that camera should look at.</param>
        /// <param name="viewDistance">Distance the camera should be from that point</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        /// <param name="viewDirection">Direction to view the point from.</param>
        /// <param name="animate">True to animate movement of camera.</param>
        public static void SetCameraView(Vector focus, double viewDistance, double width, double height, Camera camera, StandardAddInServer.ViewDirection viewDirection = StandardAddInServer.ViewDirection.Y, bool animate = true)
        {
            Point focusPoint = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreatePoint(focus.X, focus.Y, focus.Z);

            camera.SetExtents(width, height);

            camera.Target = focusPoint;

            // Flip view for negative direction
            if ((viewDirection & StandardAddInServer.ViewDirection.Negative) == StandardAddInServer.ViewDirection.Negative)
                viewDistance = -viewDistance;

            UnitVector up = null;

            // Find camera position and upwards direction
            if ((viewDirection & StandardAddInServer.ViewDirection.X) == StandardAddInServer.ViewDirection.X)
            {
                focus.X += viewDistance;
                up = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreateUnitVector(0, 1, 0);
            }

            if ((viewDirection & StandardAddInServer.ViewDirection.Y) == StandardAddInServer.ViewDirection.Y)
            {
                focus.Y += viewDistance;
                up = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreateUnitVector(0, 0, 1);
            }

            if ((viewDirection & StandardAddInServer.ViewDirection.Z) == StandardAddInServer.ViewDirection.Z)
            {
                focus.Z += viewDistance;
                up = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreateUnitVector(0, 1, 0);
            }

            camera.Eye = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreatePoint(focus.X, focus.Y, focus.Z);
            camera.UpVector = up;

            // Apply settings
            if (animate)
                camera.Apply();
            else
                camera.ApplyWithoutTransition();
        }

        /// <summary>
        /// Moves the camera to the midpoint of all the specified occurrences. Used in the wizard to point out the specified occurence.
        /// </summary>
        /// <param name="occurrences">The <see cref="ComponentOccurrence"/>s for the <see cref="Camera"/> to focus on</param>
        /// <param name="viewDistance">The distence from <paramref name="occurrence"/> that the camera will be</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        /// <param name="viewDirection">The direction of the camera</param>
        /// <param name="animate">True if you want to animate the camera moving to the new position</param>
        public static void FocusCameraOnOccurrences(List<ComponentOccurrence> occurrences, double viewDistance, Camera camera, double zoom,
            StandardAddInServer.ViewDirection viewDirection = StandardAddInServer.ViewDirection.Y, bool animate = false)
        {
            if (occurrences.Count < 1)
                return;

            var translation = GetOccurrencesCenter(occurrences);

            SetCameraView(translation, viewDistance, camera, zoom, viewDirection, animate);
        }

        /// <summary>
        /// Moves the camera to the midpoint of all the specified occurrences. Used in the wizard to point out the specified occurence.
        /// </summary>
        /// <param name="occurrences">The <see cref="ComponentOccurrence"/>s for the <see cref="Camera"/> to focus on</param>
        /// <param name="viewDistance">The distence from <paramref name="occurrence"/> that the camera will be</param>
        /// <param name="camera"></param>
        /// <param name="zoom"></param>
        /// <param name="viewDirection">The direction of the camera</param>
        /// <param name="animate">True if you want to animate the camera moving to the new position</param>
        public static void FocusCameraOnOccurrences(List<ComponentOccurrence> occurrences, double viewDistance, Camera camera,
            StandardAddInServer.ViewDirection viewDirection = StandardAddInServer.ViewDirection.Y, bool animate = false)
        {
            if (occurrences.Count < 1)
                return;

            var translation = GetOccurrencesCenter(occurrences);
            var combineBoundingBoxes = CombineBoundingBoxes(occurrences);

            SetCameraView(translation, combineBoundingBoxes, viewDistance, camera, animate);
        }

        public static Vector GetOccurrencesCenter(List<ComponentOccurrence> occurrences)
        {
            double xSum = 0, ySum = 0, zSum = 0;
            int i = 0;
            foreach (ComponentOccurrence occurrence in occurrences)
            {
                xSum += occurrence.Transformation.Translation.X;
                ySum += occurrence.Transformation.Translation.Y;
                zSum += occurrence.Transformation.Translation.Z;

                i++;
            }


            Vector translation = StandardAddInServer.Instance.MainApplication.TransientGeometry.CreateVector((xSum / i), (ySum / i), (zSum / i));
            return translation;
        }

        public static Box CombineBoundingBoxes(List<ComponentOccurrence> occurrences)
        {
            var resultBox = occurrences[0].RangeBox.Copy();
            foreach (var occurrence in occurrences.GetRange(1, occurrences.Count-1))
            {
                resultBox.Extend(occurrence.RangeBox.MaxPoint);
                resultBox.Extend(occurrence.RangeBox.MinPoint);
            }
            return resultBox;
        }
    }
}