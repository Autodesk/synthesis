using System;
using Inventor;
using InventorRobotExporter.GUI.Guide;

namespace InventorRobotExporter.Utilities
{
    public abstract class DockableWindowManagerBase
    {
        private DockableWindow dockableWindow;
        private bool visible;

        public event EventHandler Activated;

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
            if (dockableWindow != null) dockableWindow.Visible = value;
            if (value) Activated?.Invoke(this, EventArgs.Empty);
        }

        protected abstract DockableWindow CreateDockableWindow(UserInterfaceManager uiMan);

        public void Init(UserInterfaceManager uiMan)
        {
            DestroyDockableWindow();
            dockableWindow = CreateDockableWindow(uiMan);
        }

        public void DestroyDockableWindow()
        {
            if (dockableWindow != null)
            {
                dockableWindow.Visible = false;
                dockableWindow.Delete();
                dockableWindow = null;
            }
        }

        public void TemporaryHide()
        {
            SoftSetVisibility(false);
        }
    }
}