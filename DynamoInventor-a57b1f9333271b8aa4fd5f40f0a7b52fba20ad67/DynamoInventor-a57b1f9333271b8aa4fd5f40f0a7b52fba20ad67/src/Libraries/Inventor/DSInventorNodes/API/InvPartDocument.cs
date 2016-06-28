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
    public class InvPartDocument //: InvDocument
    {
        #region Internal properties
        internal Inventor.PartDocument InternalPartDocument { get; set; }

        internal int Internal_ComatoseNodesCount
        {
            get { return PartDocumentInstance._ComatoseNodesCount; }
        }

        internal CommandIDEnum Internal_DefaultCommand
        {
            get { return PartDocumentInstance._DefaultCommand; }
        }

        internal _DocPerformanceMonitor Internal_DocPerformanceMonitor
        {
            get { return PartDocumentInstance._DocPerformanceMonitor; }
        }

        internal string Internal_InternalName
        {
            get { return PartDocumentInstance._InternalName; }
        }

        internal string Internal_PrimaryDeselGUID
        {
            get { return PartDocumentInstance._PrimaryDeselGUID; }
        }

        internal int Internal_SickNodesCount
        {
            get { return PartDocumentInstance._SickNodesCount; }
        }

        internal Object InternalActivatedObject
        {
            get { return PartDocumentInstance.ActivatedObject; }
        }

        internal DocumentsEnumerator InternalAllReferencedDocuments
        {
            get { return PartDocumentInstance.AllReferencedDocuments; }
        }

        internal AssetsEnumerator InternalAppearanceAssets
        {
            get { return PartDocumentInstance.AppearanceAssets; }
        }

        internal Assets InternalAssets
        {
            get { return PartDocumentInstance.Assets; }
        }

        internal AttributeManager InternalAttributeManager
        {
            get { return PartDocumentInstance.AttributeManager; }
        }

        internal AttributeSets InternalAttributeSets
        {
            get { return PartDocumentInstance.AttributeSets; }
        }

        internal BrowserPanes InternalBrowserPanes
        {
            get { return PartDocumentInstance.BrowserPanes; }
        }

        internal bool InternalCompacted
        {
            get { return PartDocumentInstance.Compacted; }
        }

        internal PartComponentDefinition InternalComponentDefinition
        {
            get { return PartDocumentInstance.ComponentDefinition; }
        }

        internal PartComponentDefinitions InternalComponentDefinitions
        {
            get { return PartDocumentInstance.ComponentDefinitions; }
        }

        internal string InternalDatabaseRevisionId
        {
            get { return PartDocumentInstance.DatabaseRevisionId; }
        }

        internal string InternalDefaultCommand
        {
            get { return PartDocumentInstance.DefaultCommand; }
        }

        internal DisabledCommandList InternalDisabledCommandList
        {
            get { return PartDocumentInstance.DisabledCommandList; }
        }

        internal DisplaySettings InternalDisplaySettings
        {
            get { return PartDocumentInstance.DisplaySettings; }
        }

        internal DocumentEvents InternalDocumentEvents
        {
            get { return PartDocumentInstance.DocumentEvents; }
        }

        internal DocumentInterests InternalDocumentInterests
        {
            get { return PartDocumentInstance.DocumentInterests; }
        }

        internal DocumentSubType InternalDocumentSubType
        {
            get { return PartDocumentInstance.DocumentSubType; }
        }

        internal DocumentTypeEnum InternalDocumentType
        {
            get { return PartDocumentInstance.DocumentType; }
        }

        internal EnvironmentManager InternalEnvironmentManager
        {
            get { return PartDocumentInstance.EnvironmentManager; }
        }

        internal File InternalFile
        {
            get { return PartDocumentInstance.File; }
        }

        internal int InternalFileSaveCounter
        {
            get { return PartDocumentInstance.FileSaveCounter; }
        }

        internal string InternalFullDocumentName
        {
            get { return PartDocumentInstance.FullDocumentName; }
        }

        internal GraphicsDataSetsCollection InternalGraphicsDataSetsCollection
        {
            get { return PartDocumentInstance.GraphicsDataSetsCollection; }
        }

        internal HighlightSets InternalHighlightSets
        {
            get { return PartDocumentInstance.HighlightSets; }
        }

        internal string InternalInternalName
        {
            get { return PartDocumentInstance.InternalName; }
        }

        internal _Document InternalInventorDocument
        {
            get { return PartDocumentInstance.InventorDocument; }
        }

        internal bool InternalIsModifiable
        {
            get { return PartDocumentInstance.IsModifiable; }
        }

        internal LightingStyles InternalLightingStyles
        {
            get { return PartDocumentInstance.LightingStyles; }
        }

        internal AssetsEnumerator InternalMaterialAssets
        {
            get { return PartDocumentInstance.MaterialAssets; }
        }

        internal Materials InternalMaterials
        {
            get { return PartDocumentInstance.Materials; }
        }

        internal ModelingSettings InternalModelingSettings
        {
            get { return PartDocumentInstance.ModelingSettings; }
        }

        internal bool InternalNeedsMigrating
        {
            get { return PartDocumentInstance.NeedsMigrating; }
        }

        internal ObjectVisibility InternalObjectVisibility
        {
            get { return PartDocumentInstance.ObjectVisibility; }
        }

        internal bool InternalOpen
        {
            get { return PartDocumentInstance.Open; }
        }

        internal FileOwnershipEnum InternalOwnershipType
        {
            get { return PartDocumentInstance.OwnershipType; }
        }

        internal Object InternalParent
        {
            get { return PartDocumentInstance.Parent; }
        }

        internal AssetsEnumerator InternalPhysicalAssets
        {
            get { return PartDocumentInstance.PhysicalAssets; }
        }

        internal PrintManager InternalPrintManager
        {
            get { return PartDocumentInstance.PrintManager; }
        }

        internal PropertySets InternalPropertySets
        {
            get { return PartDocumentInstance.PropertySets; }
        }

        internal CommandTypesEnum InternalRecentChanges
        {
            get { return PartDocumentInstance.RecentChanges; }
        }

        internal DocumentDescriptorsEnumerator InternalReferencedDocumentDescriptors
        {
            get { return PartDocumentInstance.ReferencedDocumentDescriptors; }
        }

        internal DocumentsEnumerator InternalReferencedDocuments
        {
            get { return PartDocumentInstance.ReferencedDocuments; }
        }

        internal ReferencedFileDescriptors InternalReferencedFileDescriptors
        {
            get { return PartDocumentInstance.ReferencedFileDescriptors; }
        }

        internal DocumentsEnumerator InternalReferencedFiles
        {
            get { return PartDocumentInstance.ReferencedFiles; }
        }

        internal ReferencedOLEFileDescriptors InternalReferencedOLEFileDescriptors
        {
            get { return PartDocumentInstance.ReferencedOLEFileDescriptors; }
        }

        internal ReferenceKeyManager InternalReferenceKeyManager
        {
            get { return PartDocumentInstance.ReferenceKeyManager; }
        }

        internal DocumentsEnumerator InternalReferencingDocuments
        {
            get { return PartDocumentInstance.ReferencingDocuments; }
        }

        internal RenderStyles InternalRenderStyles
        {
            get { return PartDocumentInstance.RenderStyles; }
        }

        internal bool InternalRequiresUpdate
        {
            get { return PartDocumentInstance.RequiresUpdate; }
        }

        internal bool InternalReservedForWrite
        {
            get { return PartDocumentInstance.ReservedForWrite; }
        }

        internal string InternalReservedForWriteLogin
        {
            get { return PartDocumentInstance.ReservedForWriteLogin; }
        }

        internal string InternalReservedForWriteName
        {
            get { return PartDocumentInstance.ReservedForWriteName; }
        }

        internal DateTime InternalReservedForWriteTime
        {
            get { return PartDocumentInstance.ReservedForWriteTime; }
        }

        internal int InternalReservedForWriteVersion
        {
            get { return PartDocumentInstance.ReservedForWriteVersion; }
        }

        internal string InternalRevisionId
        {
            get { return PartDocumentInstance.RevisionId; }
        }

        internal SelectSet InternalSelectSet
        {
            get { return PartDocumentInstance.SelectSet; }
        }

        internal Sketch3DSettings InternalSketch3DSettings
        {
            get { return PartDocumentInstance.Sketch3DSettings; }
        }

        internal bool InternalSketchActive
        {
            get { return PartDocumentInstance.SketchActive; }
        }

        internal SketchSettings InternalSketchSettings
        {
            get { return PartDocumentInstance.SketchSettings; }
        }

        internal SoftwareVersion InternalSoftwareVersionCreated
        {
            get { return PartDocumentInstance.SoftwareVersionCreated; }
        }

        internal SoftwareVersion InternalSoftwareVersionSaved
        {
            get { return PartDocumentInstance.SoftwareVersionSaved; }
        }

        internal ReferenceStatusEnum InternalSubstitutePartStatus
        {
            get { return PartDocumentInstance.SubstitutePartStatus; }
        }

        //internal IPictureDisp InternalThumbnail
        //{
        //    get { return PartDocumentInstance.Thumbnail; }
        //}

        internal ThumbnailSaveOptionEnum InternalThumbnailSaveOption
        {
            get { return PartDocumentInstance.ThumbnailSaveOption; }
        }

        internal ObjectTypeEnum InternalType
        {
            get { return PartDocumentInstance.Type; }
        }

        internal UnitsOfMeasure InternalUnitsOfMeasure
        {
            get { return PartDocumentInstance.UnitsOfMeasure; }
        }

        internal InventorVBAProject InternalVBAProject
        {
            get { return PartDocumentInstance.VBAProject; }
        }

        internal Views InternalViews
        {
            get { return PartDocumentInstance.Views; }
        }

        internal bool Internal_ExcludeFromBOM { get; set; }

        internal Asset InternalActiveAppearance { get; set; }

        internal LightingStyle InternalActiveLightingStyle { get; set; }

        internal Asset InternalActiveMaterial { get; set; }

        internal RenderStyle InternalActiveRenderStyle { get; set; }

        internal AppearanceSourceTypeEnum InternalAppearanceSourceType { get; set; }

        internal bool InternalDirty { get; set; }

        internal CommandTypesEnum InternalDisabledCommandTypes { get; set; }

        internal string InternalDisplayName { get; set; }

        internal bool InternalDisplayNameOverridden { get; set; }

        internal string InternalFullFileName { get; set; }

        internal bool InternalIsSubstitutePart { get; set; }

        internal bool InternalReservedForWriteByMe { get; set; }

        internal SelectionPriorityEnum InternalSelectionPriority { get; set; }

        internal string InternalSubType { get; set; }
        #endregion

        #region Private constructors
        private InvPartDocument(InvPartDocument invPartDocument)
        {
            InternalPartDocument = invPartDocument.InternalPartDocument;
        }

        private InvPartDocument(Inventor.PartDocument invPartDocument)
        {
            InternalPartDocument = invPartDocument;
        }

        private InvPartDocument(InvDocument invDocument)
        {

            InternalPartDocument = (Inventor.PartDocument)invDocument.DocumentInstance;
        }
        #endregion

        #region Private methods
        private void InternalActivate()
        {
            PartDocumentInstance.Activate();
        }

        private void InternalBatchEdit(string batchEditParams, out MemberManagerErrorsEnum error)
        {
            PartDocumentInstance.BatchEdit( batchEditParams, out  error);
        }

        private void InternalClose(bool skipSave)
        {
            PartDocumentInstance.Close( skipSave);
        }

        private HighlightSet InternalCreateHighlightSet()
        {
            return PartDocumentInstance.CreateHighlightSet();
        }

        private DocumentsEnumerator InternalFindWhereUsed(string fullFileName)
        {
            return PartDocumentInstance.FindWhereUsed( fullFileName);
        }

        private void InternalGetLocationFoundIn(out string locationName, out LocationTypeEnum locationType)
        {
            PartDocumentInstance.GetLocationFoundIn(out  locationName, out  locationType);
        }

        private void InternalGetMissingAddInBehavior(out string clientId, out CommandTypesEnum disabledCommandTypesEnum, out ObjectCollection disabledCommands)
        {
            PartDocumentInstance.GetMissingAddInBehavior(out  clientId, out  disabledCommandTypesEnum, out  disabledCommands);
        }

        private Object InternalGetPrivateStorage(string storageName, bool createIfNecessary)
        {
            return PartDocumentInstance.GetPrivateStorage( storageName,  createIfNecessary);
        }

        private Object InternalGetPrivateStream(string streamName, bool createIfNecessary)
        {
            return PartDocumentInstance.GetPrivateStream( streamName,  createIfNecessary);
        }

        private void InternalGetSelectedObject(GenericObject selection, out ObjectTypeEnum objectType, out NameValueMap additionalData, out ComponentOccurrence containingOccurrence, ref Object selectedObject)
        {
            PartDocumentInstance.GetSelectedObject( selection, out  objectType, out  additionalData, out  containingOccurrence, ref  selectedObject);
        }

        private bool InternalHasPrivateStorage(string storageName)
        {
            return PartDocumentInstance.HasPrivateStorage( storageName);
        }

        private bool InternalHasPrivateStream(string streamName)
        {
            return PartDocumentInstance.HasPrivateStream( streamName);
        }

        private void InternalLockSaveSet()
        {
            PartDocumentInstance.LockSaveSet();
        }

        private void InternalMigrate()
        {
            PartDocumentInstance.Migrate();
        }

        private void InternalPutGraphicsLevelsOfDetail(int levelsOfDetail)
        {
            PartDocumentInstance.PutGraphicsLevelsOfDetail( levelsOfDetail);
        }

        private void InternalPutInternalName(string name, string number, string custom, out string internalName)
        {
            PartDocumentInstance.PutInternalName( name,  number,  custom, out  internalName);
        }

        private void InternalPutInternalNameAndRevisionId(string internalNameToken, string revisionIdToken, out string internalName, out string revisionId)
        {
            PartDocumentInstance.PutInternalNameAndRevisionId( internalNameToken,  revisionIdToken, out  internalName, out  revisionId);
        }

        private void InternalRebuild()
        {
            PartDocumentInstance.Rebuild();
        }

        private bool InternalRebuild2(bool acceptErrorsAndContinue)
        {
            return PartDocumentInstance.Rebuild2( acceptErrorsAndContinue);
        }

        private void InternalReleaseReference()
        {
            PartDocumentInstance.ReleaseReference();
        }

        private void InternalRevertReservedForWriteByMe()
        {
            PartDocumentInstance.RevertReservedForWriteByMe();
        }

        private void InternalSave()
        {
            PartDocumentInstance.Save();
        }

        private void InternalSave2(bool saveDependents, Object documentsToSave)
        {
            PartDocumentInstance.Save2( saveDependents,  documentsToSave);
        }

        private void InternalSaveAs(string fileName, bool saveCopyAs)
        {
            PartDocumentInstance.SaveAs( fileName,  saveCopyAs);
        }

        private void InternalSetMissingAddInBehavior(string clientId, CommandTypesEnum disabledCommandTypesEnum, Object disabledCommands)
        {
            PartDocumentInstance.SetMissingAddInBehavior( clientId,  disabledCommandTypesEnum,  disabledCommands);
        }

        private void InternalSetThumbnailSaveOption(ThumbnailSaveOptionEnum saveOption, string imageFullFileName)
        {
            PartDocumentInstance.SetThumbnailSaveOption( saveOption,  imageFullFileName);
        }

        private void InternalUpdate()
        {
            PartDocumentInstance.Update();
        }

        private bool InternalUpdate2(bool acceptErrorsAndContinue)
        {
            return PartDocumentInstance.Update2( acceptErrorsAndContinue);
        }

        private bool InternalUpdateSubstitutePart(bool ignoreErrors)
        {
            return PartDocumentInstance.UpdateSubstitutePart( ignoreErrors);
        }

        #endregion

        #region Public properties
        public Inventor.PartDocument PartDocumentInstance
        {
            get { return InternalPartDocument; }
            set { InternalPartDocument = value; }
        }

        public int _ComatoseNodesCount
        {
            get { return Internal_ComatoseNodesCount; }
        }

        public CommandIDEnum _DefaultCommand
        {
            get { return Internal_DefaultCommand; }
        }

        public _DocPerformanceMonitor _DocPerformanceMonitor
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

        public DocumentsEnumerator AllReferencedDocuments
        {
            get { return InternalAllReferencedDocuments; }
        }

        public AssetsEnumerator AppearanceAssets
        {
            get { return InternalAppearanceAssets; }
        }

        public Assets Assets
        {
            get { return InternalAssets; }
        }

        public AttributeManager AttributeManager
        {
            get { return InternalAttributeManager; }
        }

        public AttributeSets AttributeSets
        {
            get { return InternalAttributeSets; }
        }

        public BrowserPanes BrowserPanes
        {
            get { return InternalBrowserPanes; }
        }

        public bool Compacted
        {
            get { return InternalCompacted; }
        }

        public PartComponentDefinition ComponentDefinition
        {
            get { return InternalComponentDefinition; }
        }

        public PartComponentDefinitions ComponentDefinitions
        {
            get { return InternalComponentDefinitions; }
        }

        public string DatabaseRevisionId
        {
            get { return InternalDatabaseRevisionId; }
        }

        public string DefaultCommand
        {
            get { return InternalDefaultCommand; }
        }

        public DisabledCommandList DisabledCommandList
        {
            get { return InternalDisabledCommandList; }
        }

        public DisplaySettings DisplaySettings
        {
            get { return InternalDisplaySettings; }
        }

        public DocumentEvents DocumentEvents
        {
            get { return InternalDocumentEvents; }
        }

        public DocumentInterests DocumentInterests
        {
            get { return InternalDocumentInterests; }
        }

        public DocumentSubType DocumentSubType
        {
            get { return InternalDocumentSubType; }
        }

        public DocumentTypeEnum DocumentType
        {
            get { return InternalDocumentType; }
        }

        public EnvironmentManager EnvironmentManager
        {
            get { return InternalEnvironmentManager; }
        }

        public File File
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

        public GraphicsDataSetsCollection GraphicsDataSetsCollection
        {
            get { return InternalGraphicsDataSetsCollection; }
        }

        public HighlightSets HighlightSets
        {
            get { return InternalHighlightSets; }
        }

        public string InternalName
        {
            get { return InternalInternalName; }
        }

        public _Document InventorDocument
        {
            get { return InternalInventorDocument; }
        }

        public bool IsModifiable
        {
            get { return InternalIsModifiable; }
        }

        public LightingStyles LightingStyles
        {
            get { return InternalLightingStyles; }
        }

        public AssetsEnumerator MaterialAssets
        {
            get { return InternalMaterialAssets; }
        }

        public Materials Materials
        {
            get { return InternalMaterials; }
        }

        public ModelingSettings ModelingSettings
        {
            get { return InternalModelingSettings; }
        }

        public bool NeedsMigrating
        {
            get { return InternalNeedsMigrating; }
        }

        public ObjectVisibility ObjectVisibility
        {
            get { return InternalObjectVisibility; }
        }

        public bool Open
        {
            get { return InternalOpen; }
        }

        public FileOwnershipEnum OwnershipType
        {
            get { return InternalOwnershipType; }
        }

        public Object Parent
        {
            get { return InternalParent; }
        }

        public AssetsEnumerator PhysicalAssets
        {
            get { return InternalPhysicalAssets; }
        }

        public PrintManager PrintManager
        {
            get { return InternalPrintManager; }
        }

        public PropertySets PropertySets
        {
            get { return InternalPropertySets; }
        }

        public CommandTypesEnum RecentChanges
        {
            get { return InternalRecentChanges; }
        }

        public DocumentDescriptorsEnumerator ReferencedDocumentDescriptors
        {
            get { return InternalReferencedDocumentDescriptors; }
        }

        public DocumentsEnumerator ReferencedDocuments
        {
            get { return InternalReferencedDocuments; }
        }

        public ReferencedFileDescriptors ReferencedFileDescriptors
        {
            get { return InternalReferencedFileDescriptors; }
        }

        public DocumentsEnumerator ReferencedFiles
        {
            get { return InternalReferencedFiles; }
        }

        public ReferencedOLEFileDescriptors ReferencedOLEFileDescriptors
        {
            get { return InternalReferencedOLEFileDescriptors; }
        }

        public ReferenceKeyManager ReferenceKeyManager
        {
            get { return InternalReferenceKeyManager; }
        }

        public DocumentsEnumerator ReferencingDocuments
        {
            get { return InternalReferencingDocuments; }
        }

        public RenderStyles RenderStyles
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

        public SelectSet SelectSet
        {
            get { return InternalSelectSet; }
        }

        public Sketch3DSettings Sketch3DSettings
        {
            get { return InternalSketch3DSettings; }
        }

        public bool SketchActive
        {
            get { return InternalSketchActive; }
        }

        public SketchSettings SketchSettings
        {
            get { return InternalSketchSettings; }
        }

        public SoftwareVersion SoftwareVersionCreated
        {
            get { return InternalSoftwareVersionCreated; }
        }

        public SoftwareVersion SoftwareVersionSaved
        {
            get { return InternalSoftwareVersionSaved; }
        }

        public ReferenceStatusEnum SubstitutePartStatus
        {
            get { return InternalSubstitutePartStatus; }
        }

        //public IPictureDisp Thumbnail
        //{
        //    get { return InternalThumbnail; }
        //}

        public ThumbnailSaveOptionEnum ThumbnailSaveOption
        {
            get { return InternalThumbnailSaveOption; }
        }

        public ObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public UnitsOfMeasure UnitsOfMeasure
        {
            get { return InternalUnitsOfMeasure; }
        }

        public InventorVBAProject VBAProject
        {
            get { return InternalVBAProject; }
        }

        public Views Views
        {
            get { return InternalViews; }
        }

        public bool _ExcludeFromBOM
        {
            get { return Internal_ExcludeFromBOM; }
            set { Internal_ExcludeFromBOM = value; }
        }

        public Asset ActiveAppearance
        {
            get { return InternalActiveAppearance; }
            set { InternalActiveAppearance = value; }
        }

        public LightingStyle ActiveLightingStyle
        {
            get { return InternalActiveLightingStyle; }
            set { InternalActiveLightingStyle = value; }
        }

        public Asset ActiveMaterial
        {
            get { return InternalActiveMaterial; }
            set { InternalActiveMaterial = value; }
        }

        public RenderStyle ActiveRenderStyle
        {
            get { return InternalActiveRenderStyle; }
            set { InternalActiveRenderStyle = value; }
        }

        //public AppearanceSourceTypeEnum AppearanceSourceType
        //{
        //    get { return InternalAppearanceSourceType; }
        //    set { InternalAppearanceSourceType = value; }
        //}

        public bool Dirty
        {
            get { return InternalDirty; }
            set { InternalDirty = value; }
        }

        public CommandTypesEnum DisabledCommandTypes
        {
            get { return InternalDisabledCommandTypes; }
            set { InternalDisabledCommandTypes = value; }
        }

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

        public bool IsSubstitutePart
        {
            get { return InternalIsSubstitutePart; }
            set { InternalIsSubstitutePart = value; }
        }

        public bool ReservedForWriteByMe
        {
            get { return InternalReservedForWriteByMe; }
            set { InternalReservedForWriteByMe = value; }
        }

        public SelectionPriorityEnum SelectionPriority
        {
            get { return InternalSelectionPriority; }
            set { InternalSelectionPriority = value; }
        }

        public string SubType
        {
            get { return InternalSubType; }
            set { InternalSubType = value; }
        }

        #endregion

        #region Public static constructors
        public static InvPartDocument ByInvPartDocument(InvPartDocument invPartDocument)
        {
            return new InvPartDocument(invPartDocument);
        }

        public static InvPartDocument ByInvPartDocument(Inventor.PartDocument invPartDocument)
        {
            return new InvPartDocument(invPartDocument);
        }

        public static InvPartDocument ByInvDocument(InvDocument invDocument)
        {
            return new InvPartDocument(invDocument);
        }
        #endregion

        #region Public methods
        public void Activate()
        {
            InternalActivate();
        }

        public void BatchEdit(string batchEditParams, out MemberManagerErrorsEnum error)
        {
            InternalBatchEdit( batchEditParams, out  error);
        }

        public void Close(bool skipSave)
        {
            InternalClose( skipSave);
        }

        public HighlightSet CreateHighlightSet()
        {
            return InternalCreateHighlightSet();
        }

        public DocumentsEnumerator FindWhereUsed(string fullFileName)
        {
            return InternalFindWhereUsed( fullFileName);
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

        public void GetSelectedObject(GenericObject selection, out ObjectTypeEnum objectType, out NameValueMap additionalData, out ComponentOccurrence containingOccurrence, ref Object selectedObject)
        {
            InternalGetSelectedObject( selection, out  objectType, out  additionalData, out  containingOccurrence, ref  selectedObject);
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

        public void PutGraphicsLevelsOfDetail(int levelsOfDetail)
        {
            InternalPutGraphicsLevelsOfDetail( levelsOfDetail);
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

        public bool UpdateSubstitutePart(bool ignoreErrors)
        {
            return InternalUpdateSubstitutePart( ignoreErrors);
        }

        #endregion
    }
}
