using System;
using Inventor;

namespace BxDRobotExporter.GUI.Editors.AdvancedJointEditor
{
    public class AdvancedJointEditor
    {
        private DockableWindow embeddedAdvancedJointEditorPane;
        private readonly AdvancedJointEditorUserControl advancedJointEditorUserControl = new AdvancedJointEditorUserControl();

        private bool visible;
        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                if (embeddedAdvancedJointEditorPane == null) return;
                if (value)
                {
                    Show();
                }
                else
                {
                    HideAndClearHighlight();
                }
            }
        }

        public AdvancedJointEditor(bool startVisible)
        {
            visible = startVisible;
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
            embeddedAdvancedJointEditorPane.Visible = visible;

            uiMan.DockableWindows.Events.OnHide += (DockableWindow window, EventTimingEnum after, NameValueMap context, out HandlingCodeEnum code) =>
            {
                if (after == EventTimingEnum.kBefore && window.Equals(embeddedAdvancedJointEditorPane))
                {
                    ClearHighlight();
                    visible = false; // 
                }
                code = HandlingCodeEnum.kEventNotHandled;
            };
        }

        public void DestroyDockableWindow()
        {
            if (embeddedAdvancedJointEditorPane != null)
            {
                embeddedAdvancedJointEditorPane.Visible = false;
                embeddedAdvancedJointEditorPane.Delete();
                embeddedAdvancedJointEditorPane = null;
            }
        }

        public void UpdateSkeleton(RobotData robotData)
        {
            advancedJointEditorUserControl.UpdateSkeleton(robotData);
        }

        /// <summary>
        /// Hides the dockable windows. Used when switching documents. Called in <see cref="RobotExporterAddInServer.ApplicationEvents_OnDeactivateDocument(_Document, EventTimingEnum, NameValueMap, out HandlingCodeEnum)"/>.
        /// </summary>
        private void HideAndClearHighlight()
        {
            ClearHighlight();
            if (embeddedAdvancedJointEditorPane != null) 
                embeddedAdvancedJointEditorPane.Visible = false;
        }

        private static void ClearHighlight()
        {
            if (RobotExporterAddInServer.Instance.Application.ActiveView != null)
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

        public void TemporaryHide()
        {
            HideAndClearHighlight();
        }
    }
}