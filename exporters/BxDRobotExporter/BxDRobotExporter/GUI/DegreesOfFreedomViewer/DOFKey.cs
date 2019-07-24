using System;
using Inventor;

namespace BxDRobotExporter.GUI.DegreesOfFreedomViewer
{
    public class DOFKey
    {
        private DockableWindow embeddedDofKeyPane;

        public bool Visible
        {
            get => embeddedDofKeyPane.Visible;
            set => embeddedDofKeyPane.Visible = value;
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
            }
        }
    }
}