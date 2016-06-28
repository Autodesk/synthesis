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
    public class InvDocument
    {
        Inventor.Document internalDoc;
        #region Internal properties
        internal Inventor.Document InternalDocument 
        {
            get
            {
                return PersistenceManager.InventorApplication.ActiveDocument;
            }
            set
            {
                internalDoc = value;
            }
        }

        internal int Internal_ComatoseNodesCount
        {
            get { return DocumentInstance._ComatoseNodesCount; }
        }

        internal InvCommandIDEnum Internal_DefaultCommand
        {
            get { return InvCommandIDEnum.ByInvCommandIDEnum(DocumentInstance._DefaultCommand); }
        }

        internal Inv_DocPerformanceMonitor Internal_DocPerformanceMonitor
        {
            get { return Inv_DocPerformanceMonitor.ByInv_DocPerformanceMonitor(DocumentInstance._DocPerformanceMonitor); }
        }

        internal string Internal_InternalName
        {
            get { return DocumentInstance._InternalName; }
        }

        internal string Internal_PrimaryDeselGUID
        {
            get { return DocumentInstance._PrimaryDeselGUID; }
        }

        internal int Internal_SickNodesCount
        {
            get { return DocumentInstance._SickNodesCount; }
        }

        internal Object InternalActivatedObject
        {
            get { return DocumentInstance.ActivatedObject; }
        }

        internal InvDocumentsEnumerator InternalAllReferencedDocuments
        {
            get { return InvDocumentsEnumerator.ByInvDocumentsEnumerator(DocumentInstance.AllReferencedDocuments); }
        }

        internal InvAttributeManager InternalAttributeManager
        {
            get { return InvAttributeManager.ByInvAttributeManager(DocumentInstance.AttributeManager); }
        }

        internal InvAttributeSets InternalAttributeSets
        {
            get { return InvAttributeSets.ByInvAttributeSets(DocumentInstance.AttributeSets); }
        }

        //internal InvBrowserPanes InternalBrowserPanes
        //{
        //    get { return InvBrowserPanes.ByInvBrowserPanes(DocumentInstance.BrowserPanes); }
        //}

        internal bool InternalCompacted
        {
            get { return DocumentInstance.Compacted; }
        }

        internal string InternalDatabaseRevisionId
        {
            get { return DocumentInstance.DatabaseRevisionId; }
        }

        internal string InternalDefaultCommand
        {
            get { return DocumentInstance.DefaultCommand; }
        }

        internal InvDocumentEvents InternalDocumentEvents
        {
            get { return InvDocumentEvents.ByInvDocumentEvents(DocumentInstance.DocumentEvents); }
        }

        internal InvDocumentInterests InternalDocumentInterests
        {
            get { return InvDocumentInterests.ByInvDocumentInterests(DocumentInstance.DocumentInterests); }
        }

        internal InvDocumentSubType InternalDocumentSubType
        {
            get { return InvDocumentSubType.ByInvDocumentSubType(DocumentInstance.DocumentSubType); }
        }

        internal InvDocumentTypeEnum InternalDocumentType
        {
            get { return DocumentInstance.DocumentType.As<InvDocumentTypeEnum>(); }
        }

        internal InvFile InternalFile
        {
            get { return InvFile.ByInvFile(DocumentInstance.File); }
        }

        internal int InternalFileSaveCounter
        {
            get { return DocumentInstance.FileSaveCounter; }
        }

        internal string InternalFullDocumentName
        {
            get { return DocumentInstance.FullDocumentName; }
        }

        //internal InvGraphicsDataSetsCollection InternalGraphicsDataSetsCollection
        //{
        //    get { return InvGraphicsDataSetsCollection.ByInvGraphicsDataSetsCollection(DocumentInstance.GraphicsDataSetsCollection); }
        //}

        internal InvHighlightSets InternalHighlightSets
        {
            get { return InvHighlightSets.ByInvHighlightSets(DocumentInstance.HighlightSets); }
        }

        internal string InternalInternalName
        {
            get { return DocumentInstance.InternalName; }
        }

        //internal Inv_Document InternalInventorDocument
        //{
        //    get { return Inv_Document.ByInv_Document(DocumentInstance.InventorDocument); }
        //}

        internal bool InternalIsModifiable
        {
            get { return DocumentInstance.IsModifiable; }
        }

        internal bool InternalNeedsMigrating
        {
            get { return DocumentInstance.NeedsMigrating; }
        }

        internal bool InternalOpen
        {
            get { return DocumentInstance.Open; }
        }

        internal InvFileOwnershipEnum InternalOwnershipType
        {
            get { return InvFileOwnershipEnum.ByInvFileOwnershipEnum(DocumentInstance.OwnershipType); }
        }

        internal Object InternalParent
        {
            get { return DocumentInstance.Parent; }
        }

        internal InvPrintManager InternalPrintManager
        {
            get { return InvPrintManager.ByInvPrintManager(DocumentInstance.PrintManager); }
        }

        internal InvPropertySets InternalPropertySets
        {
            get { return InvPropertySets.ByInvPropertySets(DocumentInstance.PropertySets); }
        }

        internal InvCommandTypesEnum InternalRecentChanges
        {
            get { return InvCommandTypesEnum.ByInvCommandTypesEnum(DocumentInstance.RecentChanges); }
        }

        internal InvDocumentDescriptorsEnumerator InternalReferencedDocumentDescriptors
        {
            get { return InvDocumentDescriptorsEnumerator.ByInvDocumentDescriptorsEnumerator(DocumentInstance.ReferencedDocumentDescriptors); }
        }

        internal InvDocumentsEnumerator InternalReferencedDocuments
        {
            get { return InvDocumentsEnumerator.ByInvDocumentsEnumerator(DocumentInstance.ReferencedDocuments); }
        }

        internal InvReferencedFileDescriptors InternalReferencedFileDescriptors
        {
            get { return InvReferencedFileDescriptors.ByInvReferencedFileDescriptors(DocumentInstance.ReferencedFileDescriptors); }
        }

        internal InvDocumentsEnumerator InternalReferencedFiles
        {
            get { return InvDocumentsEnumerator.ByInvDocumentsEnumerator(DocumentInstance.ReferencedFiles); }
        }

        internal InvReferencedOLEFileDescriptors InternalReferencedOLEFileDescriptors
        {
            get { return InvReferencedOLEFileDescriptors.ByInvReferencedOLEFileDescriptors(DocumentInstance.ReferencedOLEFileDescriptors); }
        }

        internal InvReferenceKeyManager InternalReferenceKeyManager
        {
            get { return InvReferenceKeyManager.ByInvReferenceKeyManager(DocumentInstance.ReferenceKeyManager); }
        }

        internal InvDocumentsEnumerator InternalReferencingDocuments
        {
            get { return InvDocumentsEnumerator.ByInvDocumentsEnumerator(DocumentInstance.ReferencingDocuments); }
        }

        internal InvRenderStyles InternalRenderStyles
        {
            get { return InvRenderStyles.ByInvRenderStyles(DocumentInstance.RenderStyles); }
        }

        internal bool InternalRequiresUpdate
        {
            get { return DocumentInstance.RequiresUpdate; }
        }

        internal bool InternalReservedForWrite
        {
            get { return DocumentInstance.ReservedForWrite; }
        }

        internal string InternalReservedForWriteLogin
        {
            get { return DocumentInstance.ReservedForWriteLogin; }
        }

        internal string InternalReservedForWriteName
        {
            get { return DocumentInstance.ReservedForWriteName; }
        }

        internal DateTime InternalReservedForWriteTime
        {
            get { return DocumentInstance.ReservedForWriteTime; }
        }

        internal int InternalReservedForWriteVersion
        {
            get { return DocumentInstance.ReservedForWriteVersion; }
        }

        internal string InternalRevisionId
        {
            get { return DocumentInstance.RevisionId; }
        }

        internal InvSelectSet InternalSelectSet
        {
            get { return InvSelectSet.ByInvSelectSet(DocumentInstance.SelectSet); }
        }

        internal InvSoftwareVersion InternalSoftwareVersionCreated
        {
            get { return InvSoftwareVersion.ByInvSoftwareVersion(DocumentInstance.SoftwareVersionCreated); }
        }

        internal InvSoftwareVersion InternalSoftwareVersionSaved
        {
            get { return InvSoftwareVersion.ByInvSoftwareVersion(DocumentInstance.SoftwareVersionSaved); }
        }

        //internal IPictureDisp InternalThumbnail
        //{
        //    get { return DocumentInstance.Thumbnail; }
        //}

        internal InvThumbnailSaveOptionEnum InternalThumbnailSaveOption
        {
            get { return InvThumbnailSaveOptionEnum.ByInvThumbnailSaveOptionEnum(DocumentInstance.ThumbnailSaveOption); }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return DocumentInstance.Type.As<InvObjectTypeEnum>(); }
        }

        internal InvUnitsOfMeasure InternalUnitsOfMeasure
        {
            get { return InvUnitsOfMeasure.ByInvUnitsOfMeasure(DocumentInstance.UnitsOfMeasure); }
        }

        internal InvInventorVBAProject InternalVBAProject
        {
            get { return InvInventorVBAProject.ByInvInventorVBAProject(DocumentInstance.VBAProject); }
        }

        internal InvViews InternalViews
        {
            get { return InvViews.ByInvViews(DocumentInstance.Views); }
        }

        internal bool InternalDirty { get; set; }

        internal CommandTypesEnum InternalDisabledCommandTypes { get; set; }

        internal string InternalDisplayName { get; set; }

        internal bool InternalDisplayNameOverridden { get; set; }

        internal string InternalFullFileName { get; set; }

        internal bool InternalReservedForWriteByMe { get; set; }

        internal SelectionPriorityEnum InternalSelectionPriority { get; set; }

        internal string InternalSubType { get; set; }
        #endregion

        #region Private constructors
        private InvDocument(InvDocument invDocument)
        {
            InternalDocument = invDocument.InternalDocument;
        }

        private InvDocument(Inventor.Document invDocument)
        {
            InternalDocument = invDocument;
        }
        #endregion

        #region Private methods
        private void InternalActivate()
        {
            DocumentInstance.Activate();
        }

        private void InternalClose(bool skipSave)
        {
            DocumentInstance.Close( skipSave);
        }

        //private InvHighlightSet InternalCreateHighlightSet()
        //{
        //    return InvHighlightSet.ByInvHighlightSet(DocumentInstance.CreateHighlightSet());
        //}

        private InvDocumentsEnumerator InternalFindWhereUsed(string fullFileName)
        {
            return InvDocumentsEnumerator.ByInvDocumentsEnumerator(DocumentInstance.FindWhereUsed(fullFileName));
        }

        private void InternalGetLocationFoundIn(out string locationName, out LocationTypeEnum locationType)
        {
            DocumentInstance.GetLocationFoundIn(out  locationName, out  locationType);
        }

        private void InternalGetMissingAddInBehavior(out string clientId, out CommandTypesEnum disabledCommandTypesEnum, out ObjectCollection disabledCommands)
        {
            DocumentInstance.GetMissingAddInBehavior(out  clientId, out  disabledCommandTypesEnum, out  disabledCommands);
        }

        private Object InternalGetPrivateStorage(string storageName, bool createIfNecessary)
        {
            return DocumentInstance.GetPrivateStorage( storageName,  createIfNecessary);
        }

        private Object InternalGetPrivateStream(string streamName, bool createIfNecessary)
        {
            return DocumentInstance.GetPrivateStream( streamName,  createIfNecessary);
        }

        private bool InternalHasPrivateStorage(string storageName)
        {
            return DocumentInstance.HasPrivateStorage( storageName);
        }

        private bool InternalHasPrivateStream(string streamName)
        {
            return DocumentInstance.HasPrivateStream( streamName);
        }

        private void InternalLockSaveSet()
        {
            DocumentInstance.LockSaveSet();
        }

        private void InternalMigrate()
        {
            DocumentInstance.Migrate();
        }

        private void InternalPutInternalName(string name, string number, string custom, out string internalName)
        {
            DocumentInstance.PutInternalName( name,  number,  custom, out  internalName);
        }

        private void InternalPutInternalNameAndRevisionId(string internalNameToken, string revisionIdToken, out string internalName, out string revisionId)
        {
            DocumentInstance.PutInternalNameAndRevisionId( internalNameToken,  revisionIdToken, out  internalName, out  revisionId);
        }

        private void InternalRebuild()
        {
            DocumentInstance.Rebuild();
        }

        private bool InternalRebuild2(bool acceptErrorsAndContinue)
        {
            return DocumentInstance.Rebuild2( acceptErrorsAndContinue);
        }

        private void InternalReleaseReference()
        {
            DocumentInstance.ReleaseReference();
        }

        private void InternalRevertReservedForWriteByMe()
        {
            DocumentInstance.RevertReservedForWriteByMe();
        }

        private void InternalSave()
        {
            DocumentInstance.Save();
        }

        private void InternalSave2(bool saveDependents, Object documentsToSave)
        {
            DocumentInstance.Save2( saveDependents,  documentsToSave);
        }

        private void InternalSaveAs(string fileName, bool saveCopyAs)
        {
            DocumentInstance.SaveAs( fileName,  saveCopyAs);
        }

        private void InternalSetMissingAddInBehavior(string clientId, CommandTypesEnum disabledCommandTypesEnum, Object disabledCommands)
        {
            DocumentInstance.SetMissingAddInBehavior( clientId,  disabledCommandTypesEnum,  disabledCommands);
        }

        private void InternalSetThumbnailSaveOption(ThumbnailSaveOptionEnum saveOption, string imageFullFileName)
        {
            DocumentInstance.SetThumbnailSaveOption( saveOption,  imageFullFileName);
        }

        private void InternalUpdate()
        {
            DocumentInstance.Update();
        }

        private bool InternalUpdate2(bool acceptErrorsAndContinue)
        {
            return DocumentInstance.Update2( acceptErrorsAndContinue);
        }

        #endregion

        #region Public properties
        public Inventor.Document DocumentInstance
        {
            get { return InternalDocument; }
            set { InternalDocument = value; }
        }

        public int _ComatoseNodesCount
        {
            get { return Internal_ComatoseNodesCount; }
        }

        //public InvCommandIDEnum _DefaultCommand
        //{
        //    get { return Internal_DefaultCommand; }
        //}

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

        public InvAttributeManager AttributeManager
        {
            get { return InternalAttributeManager; }
        }

        public InvAttributeSets AttributeSets
        {
            get { return InternalAttributeSets; }
        }

        //public InvBrowserPanes BrowserPanes
        //{
        //    get { return InternalBrowserPanes; }
        //}

        public bool Compacted
        {
            get { return InternalCompacted; }
        }

        public string DatabaseRevisionId
        {
            get { return InternalDatabaseRevisionId; }
        }

        public string DefaultCommand
        {
            get { return InternalDefaultCommand; }
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

        public bool NeedsMigrating
        {
            get { return InternalNeedsMigrating; }
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

        public InvSoftwareVersion SoftwareVersionCreated
        {
            get { return InternalSoftwareVersionCreated; }
        }

        public InvSoftwareVersion SoftwareVersionSaved
        {
            get { return InternalSoftwareVersionSaved; }
        }

        //public IPictureDisp Thumbnail
        //{
        //    get { return InternalThumbnail; }
        //}

        public InvThumbnailSaveOptionEnum ThumbnailSaveOption
        {
            get { return InternalThumbnailSaveOption; }
        }

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

        public bool Dirty
        {
            get { return InternalDirty; }
            set { InternalDirty = value; }
        }

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

        public bool ReservedForWriteByMe
        {
            get { return InternalReservedForWriteByMe; }
            set { InternalReservedForWriteByMe = value; }
        }

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
        public static InvDocument ByInvDocument(InvDocument invDocument)
        {
            return new InvDocument(invDocument);
        }
        public static InvDocument ByInvDocument(Inventor.Document invDocument)
        {
            return new InvDocument(invDocument);
        }
        #endregion

        #region Public methods
        public void Activate()
        {
            InternalActivate();
        }

        public dynamic AsDocumentType()
        {
            return InternalAsDocumentType();
        }

        private dynamic InternalAsDocumentType()
        {
            if (DocumentInstance.DocumentType == DocumentTypeEnum.kUnknownDocumentObject)
            {
                return null;
            }

            else if (DocumentInstance.DocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
            {
                return InvAssemblyDocument.ByInvAssemblyDocument((Inventor.AssemblyDocument)DocumentInstance);
            }

            else if (DocumentInstance.DocumentType == DocumentTypeEnum.kPartDocumentObject)
            {
                return InvPartDocument.ByInvPartDocument((Inventor.PartDocument)DocumentInstance);
            }

            else
            {
                return null;
            }
        }

        public void Close(bool skipSave)
        {
            InternalClose( skipSave);
        }

        //public InvHighlightSet CreateHighlightSet()
        //{
        //    return InternalCreateHighlightSet();
        //}

        public InvDocumentsEnumerator FindWhereUsed(string fullFileName)
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
            return InternalGetPrivateStorage( storageName,  createIfNecessary);
        }

        public Object GetPrivateStream(string streamName, bool createIfNecessary)
        {
            return InternalGetPrivateStream( streamName,  createIfNecessary);
        }

        public bool HasPrivateStorage(string storageName)
        {
            return InternalHasPrivateStorage( storageName);
        }

        public bool HasPrivateStream(string streamName)
        {
            return InternalHasPrivateStream( streamName);
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
            InternalPutInternalName( name,  number,  custom, out  internalName);
        }

        public void PutInternalNameAndRevisionId(string internalNameToken, string revisionIdToken, out string internalName, out string revisionId)
        {
            InternalPutInternalNameAndRevisionId( internalNameToken,  revisionIdToken, out  internalName, out  revisionId);
        }

        public void Rebuild()
        {
            InternalRebuild();
        }

        public bool Rebuild2(bool acceptErrorsAndContinue)
        {
            return InternalRebuild2( acceptErrorsAndContinue);
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
            InternalSave2( saveDependents,  documentsToSave);
        }

        public void SaveAs(string fileName, bool saveCopyAs)
        {
            InternalSaveAs( fileName,  saveCopyAs);
        }

        public void SetMissingAddInBehavior(string clientId, CommandTypesEnum disabledCommandTypesEnum, Object disabledCommands)
        {
            InternalSetMissingAddInBehavior( clientId,  disabledCommandTypesEnum,  disabledCommands);
        }

        public void SetThumbnailSaveOption(ThumbnailSaveOptionEnum saveOption, string imageFullFileName)
        {
            InternalSetThumbnailSaveOption( saveOption,  imageFullFileName);
        }

        public void Update()
        {
            InternalUpdate();
        }

        public bool Update2(bool acceptErrorsAndContinue)
        {
            return InternalUpdate2( acceptErrorsAndContinue);
        }

        #endregion
    }
}
