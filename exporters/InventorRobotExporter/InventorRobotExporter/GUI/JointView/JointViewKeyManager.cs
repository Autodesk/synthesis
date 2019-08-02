using System;
using Inventor;
using InventorRobotExporter.Utilities;

namespace InventorRobotExporter.GUI.JointView
{
    public class JointViewKey : DockableWindowManagerBase
    {
        protected override DockableWindow CreateDockableWindow(UserInterfaceManager uiMan)
        {
            var dockableWindow = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:KeyPane", "Joint View Key");
            dockableWindow.DockingState = DockingStateEnum.kFloat;
            dockableWindow.Width = 220;
            dockableWindow.Height = 130;
            dockableWindow.SetMinimumSize(120, 220);
            dockableWindow.ShowVisibilityCheckBox = false;
            dockableWindow.ShowTitleBar = true;
            var keyPanel = new JointViewKeyPane();
            dockableWindow.AddChild(keyPanel.Handle);
            dockableWindow.Visible = false;
            return dockableWindow;
        }
    }
}