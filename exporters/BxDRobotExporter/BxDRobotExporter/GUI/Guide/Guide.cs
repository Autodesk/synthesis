using System;
using Inventor;

namespace BxDRobotExporter.GUI.Guide
{
    public class Guide
    {
        private DockableWindow embeddedGuidePane;

        public bool Visible { get; set; }

        public void CreateDockableWindow(UserInterfaceManager uiMan)
        {
            DestroyDockableWindow();
            embeddedGuidePane = uiMan.DockableWindows.Add(Guid.NewGuid().ToString(), "BxD:RobotExporter:GuidePane", "Robot Export Guide");
            embeddedGuidePane.DockingState = DockingStateEnum.kDockRight;
            embeddedGuidePane.Width = 600;
            embeddedGuidePane.ShowVisibilityCheckBox = false;
            embeddedGuidePane.ShowTitleBar = true;
            var guidePanel = new ExportGuidePanel();
            embeddedGuidePane.AddChild(guidePanel.Handle);
            embeddedGuidePane.Visible = true;
        }

        public void DestroyDockableWindow()
        {
            if (embeddedGuidePane != null)
            {
                embeddedGuidePane.Visible = false;
                embeddedGuidePane.Delete();
            }
        }
    }
}