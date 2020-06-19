using System;
using Inventor;
using SynthesisExporterInventor.Utilities;

namespace SynthesisExporterInventor.GUI.Guide
{
    public class GuideManager : DockableWindowManagerBase
    {

        protected override DockableWindow CreateDockableWindow(UserInterfaceManager uiMan)
        {
            var dockableWindow = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:GuidePane", "Robot Export Guide");
            dockableWindow.DockingState = DockingStateEnum.kDockRight;
            dockableWindow.Width = 600;
            dockableWindow.ShowVisibilityCheckBox = false;
            dockableWindow.ShowTitleBar = true;
            var guidePanel = new ExportGuidePanel();
            dockableWindow.AddChild(guidePanel.Handle);
            dockableWindow.Visible = Visible;

            RobotExporterAddInServer.Instance.AddInSettingsManager.SettingsChanged += values => Visible = values.ShowGuide; 
            
            return dockableWindow;
        }

    }
}