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
    public class InvAssemblyDocument// : InvDocument
    {
        #region Internal properties
        internal Inventor.AssemblyDocument InternalAssemblyDocument { get; set; }

        internal int Internal_ComatoseNodesCount
        {
            get { return AssemblyDocumentInstance._ComatoseNodesCount; }
        }

        internal InvCommandIDEnum Internal_DefaultCommand
        {
            get { return InvCommandIDEnum.ByInvCommandIDEnum(AssemblyDocumentInstance._DefaultCommand); }
        }

        internal Inv_DocPerformanceMonitor Internal_DocPerformanceMonitor
        {
            get { return Inv_DocPerformanceMonitor.ByInv_DocPerformanceMonitor(AssemblyDocumentInstance._DocPerformanceMonitor); }
        }

        internal string Internal_InternalName
        {
            get { return AssemblyDocumentInstance._InternalName; }
        }

        internal string Internal_PrimaryDeselGUID
        {
            get { return AssemblyDocumentInstance._PrimaryDeselGUID; }
        }

        internal int Internal_SickNodesCount
        {
            get { return AssemblyDocumentInstance._SickNodesCount; }
        }

        internal Object InternalActivatedObject
        {
            get { return AssemblyDocumentInstance.ActivatedObject; }
        }

        internal InvDocumentsEnumerator InternalAllReferencedDocuments
        {
            get { return InvDocumentsEnumerator.ByInvDocumentsEnumerator(AssemblyDocumentInstance.AllReferencedDocuments); }
        }

        internal InvAssetsEnumerator InternalAppearanceAssets
        {
            get { return InvAssetsEnumerator.ByInvAssetsEnumerator(AssemblyDocumentInstance.AppearanceAssets); }
        }

        internal InvAssets InternalAssets
        {
            get { return InvAssets.ByInvAssets(AssemblyDocumentInstance.Assets); }
        }

        internal InvAttributeManager InternalAttributeManager
        {
            get { return InvAttributeManager.ByInvAttributeManager(AssemblyDocumentInstance.AttributeManager); }
        }

        internal InvAttributeSets InternalAttributeSets
        {
            get { return InvAttributeSets.ByInvAttributeSets(AssemblyDocumentInstance.AttributeSets); }
        }

        //No type info to reflect
        //internal InvBrowserPanes InternalBrowserPanes
        //{
        //    get { return InvBrowserPanes.ByInvBrowserPanes(AssemblyDocumentInstance.BrowserPanes); }
        //}

        //internal InvCachedGraphicsStatusEnum InternalCachedGraphicsStatus
        //{
        //    get { return InvCachedGraphicsStatusEnum.ByInvCachedGraphicsStatusEnum(AssemblyDocumentInstance.CachedGraphicsStatus); }
        //}

        internal bool InternalCompacted
        {
            get { return AssemblyDocumentInstance.Compacted; }
        }

        internal InvAssemblyComponentDefinition InternalComponentDefinition
        {
            get { return InvAssemblyComponentDefinition.ByInvAssemblyComponentDefinition(AssemblyDocumentInstance.ComponentDefinition); }
        }

        internal InvAssemblyComponentDefinitions InternalComponentDefinitions
        {
            get { return InvAssemblyComponentDefinitions.ByInvAssemblyComponentDefinitions(AssemblyDocumentInstance.ComponentDefinitions); }
        }

        //internal string InternalDatabaseRevisionId
        //{
        //    get { return AssemblyDocumentInstance.DatabaseRevisionId; }
        //}

        //internal string InternalDefaultCommand
        //{
        //    get { return AssemblyDocumentInstance.DefaultCommand; }
        //}

        internal string InternalDesignViewInfo
        {
            get { return AssemblyDocumentInstance.DesignViewInfo; }
        }

        internal InvDisabledCommandList InternalDisabledCommandList
        {
            get { return InvDisabledCommandList.ByInvDisabledCommandList(AssemblyDocumentInstance.DisabledCommandList); }
        }

        internal InvDisplaySettings InternalDisplaySettings
        {
            get { return InvDisplaySettings.ByInvDisplaySettings(AssemblyDocumentInstance.DisplaySettings); }
        }

        internal InvDocumentEvents InternalDocumentEvents
        {
            get { return InvDocumentEvents.ByInvDocumentEvents(AssemblyDocumentInstance.DocumentEvents); }
        }

        internal InvDocumentInterests InternalDocumentInterests
        {
            get { return InvDocumentInterests.ByInvDocumentInterests(AssemblyDocumentInstance.DocumentInterests); }
        }

        internal InvDocumentSubType InternalDocumentSubType
        {
            get { return InvDocumentSubType.ByInvDocumentSubType(AssemblyDocumentInstance.DocumentSubType); }
        }

        internal InvDocumentTypeEnum InternalDocumentType
        {
            //get { return InvDocumentTypeEnum.ByInvDocumentTypeEnum(AssemblyDocumentInstance.DocumentType); }
            get { return AssemblyDocumentInstance.DocumentType.As<InvDocumentTypeEnum>(); }
        }

        internal InvEnvironmentManager InternalEnvironmentManager
        {
            get { return InvEnvironmentManager.ByInvEnvironmentManager(AssemblyDocumentInstance.EnvironmentManager); }
        }

        internal InvFile InternalFile
        {
            get { return InvFile.ByInvFile(AssemblyDocumentInstance.File); }
        }

        internal int InternalFileSaveCounter
        {
            get { return AssemblyDocumentInstance.FileSaveCounter; }
        }

        internal string InternalFullDocumentName
        {
            get { return AssemblyDocumentInstance.FullDocumentName; }
        }

        //No type info
        //internal InvGraphicsDataSetsCollection InternalGraphicsDataSetsCollection
        //{
        //    get { return InvGraphicsDataSetsCollection.ByInvGraphicsDataSetsCollection(AssemblyDocumentInstance.GraphicsDataSetsCollection); }
        //}

        internal InvHighlightSets InternalHighlightSets
        {
            get { return InvHighlightSets.ByInvHighlightSets(AssemblyDocumentInstance.HighlightSets); }
        }

        internal string InternalInternalName
        {
            get { return AssemblyDocumentInstance.InternalName; }
        }

        //No type info
        //internal Inv_Document InternalInventorDocument
        //{
        //    get { return Inv_Document.ByInv_Document(AssemblyDocumentInstance.InventorDocument); }
        //}


        internal bool InternalIsModifiable
        {
            get { return AssemblyDocumentInstance.IsModifiable; }
        }

        internal string InternalLevelOfDetailName
        {
            get { return AssemblyDocumentInstance.LevelOfDetailName; }
        }

        internal InvLightingStyles InternalLightingStyles
        {
            get { return InvLightingStyles.ByInvLightingStyles(AssemblyDocumentInstance.LightingStyles); }
        }

        internal InvAssetsEnumerator InternalMaterialAssets
        {
            get { return InvAssetsEnumerator.ByInvAssetsEnumerator(AssemblyDocumentInstance.MaterialAssets); }
        }

        internal InvMaterials InternalMaterials
        {
            get { return InvMaterials.ByInvMaterials(AssemblyDocumentInstance.Materials); }
        }

        internal InvModelingSettings InternalModelingSettings
        {
            get { return InvModelingSettings.ByInvModelingSettings(AssemblyDocumentInstance.ModelingSettings); }
        }

        internal bool InternalNeedsMigrating
        {
            get { return AssemblyDocumentInstance.NeedsMigrating; }
        }

        internal InvObjectVisibility InternalObjectVisibility
        {
            get { return InvObjectVisibility.ByInvObjectVisibility(AssemblyDocumentInstance.ObjectVisibility); }
        }

        internal bool InternalOpen
        {
            get { return AssemblyDocumentInstance.Open; }
        }

        internal InvFileOwnershipEnum InternalOwnershipType
        {
            get { return InvFileOwnershipEnum.ByInvFileOwnershipEnum(AssemblyDocumentInstance.OwnershipType); }
        }

        internal Object InternalParent
        {
            get { return AssemblyDocumentInstance.Parent; }
        }

        internal InvAssetsEnumerator InternalPhysicalAssets
        {
            get { return InvAssetsEnumerator.ByInvAssetsEnumerator(AssemblyDocumentInstance.PhysicalAssets); }
        }

        internal InvPrintManager InternalPrintManager
        {
            get { return InvPrintManager.ByInvPrintManager(AssemblyDocumentInstance.PrintManager); }
        }

        internal InvPropertySets InternalPropertySets
        {
            get { return InvPropertySets.ByInvPropertySets(AssemblyDocumentInstance.PropertySets); }
        }

        internal InvCommandTypesEnum InternalRecentChanges
        {
            get { return InvCommandTypesEnum.ByInvCommandTypesEnum(AssemblyDocumentInstance.RecentChanges); }
        }

        internal InvDocumentDescriptorsEnumerator InternalReferencedDocumentDescriptors
        {
            get { return InvDocumentDescriptorsEnumerator.ByInvDocumentDescriptorsEnumerator(AssemblyDocumentInstance.ReferencedDocumentDescriptors); }
        }

        internal InvDocumentsEnumerator InternalReferencedDocuments
        {
            get { return InvDocumentsEnumerator.ByInvDocumentsEnumerator(AssemblyDocumentInstance.ReferencedDocuments); }
        }

        internal InvReferencedFileDescriptors InternalReferencedFileDescriptors
        {
            get { return InvReferencedFileDescriptors.ByInvReferencedFileDescriptors(AssemblyDocumentInstance.ReferencedFileDescriptors); }
        }

        internal InvDocumentsEnumerator InternalReferencedFiles
        {
            get { return InvDocumentsEnumerator.ByInvDocumentsEnumerator(AssemblyDocumentInstance.ReferencedFiles); }
        }

        internal InvReferencedOLEFileDescriptors InternalReferencedOLEFileDescriptors
        {
            get { return InvReferencedOLEFileDescriptors.ByInvReferencedOLEFileDescriptors(AssemblyDocumentInstance.ReferencedOLEFileDescriptors); }
        }

        internal InvReferenceKeyManager InternalReferenceKeyManager
        {
            get { return InvReferenceKeyManager.ByInvReferenceKeyManager(AssemblyDocumentInstance.ReferenceKeyManager); }
        }

        internal InvDocumentsEnumerator InternalReferencingDocuments
        {
            get { return InvDocumentsEnumerator.ByInvDocumentsEnumerator(AssemblyDocumentInstance.ReferencingDocuments); }
        }

        internal InvRenderStyles InternalRenderStyles
        {
            get { return InvRenderStyles.ByInvRenderStyles(AssemblyDocumentInstance.RenderStyles); }
        }

        internal bool InternalRequiresUpdate
        {
            get { return AssemblyDocumentInstance.RequiresUpdate; }
        }

        internal bool InternalReservedForWrite
        {
            get { return AssemblyDocumentInstance.ReservedForWrite; }
        }

        internal string InternalReservedForWriteLogin
        {
            get { return AssemblyDocumentInstance.ReservedForWriteLogin; }
        }

        internal string InternalReservedForWriteName
        {
            get { return AssemblyDocumentInstance.ReservedForWriteName; }
        }

        internal DateTime InternalReservedForWriteTime
        {
            get { return AssemblyDocumentInstance.ReservedForWriteTime; }
        }

        internal int InternalReservedForWriteVersion
        {
            get { return AssemblyDocumentInstance.ReservedForWriteVersion; }
        }

        internal string InternalRevisionId
        {
            get { return AssemblyDocumentInstance.RevisionId; }
        }

        internal InvSelectSet InternalSelectSet
        {
            get { return InvSelectSet.ByInvSelectSet(AssemblyDocumentInstance.SelectSet); }
        }

        internal InvSketchSettings InternalSketchSettings
        {
            get { return InvSketchSettings.ByInvSketchSettings(AssemblyDocumentInstance.SketchSettings); }
        }

        internal InvSoftwareVersion InternalSoftwareVersionCreated
        {
            get { return InvSoftwareVersion.ByInvSoftwareVersion(AssemblyDocumentInstance.SoftwareVersionCreated); }
        }

        internal InvSoftwareVersion InternalSoftwareVersionSaved
        {
            get { return InvSoftwareVersion.ByInvSoftwareVersion(AssemblyDocumentInstance.SoftwareVersionSaved); }
        }

        //this is some old shit
        //internal IPictureDisp InternalThumbnail
        //{
        //    get { return AssemblyDocumentInstance.Thumbnail; }
        //}

        internal InvThumbnailSaveOptionEnum InternalThumbnailSaveOption
        {
            get { return InvThumbnailSaveOptionEnum.ByInvThumbnailSaveOptionEnum(AssemblyDocumentInstance.ThumbnailSaveOption); }
        }

        //Missing a cast?
        //internal InvOGSSceneNode InternalTopOGSSceneNode
        //{
        //    get { return InvOGSSceneNode.ByInvOGSSceneNode(AssemblyDocumentInstance.TopOGSSceneNode); }
        //}


        internal InvObjectTypeEnum InternalType
        {
            get { return AssemblyDocumentInstance.Type.As<InvObjectTypeEnum>(); }
        }


        internal InvUnitsOfMeasure InternalUnitsOfMeasure
        {
            get { return InvUnitsOfMeasure.ByInvUnitsOfMeasure(AssemblyDocumentInstance.UnitsOfMeasure); }
        }


        internal InvInventorVBAProject InternalVBAProject
        {
            get { return InvInventorVBAProject.ByInvInventorVBAProject(AssemblyDocumentInstance.VBAProject); }
        }


        internal InvViews InternalViews
        {
            get { return InvViews.ByInvViews(AssemblyDocumentInstance.Views); }
        }


        internal bool Internal_ExcludeFromBOM { get; set; }

        internal LightingStyle InternalActiveLightingStyle { get; set; }

        internal bool InternalDirty { get; set; }

        internal CommandTypesEnum InternalDisabledCommandTypes { get; set; }

        internal string InternalDisplayName { get; set; }

        internal bool InternalDisplayNameOverridden { get; set; }

        internal string InternalFullFileName { get; set; }

        internal bool InternalIsOpenExpress { get; set; }

        internal bool InternalIsOpenLightweight { get; set; }

        internal bool InternalReservedForWriteByMe { get; set; }

        internal SelectionPriorityEnum InternalSelectionPriority { get; set; }

        internal string InternalSubType { get; set; }

       #endregion

        #region Private constructors
        private InvAssemblyDocument(InvAssemblyDocument invAssemblyDocument)
        {
            InternalAssemblyDocument = invAssemblyDocument.InternalAssemblyDocument;
        }

        private InvAssemblyDocument(Inventor.AssemblyDocument invAssemblyDocument)
        {
            InternalAssemblyDocument = invAssemblyDocument;
        }

        private InvAssemblyDocument(InvDocument invDocument)
        {

            InternalAssemblyDocument = (Inventor.AssemblyDocument)invDocument.DocumentInstance;
        }
        #endregion

        #region Private methods
        private void InternalActivate()
        {
            AssemblyDocumentInstance.Activate();
        }

        private void InternalClose(bool skipSave)
        {
            AssemblyDocumentInstance.Close( skipSave);
        }

        private HighlightSet InternalCreateHighlightSet()
        {
            return AssemblyDocumentInstance.CreateHighlightSet();
        }

        private DocumentsEnumerator InternalFindWhereUsed(string fullFileName)
        {
            return AssemblyDocumentInstance.FindWhereUsed( fullFileName);
        }

        private void InternalGetLocationFoundIn(out string locationName, out LocationTypeEnum locationType)
        {
            AssemblyDocumentInstance.GetLocationFoundIn(out  locationName, out  locationType);
        }

        private void InternalGetMissingAddInBehavior(out string clientId, out CommandTypesEnum disabledCommandTypesEnum, out ObjectCollection disabledCommands)
        {
            AssemblyDocumentInstance.GetMissingAddInBehavior(out  clientId, out  disabledCommandTypesEnum, out  disabledCommands);
        }

        private Object InternalGetPrivateStorage(string storageName, bool createIfNecessary)
        {
            return AssemblyDocumentInstance.GetPrivateStorage( storageName,  createIfNecessary);
        }

        private Object InternalGetPrivateStream(string streamName, bool createIfNecessary)
        {
            return AssemblyDocumentInstance.GetPrivateStream( streamName,  createIfNecessary);
        }

        private void InternalGetSelectedObject(GenericObject selection, out ObjectTypeEnum objectType, out NameValueMap additionalData, out ComponentOccurrence containingOccurrence, ref Object selectedObject)
        {
            AssemblyDocumentInstance.GetSelectedObject( selection, out  objectType, out  additionalData, out  containingOccurrence, ref  selectedObject);
        }

        private bool InternalHasPrivateStorage(string storageName)
        {
            return AssemblyDocumentInstance.HasPrivateStorage( storageName);
        }

        private bool InternalHasPrivateStream(string streamName)
        {
            return AssemblyDocumentInstance.HasPrivateStream( streamName);
        }

        private void InternalLockSaveSet()
        {
            AssemblyDocumentInstance.LockSaveSet();
        }

        private void InternalMigrate()
        {
            AssemblyDocumentInstance.Migrate();
        }

        private void InternalPutInternalName(string name, string number, string custom, out string internalName)
        {
            AssemblyDocumentInstance.PutInternalName( name,  number,  custom, out  internalName);
        }

        private void InternalPutInternalNameAndRevisionId(string internalNameToken, string revisionIdToken, out string internalName, out string revisionId)
        {
            AssemblyDocumentInstance.PutInternalNameAndRevisionId( internalNameToken,  revisionIdToken, out  internalName, out  revisionId);
        }

        private void InternalRebuild()
        {
            AssemblyDocumentInstance.Rebuild();
        }

        private bool InternalRebuild2(bool acceptErrorsAndContinue)
        {
            return AssemblyDocumentInstance.Rebuild2( acceptErrorsAndContinue);
        }

        private void InternalReleaseReference()
        {
            AssemblyDocumentInstance.ReleaseReference();
        }

        private void InternalRevertReservedForWriteByMe()
        {
            AssemblyDocumentInstance.RevertReservedForWriteByMe();
        }

        private void InternalSave()
        {
            AssemblyDocumentInstance.Save();
        }

        private void InternalSave2(bool saveDependents, Object documentsToSave)
        {
            AssemblyDocumentInstance.Save2( saveDependents,  documentsToSave);
        }

        private void InternalSaveAs(string fileName, bool saveCopyAs)
        {
            AssemblyDocumentInstance.SaveAs( fileName,  saveCopyAs);
        }

        private void InternalSetMissingAddInBehavior(string clientId, CommandTypesEnum disabledCommandTypesEnum, Object disabledCommands)
        {
            AssemblyDocumentInstance.SetMissingAddInBehavior( clientId,  disabledCommandTypesEnum,  disabledCommands);
        }

        private void InternalSetThumbnailSaveOption(ThumbnailSaveOptionEnum saveOption, string imageFullFileName)
        {
            AssemblyDocumentInstance.SetThumbnailSaveOption( saveOption,  imageFullFileName);
        }

        private void InternalUpdate()
        {
            AssemblyDocumentInstance.Update();
        }

        private bool InternalUpdate2(bool acceptErrorsAndContinue)
        {
            return AssemblyDocumentInstance.Update2( acceptErrorsAndContinue);
        }

        #endregion

        #region Public properties
        public Inventor.AssemblyDocument AssemblyDocumentInstance
        {
            get { return InternalAssemblyDocument; }
            set { InternalAssemblyDocument = value; }
        }

        public int _ComatoseNodesCount
        {
            get { return Internal_ComatoseNodesCount; }
        }

        public InvCommandIDEnum _DefaultCommand
        {
            get { return Internal_DefaultCommand; }
        }

        public Inv_DocPerformanceMonitor _DocPerformanceMonitor
        {
            get { return Internal_DocPerformanceMonitor; }
        }

        public string _InternalName
        {
            get { return Internal_InternalName; }
        }

        public string _PrimaryDeselGUID
        {
            get { return Internal_PrimaryDeselGUID; }
        }

        public int _SickNodesCount
        {
            get { return Internal_SickNodesCount; }
        }

        public Object ActivatedObject
        {
            get { return InternalActivatedObject; }
        }

        public InvDocumentsEnumerator AllReferencedDocuments
        {
            get { return InternalAllReferencedDocuments; }
        }

        public InvAssetsEnumerator AppearanceAssets
        {
            get { return InternalAppearanceAssets; }
        }

        public InvAssets Assets
        {
            get { return InternalAssets; }
        }

        public InvAttributeManager AttributeManager
        {
            get { return InternalAttributeManager; }
        }

        public InvAttributeSets AttributeSets
        {
            get { return InternalAttributeSets; }
        }

        //Mystery properties.  These have no type information to reflect.
        //public InvBrowserPanes BrowserPanes
        //{
        //    get { return InternalBrowserPanes; }
        //}

        //public InvCachedGraphicsStatusEnum CachedGraphicsStatus
        //{
        //    get { return InternalCachedGraphicsStatus; }
        //}

        public bool Compacted
        {
            get { return InternalCompacted; }
        }

        public InvAssemblyComponentDefinition ComponentDefinition
        {
            get { return InternalComponentDefinition; }
        }

        public InvAssemblyComponentDefinitions ComponentDefinitions
        {
            get { return InternalComponentDefinitions; }
        }

        //public string DatabaseRevisionId
        //{
        //    get { return InternalDatabaseRevisionId; }
        //}

        //public string DefaultCommand
        //{
        //    get { return InternalDefaultCommand; }
        //}

        public string DesignViewInfo
        {
            get { return InternalDesignViewInfo; }
        }

        public InvDisabledCommandList DisabledCommandList
        {
            get { return InternalDisabledCommandList; }
        }

        public InvDisplaySettings DisplaySettings
        {
            get { return InternalDisplaySettings; }
        }

        public InvDocumentEvents DocumentEvents
        {
            get { return InternalDocumentEvents; }
        }

        public InvDocumentInterests DocumentInterests
        {
            get { return InternalDocumentInterests; }
        }

        public InvDocumentSubType DocumentSubType
        {
            get { return InternalDocumentSubType; }
        }

        public InvDocumentTypeEnum DocumentType
        {
            get { return InternalDocumentType; }
        }

        public InvEnvironmentManager EnvironmentManager
        {
            get { return InternalEnvironmentManager; }
        }

        public InvFile File
        {
            get { return InternalFile; }
        }

        public int FileSaveCounter
        {
            get { return InternalFileSaveCounter; }
        }

        public string FullDocumentName
        {
            get { return InternalFullDocumentName; }
        }

        //Has no type info
        //public InvGraphicsDataSetsCollection GraphicsDataSetsCollection
        //{
        //    get { return InternalGraphicsDataSetsCollection; }
        //}

        public InvHighlightSets HighlightSets
        {
            get { return InternalHighlightSets; }
        }

        public string InternalName
        {
            get { return InternalInternalName; }
        }

        //public Inv_Document InventorDocument
        //{
        //    get { return InternalInventorDocument; }
        //}

        public bool IsModifiable
        {
            get { return InternalIsModifiable; }
        }

        public string LevelOfDetailName
        {
            get { return InternalLevelOfDetailName; }
        }

        public InvLightingStyles LightingStyles
        {
            get { return InternalLightingStyles; }
        }

        public InvAssetsEnumerator MaterialAssets
        {
            get { return InternalMaterialAssets; }
        }

        public InvMaterials Materials
        {
            get { return InternalMaterials; }
        }

        public InvModelingSettings ModelingSettings
        {
            get { return InternalModelingSettings; }
        }

        public bool NeedsMigrating
        {
            get { return InternalNeedsMigrating; }
        }

        public InvObjectVisibility ObjectVisibility
        {
            get { return InternalObjectVisibility; }
        }

        public bool Open
        {
            get { return InternalOpen; }
        }

        public InvFileOwnershipEnum OwnershipType
        {
            get { return InternalOwnershipType; }
        }

        public Object Parent
        {
            get { return InternalParent; }
        }

        public InvAssetsEnumerator PhysicalAssets
        {
            get { return InternalPhysicalAssets; }
        }

        public InvPrintManager PrintManager
        {
            get { return InternalPrintManager; }
        }

        public InvPropertySets PropertySets
        {
            get { return InternalPropertySets; }
        }

        public InvCommandTypesEnum RecentChanges
        {
            get { return InternalRecentChanges; }
        }

        public InvDocumentDescriptorsEnumerator ReferencedDocumentDescriptors
        {
            get { return InternalReferencedDocumentDescriptors; }
        }

        public InvDocumentsEnumerator ReferencedDocuments
        {
            get { return InternalReferencedDocuments; }
        }

        public InvReferencedFileDescriptors ReferencedFileDescriptors
        {
            get { return InternalReferencedFileDescriptors; }
        }

        public InvDocumentsEnumerator ReferencedFiles
        {
            get { return InternalReferencedFiles; }
        }

        public InvReferencedOLEFileDescriptors ReferencedOLEFileDescriptors
        {
            get { return InternalReferencedOLEFileDescriptors; }
        }

        public InvReferenceKeyManager ReferenceKeyManager
        {
            get { return InternalReferenceKeyManager; }
        }

        public InvDocumentsEnumerator ReferencingDocuments
        {
            get { return InternalReferencingDocuments; }
        }

        public InvRenderStyles RenderStyles
        {
            get { return InternalRenderStyles; }
        }

        public bool RequiresUpdate
        {
            get { return InternalRequiresUpdate; }
        }

        public bool ReservedForWrite
        {
            get { return InternalReservedForWrite; }
        }

        public string ReservedForWriteLogin
        {
            get { return InternalReservedForWriteLogin; }
        }

        public string ReservedForWriteName
        {
            get { return InternalReservedForWriteName; }
        }

        public DateTime ReservedForWriteTime
        {
            get { return InternalReservedForWriteTime; }
        }

        public int ReservedForWriteVersion
        {
            get { return InternalReservedForWriteVersion; }
        }

        public string RevisionId
        {
            get { return InternalRevisionId; }
        }

        public InvSelectSet SelectSet
        {
            get { return InternalSelectSet; }
        }

        public InvSketchSettings SketchSettings
        {
            get { return InternalSketchSettings; }
        }

        public InvSoftwareVersion SoftwareVersionCreated
        {
            get { return InternalSoftwareVersionCreated; }
        }

        public InvSoftwareVersion SoftwareVersionSaved
        {
            get { return InternalSoftwareVersionSaved; }
        }

        //who cares
        //public IPictureDisp Thumbnail
        //{
        //    get { return InternalThumbnail; }
        //}

        public InvThumbnailSaveOptionEnum ThumbnailSaveOption
        {
            get { return InternalThumbnailSaveOption; }
        }

        //Missing a cast?
        //public InvOGSSceneNode TopOGSSceneNode
        //{
        //    get { return InternalTopOGSSceneNode; }
        //}

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public InvUnitsOfMeasure UnitsOfMeasure
        {
            get { return InternalUnitsOfMeasure; }
        }

        public InvInventorVBAProject VBAProject
        {
            get { return InternalVBAProject; }
        }

        public InvViews Views
        {
            get { return InternalViews; }
        }

        public bool _ExcludeFromBOM
        {
            get { return Internal_ExcludeFromBOM; }
            set { Internal_ExcludeFromBOM = value; }
        }

        //Needs to return 'LightingStyle
        //public InvLightingStyle ActiveLightingStyle
        //{
        //    get { return InternalActiveLightingStyle; }
        //    set { InternalActiveLightingStyle = value; }
        //}

        public bool Dirty
        {
            get { return InternalDirty; }
            set { InternalDirty = value; }
        }

        //python needs updating for read/write internal properties to match what read-only does.
        //public InvCommandTypesEnum DisabledCommandTypes
        //{
        //    get { return InternalDisabledCommandTypes; }
        //    set { InternalDisabledCommandTypes = value; }
        //}

        public string DisplayName
        {
            get { return InternalDisplayName; }
            set { InternalDisplayName = value; }
        }

        public bool DisplayNameOverridden
        {
            get { return InternalDisplayNameOverridden; }
            set { InternalDisplayNameOverridden = value; }
        }

        public string FullFileName
        {
            get { return InternalFullFileName; }
            set { InternalFullFileName = value; }
        }

        public bool IsOpenExpress
        {
            get { return InternalIsOpenExpress; }
            set { InternalIsOpenExpress = value; }
        }

        public bool IsOpenLightweight
        {
            get { return InternalIsOpenLightweight; }
            set { InternalIsOpenLightweight = value; }
        }

        public bool ReservedForWriteByMe
        {
            get { return InternalReservedForWriteByMe; }
            set { InternalReservedForWriteByMe = value; }
        }

        //python needs updating for read/write internal properties to match what read-only does.
        //public InvSelectionPriorityEnum SelectionPriority
        //{
        //    get { return InternalSelectionPriority; }
        //    set { InternalSelectionPriority = value; }
        //}

        public string SubType
        {
            get { return InternalSubType; }
            set { InternalSubType = value; }
        }

        #endregion

        #region Public static constructors
        public static InvAssemblyDocument ByInvAssemblyDocument(InvAssemblyDocument invAssemblyDocument)
        {
            return new InvAssemblyDocument(invAssemblyDocument);
        }
        public static InvAssemblyDocument ByInvAssemblyDocument(Inventor.AssemblyDocument invAssemblyDocument)
        {
            return new InvAssemblyDocument(invAssemblyDocument);
        }

        public static InvAssemblyDocument ByInvDocument(InvDocument invDocument)
        {
            return new InvAssemblyDocument(invDocument);
        }
        #endregion

        #region Public methods

        public void Activate()
        {
            InternalActivate();
        }

        public void Close(bool skipSave)
        {
            InternalClose(skipSave);
        }

        public HighlightSet CreateHighlightSet()
        {
            return InternalCreateHighlightSet();
        }

        public DocumentsEnumerator FindWhereUsed(string fullFileName)
        {
            return InternalFindWhereUsed(fullFileName);
        }

        public void GetLocationFoundIn(out string locationName, out LocationTypeEnum locationType)
        {
            InternalGetLocationFoundIn(out  locationName, out  locationType);
        }

        public void GetMissingAddInBehavior(out string clientId, out CommandTypesEnum disabledCommandTypesEnum, out ObjectCollection disabledCommands)
        {
            InternalGetMissingAddInBehavior(out  clientId, out  disabledCommandTypesEnum, out  disabledCommands);
        }

        public Object GetPrivateStorage(string storageName, bool createIfNecessary)
        {
            return InternalGetPrivateStorage(storageName, createIfNecessary);
        }

        public Object GetPrivateStream(string streamName, bool createIfNecessary)
        {
            return InternalGetPrivateStream(streamName, createIfNecessary);
        }

        public void GetSelectedObject(GenericObject selection, out ObjectTypeEnum objectType, out NameValueMap additionalData, out ComponentOccurrence containingOccurrence, ref Object selectedObject)
        {
            InternalGetSelectedObject( selection, out  objectType, out  additionalData, out  containingOccurrence, ref  selectedObject);
        }

        public bool HasPrivateStorage(string storageName)
        {
            return InternalHasPrivateStorage(storageName);
        }

        public bool HasPrivateStream(string streamName)
        {
            return InternalHasPrivateStream(streamName);
        }

        public void LockSaveSet()
        {
            InternalLockSaveSet();
        }

        public void Migrate()
        {
            InternalMigrate();
        }

        public void PutInternalName(string name, string number, string custom, out string internalName)
        {
            InternalPutInternalName(name, number, custom, out  internalName);
        }

        public void PutInternalNameAndRevisionId(string internalNameToken, string revisionIdToken, out string internalName, out string revisionId)
        {
            InternalPutInternalNameAndRevisionId(internalNameToken, revisionIdToken, out  internalName, out  revisionId);
        }

        public void Rebuild()
        {
            InternalRebuild();
        }

        public bool Rebuild2(bool acceptErrorsAndContinue)
        {
            return InternalRebuild2(acceptErrorsAndContinue);
        }

        public void ReleaseReference()
        {
            InternalReleaseReference();
        }

        public void RevertReservedForWriteByMe()
        {
            InternalRevertReservedForWriteByMe();
        }

        public void Save()
        {
            InternalSave();
        }

        public void Save2(bool saveDependents, Object documentsToSave)
        {
            InternalSave2(saveDependents, documentsToSave);
        }

        public void SaveAs(string fileName, bool saveCopyAs)
        {
            InternalSaveAs(fileName, saveCopyAs);
        }

        public void SetMissingAddInBehavior(string clientId, CommandTypesEnum disabledCommandTypesEnum, Object disabledCommands)
        {
            InternalSetMissingAddInBehavior(clientId, disabledCommandTypesEnum, disabledCommands);
        }

        public void SetThumbnailSaveOption(ThumbnailSaveOptionEnum saveOption, string imageFullFileName)
        {
            InternalSetThumbnailSaveOption(saveOption, imageFullFileName);
        }

        public void Update()
        {
            InternalUpdate();
        }

        public bool Update2(bool acceptErrorsAndContinue)
        {
            return InternalUpdate2(acceptErrorsAndContinue);
        }

        #endregion
    }
}
