using System;
using System.ComponentModel;
using InventorRobotExporter.Managers;
using InventorRobotExporter.Utilities;
using Inventor;

namespace InventorRobotExporter.GUI.Editors.AdvancedJointEditor
{
    public class AdvancedJointEditor
    {
        private DockableWindow embeddedAdvancedJointEditorPane;
        private readonly AdvancedJointEditorUserControl advancedJointEditorUserControl = new AdvancedJointEditorUserControl();
        public event EventHandler Activated;

        private bool visible; // Whether the joint editor _should_ be visible
        public bool Visible
        {
            get => visible;
            set
            {
                if (value == embeddedAdvancedJointEditorPane.Visible) return; // Only on change
                visible = value;
                if (embeddedAdvancedJointEditorPane == null) return;
                if (value)
                {
                    Show();
                    Activated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    HideAndClearHighlight();
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
            embeddedAdvancedJointEditorPane.Visible = visible;

            uiMan.DockableWindows.Events.OnHide += (DockableWindow window, EventTimingEnum after, NameValueMap context, out HandlingCodeEnum code) =>
            {
                if (after == EventTimingEnum.kBefore && window.Equals(embeddedAdvancedJointEditorPane))
                {
                    ClearHighlight(); // Must not hide pane again because this event will be fired again
                    visible = false;
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

        public void LoadRobot(RobotDataManager robotDataManager)
        {
            advancedJointEditorUserControl.UpdateSkeleton(robotDataManager);
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
                advancedJointEditorUserControl.UpdateJointList();
                embeddedAdvancedJointEditorPane.Visible = true;
            }
        }

        public void TemporaryHide()
        {
            if (!visible) return;

            // Hmmm
            Visible = false;
            visible = true; // because visible is set to false when OnHide is fired
        }
    }
}