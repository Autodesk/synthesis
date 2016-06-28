using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Inventor;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using DSNodeServices;
using Dynamo.Models;
using Dynamo.Utilities;
using InventorLibrary.GeometryConversion;
using InventorServices.Persistence;

namespace InventorLibrary.API
{
    [IsVisibleInDynamoLibrary(false)]
    public class InvApplication
    {
        #region Internal properties
        internal Inventor.Application InternalApplication { get; set; }

        //internal Inv_AppPerformanceMonitor Internal_AppPerformanceMonitor
        //{
        //    get { return Inv_AppPerformanceMonitor.ByInv_AppPerformanceMonitor(ApplicationInstance._AppPerformanceMonitor); }
        //}


        internal bool Internal_CanReplayTranscript
        {
            get { return ApplicationInstance._CanReplayTranscript; }
        }

        //internal InvDebugInstrumentation Internal_DebugInstrumentation
        //{
        //    get { return InvDebugInstrumentation.ByInvDebugInstrumentation(ApplicationInstance._DebugInstrumentation); }
        //}


        internal string Internal_TranscriptFileName
        {
            get { return ApplicationInstance._TranscriptFileName; }
        }

        //internal InvColorScheme InternalActiveColorScheme
        //{
        //    get { return InvColorScheme.ByInvColorScheme(ApplicationInstance.ActiveColorScheme); }
        //}

        //this is what is being generated '_Document' rather than 'Document'
        //internal Inv_Document InternalActiveDocument
        //{
        //    get { return Inv_Document.ByInv_Document(ApplicationInstance.ActiveDocument); }
        //}

        internal InvDocument InternalActiveDocument
        {
            get 
            {
                return InvDocument.ByInvDocument(ApplicationInstance.ActiveDocument);
            }
        }


        internal InvDocumentTypeEnum InternalActiveDocumentType
        {
            get { return ApplicationInstance.ActiveDocumentType.As<InvDocumentTypeEnum>(); }
        }


        //internal Inv_Document InternalActiveEditDocument
        //{
        //    get { return Inv_Document.ByInv_Document(ApplicationInstance.ActiveEditDocument); }
        //}

        //this is what is being generated '_Document' rather than 'Document'
        internal InvDocument InternalActiveEditDocument
        {
            get
            {
                return InvDocument.ByInvDocument(ApplicationInstance.ActiveEditDocument);
            }
        }

        internal Object InternalActiveEditObject
        {
            get { return ApplicationInstance.ActiveEditObject; }
        }

        //internal InvEnvironment InternalActiveEnvironment
        //{
        //    get { return InvEnvironment.ByInvEnvironment(ApplicationInstance.ActiveEnvironment); }
        //}


        //internal InvView InternalActiveView
        //{
        //    get { return InvView.ByInvView(ApplicationInstance.ActiveView); }
        //}


        internal string InternalAllUsersAppDataPath
        {
            get { return ApplicationInstance.AllUsersAppDataPath; }
        }

        //internal InvApplicationAddIns InternalApplicationAddIns
        //{
        //    get { return InvApplicationAddIns.ByInvApplicationAddIns(ApplicationInstance.ApplicationAddIns); }
        //}


        //internal InvApplicationEvents InternalApplicationEvents
        //{
        //    get { return InvApplicationEvents.ByInvApplicationEvents(ApplicationInstance.ApplicationEvents); }
        //}


        //internal InvAssemblyEvents InternalAssemblyEvents
        //{
        //    get { return InvAssemblyEvents.ByInvAssemblyEvents(ApplicationInstance.AssemblyEvents); }
        //}


        //internal InvAssemblyOptions InternalAssemblyOptions
        //{
        //    get { return InvAssemblyOptions.ByInvAssemblyOptions(ApplicationInstance.AssemblyOptions); }
        //}


        //internal InvAssetLibraries InternalAssetLibraries
        //{
        //    get { return InvAssetLibraries.ByInvAssetLibraries(ApplicationInstance.AssetLibraries); }
        //}


        //internal InvCameraEvents InternalCameraEvents
        //{
        //    get { return InvCameraEvents.ByInvCameraEvents(ApplicationInstance.CameraEvents); }
        //}


        //internal InvChangeManager InternalChangeManager
        //{
        //    get { return InvChangeManager.ByInvChangeManager(ApplicationInstance.ChangeManager); }
        //}


        //internal InvColorSchemes InternalColorSchemes
        //{
        //    get { return InvColorSchemes.ByInvColorSchemes(ApplicationInstance.ColorSchemes); }
        //}


        //internal InvCommandManager InternalCommandManager
        //{
        //    get { return InvCommandManager.ByInvCommandManager(ApplicationInstance.CommandManager); }
        //}


        //internal InvContentCenter InternalContentCenter
        //{
        //    get { return InvContentCenter.ByInvContentCenter(ApplicationInstance.ContentCenter); }
        //}


        //internal InvContentCenterOptions InternalContentCenterOptions
        //{
        //    get { return InvContentCenterOptions.ByInvContentCenterOptions(ApplicationInstance.ContentCenterOptions); }
        //}


        internal string InternalCurrentUserAppDataPath
        {
            get { return ApplicationInstance.CurrentUserAppDataPath; }
        }

        //internal InvDesignProjectManager InternalDesignProjectManager
        //{
        //    get { return InvDesignProjectManager.ByInvDesignProjectManager(ApplicationInstance.DesignProjectManager); }
        //}


        //internal InvDisplayOptions InternalDisplayOptions
        //{
        //    get { return InvDisplayOptions.ByInvDisplayOptions(ApplicationInstance.DisplayOptions); }
        //}


        internal InvDocuments InternalDocuments
        {
            get { return InvDocuments.ByInvDocuments(ApplicationInstance.Documents); }
        }


        //internal InvDrawingOptions InternalDrawingOptions
        //{
        //    get { return InvDrawingOptions.ByInvDrawingOptions(ApplicationInstance.DrawingOptions); }
        //}


        //internal InvEnvironmentBaseCollection InternalEnvironmentBaseCollection
        //{
        //    get { return InvEnvironmentBaseCollection.ByInvEnvironmentBaseCollection(ApplicationInstance.EnvironmentBaseCollection); }
        //}


        //internal InvEnvironments InternalEnvironments
        //{
        //    get { return InvEnvironments.ByInvEnvironments(ApplicationInstance.Environments); }
        //}


        //internal InvErrorManager InternalErrorManager
        //{
        //    get { return InvErrorManager.ByInvErrorManager(ApplicationInstance.ErrorManager); }
        //}


        internal InvAssetsEnumerator InternalFavoriteAssets
        {
            get { return InvAssetsEnumerator.ByInvAssetsEnumerator(ApplicationInstance.FavoriteAssets); }
        }


        //internal InvFileAccessEvents InternalFileAccessEvents
        //{
        //    get { return InvFileAccessEvents.ByInvFileAccessEvents(ApplicationInstance.FileAccessEvents); }
        //}


        //internal InvFileLocations InternalFileLocations
        //{
        //    get { return InvFileLocations.ByInvFileLocations(ApplicationInstance.FileLocations); }
        //}


        //internal InvFileManager InternalFileManager
        //{
        //    get { return InvFileManager.ByInvFileManager(ApplicationInstance.FileManager); }
        //}


        //internal InvFileOptions InternalFileOptions
        //{
        //    get { return InvFileOptions.ByInvFileOptions(ApplicationInstance.FileOptions); }
        //}


        //internal InvFileUIEvents InternalFileUIEvents
        //{
        //    get { return InvFileUIEvents.ByInvFileUIEvents(ApplicationInstance.FileUIEvents); }
        //}


        //internal InvGeneralOptions InternalGeneralOptions
        //{
        //    get { return InvGeneralOptions.ByInvGeneralOptions(ApplicationInstance.GeneralOptions); }
        //}


        //internal InvHardwareOptions InternalHardwareOptions
        //{
        //    get { return InvHardwareOptions.ByInvHardwareOptions(ApplicationInstance.HardwareOptions); }
        //}


        //internal InvHelpManager InternalHelpManager
        //{
        //    get { return InvHelpManager.ByInvHelpManager(ApplicationInstance.HelpManager); }
        //}


        //internal InviFeatureOptions InternaliFeatureOptions
        //{
        //    get { return InviFeatureOptions.ByInviFeatureOptions(ApplicationInstance.iFeatureOptions); }
        //}


        internal string InternalInstallPath
        {
            get { return ApplicationInstance.InstallPath; }
        }

        internal bool InternalIsCIPEnabled
        {
            get { return ApplicationInstance.IsCIPEnabled; }
        }

        internal string InternalLanguageCode
        {
            get { return ApplicationInstance.LanguageCode; }
        }

        internal string InternalLanguageName
        {
            get { return ApplicationInstance.LanguageName; }
        }

        //internal InvLanguageTools InternalLanguageTools
        //{
        //    get { return InvLanguageTools.ByInvLanguageTools(ApplicationInstance.LanguageTools); }
        //}


        internal int InternalLocale
        {
            get { return ApplicationInstance.Locale; }
        }

        internal int InternalMainFrameHWND
        {
            get { return ApplicationInstance.MainFrameHWND; }
        }

        //internal InvMeasureTools InternalMeasureTools
        //{
        //    get { return InvMeasureTools.ByInvMeasureTools(ApplicationInstance.MeasureTools); }
        //}


        //internal InvModelingEvents InternalModelingEvents
        //{
        //    get { return InvModelingEvents.ByInvModelingEvents(ApplicationInstance.ModelingEvents); }
        //}


        //internal InvNotebookOptions InternalNotebookOptions
        //{
        //    get { return InvNotebookOptions.ByInvNotebookOptions(ApplicationInstance.NotebookOptions); }
        //}


        //internal InvPartOptions InternalPartOptions
        //{
        //    get { return InvPartOptions.ByInvPartOptions(ApplicationInstance.PartOptions); }
        //}


        //internal InvObjectsEnumeratorByVariant InternalPreferences
        //{
        //    get { return InvObjectsEnumeratorByVariant.ByInvObjectsEnumeratorByVariant(ApplicationInstance.Preferences); }
        //}


        //internal InvPresentationOptions InternalPresentationOptions
        //{
        //    get { return InvPresentationOptions.ByInvPresentationOptions(ApplicationInstance.PresentationOptions); }
        //}


        internal bool InternalReady
        {
            get { return ApplicationInstance.Ready; }
        }

        //internal InvReferenceKeyEvents InternalReferenceKeyEvents
        //{
        //    get { return InvReferenceKeyEvents.ByInvReferenceKeyEvents(ApplicationInstance.ReferenceKeyEvents); }
        //}


        //internal InvRepresentationEvents InternalRepresentationEvents
        //{
        //    get { return InvRepresentationEvents.ByInvRepresentationEvents(ApplicationInstance.RepresentationEvents); }
        //}


        //internal InvSaveOptions InternalSaveOptions
        //{
        //    get { return InvSaveOptions.ByInvSaveOptions(ApplicationInstance.SaveOptions); }
        //}


        //internal InvSketch3DOptions InternalSketch3DOptions
        //{
        //    get { return InvSketch3DOptions.ByInvSketch3DOptions(ApplicationInstance.Sketch3DOptions); }
        //}


        //internal InvSketchEvents InternalSketchEvents
        //{
        //    get { return InvSketchEvents.ByInvSketchEvents(ApplicationInstance.SketchEvents); }
        //}


        //internal InvSketchOptions InternalSketchOptions
        //{
        //    get { return InvSketchOptions.ByInvSketchOptions(ApplicationInstance.SketchOptions); }
        //}


        internal InvSoftwareVersion InternalSoftwareVersion
        {
            get { return InvSoftwareVersion.ByInvSoftwareVersion(ApplicationInstance.SoftwareVersion); }
        }


        //internal InvStyleEvents InternalStyleEvents
        //{
        //    get { return InvStyleEvents.ByInvStyleEvents(ApplicationInstance.StyleEvents); }
        //}


        //internal InvStylesManager InternalStylesManager
        //{
        //    get { return InvStylesManager.ByInvStylesManager(ApplicationInstance.StylesManager); }
        //}


        //internal InvStatusEnum InternalSubscriptionStatus
        //{
        //    get { return InvStatusEnum.ByInvStatusEnum(ApplicationInstance.SubscriptionStatus); }
        //}


        //internal InvTestManager InternalTestManager
        //{
        //    get { return InvTestManager.ByInvTestManager(ApplicationInstance.TestManager); }
        //}


        //internal InvTransactionManager InternalTransactionManager
        //{
        //    get { return InvTransactionManager.ByInvTransactionManager(ApplicationInstance.TransactionManager); }
        //}


        //internal InvTransientBRep InternalTransientBRep
        //{
        //    get { return InvTransientBRep.ByInvTransientBRep(ApplicationInstance.TransientBRep); }
        //}


        //internal InvTransientGeometry InternalTransientGeometry
        //{
        //    get { return InvTransientGeometry.ByInvTransientGeometry(ApplicationInstance.TransientGeometry); }
        //}


        //internal InvTransientObjects InternalTransientObjects
        //{
        //    get { return InvTransientObjects.ByInvTransientObjects(ApplicationInstance.TransientObjects); }
        //}


        internal InvObjectTypeEnum InternalType
        {
            get { return ApplicationInstance.Type.As<InvObjectTypeEnum>(); }
        }


        internal InvUnitsOfMeasure InternalUnitsOfMeasure
        {
            get { return InvUnitsOfMeasure.ByInvUnitsOfMeasure(ApplicationInstance.UnitsOfMeasure); }
        }


        //internal InvUserInterfaceManager InternalUserInterfaceManager
        //{
        //    get { return InvUserInterfaceManager.ByInvUserInterfaceManager(ApplicationInstance.UserInterfaceManager); }
        //}


        //internal InvInventorVBAProjects InternalVBAProjects
        //{
        //    get { return InvInventorVBAProjects.ByInvInventorVBAProjects(ApplicationInstance.VBAProjects); }
        //}


        internal Object InternalVBE
        {
            get { return ApplicationInstance.VBE; }
        }

        //internal InvViewsEnumerator InternalViews
        //{
        //    get { return InvViewsEnumerator.ByInvViewsEnumerator(ApplicationInstance.Views); }
        //}


        internal bool Internal_ForceMigrationOnOpen { get; set; }

        internal bool Internal_HarvestStylesOnOpen { get; set; }

        internal bool Internal_PurgeStylesOnOpen { get; set; }

        internal bool Internal_SuppressFileDirtyEvents { get; set; }

        internal Object Internal_TestIO { get; set; }

        internal bool Internal_TranscriptAPIChanges { get; set; }

        internal AssetLibrary InternalActiveAppearanceLibrary { get; set; }

        internal AssetLibrary InternalActiveMaterialLibrary { get; set; }

        internal bool InternalCameraRollMode3Dx { get; set; }

        internal string InternalCaption { get; set; }

        internal bool InternalFlythroughMode3Dx { get; set; }

        internal int InternalHeight { get; set; }

        internal int InternalLeft { get; set; }

        internal MaterialDisplayUnitsEnum InternalMaterialDisplayUnits { get; set; }

        internal bool InternalMRUDisplay { get; set; }

        internal bool InternalMRUEnabled { get; set; }

        internal bool InternalOpenDocumentsDisplay { get; set; }

        internal bool InternalScreenUpdating { get; set; }

        internal bool InternalSilentOperation { get; set; }

        internal string InternalStatusBarText { get; set; }

        internal bool InternalSupportsFileManagement { get; set; }

        internal int InternalTop { get; set; }

        internal string InternalUserName { get; set; }

        internal bool InternalVisible { get; set; }

        internal int InternalWidth { get; set; }

        internal WindowsSizeEnum InternalWindowState { get; set; }
        #endregion

        #region Private constructors
        private InvApplication(InvApplication invApplication)
        {
            InternalApplication = invApplication.InternalApplication;
            
            //get a reference to the active application obtained in addin startup
            Inventor.Application invApp = InternalApplication;


            AssemblyDocument assDoc = (AssemblyDocument)invApp.ActiveDocument;

            Inventor.Point transPoint = invApp.TransientGeometry.CreatePoint(0,0,0);

            WorkPoint wp = assDoc.ComponentDefinition.WorkPoints.AddFixed(transPoint, false);

            //get the active document
            Document activeDoc = invApp.ActiveDocument;

            //cast to the right type
            AssemblyDocument assemblyDocument = (AssemblyDocument)activeDoc;

            //get the ComponentDefinition
            AssemblyComponentDefinition assCompDef = (AssemblyComponentDefinition)assemblyDocument.ComponentDefinition;

            //get the WorkPoints collection
            WorkPoints workPoints= assCompDef.WorkPoints;

            //create an Inventor.Point transient geometry object
            Inventor.Point transientPoint = invApp.TransientGeometry.CreatePoint(0, 0, 0);

            //add WorkPoint 
            WorkPoint workPoint = workPoints.AddFixed(transientPoint, false);































        }

        private InvApplication(Inventor.Application invApplication)
        {
            InternalApplication = invApplication;
        }
        #endregion

        #region Private methods
        private void InternalConstructInternalNameAndRevisionId(string internalNameToken, string revisionIdToken, out string internalName, out string revisionId)
        {
            ApplicationInstance.ConstructInternalNameAndRevisionId( internalNameToken,  revisionIdToken, out  internalName, out  revisionId);
        }

        private void InternalCreateFileDialog(out FileDialog dialog)
        {
            ApplicationInstance.CreateFileDialog(out  dialog);
        }

        private void InternalCreateInternalName(string name, string number, string custom, out string internalName)
        {
            ApplicationInstance.CreateInternalName( name,  number,  custom, out  internalName);
        }

        private ProgressBar InternalCreateProgressBar(bool displayInStatusBar, int numberOfSteps, string title, bool allowCancel, int hWND)
        {
            return ApplicationInstance.CreateProgressBar( displayInStatusBar,  numberOfSteps,  title,  allowCancel,  hWND);
        }

        private void InternalGetAppFrameExtents(out int top, out int left, out int height, out int width)
        {
            ApplicationInstance.GetAppFrameExtents(out  top, out  left, out  height, out  width);
        }

        private Object InternalGetInterfaceObject(string progIDorCLSID)
        {
            return ApplicationInstance.GetInterfaceObject( progIDorCLSID);
        }

        private Object InternalGetInterfaceObject32(string progIDorCLSID)
        {
            return ApplicationInstance.GetInterfaceObject32( progIDorCLSID);
        }

        private string InternalGetTemplateFile(DocumentTypeEnum documentType, SystemOfMeasureEnum systemOfMeasure, DraftingStandardEnum draftingStandard, Object documentSubType)
        {
            return ApplicationInstance.GetTemplateFile( documentType,  systemOfMeasure,  draftingStandard,  documentSubType);
        }

        private string InternalLicenseChallenge()
        {
            return ApplicationInstance.LicenseChallenge();
        }

        private void InternalLicenseResponse(string response)
        {
            ApplicationInstance.LicenseResponse( response);
        }

        private bool InternalLogin()
        {
            return ApplicationInstance.Login();
        }

        private void InternalMove(int top, int left, int height, int width)
        {
            ApplicationInstance.Move( top,  left,  height,  width);
        }

        private void InternalQuit()
        {
            ApplicationInstance.Quit();
        }

        private void InternalReserveLicense(string clientId)
        {
            ApplicationInstance.ReserveLicense( clientId);
        }

        private void InternalUnreserveLicense(string clientId)
        {
            ApplicationInstance.UnreserveLicense( clientId);
        }

        private void InternalWriteCIPWaypoint(string waypointData)
        {
            ApplicationInstance.WriteCIPWaypoint( waypointData);
        }

        #endregion

        #region Public properties
        public Inventor.Application ApplicationInstance
        {
            get { return InternalApplication; }
            set { InternalApplication = value; }
        }

        //public Inv_AppPerformanceMonitor _AppPerformanceMonitor
        //{
        //    get { return Internal_AppPerformanceMonitor; }
        //}

        public bool _CanReplayTranscript
        {
            get { return Internal_CanReplayTranscript; }
        }

        //public InvDebugInstrumentation _DebugInstrumentation
        //{
        //    get { return Internal_DebugInstrumentation; }
        //}

        public string _TranscriptFileName
        {
            get { return Internal_TranscriptFileName; }
        }

        //public InvColorScheme ActiveColorScheme
        //{
        //    get { return InternalActiveColorScheme; }
        //}

        ////public Inv_Document ActiveDocument
        ////{
        ////    get { return InternalActiveDocument; }
        ////}

        public InvDocument ActiveDocument
        {
            get { return InternalActiveDocument; }
        }

        public InvDocumentTypeEnum ActiveDocumentType
        {
            get { return InternalActiveDocumentType; }
        }

        ////public Inv_Document ActiveEditDocument
        ////{
        ////    get { return InternalActiveEditDocument; }
        ////}

        public InvDocument ActiveEditDocument
        {
            get { return InternalActiveEditDocument; }
        }

        public Object ActiveEditObject
        {
            get { return InternalActiveEditObject; }
        }

        //public InvEnvironment ActiveEnvironment
        //{
        //    get { return InternalActiveEnvironment; }
        //}

        //public InvView ActiveView
        //{
        //    get { return InternalActiveView; }
        //}

        public string AllUsersAppDataPath
        {
            get { return InternalAllUsersAppDataPath; }
        }

        //public InvApplicationAddIns ApplicationAddIns
        //{
        //    get { return InternalApplicationAddIns; }
        //}

        //public InvApplicationEvents ApplicationEvents
        //{
        //    get { return InternalApplicationEvents; }
        //}

        //public InvAssemblyEvents AssemblyEvents
        //{
        //    get { return InternalAssemblyEvents; }
        //}

        //public InvAssemblyOptions AssemblyOptions
        //{
        //    get { return InternalAssemblyOptions; }
        //}

        //public InvAssetLibraries AssetLibraries
        //{
        //    get { return InternalAssetLibraries; }
        //}

        //public InvCameraEvents CameraEvents
        //{
        //    get { return InternalCameraEvents; }
        //}

        //public InvChangeManager ChangeManager
        //{
        //    get { return InternalChangeManager; }
        //}

        //public InvColorSchemes ColorSchemes
        //{
        //    get { return InternalColorSchemes; }
        //}

        //public InvCommandManager CommandManager
        //{
        //    get { return InternalCommandManager; }
        //}

        //public InvContentCenter ContentCenter
        //{
        //    get { return InternalContentCenter; }
        //}

        //public InvContentCenterOptions ContentCenterOptions
        //{
        //    get { return InternalContentCenterOptions; }
        //}

        public string CurrentUserAppDataPath
        {
            get { return InternalCurrentUserAppDataPath; }
        }

        //public InvDesignProjectManager DesignProjectManager
        //{
        //    get { return InternalDesignProjectManager; }
        //}

        //public InvDisplayOptions DisplayOptions
        //{
        //    get { return InternalDisplayOptions; }
        //}

        public InvDocuments Documents
        {
            get { return InternalDocuments; }
        }

        //public InvDrawingOptions DrawingOptions
        //{
        //    get { return InternalDrawingOptions; }
        //}

        //public InvEnvironmentBaseCollection EnvironmentBaseCollection
        //{
        //    get { return InternalEnvironmentBaseCollection; }
        //}

        //public InvEnvironments Environments
        //{
        //    get { return InternalEnvironments; }
        //}

        //public InvErrorManager ErrorManager
        //{
        //    get { return InternalErrorManager; }
        //}

        public InvAssetsEnumerator FavoriteAssets
        {
            get { return InternalFavoriteAssets; }
        }

        //public InvFileAccessEvents FileAccessEvents
        //{
        //    get { return InternalFileAccessEvents; }
        //}

        //public InvFileLocations FileLocations
        //{
        //    get { return InternalFileLocations; }
        //}

        //public InvFileManager FileManager
        //{
        //    get { return InternalFileManager; }
        //}

        //public InvFileOptions FileOptions
        //{
        //    get { return InternalFileOptions; }
        //}

        //public InvFileUIEvents FileUIEvents
        //{
        //    get { return InternalFileUIEvents; }
        //}

        //public InvGeneralOptions GeneralOptions
        //{
        //    get { return InternalGeneralOptions; }
        //}

        //public InvHardwareOptions HardwareOptions
        //{
        //    get { return InternalHardwareOptions; }
        //}

        //public InvHelpManager HelpManager
        //{
        //    get { return InternalHelpManager; }
        //}

        //public InviFeatureOptions iFeatureOptions
        //{
        //    get { return InternaliFeatureOptions; }
        //}

        public string InstallPath
        {
            get { return InternalInstallPath; }
        }

        public bool IsCIPEnabled
        {
            get { return InternalIsCIPEnabled; }
        }

        public string LanguageCode
        {
            get { return InternalLanguageCode; }
        }

        public string LanguageName
        {
            get { return InternalLanguageName; }
        }

        //public InvLanguageTools LanguageTools
        //{
        //    get { return InternalLanguageTools; }
        //}

        public int Locale
        {
            get { return InternalLocale; }
        }

        public int MainFrameHWND
        {
            get { return InternalMainFrameHWND; }
        }

        //public InvMeasureTools MeasureTools
        //{
        //    get { return InternalMeasureTools; }
        //}

        //public InvModelingEvents ModelingEvents
        //{
        //    get { return InternalModelingEvents; }
        //}

        //public InvNotebookOptions NotebookOptions
        //{
        //    get { return InternalNotebookOptions; }
        //}

        //public InvPartOptions PartOptions
        //{
        //    get { return InternalPartOptions; }
        //}

        //public InvObjectsEnumeratorByVariant Preferences
        //{
        //    get { return InternalPreferences; }
        //}

        //public InvPresentationOptions PresentationOptions
        //{
        //    get { return InternalPresentationOptions; }
        //}

        public bool Ready
        {
            get { return InternalReady; }
        }

        //public InvReferenceKeyEvents ReferenceKeyEvents
        //{
        //    get { return InternalReferenceKeyEvents; }
        //}

        //public InvRepresentationEvents RepresentationEvents
        //{
        //    get { return InternalRepresentationEvents; }
        //}

        //public InvSaveOptions SaveOptions
        //{
        //    get { return InternalSaveOptions; }
        //}

        //public InvSketch3DOptions Sketch3DOptions
        //{
        //    get { return InternalSketch3DOptions; }
        //}

        //public InvSketchEvents SketchEvents
        //{
        //    get { return InternalSketchEvents; }
        //}

        //public InvSketchOptions SketchOptions
        //{
        //    get { return InternalSketchOptions; }
        //}

        public InvSoftwareVersion SoftwareVersion
        {
            get { return InternalSoftwareVersion; }
        }

        //public InvStyleEvents StyleEvents
        //{
        //    get { return InternalStyleEvents; }
        //}

        //public InvStylesManager StylesManager
        //{
        //    get { return InternalStylesManager; }
        //}

        //public InvStatusEnum SubscriptionStatus
        //{
        //    get { return InternalSubscriptionStatus; }
        //}

        //public InvTestManager TestManager
        //{
        //    get { return InternalTestManager; }
        //}

        //public InvTransactionManager TransactionManager
        //{
        //    get { return InternalTransactionManager; }
        //}

        //public InvTransientBRep TransientBRep
        //{
        //    get { return InternalTransientBRep; }
        //}

        //public InvTransientGeometry TransientGeometry
        //{
        //    get { return InternalTransientGeometry; }
        //}

        //public InvTransientObjects TransientObjects
        //{
        //    get { return InternalTransientObjects; }
        //}

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public InvUnitsOfMeasure UnitsOfMeasure
        {
            get { return InternalUnitsOfMeasure; }
        }

        //public InvUserInterfaceManager UserInterfaceManager
        //{
        //    get { return InternalUserInterfaceManager; }
        //}

        //public InvInventorVBAProjects VBAProjects
        //{
        //    get { return InternalVBAProjects; }
        //}

        public Object VBE
        {
            get { return InternalVBE; }
        }

        //public InvViewsEnumerator Views
        //{
        //    get { return InternalViews; }
        //}

        public bool _ForceMigrationOnOpen
        {
            get { return Internal_ForceMigrationOnOpen; }
            set { Internal_ForceMigrationOnOpen = value; }
        }

        public bool _HarvestStylesOnOpen
        {
            get { return Internal_HarvestStylesOnOpen; }
            set { Internal_HarvestStylesOnOpen = value; }
        }

        public bool _PurgeStylesOnOpen
        {
            get { return Internal_PurgeStylesOnOpen; }
            set { Internal_PurgeStylesOnOpen = value; }
        }

        public bool _SuppressFileDirtyEvents
        {
            get { return Internal_SuppressFileDirtyEvents; }
            set { Internal_SuppressFileDirtyEvents = value; }
        }

        public Object _TestIO
        {
            get { return Internal_TestIO; }
            set { Internal_TestIO = value; }
        }

        public bool _TranscriptAPIChanges
        {
            get { return Internal_TranscriptAPIChanges; }
            set { Internal_TranscriptAPIChanges = value; }
        }

        //public InvAssetLibrary ActiveAppearanceLibrary
        //{
        //    get { return InternalActiveAppearanceLibrary; }
        //    set { InternalActiveAppearanceLibrary = value; }
        //}

        //public InvAssetLibrary ActiveMaterialLibrary
        //{
        //    get { return InternalActiveMaterialLibrary; }
        //    set { InternalActiveMaterialLibrary = value; }
        //}

        public bool CameraRollMode3Dx
        {
            get { return InternalCameraRollMode3Dx; }
            set { InternalCameraRollMode3Dx = value; }
        }

        public string Caption
        {
            get { return InternalCaption; }
            set { InternalCaption = value; }
        }

        public bool FlythroughMode3Dx
        {
            get { return InternalFlythroughMode3Dx; }
            set { InternalFlythroughMode3Dx = value; }
        }

        public int Height
        {
            get { return InternalHeight; }
            set { InternalHeight = value; }
        }

        public int Left
        {
            get { return InternalLeft; }
            set { InternalLeft = value; }
        }

        //public InvMaterialDisplayUnitsEnum MaterialDisplayUnits
        //{
        //    get { return InternalMaterialDisplayUnits; }
        //    set { InternalMaterialDisplayUnits = value; }
        //}

        public bool MRUDisplay
        {
            get { return InternalMRUDisplay; }
            set { InternalMRUDisplay = value; }
        }

        public bool MRUEnabled
        {
            get { return InternalMRUEnabled; }
            set { InternalMRUEnabled = value; }
        }

        public bool OpenDocumentsDisplay
        {
            get { return InternalOpenDocumentsDisplay; }
            set { InternalOpenDocumentsDisplay = value; }
        }

        public bool ScreenUpdating
        {
            get { return InternalScreenUpdating; }
            set { InternalScreenUpdating = value; }
        }

        public bool SilentOperation
        {
            get { return InternalSilentOperation; }
            set { InternalSilentOperation = value; }
        }

        public string StatusBarText
        {
            get { return InternalStatusBarText; }
            set { InternalStatusBarText = value; }
        }

        public bool SupportsFileManagement
        {
            get { return InternalSupportsFileManagement; }
            set { InternalSupportsFileManagement = value; }
        }

        public int Top
        {
            get { return InternalTop; }
            set { InternalTop = value; }
        }

        public string UserName
        {
            get { return InternalUserName; }
            set { InternalUserName = value; }
        }

        public bool Visible
        {
            get { return InternalVisible; }
            set { InternalVisible = value; }
        }

        public int Width
        {
            get { return InternalWidth; }
            set { InternalWidth = value; }
        }

        //public InvWindowsSizeEnum WindowState
        //{
        //    get { return InternalWindowState; }
        //    set { InternalWindowState = value; }
        //}

        #endregion

        #region Public static constructors
        //public static InvApplication ByInvApplication(InvApplication invApplication)
        //{
        //    return new InvApplication(invApplication);
        //}

        //public static InvApplication ByInvApplication(Inventor.Application invApplication)
        //{
        //    return new InvApplication(invApplication);
        //}

        public static InvApplication GetInvApplication()
        {           
            return new InvApplication(PersistenceManager.InventorApplication);
        }
        #endregion

        #region Public methods
        public void ConstructInternalNameAndRevisionId(string internalNameToken, string revisionIdToken, out string internalName, out string revisionId)
        {
            InternalConstructInternalNameAndRevisionId( internalNameToken,  revisionIdToken, out  internalName, out  revisionId);
        }

        public void CreateFileDialog(out FileDialog dialog)
        {
            InternalCreateFileDialog(out  dialog);
        }

        public void CreateInternalName(string name, string number, string custom, out string internalName)
        {
            InternalCreateInternalName( name,  number,  custom, out  internalName);
        }

        ////public InvProgressBar CreateProgressBar(bool displayInStatusBar, int numberOfSteps, string title, bool allowCancel, int hWND)
        ////{
        ////    return InternalCreateProgressBar( displayInStatusBar,  numberOfSteps,  title,  allowCancel,  hWND);
        ////}

        public void GetAppFrameExtents(out int top, out int left, out int height, out int width)
        {
            InternalGetAppFrameExtents(out  top, out  left, out  height, out  width);
        }

        public Object GetInterfaceObject(string progIDorCLSID)
        {
            return InternalGetInterfaceObject( progIDorCLSID);
        }

        public Object GetInterfaceObject32(string progIDorCLSID)
        {
            return InternalGetInterfaceObject32( progIDorCLSID);
        }

        public string GetTemplateFile(DocumentTypeEnum documentType, SystemOfMeasureEnum systemOfMeasure, DraftingStandardEnum draftingStandard, Object documentSubType)
        {
            return InternalGetTemplateFile( documentType,  systemOfMeasure,  draftingStandard,  documentSubType);
        }

        public string LicenseChallenge()
        {
            return InternalLicenseChallenge();
        }

        public void LicenseResponse(string response)
        {
            InternalLicenseResponse( response);
        }

        public bool Login()
        {
            return InternalLogin();
        }

        public void Move(int top, int left, int height, int width)
        {
            InternalMove( top,  left,  height,  width);
        }

        public void Quit()
        {
            InternalQuit();
        }

        public void ReserveLicense(string clientId)
        {
            InternalReserveLicense( clientId);
        }

        public void UnreserveLicense(string clientId)
        {
            InternalUnreserveLicense( clientId);
        }

        public void WriteCIPWaypoint(string waypointData)
        {
            InternalWriteCIPWaypoint( waypointData);
        }

        #endregion
    }
}
