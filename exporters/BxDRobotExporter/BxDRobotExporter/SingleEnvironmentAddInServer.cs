using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BxDRobotExporter.Messages;
using BxDRobotExporter.Properties;
using Inventor;
using Application = Inventor.Application;
using Environment = Inventor.Environment;

namespace BxDRobotExporter
{
    public abstract class SingleEnvironmentAddInServer : ApplicationAddInServer
    {
        public Application Application { get; private set; }
        public Document OpenDocument { get; private set; }

        private bool environmentOpen;
        public bool EnvironmentOpen
        {
            get => environmentOpen;
            private set
            {
                if (environmentOpen == value) return; // only if value changed
                environmentOpen = value;
                
                if (environmentOpen)
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    OpenDocument = Application.ActiveDocument;
                    OnEnvironmentOpen();
                    environmentVisible = true;
                }
                else
                {
                    environmentVisible = false;
                    OnEnvironmentClose();
                    InventorUtils.ForceQuitExporter(OpenDocument);
                    // Dispose of document
                    if (OpenDocument != null)
                        Marshal.ReleaseComObject(OpenDocument);
                    OpenDocument = null;
                }
            }
        }
        
        private bool environmentVisible;
        public bool EnvironmentVisible
        {
            get => environmentVisible;
            private set
            {
                if (environmentVisible == value) return; // only if value changed
                environmentVisible = value;
                
                if (environmentVisible)
                {
                    OnEnvironmentShow();
                }
                else
                {
                    OnEnvironmentHide();
                }
            }
        }

        private Environment environment;
        protected abstract Environment CreateEnvironment();

        protected abstract void DestroyEnvironment();

        protected abstract void OnEnvironmentOpen();

        protected abstract void OnEnvironmentClose();

        protected abstract void OnEnvironmentShow();

        protected abstract void OnEnvironmentHide();
        protected abstract bool DocumentCondition(Document document);

        public void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            Application = addInSiteObject.Application;
            environment = CreateEnvironment();
            SetupEnvironmentHandlers();
        }

        public void Deactivate()
        {
            DestroyEnvironment();
            Marshal.ReleaseComObject(Application);
            Application = null;
            
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void SetupEnvironmentHandlers()
        {
            Application.UserInterfaceManager.UserInterfaceEvents.OnEnvironmentChange += UIEvents_OnEnvironmentChange;
            Application.ApplicationEvents.OnActivateDocument += ApplicationEvents_OnActivateDocument;
            Application.ApplicationEvents.OnDeactivateDocument += ApplicationEvents_OnDeactivateDocument;
            Application.ApplicationEvents.OnCloseDocument += ApplicationEvents_OnCloseDocument;
        }

        private void ApplicationEvents_OnActivateDocument(_Document documentObject, EventTimingEnum beforeOrAfter, NameValueMap context, out HandlingCodeEnum handlingCode)
        {
            if (beforeOrAfter == EventTimingEnum.kBefore)
            {
                if (IsNewExporterEnvironmentAllowed(documentObject))
                    InventorUtils.EnableEnvironment(Application, environment);
                else
                    InventorUtils.DisableEnvironment(Application, environment);
                
                if (IsDocumentOpenInTheExporter(documentObject))
                    EnvironmentVisible = true;
            }
            else if (beforeOrAfter == EventTimingEnum.kAfter && Settings.Default.ShowFirstLaunchInfo && IsNewExporterEnvironmentAllowed(documentObject)) 
                new FirstLaunchInfo().ShowDialog();

            handlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        private void ApplicationEvents_OnDeactivateDocument(_Document documentObject, EventTimingEnum beforeOrAfter, NameValueMap context, out HandlingCodeEnum handlingCode)
        {
            if (beforeOrAfter == EventTimingEnum.kBefore && IsDocumentOpenInTheExporter(documentObject))
                EnvironmentVisible = false;
            handlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        private void ApplicationEvents_OnCloseDocument(_Document documentObject, string fullDocumentName, EventTimingEnum beforeOrAfter, NameValueMap context, out HandlingCodeEnum handlingCode)
        { // NOTE: this is actually necessary as the OnEnvironmentChange is not actually fired
            // If the robot export environment is open and the document that is about to be closed is the assembly document with the robot exporter opened
            if (beforeOrAfter == EventTimingEnum.kBefore && IsDocumentOpenInTheExporter(documentObject))
                EnvironmentOpen = false;
            handlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        private void UIEvents_OnEnvironmentChange(Environment environment, EnvironmentStateEnum environmentState, EventTimingEnum beforeOrAfter, NameValueMap context, out HandlingCodeEnum handlingCode)
        {
            // If the environment changing is the exporter environment
            if (beforeOrAfter == EventTimingEnum.kBefore && environment.Equals(this.environment))
            {
                var documentObject = context.Item["Document"];
                // If the exporter environment is opening
                if (environmentState == EnvironmentStateEnum.kActivateEnvironmentState)
                {
                    if (IsNewExporterEnvironmentAllowed(documentObject))
                        EnvironmentOpen = true;
                    else
                    { // TODO: Make this text generic
                        MessageBox.Show("The Robot Exporter only supports assembly documents.", // or, the environment is already open, but the env button hiding seems to work in that case
                            "Unsupported Document Type", MessageBoxButtons.OK);
                        InventorUtils.ForceQuitExporter(documentObject);
                    }
                }
                // If the exporter environment is closing
                else if (environmentState == EnvironmentStateEnum.kTerminateEnvironmentState && IsDocumentOpenInTheExporter(documentObject))
                    EnvironmentOpen = false;
            }

            handlingCode = HandlingCodeEnum.kEventNotHandled;
        }

        private bool IsNewExporterEnvironmentAllowed(object documentObject)
        {
            return !EnvironmentOpen && documentObject is Document document && DocumentCondition(document);
        }

        private bool IsDocumentOpenInTheExporter(object documentObject)
        {
            return EnvironmentOpen && OpenDocument != null && documentObject is Document document && document == OpenDocument;
        }

        public void ExecuteCommand(int commandId) {}
        public object Automation => null;
    }
}