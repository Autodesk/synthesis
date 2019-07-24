using System;
using Inventor;

namespace BxDRobotExporter.GUI.Editors.AdvancedJointEditor
{
    public class AdvancedJointEditor
    {
        private DockableWindow embeddedAdvancedJointEditorPane;
        private readonly AdvancedJointEditorUserControl advancedJointEditorUserControl = new AdvancedJointEditorUserControl();
        
        public bool Visible
        {
            get => embeddedAdvancedJointEditorPane.Visible;
            set
            {
                if (embeddedAdvancedJointEditorPane == null) return;
                if (value)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        public void CreateDockableWindow(UserInterfaceManager uiMan)
        {
            DestroyDockableWindow();
            embeddedAdvancedJointEditorPane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:JointEditor", "Advanced Robot Joint Editor");
            embeddedAdvancedJointEditorPane.DockingState = DockingStateEnum.kDockBottom;
            embeddedAdvancedJointEditorPane.Height = 250;
            embeddedAdvancedJointEditorPane.ShowVisibilityCheckBox = false;
            embeddedAdvancedJointEditorPane.ShowTitleBar = true;
            embeddedAdvancedJointEditorPane.AddChild(advancedJointEditorUserControl.Handle);
            embeddedAdvancedJointEditorPane.Visible = false;
        }

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

        /// <summary>
        /// Hides the dockable windows. Used when switching documents. Called in <see cref="RobotExporterAddInServer.ApplicationEvents_OnDeactivateDocument(_Document, EventTimingEnum, NameValueMap, out HandlingCodeEnum)"/>.
        /// </summary>
        private void Hide() // TODO: Figure out how to call this when the advanced editor tab is closed manually (Inventor API)
        {
            if (embeddedAdvancedJointEditorPane == null) return;
            embeddedAdvancedJointEditorPane.Visible = false;
            InventorUtils.FocusAndHighlightNodes(null, RobotExporterAddInServer.Instance.Application.ActiveView.Camera, 1);
        }

        /// <summary>
        /// Shows the dockable windows again when assembly document is switched back to. Called in <see cref="RobotExporterAddInServer.ApplicationEvents_OnActivateDocument(_Document, EventTimingEnum, NameValueMap, out HandlingCodeEnum)"/>.
        /// </summary>
        private void Show()
        {
            if (embeddedAdvancedJointEditorPane != null)
            {
                embeddedAdvancedJointEditorPane.Visible = true;
            }
        }
    }
}