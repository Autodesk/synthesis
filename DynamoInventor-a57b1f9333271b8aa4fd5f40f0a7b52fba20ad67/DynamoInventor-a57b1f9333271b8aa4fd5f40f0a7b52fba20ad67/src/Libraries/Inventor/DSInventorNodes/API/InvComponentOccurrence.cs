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
using Point = Autodesk.DesignScript.Geometry.Point;

namespace InventorLibrary.API
{
    [IsVisibleInDynamoLibrary(false)]
    public class InvComponentOccurrence
    {
        #region Internal properties
        internal Inventor.ComponentOccurrence InternalComponentOccurrence { get; set; }

        internal string Internal_DisplayName
        {
            get { return ComponentOccurrenceInstance._DisplayName; }
        }

        internal bool Internal_IsSimulationOccurrence
        {
            get { return ComponentOccurrenceInstance._IsSimulationOccurrence; }
        }

        internal string InternalActiveDesignViewRepresentation
        {
            get { return ComponentOccurrenceInstance.ActiveDesignViewRepresentation; }
        }

        internal string InternalActiveLevelOfDetailRepresentation
        {
            get { return ComponentOccurrenceInstance.ActiveLevelOfDetailRepresentation; }
        }

        internal Object InternalApplication
        {
            get { return ComponentOccurrenceInstance.Application; }
        }

        internal InvAttributeSets InternalAttributeSets
        {
            get { return InvAttributeSets.ByInvAttributeSets(ComponentOccurrenceInstance.AttributeSets); }
        }

        ////internal InvAssemblyConstraintsEnumerator InternalConstraints
        ////{
        ////    get { return InvAssemblyConstraintsEnumerator.ByInvAssemblyConstraintsEnumerator(ComponentOccurrenceInstance.Constraints); }
        ////}

        internal InvComponentDefinition InternalContextDefinition
        {
            get { return InvComponentDefinition.ByInvComponentDefinition(ComponentOccurrenceInstance.ContextDefinition); }
        }

        internal InvComponentDefinition InternalDefinition
        {
            get { return InvComponentDefinition.ByInvComponentDefinition(ComponentOccurrenceInstance.Definition); }
        }

        internal InvDocumentTypeEnum InternalDefinitionDocumentType
        {
            get { return ComponentOccurrenceInstance.DefinitionDocumentType.As<InvDocumentTypeEnum>(); }
        }

        ////internal InvComponentDefinitionReference InternalDefinitionReference
        ////{
        ////    get { return InvComponentDefinitionReference.ByInvComponentDefinitionReference(ComponentOccurrenceInstance.DefinitionReference); }
        ////}

        internal bool InternalEdited
        {
            get { return ComponentOccurrenceInstance.Edited; }
        }

        internal bool InternalHasBodyOverride
        {
            get { return ComponentOccurrenceInstance.HasBodyOverride; }
        }

        ////internal InviMateDefinitionsEnumerator InternaliMateDefinitions
        ////{
        ////    get { return InviMateDefinitionsEnumerator.ByInviMateDefinitionsEnumerator(ComponentOccurrenceInstance.iMateDefinitions); }
        ////}

        internal bool InternalIsiAssemblyMember
        {
            get { return ComponentOccurrenceInstance.IsiAssemblyMember; }
        }

        internal bool InternalIsiPartMember
        {
            get { return ComponentOccurrenceInstance.IsiPartMember; }
        }

        internal bool InternalIsPatternElement
        {
            get { return ComponentOccurrenceInstance.IsPatternElement; }
        }

        internal bool InternalIsSubstituteOccurrence
        {
            get { return ComponentOccurrenceInstance.IsSubstituteOccurrence; }
        }

        ////internal InvAssemblyJointsEnumerator InternalJoints
        ////{
        ////    get { return InvAssemblyJointsEnumerator.ByInvAssemblyJointsEnumerator(ComponentOccurrenceInstance.Joints); }
        ////}

        ////internal InvMassProperties InternalMassProperties
        ////{
        ////    get { return InvMassProperties.ByInvMassProperties(ComponentOccurrenceInstance.MassProperties); }
        ////}

        ////internal InvComponentOccurrencesEnumerator InternalOccurrencePath
        ////{
        ////    get { return InvComponentOccurrencesEnumerator.ByInvComponentOccurrencesEnumerator(ComponentOccurrenceInstance.OccurrencePath); }
        ////}

        internal InvAssemblyComponentDefinition InternalParent
        {
            get { return InvAssemblyComponentDefinition.ByInvAssemblyComponentDefinition(ComponentOccurrenceInstance.Parent); }
        }

        internal InvComponentOccurrence InternalParentOccurrence
        {
            get { return InvComponentOccurrence.ByInvComponentOccurrence(ComponentOccurrenceInstance.ParentOccurrence); }
        }

        ////internal InvOccurrencePatternElement InternalPatternElement
        ////{
        ////    get { return InvOccurrencePatternElement.ByInvOccurrencePatternElement(ComponentOccurrenceInstance.PatternElement); }
        ////}

        ////internal InvBox InternalRangeBox
        ////{
        ////    get { return InvBox.ByInvBox(ComponentOccurrenceInstance.RangeBox); }
        ////}

        ////internal InvDocumentDescriptor InternalReferencedDocumentDescriptor
        ////{
        ////    get { return InvDocumentDescriptor.ByInvDocumentDescriptor(ComponentOccurrenceInstance.ReferencedDocumentDescriptor); }
        ////}

        ////internal InvReferencedFileDescriptor InternalReferencedFileDescriptor
        ////{
        ////    get { return InvReferencedFileDescriptor.ByInvReferencedFileDescriptor(ComponentOccurrenceInstance.ReferencedFileDescriptor); }
        ////}

        ////internal InvComponentOccurrencesEnumerator InternalSubOccurrences
        ////{
        ////    get { return InvComponentOccurrencesEnumerator.ByInvComponentOccurrencesEnumerator(ComponentOccurrenceInstance.SubOccurrences); }
        ////}

        internal bool InternalSuppressed
        {
            get { return ComponentOccurrenceInstance.Suppressed; }
        }

        ////internal InvSurfaceBodies InternalSurfaceBodies
        ////{
        ////    get { return InvSurfaceBodies.ByInvSurfaceBodies(ComponentOccurrenceInstance.SurfaceBodies); }
        ////}

        internal InvObjectTypeEnum InternalType
        {
            get { return ComponentOccurrenceInstance.Type.As<InvObjectTypeEnum>(); }
        }

        internal string InternalActivePositionalRepresentation { get; set; }

        internal string InternalActivePositionalState { get; set; }

        internal bool InternalAdaptive { get; set; }

        internal Asset InternalAppearance { get; set; }

        internal AppearanceSourceTypeEnum InternalAppearanceSourceType { get; set; }

        internal BOMStructureEnum InternalBOMStructure { get; set; }

        internal bool InternalContactSet { get; set; }

        internal bool InternalCustomAdaptive { get; set; }

        internal ActionTypeEnum InternalDisabledActionTypes { get; set; }

        internal bool InternalEnabled { get; set; }

        internal bool InternalExcluded { get; set; }

        internal bool InternalFlexible { get; set; }

        internal bool InternalGrounded { get; set; }

        internal Object InternalInterchangeableComponents { get; set; }

        internal bool InternalIsAssociativeToDesignViewRepresentation { get; set; }

        internal bool InternalLocalAdaptive { get; set; }

        internal string InternalName { get; set; }

        internal double InternalOverrideOpacity { get; set; }

        internal bool InternalReference { get; set; }

        internal RenderStyle InternalRenderStyle { get; set; }

        internal bool InternalShowDegreesOfFreedom { get; set; }

        internal Matrix InternalTransformation { get; set; }

        internal bool InternalVisible { get; set; }
        #endregion

        #region Private constructors
        private InvComponentOccurrence(InvComponentOccurrence invComponentOccurrence)
        {
            InternalComponentOccurrence = invComponentOccurrence.InternalComponentOccurrence;
        }

        private InvComponentOccurrence(Inventor.ComponentOccurrence invComponentOccurrence)
        {
            InternalComponentOccurrence = invComponentOccurrence;
        }
        #endregion

        #region Private methods
        private void InternalChangeRowOfiAssemblyMember(Object newRow, Object options)
        {
            ComponentOccurrenceInstance.ChangeRowOfiAssemblyMember( newRow,  options);
        }

        private void InternalChangeRowOfiPartMember(Object newRow, Object customInput)
        {
            ComponentOccurrenceInstance.ChangeRowOfiPartMember( newRow,  customInput);
        }

        private void InternalCreateGeometryProxy(Object geometry, out Object result)
        {
            ComponentOccurrenceInstance.CreateGeometryProxy( geometry, out  result);
        }

        private void InternalDelete()
        {
            ComponentOccurrenceInstance.Delete();
        }

        private void InternalEdit()
        {
            ComponentOccurrenceInstance.Edit();
        }

        private void InternalExitEdit(ExitTypeEnum exitTo)
        {
            ComponentOccurrenceInstance.ExitEdit( exitTo);
        }

        private void InternalGetDegreesOfFreedom(out int translationDegreesCount, out ObjectsEnumerator translationDegreesVectors, out int rotationDegreesCount, out ObjectsEnumerator rotationDegreesVectors, out Point dOFCenter)
        {
            Inventor.Point dOFCenterInv;
            ComponentOccurrenceInstance.GetDegreesOfFreedom(out  translationDegreesCount, out  translationDegreesVectors, out  rotationDegreesCount, out  rotationDegreesVectors, out  dOFCenterInv);
            dOFCenter = dOFCenterInv.ToPoint();
        }

        private DisplayModeEnum InternalGetDisplayMode(out DisplayModeSourceTypeEnum displayModeSourceType)
        {
            return ComponentOccurrenceInstance.GetDisplayMode(out  displayModeSourceType);
        }

        private void InternalGetReferenceKey(ref byte[] referenceKey, int keyContext)
        {
            ComponentOccurrenceInstance.GetReferenceKey(ref  referenceKey,  keyContext);
        }

        private RenderStyle InternalGetRenderStyle(out StyleSourceTypeEnum styleSourceType)
        {
            return ComponentOccurrenceInstance.GetRenderStyle(out  styleSourceType);
        }

        private void InternalReplace(string fileName, bool replaceAll)
        {
            ComponentOccurrenceInstance.Replace( fileName,  replaceAll);
        }

        private void InternalSetDesignViewRepresentation(string representation, string reserved, bool associative)
        {
            ComponentOccurrenceInstance.SetDesignViewRepresentation( representation,  reserved,  associative);
        }

        private void InternalSetDisplayMode(DisplayModeSourceTypeEnum displayModeSourceType, Object displayMode)
        {
            ComponentOccurrenceInstance.SetDisplayMode( displayModeSourceType,  displayMode);
        }

        private void InternalSetLevelOfDetailRepresentation(string representation, bool skipDocumentSave)
        {
            ComponentOccurrenceInstance.SetLevelOfDetailRepresentation( representation,  skipDocumentSave);
        }

        private void InternalSetRenderStyle(StyleSourceTypeEnum styleSourceType, Object renderStyle)
        {
            ComponentOccurrenceInstance.SetRenderStyle( styleSourceType,  renderStyle);
        }

        private void InternalSetTransformWithoutConstraints(Matrix matrix)
        {
            ComponentOccurrenceInstance.SetTransformWithoutConstraints( matrix);
        }

        private void InternalShowRelationships()
        {
            ComponentOccurrenceInstance.ShowRelationships();
        }

        private void InternalSuppress(bool skipDocumentSave)
        {
            ComponentOccurrenceInstance.Suppress( skipDocumentSave);
        }

        private void InternalUnsuppress()
        {
            ComponentOccurrenceInstance.Unsuppress();
        }

        #endregion

        #region Public properties
        public Inventor.ComponentOccurrence ComponentOccurrenceInstance
        {
            get { return InternalComponentOccurrence; }
            set { InternalComponentOccurrence = value; }
        }

        public string _DisplayName
        {
            get { return Internal_DisplayName; }
        }

        public bool _IsSimulationOccurrence
        {
            get { return Internal_IsSimulationOccurrence; }
        }

        public string ActiveDesignViewRepresentation
        {
            get { return InternalActiveDesignViewRepresentation; }
        }

        public string ActiveLevelOfDetailRepresentation
        {
            get { return InternalActiveLevelOfDetailRepresentation; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        public InvAttributeSets AttributeSets
        {
            get { return InternalAttributeSets; }
        }

        ////public InvAssemblyConstraintsEnumerator Constraints
        ////{
        ////    get { return InternalConstraints; }
        ////}

        public InvComponentDefinition ContextDefinition
        {
            get { return InternalContextDefinition; }
        }

        public InvComponentDefinition Definition
        {
            get { return InternalDefinition; }
        }

        public InvDocumentTypeEnum DefinitionDocumentType
        {
            get { return InternalDefinitionDocumentType; }
        }

        ////public InvComponentDefinitionReference DefinitionReference
        ////{
        ////    get { return InternalDefinitionReference; }
        ////}

        public bool Edited
        {
            get { return InternalEdited; }
        }

        public bool HasBodyOverride
        {
            get { return InternalHasBodyOverride; }
        }

        ////public InviMateDefinitionsEnumerator iMateDefinitions
        ////{
        ////    get { return InternaliMateDefinitions; }
        ////}

        public bool IsiAssemblyMember
        {
            get { return InternalIsiAssemblyMember; }
        }

        public bool IsiPartMember
        {
            get { return InternalIsiPartMember; }
        }

        public bool IsPatternElement
        {
            get { return InternalIsPatternElement; }
        }

        public bool IsSubstituteOccurrence
        {
            get { return InternalIsSubstituteOccurrence; }
        }

        ////public InvAssemblyJointsEnumerator Joints
        ////{
        ////    get { return InternalJoints; }
        ////}

        ////public InvMassProperties MassProperties
        ////{
        ////    get { return InternalMassProperties; }
        ////}

        ////public InvComponentOccurrencesEnumerator OccurrencePath
        ////{
        ////    get { return InternalOccurrencePath; }
        ////}

        public InvAssemblyComponentDefinition Parent
        {
            get { return InternalParent; }
        }

        public InvComponentOccurrence ParentOccurrence
        {
            get { return InternalParentOccurrence; }
        }

        ////public InvOccurrencePatternElement PatternElement
        ////{
        ////    get { return InternalPatternElement; }
        ////}

        ////public InvBox RangeBox
        ////{
        ////    get { return InternalRangeBox; }
        ////}

        ////public InvDocumentDescriptor ReferencedDocumentDescriptor
        ////{
        ////    get { return InternalReferencedDocumentDescriptor; }
        ////}

        ////public InvReferencedFileDescriptor ReferencedFileDescriptor
        ////{
        ////    get { return InternalReferencedFileDescriptor; }
        ////}

        ////public InvComponentOccurrencesEnumerator SubOccurrences
        ////{
        ////    get { return InternalSubOccurrences; }
        ////}

        public bool Suppressed
        {
            get { return InternalSuppressed; }
        }

        ////public InvSurfaceBodies SurfaceBodies
        ////{
        ////    get { return InternalSurfaceBodies; }
        ////}

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public string ActivePositionalRepresentation
        {
            get { return InternalActivePositionalRepresentation; }
            set { InternalActivePositionalRepresentation = value; }
        }

        public string ActivePositionalState
        {
            get { return InternalActivePositionalState; }
            set { InternalActivePositionalState = value; }
        }

        public bool Adaptive
        {
            get { return InternalAdaptive; }
            set { InternalAdaptive = value; }
        }

        ////public InvAsset Appearance
        ////{
        ////    get { return InternalAppearance; }
        ////    set { InternalAppearance = value; }
        ////}

        ////public InvAppearanceSourceTypeEnum AppearanceSourceType
        ////{
        ////    get { return InternalAppearanceSourceType; }
        ////    set { InternalAppearanceSourceType = value; }
        ////}

        ////public InvBOMStructureEnum BOMStructure
        ////{
        ////    get { return InternalBOMStructure; }
        ////    set { InternalBOMStructure = value; }
        ////}

        public bool ContactSet
        {
            get { return InternalContactSet; }
            set { InternalContactSet = value; }
        }

        public bool CustomAdaptive
        {
            get { return InternalCustomAdaptive; }
            set { InternalCustomAdaptive = value; }
        }

        ////public InvActionTypeEnum DisabledActionTypes
        ////{
        ////    get { return InternalDisabledActionTypes; }
        ////    set { InternalDisabledActionTypes = value; }
        ////}

        public bool Enabled
        {
            get { return InternalEnabled; }
            set { InternalEnabled = value; }
        }

        public bool Excluded
        {
            get { return InternalExcluded; }
            set { InternalExcluded = value; }
        }

        public bool Flexible
        {
            get { return InternalFlexible; }
            set { InternalFlexible = value; }
        }

        public bool Grounded
        {
            get { return InternalGrounded; }
            set { InternalGrounded = value; }
        }

        public Object InterchangeableComponents
        {
            get { return InternalInterchangeableComponents; }
            set { InternalInterchangeableComponents = value; }
        }

        public bool IsAssociativeToDesignViewRepresentation
        {
            get { return InternalIsAssociativeToDesignViewRepresentation; }
            set { InternalIsAssociativeToDesignViewRepresentation = value; }
        }

        public bool LocalAdaptive
        {
            get { return InternalLocalAdaptive; }
            set { InternalLocalAdaptive = value; }
        }

        public string Name
        {
            get { return InternalName; }
            set { InternalName = value; }
        }

        public double OverrideOpacity
        {
            get { return InternalOverrideOpacity; }
            set { InternalOverrideOpacity = value; }
        }

        public bool Reference
        {
            get { return InternalReference; }
            set { InternalReference = value; }
        }

        ////public InvRenderStyle RenderStyle
        ////{
        ////    get { return InternalRenderStyle; }
        ////    set { InternalRenderStyle = value; }
        ////}

        public bool ShowDegreesOfFreedom
        {
            get { return InternalShowDegreesOfFreedom; }
            set { InternalShowDegreesOfFreedom = value; }
        }

        ////public InvMatrix Transformation
        ////{
        ////    get { return InternalTransformation; }
        ////    set { InternalTransformation = value; }
        ////}

        public bool Visible
        {
            get { return InternalVisible; }
            set { InternalVisible = value; }
        }

        #endregion

        #region Public static constructors
        public static InvComponentOccurrence ByInvComponentOccurrence(InvComponentOccurrence invComponentOccurrence)
        {
            return new InvComponentOccurrence(invComponentOccurrence);
        }
        public static InvComponentOccurrence ByInvComponentOccurrence(Inventor.ComponentOccurrence invComponentOccurrence)
        {
            return new InvComponentOccurrence(invComponentOccurrence);
        }
        #endregion

        #region Public methods
        public void ChangeRowOfiAssemblyMember(Object newRow, Object options)
        {
            InternalChangeRowOfiAssemblyMember( newRow,  options);
        }

        public void ChangeRowOfiPartMember(Object newRow, Object customInput)
        {
            InternalChangeRowOfiPartMember( newRow,  customInput);
        }

        public void CreateGeometryProxy(Object geometry, out Object result)
        {
            InternalCreateGeometryProxy( geometry, out  result);
        }

        public void Delete()
        {
            InternalDelete();
        }

        public void Edit()
        {
            InternalEdit();
        }

        public void ExitEdit(ExitTypeEnum exitTo)
        {
            InternalExitEdit( exitTo);
        }

        public void GetDegreesOfFreedom(out int translationDegreesCount, out ObjectsEnumerator translationDegreesVectors, out int rotationDegreesCount, out ObjectsEnumerator rotationDegreesVectors, out Point dOFCenter)
        {
            InternalGetDegreesOfFreedom(out  translationDegreesCount, out  translationDegreesVectors, out  rotationDegreesCount, out  rotationDegreesVectors, out  dOFCenter);
        }

        public DisplayModeEnum GetDisplayMode(out DisplayModeSourceTypeEnum displayModeSourceType)
        {
            return InternalGetDisplayMode(out  displayModeSourceType);
        }

        public void GetReferenceKey(ref byte[] referenceKey, int keyContext)
        {
            InternalGetReferenceKey(ref  referenceKey,  keyContext);
        }

        public RenderStyle GetRenderStyle(out StyleSourceTypeEnum styleSourceType)
        {
            return InternalGetRenderStyle(out  styleSourceType);
        }

        public void Replace(string fileName, bool replaceAll)
        {
            InternalReplace( fileName,  replaceAll);
        }

        public void SetDesignViewRepresentation(string representation, string reserved, bool associative)
        {
            InternalSetDesignViewRepresentation( representation,  reserved,  associative);
        }

        public void SetDisplayMode(DisplayModeSourceTypeEnum displayModeSourceType, Object displayMode)
        {
            InternalSetDisplayMode( displayModeSourceType,  displayMode);
        }

        public void SetLevelOfDetailRepresentation(string representation, bool skipDocumentSave)
        {
            InternalSetLevelOfDetailRepresentation( representation,  skipDocumentSave);
        }

        public void SetRenderStyle(StyleSourceTypeEnum styleSourceType, Object renderStyle)
        {
            InternalSetRenderStyle( styleSourceType,  renderStyle);
        }

        public void SetTransformWithoutConstraints(Matrix matrix)
        {
            InternalSetTransformWithoutConstraints( matrix);
        }

        public void ShowRelationships()
        {
            InternalShowRelationships();
        }

        public void Suppress(bool skipDocumentSave)
        {
            InternalSuppress( skipDocumentSave);
        }

        public void Unsuppress()
        {
            InternalUnsuppress();
        }

        #endregion
    }
}
