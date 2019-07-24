using System;
using Inventor;

namespace BxDRobotExporter.GUI.DegreesOfFreedomViewer
{
    public class DOFKey
    {
        private DockableWindow embeddedDofKeyPane;

        private bool visible;
        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                SoftSetVisibility(value);
            }
        }

        private void SoftSetVisibility(bool value)
        {
            if (embeddedDofKeyPane!= null) embeddedDofKeyPane.Visible = value;
        }

        public void CreateDockableWindow(UserInterfaceManager uiMan)
        {
            DestroyDockableWindow();
            embeddedDofKeyPane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:KeyPane", "Degrees of Freedom Key");
            embeddedDofKeyPane.DockingState = DockingStateEnum.kFloat;
            embeddedDofKeyPane.Width = 220;
            embeddedDofKeyPane.Height = 130;
            embeddedDofKeyPane.SetMinimumSize(120, 220);
            embeddedDofKeyPane.ShowVisibilityCheckBox = false;
            embeddedDofKeyPane.ShowTitleBar = true;
            var keyPanel = new DofKeyPane();
            embeddedDofKeyPane.AddChild(keyPanel.Handle);
            embeddedDofKeyPane.Visible = false;
        }

        public void DestroyDockableWindow()
        {
            if (embeddedDofKeyPane != null)
            {
                embeddedDofKeyPane.Visible = false;
                embeddedDofKeyPane.Delete();
                embeddedDofKeyPane = null;
            }
        }

        public void TemporaryHide()
        {
            SoftSetVisibility(false);
        }
    }
}