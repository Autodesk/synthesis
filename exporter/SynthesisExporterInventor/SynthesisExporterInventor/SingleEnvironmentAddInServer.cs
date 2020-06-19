using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SynthesisExporterInventor.GUI.Messages;
using SynthesisExporterInventor.Properties;
using SynthesisExporterInventor.Utilities;
using Inventor;
using Application = Inventor.Application;
using Environment = Inventor.Environment;

namespace SynthesisExporterInventor
{
    
    /// <summary>
    /// Abstract class for Inventor AddInServers that manage a single environment which can only be open in one document at a time.
    /// Contains logic for detecting when the environment opens/closes and when the document in which the environment is open is shown/hidden.
    /// Additionally provides <see cref="IsDocumentSupported"/> to allow base classes to respond with whether or not they support a certain document.
    /// </summary>
    public abstract class SingleEnvironmentAddInServer : ApplicationAddInServer
    {
        /// <summary>
        /// The <see cref="Application"/> which the add-in has been opened in
        /// </summary>
        public Application Application { get; private set; }
        
        /// <summary>
        /// The <see cref="Document"/> which the environment has been opened in
        /// </summary>
        protected Document OpenDocument { get; private set; }

        /// <summary>
        /// Whether the environment is open in Inventor
        /// </summary>
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
        private bool environmentOpen;
        
        /// <summary>
        /// Whether the environment is open in the focused document in Inventor
        /// </summary>
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
        private bool environmentVisible;

        
        /// <summary>
        /// Creates the environment which will be managed by the add-in, called once when the add-in is activated
        /// </summary>
        /// <returns>The created environment managed by the plugin</returns>
        protected abstract Environment CreateEnvironment();

        /// <summary>
        /// Destroy the environment which is managed by the add-in, called once when the add-in is deactivated
        /// </summary>
        protected abstract void DestroyEnvironment();

        /// <summary>
        /// Fires when the environment managed by the add-in is opened
        /// </summary>
        protected abstract void OnEnvironmentOpen();

        /// <summary>
        /// Fires when the environment managed by the add-in is closed
        /// </summary>
        protected abstract void OnEnvironmentClose();

        /// <summary>
        /// Fires when the environment managed by the add-in is focused
        /// </summary>
        protected abstract void OnEnvironmentShow();

        /// <summary>
        /// Fires when the environment managed by the add-in is hidden
        /// </summary>
        protected abstract void OnEnvironmentHide();
        
        /// <summary>
        /// Checks whether the document in which the user has attempted to open the environment is supported
        /// </summary>
        /// <param name="document">The document to be checked</param>
        /// <returns>If the document type is supported by the add-in</returns>
        protected abstract bool IsDocumentSupported(Document document);

        private Environment environment;

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
            {
                MessageBox.Show("The Synthesis robot exporter add-in has been installed.\nTo access the exporter, select the \"Robot Export\" button under the \"Environments\" tab.", "Synthesis Add-In", MessageBoxButtons.OK);
                Settings.Default.ShowFirstLaunchInfo = false;
                Settings.Default.Save();
            }

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
            return !EnvironmentOpen && documentObject is Document document && IsDocumentSupported(document);
        }

        private bool IsDocumentOpenInTheExporter(object documentObject)
        {
            return EnvironmentOpen && OpenDocument != null && documentObject is Document document && document == OpenDocument;
        }

        public void ExecuteCommand(int commandId) {}
        public object Automation => null;
    }
}