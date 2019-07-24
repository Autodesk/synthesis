using System;
using Inventor;

namespace BxDRobotExporter.GUI.Guide
{
    public class Guide
    {
        private DockableWindow embeddedGuidePane;

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
            if (embeddedGuidePane != null) embeddedGuidePane.Visible = value;
        }

        public Guide(bool startVisible)
        {
            visible = startVisible;
        }

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
            embeddedGuidePane.Visible = visible;
        }

        public void DestroyDockableWindow()
        {
            if (embeddedGuidePane != null)
            {
                embeddedGuidePane.Visible = false;
                embeddedGuidePane.Delete();
                embeddedGuidePane = null;
            }
        }

        public void TemporaryHide()
        {
            SoftSetVisibility(false);
        }
    }
}