using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Inventor;
using SynthesisInventorGltfExporter.Properties;
using Application = Inventor.Application;

namespace SynthesisInventorGltfExporter
{
    
    public abstract class RibbonAddInServer : ApplicationAddInServer
    {
        /// <summary>
        /// The <see cref="Application"/> which the add-in has been opened in
        /// </summary>
        public Application Application { get; private set; }
        
        public void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            Application = addInSiteObject.Application;
            
            try
            {
                ConfigureButtons();
                if (firstTime)
                {
                    ConfigureRibbon();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            Application.ApplicationEvents.OnActivateDocument += ApplicationEvents_OnActivateDocument;
        }

        private void ApplicationEvents_OnActivateDocument(_Document documentObject, EventTimingEnum beforeOrAfter, NameValueMap context, out HandlingCodeEnum handlingCode)
        {
            if (beforeOrAfter == EventTimingEnum.kAfter)
            {
                if (Settings.Default.FirstLaunch)
                {
                    Settings.Default.FirstLaunch = false;
                    Settings.Default.Save();
                    FirstLaunch();
                }
            }

            handlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        public void Deactivate()
        {
            Marshal.ReleaseComObject(Application);
            Application = null;
            
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        protected abstract void ConfigureRibbon();

        protected abstract void ConfigureButtons();

        protected abstract void FirstLaunch();

        public void ExecuteCommand(int commandId) {}

        public object Automation => null;
    }
}