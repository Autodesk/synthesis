using System;
using Inventor;

namespace BxDRobotExporter.GUI.Editors.AdvancedJointEditor
{
    public class AdvancedJointEditorDockableWindow
    {
        private DockableWindow embeddedAdvancedJointEditorPane;
        private AdvancedJointEditorUserControl advancedJointEditorUserControl;

        public void CreateDockableWindow(UserInterfaceManager uiMan, RigidNode_Base skeletonBase)
        {
            DestroyDockableWindow();
            embeddedAdvancedJointEditorPane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:JointEditor", "Advanced Robot Joint Editor");

            embeddedAdvancedJointEditorPane.DockingState = DockingStateEnum.kDockBottom;
            embeddedAdvancedJointEditorPane.Height = 250;
            embeddedAdvancedJointEditorPane.ShowVisibilityCheckBox = false;
            embeddedAdvancedJointEditorPane.ShowTitleBar = true;
            advancedJointEditorUserControl = new AdvancedJointEditorUserControl(skeletonBase);
            embeddedAdvancedJointEditorPane.AddChild(advancedJointEditorUserControl.Handle);
            embeddedAdvancedJointEditorPane.Visible = true;        }

        public void DestroyDockableWindow()
        {
            if (embeddedAdvancedJointEditorPane != null)
            {
                embeddedAdvancedJointEditorPane.Visible = false;
                embeddedAdvancedJointEditorPane.Delete();
            }
        }

        public void UpdateSkeleton(RigidNode_Base instanceSkeletonBase)
        {
            advancedJointEditorUserControl.UpdateSkeleton(instanceSkeletonBase);
        }
        
        
        public bool IsVisible()
        {
            if (embeddedAdvancedJointEditorPane == null) return false;
            return embeddedAdvancedJointEditorPane.Visible;
        }

        public void Toggle()
        {
            if (embeddedAdvancedJointEditorPane != null)
            {
                if (IsVisible())
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }
        }

        /// <summary>
        /// Hides the dockable windows. Used when switching documents. Called in <see cref="RobotExporterAddInServer.ApplicationEvents_OnDeactivateDocument(_Document, EventTimingEnum, NameValueMap, out HandlingCodeEnum)"/>.
        /// </summary>
        public void Hide() // TODO: Figure out how to call this when the advanced editor tab is closed manually (Inventor API)
        {
            if (embeddedAdvancedJointEditorPane != null)
            {
                embeddedAdvancedJointEditorPane.Visible = false;
                InventorUtils.FocusAndHighlightNodes(null, RobotExporterAddInServer.Instance.MainApplication.ActiveView.Camera, 1);
            }
        }

        /// <summary>
        /// Shows the dockable windows again when assembly document is switched back to. Called in <see cref="RobotExporterAddInServer.ApplicationEvents_OnActivateDocument(_Document, EventTimingEnum, NameValueMap, out HandlingCodeEnum)"/>.
        /// </summary>
        public void Show()
        {
            if (embeddedAdvancedJointEditorPane != null)
            {
                embeddedAdvancedJointEditorPane.Visible = true;
            }
        }
    }
}