using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using IPictureDisp = stdole.IPictureDisp;

namespace BxDRobotExporter
{
    internal static class InventorUtils
    {
        static internal SynthesisGui Gui;
        private static DockableWindow embededJointPane;
        public static DockableWindow EmbededGuidePane;
        public static DockableWindow EmbededKeyPane;

                
        /// <summary>
        /// Creates a <see cref="DockableWindow"/> containing all of the components of the SynthesisGUI object
        /// </summary>
        /// <param name="app"></param>
        public static void CreateDockableWindows(Application app)
        {

            UserInterfaceManager uiMan = app.UserInterfaceManager;
            embededJointPane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:JointEditor", "Advanced Robot Joint Editor");

            embededJointPane.DockingState = DockingStateEnum.kDockBottom;
            embededJointPane.Height = 250;
            embededJointPane.ShowVisibilityCheckBox = false;
            embededJointPane.ShowTitleBar = true;
            RobotExporterAddInServer.Instance.AdvancedAdvancedJointEditor = new AdvancedJointEditorUserControl();

            RobotExporterAddInServer.Instance.AdvancedAdvancedJointEditor.SetSkeleton(Gui.SkeletonBase);
            RobotExporterAddInServer.Instance.AdvancedAdvancedJointEditor.SelectedJoint += nodes => FocusAndHighlightNodes(nodes, RobotExporterAddInServer.Instance.MainApplication.ActiveView.Camera,  1);
            RobotExporterAddInServer.Instance.AdvancedAdvancedJointEditor.ModifiedJoint += delegate (List<RigidNode_Base> nodes)
            {

                if (nodes == null || nodes.Count == 0) return;

                foreach (RigidNode_Base node in nodes)
                {
                    if (node.GetSkeletalJoint() != null && node.GetSkeletalJoint().cDriver != null &&
                        node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>() != null &&
                        node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>().radius == 0 &&
                        node is OglRigidNode)
                    {
                        (node as OglRigidNode).GetWheelInfo(out float radius, out float width, out BXDVector3 center);

                        WheelDriverMeta wheelDriver = node.GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>();
                        wheelDriver.center = center;
                        wheelDriver.radius = radius;
                        wheelDriver.width = width;
                        node.GetSkeletalJoint().cDriver.AddInfo(wheelDriver);

                    }
                }
            };
            embededJointPane.AddChild(RobotExporterAddInServer.Instance.AdvancedAdvancedJointEditor.Handle);

            embededJointPane.Visible = true;

            EmbededKeyPane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:KeyPane", "Degrees of Freedom Key");
            EmbededKeyPane.DockingState = DockingStateEnum.kFloat;
            EmbededKeyPane.Width = 220;
            EmbededKeyPane.Height = 130;
            EmbededKeyPane.SetMinimumSize(120, 220);
            EmbededKeyPane.ShowVisibilityCheckBox = false;
            EmbededKeyPane.ShowTitleBar = true;
            var keyPanel = new DofKeyPane();
            EmbededKeyPane.AddChild(keyPanel.Handle);
            EmbededKeyPane.Visible = false;

            EmbededGuidePane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:GuidePane", "Robot Export Guide");
            EmbededGuidePane.DockingState = DockingStateEnum.kDockRight;
            EmbededGuidePane.Width = 600;
            EmbededGuidePane.ShowVisibilityCheckBox = false;
            EmbededGuidePane.ShowTitleBar = true;
            var guidePanel = new ExportGuidePanel();
            EmbededGuidePane.AddChild(guidePanel.Handle);
            EmbededGuidePane.Visible = true;
        }

        public static void CreateChildDialog()
        {
            try
            {
                Gui = new SynthesisGui(RobotExporterAddInServer.Instance.MainApplication);  // pass the main application to the GUI so classes RobotExporter can access Inventor to read the joints
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
            if (embededJointPane != null)
            {
                embededJointPane.Visible = false;
                embededJointPane.Delete();
            }
            if (EmbededGuidePane != null)
            {
                EmbededGuidePane.Visible = false;
                EmbededGuidePane.Delete();
            }

            if (EmbededKeyPane != null)
            {
                EmbededKeyPane.Visible = false;
                EmbededKeyPane.Delete();
            }
        }

        public static bool IsAdvancedJointEditorVisible()
        {
            if (embededJointPane == null) return false;
            return embededJointPane.Visible;
        }

        public static void ToggleAdvancedJointEditor()
        {
            if (embededJointPane != null)
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
        /// Hides the dockable windows. Used when switching documents. Called in <see cref="RobotExporterAddInServer.ApplicationEvents_OnDeactivateDocument(_Document, EventTimingEnum, NameValueMap, out HandlingCodeEnum)"/>.
        /// </summary>
        public static void HideAdvancedJointEditor() // TODO: Figure out how to call this when the advanced editor tab is closed manually (Inventor API)
        {
            if (embededJointPane != null)
            {
                embededJointPane.Visible = false;
                FocusAndHighlightNodes(null, RobotExporterAddInServer.Instance.MainApplication.ActiveView.Camera, 1);
            }
        }

        /// <summary>
        /// Shows the dockable windows again when assembly document is switched back to. Called in <see cref="RobotExporterAddInServer.ApplicationEvents_OnActivateDocument(_Document, EventTimingEnum, NameValueMap, out HandlingCodeEnum)"/>.
        /// </summary>
        public static void ShowAdvancedJointEditor()
        {
            if (embededJointPane != null)
            {
                embededJointPane.Visible = true;
            }
        }

        /// <summary>
        /// Converts from a <see cref="System.Drawing.Color"/> to an <see cref="Inventor.Color"/>
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color GetInventorColor(System.Drawing.Color color)
        {
            return RobotExporterAddInServer.Instance.MainApplication.TransientObjects.CreateColor(color.R, color.G, color.B);
        }

        /// <summary>
        /// Initializes all of the <see cref="SynthesisGui"/> settings to the proper values. Should be called once in the Activate class
        /// </summary>
        public static void LoadSettings()
        {
            // Old configurations get overriden (version numbers below 1)
            if (Settings.Default.SaveLocation == "" || Settings.Default.SaveLocation == "firstRun" || Settings.Default.ConfigVersion < 2)
                Settings.Default.SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Robots";

            if (Settings.Default.ConfigVersion < 3)
            {
                SynthesisGui.PluginSettings = ExporterSettingsForm.Values = new ExporterSettingsForm.PluginSettingsValues
                {
                    InventorChildColor = Settings.Default.ChildColor,
                    GeneralSaveLocation = Settings.Default.SaveLocation,
                    GeneralUseFancyColors = Settings.Default.FancyColors,
                    OpenSynthesis = Settings.Default.ExportToField,
                    FieldName = Settings.Default.SelectedField,
                    DefaultRobotCompetition = "GENERIC",
                    UseAnalytics = true
                };
            }
            else
            {
                SynthesisGui.PluginSettings = ExporterSettingsForm.Values = new ExporterSettingsForm.PluginSettingsValues
                {
                    InventorChildColor = Settings.Default.ChildColor,
                    GeneralSaveLocation = Settings.Default.SaveLocation,
                    GeneralUseFancyColors = Settings.Default.FancyColors,
                    OpenSynthesis = Settings.Default.ExportToField,
                    FieldName = Settings.Default.SelectedField,
                    DefaultRobotCompetition = Settings.Default.DefaultRobotCompetition,
                    UseAnalytics = Settings.Default.UseAnalytics
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
            
            RobotExporterAddInServer.Instance.ChildHighlight.Clear();
            // Highlighting must occur after the camera is moved, as inventor clears highlight objects when the camera is moved
            FocusCameraOnOccurrences(occurrences, 15, camera, zoom, InventorUtils.ViewDirection.Y);
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
            
            RobotExporterAddInServer.Instance.ChildHighlight.Clear();
            // Highlighting must occur after the camera is moved, as inventor clears highlight objects when the camera is moved
            FocusCameraOnOccurrences(occurrences, 15, camera, InventorUtils.ViewDirection.Y);
            HighlightOccurrences(occurrences);
        }

        public static void HighlightOccurrences(List<ComponentOccurrence> occurrences)
        {
            RobotExporterAddInServer.Instance.ClearDofHighlight();
            RobotExporterAddInServer.Instance.ChildHighlight.Clear();

            foreach (var componentOccurrence in occurrences)
            {
                RobotExporterAddInServer.Instance.ChildHighlight.AddItem(componentOccurrence);
            }
        }

        public static void ClearHighlight()
        {
            RobotExporterAddInServer.Instance.ClearDofHighlight();
            RobotExporterAddInServer.Instance.ChildHighlight.Clear();
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
            foreach (ComponentOccurrence component in RobotExporterAddInServer.Instance.AsmDocument.ComponentDefinition.Occurrences)
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
        public static void SetCameraView(Vector focus, double viewDistance, Camera camera, double zoom, InventorUtils.ViewDirection viewDirection = InventorUtils.ViewDirection.Y, bool animate = true)
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
            SetCameraView(focus, viewDistance, boxVector.X, boxVector.Z, camera, InventorUtils.ViewDirection.Y, animate);
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
        public static void SetCameraView(Vector focus, double viewDistance, double width, double height, Camera camera, InventorUtils.ViewDirection viewDirection = InventorUtils.ViewDirection.Y, bool animate = true)
        {
            Point focusPoint = RobotExporterAddInServer.Instance.MainApplication.TransientGeometry.CreatePoint(focus.X, focus.Y, focus.Z);

            camera.SetExtents(width, height);

            camera.Target = focusPoint;

            // Flip view for negative direction
            if ((viewDirection & InventorUtils.ViewDirection.Negative) == InventorUtils.ViewDirection.Negative)
                viewDistance = -viewDistance;

            UnitVector up = null;

            // Find camera position and upwards direction
            if ((viewDirection & InventorUtils.ViewDirection.X) == InventorUtils.ViewDirection.X)
            {
                focus.X += viewDistance;
                up = RobotExporterAddInServer.Instance.MainApplication.TransientGeometry.CreateUnitVector(0, 1, 0);
            }

            if ((viewDirection & InventorUtils.ViewDirection.Y) == InventorUtils.ViewDirection.Y)
            {
                focus.Y += viewDistance;
                up = RobotExporterAddInServer.Instance.MainApplication.TransientGeometry.CreateUnitVector(0, 0, 1);
            }

            if ((viewDirection & InventorUtils.ViewDirection.Z) == InventorUtils.ViewDirection.Z)
            {
                focus.Z += viewDistance;
                up = RobotExporterAddInServer.Instance.MainApplication.TransientGeometry.CreateUnitVector(0, 1, 0);
            }

            camera.Eye = RobotExporterAddInServer.Instance.MainApplication.TransientGeometry.CreatePoint(focus.X, focus.Y, focus.Z);
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
            InventorUtils.ViewDirection viewDirection = InventorUtils.ViewDirection.Y, bool animate = false)
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
            InventorUtils.ViewDirection viewDirection = InventorUtils.ViewDirection.Y, bool animate = false)
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


            Vector translation = RobotExporterAddInServer.Instance.MainApplication.TransientGeometry.CreateVector((xSum / i), (ySum / i), (zSum / i));
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

        public static void CreateHighlightSet(List<RigidNode_Base> nodes, HighlightSet highlightSet)
        {
            highlightSet.Clear();
            foreach (var componentOccurrence in InventorUtils.GetComponentOccurrencesFromNodes(nodes))
            {
                highlightSet.AddItem(componentOccurrence);
            }
        }

        /// <summary>
        /// Causes the exporter to close.
        /// </summary>
        /// <param name="suppressClosingEvent">Whether or not the exporter closing handler should be suppressed from being called.</param>
        public static async void ForceQuitExporter(AssemblyDocument document)
        {
            await Task.Delay(1); // Delay is needed so that environment is closed after it has finished opening
            document.EnvironmentManager.SetCurrentEnvironment(document.EnvironmentManager.EditObjectEnvironment);
        }

        /// <summary>
        /// Causes the exporter to close.
        /// </summary>
        /// <param name="suppressClosingEvent">Whether or not the exporter closing handler should be suppressed from being called.</param>
        public static async void ForceQuitExporter(DrawingDocument document)
        {
            await Task.Delay(1); // Delay is needed so that environment is closed after it has finished opening
            document.EnvironmentManager.SetCurrentEnvironment(document.EnvironmentManager.EditObjectEnvironment);
        }

        /// <summary>
        /// Causes the exporter to close.
        /// </summary>
        /// <param name="suppressClosingEvent">Whether or not the exporter closing handler should be suppressed from being called.</param>
        public static async void ForceQuitExporter(PartDocument document)
        {
            await Task.Delay(1); // Delay is needed so that environment is closed after it has finished opening
            document.EnvironmentManager.SetCurrentEnvironment(document.EnvironmentManager.EditObjectEnvironment);
        }

        /// <summary>
        /// Causes the exporter to close.
        /// </summary>
        /// <param name="suppressClosingEvent">Whether or not the exporter closing handler should be suppressed from being called.</param>
        public static async void ForceQuitExporter(PresentationDocument document)
        {
            await Task.Delay(1); // Delay is needed so that environment is closed after it has finished opening
            document.EnvironmentManager.SetCurrentEnvironment(document.EnvironmentManager.EditObjectEnvironment);
        }

        /// <summary>
        /// Disables all components in a document that are not connected to another component by a joint.
        /// </summary>
        /// <param name="asmDocument">Document to traverse.</param>
        /// <returns>List of disabled components.</returns>
        public static List<ComponentOccurrence> DisableUnconnectedComponents(AssemblyDocument asmDocument)
        {
            // Find all components in the assembly that are connected to a joint
            var jointedAssemblyOccurences = new List<ComponentOccurrence>();
            foreach (AssemblyJoint joint in asmDocument.ComponentDefinition.Joints)
            {
                if (!joint.Definition.JointType.Equals(AssemblyJointTypeEnum.kRigidJointType))
                {
                    jointedAssemblyOccurences.Add(joint.AffectedOccurrenceOne);
                    jointedAssemblyOccurences.Add(joint.AffectedOccurrenceTwo);
                }
            }

            // Hide any components not associated with a joint
            var disabledAssemblyOccurences = new List<ComponentOccurrence>();
            foreach (ComponentOccurrence c in asmDocument.ComponentDefinition.Occurrences)
            {
                if (!jointedAssemblyOccurences.Contains(c) || c.Grounded)
                {
                    try
                    {
                        //accounts for components that can't be disabled
                        disabledAssemblyOccurences.Add(c);
                        c.Enabled = false;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return disabledAssemblyOccurences;
        }

        /// <summary>
        /// Enables all components in a list.
        /// </summary>
        /// <param name="components">Components to enable.</param>
        public static void EnableComponents(List<ComponentOccurrence> components)
        {
            foreach (var c in components)
            {
                try
                {
                    //accounts for components that can't be disabled
                    c.Enabled = true;
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Checks if a baseNode matches up with the assembly. Passed as a <see cref="ValidationAction"/> to
        /// </summary>
        /// <param name="baseNode"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static bool ValidateAssembly(RigidNode_Base baseNode, out string message)
        {
            var validationCount = 0;
            var failedCount = 0;
            var nodes = baseNode.ListAllNodes();
            foreach (var node in nodes)
            {
                var failedValidation = false;
                foreach (var componentName in node.ModelFullID.Split(new string[] {"-_-"},
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!CheckForOccurrence(componentName))
                    {
                        failedCount++;
                        failedValidation = true;
                    }
                }

                if (!failedValidation)
                {
                    validationCount++;
                }
            }

            if (validationCount == nodes.Count)
            {
                message = String.Format("The assembly validated successfully. {0} / {1} nodes checked out.",
                    validationCount, nodes.Count);
                return true;
            }
            else
            {
                message = String.Format(
                    "The assembly failed to validate. {0} / {1} nodes checked out. {2} parts/assemblies were not found.",
                    validationCount, nodes.Count, failedCount);
                return false;
            }
        }

        /// <summary>
        /// Checks to see if a <see cref="ComponentOccurrence"/> of the specified name exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool CheckForOccurrence(string name)
        {
            foreach (ComponentOccurrence component in RobotExporterAddInServer.Instance.AsmDocument.ComponentDefinition.Occurrences)
            {
                if (component.Name == name)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the tooltip of a <see cref="ButtonDefinition"/>
        /// </summary>
        /// <param name="button">The <see cref="ButtonDefinition"/> the tool tip is being applied to</param>
        /// <param name="description">The description of the command which the <paramref name="button"/> executes</param>
        /// <param name="expandedDescription">The expanded description of the command which appears after hovering the cursor over the button for a few seconds</param>
        /// <param name="picture">The image that appears along side the <paramref name="expandedDescription"/></param>
        /// <param name="title">The bolded title appearing at the top of the tooltip</param>
        public static void ToolTip(ButtonDefinition button, string title, string description,
            string expandedDescription = null, IPictureDisp picture = null)
        {
            if (description != null)
                button.ProgressiveToolTip.Description = description;
            if (expandedDescription != null)
            {
                button.ProgressiveToolTip.ExpandedDescription = expandedDescription;
                button.ProgressiveToolTip.IsProgressive = true;
            }

            if (picture != null)
            {
                button.ProgressiveToolTip.Image = picture;
                button.ProgressiveToolTip.IsProgressive = true;
            }

            button.ProgressiveToolTip.Title = title;
        }

        /// <summary>
        /// <see cref="X"/>, <see cref="Y"/>, <see cref="Z"/> position the camera right of, in front of, and above the target relative to the front of the robot.
        /// Bitwise OR <see cref="X"/>, <see cref="Y"/>, or <see cref="Z"/> with <see cref="Negative"/> to invert these (make them left of, behind or above the robot).
        /// </summary>
        public enum ViewDirection : byte
        {
            /// <summary>
            /// Positions the camera to the right of the robot.
            /// </summary>
            X = 0b00000001,

            /// <summary>
            /// Positions the camera above the robot.
            /// </summary>
            Y = 0b00000010,

            /// <summary>
            /// Positions the robot in front of the robot.
            /// </summary>
            Z = 0b00000100,
            Negative = 0b00001000
        }
    }
}