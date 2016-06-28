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
    public class InvPartComponentDefinition
    {
        #region Internal properties
        internal Inventor.PartComponentDefinition InternalPartComponentDefinition { get; set; }

        //internal InvAnalysisManager InternalAnalysisManager
        //{
        //    get { return InvAnalysisManager.ByInvAnalysisManager(PartComponentDefinitionInstance.AnalysisManager); }
        //}

        //internal InvAnnotationPlanes InternalAnnotationPlanes
        //{
        //    get { return InvAnnotationPlanes.ByInvAnnotationPlanes(PartComponentDefinitionInstance.AnnotationPlanes); }
        //}

        //internal InvObjectsEnumerator InternalAppearanceOverridesObjects
        //{
        //    get { return InvObjectsEnumerator.ByInvObjectsEnumerator(PartComponentDefinitionInstance.AppearanceOverridesObjects); }
        //}

        internal Object InternalApplication
        {
            get { return PartComponentDefinitionInstance.Application; }
        }

        internal InvAttributeSets InternalAttributeSets
        {
            get { return InvAttributeSets.ByInvAttributeSets(PartComponentDefinitionInstance.AttributeSets); }
        }

        //internal InvBIMComponent InternalBIMComponent
        //{
        //    get { return InvBIMComponent.ByInvBIMComponent(PartComponentDefinitionInstance.BIMComponent); }
        //}

        //internal InvBOMQuantity InternalBOMQuantity
        //{
        //    get { return InvBOMQuantity.ByInvBOMQuantity(PartComponentDefinitionInstance.BOMQuantity); }
        //}

        //internal InvClientGraphicsCollection InternalClientGraphicsCollection
        //{
        //    get { return InvClientGraphicsCollection.ByInvClientGraphicsCollection(PartComponentDefinitionInstance.ClientGraphicsCollection); }
        //}

        //internal InvDataIO InternalDataIO
        //{
        //    get { return InvDataIO.ByInvDataIO(PartComponentDefinitionInstance.DataIO); }
        //}

        internal Object InternalDocument
        {
            get { return PartComponentDefinitionInstance.Document; }
        }

        //internal InvPartFeatures InternalFeatures
        //{
        //    get { return InvPartFeatures.ByInvPartFeatures(PartComponentDefinitionInstance.Features); }
        //}

        internal bool InternalHasMultipleSolidBodies
        {
            get { return PartComponentDefinitionInstance.HasMultipleSolidBodies; }
        }

        //internal InviMateDefinitions InternaliMateDefinitions
        //{
        //    get { return InviMateDefinitions.ByInviMateDefinitions(PartComponentDefinitionInstance.iMateDefinitions); }
        //}

        //internal InvComponentDefinitionReferences InternalImmediateReferencedDefinitions
        //{
        //    get { return InvComponentDefinitionReferences.ByInvComponentDefinitionReferences(PartComponentDefinitionInstance.ImmediateReferencedDefinitions); }
        //}

        //internal InviPartFactory InternaliPartFactory
        //{
        //    get { return InviPartFactory.ByInviPartFactory(PartComponentDefinitionInstance.iPartFactory); }
        //}

        //internal InviPartMember InternaliPartMember
        //{
        //    get { return InviPartMember.ByInviPartMember(PartComponentDefinitionInstance.iPartMember); }
        //}

        internal bool InternalIsContentMember
        {
            get { return PartComponentDefinitionInstance.IsContentMember; }
        }

        internal bool InternalIsiPartFactory
        {
            get { return PartComponentDefinitionInstance.IsiPartFactory; }
        }

        internal bool InternalIsiPartMember
        {
            get { return PartComponentDefinitionInstance.IsiPartMember; }
        }

        //internal InvMassProperties InternalMassProperties
        //{
        //    get { return InvMassProperties.ByInvMassProperties(PartComponentDefinitionInstance.MassProperties); }
        //}

        //internal InvModelAnnotations InternalModelAnnotations
        //{
        //    get { return InvModelAnnotations.ByInvModelAnnotations(PartComponentDefinitionInstance.ModelAnnotations); }
        //}

        internal string InternalModelGeometryVersion
        {
            get { return PartComponentDefinitionInstance.ModelGeometryVersion; }
        }

        internal InvComponentOccurrences InternalOccurrences
        {
            get { return InvComponentOccurrences.ByInvComponentOccurrences(PartComponentDefinitionInstance.Occurrences); }
        }

        //internal InvParameters InternalParameters
        //{
        //    get { return InvParameters.ByInvParameters(PartComponentDefinitionInstance.Parameters); }
        //}

        //internal InvPartEvents InternalPartEvents
        //{
        //    get { return InvPartEvents.ByInvPartEvents(PartComponentDefinitionInstance.PartEvents); }
        //}

        //internal InvBox InternalRangeBox
        //{
        //    get { return InvBox.ByInvBox(PartComponentDefinitionInstance.RangeBox); }
        //}

        //internal InvReferenceComponents InternalReferenceComponents
        //{
        //    get { return InvReferenceComponents.ByInvReferenceComponents(PartComponentDefinitionInstance.ReferenceComponents); }
        //}

        //internal InvRepresentationsManager InternalRepresentationsManager
        //{
        //    get { return InvRepresentationsManager.ByInvRepresentationsManager(PartComponentDefinitionInstance.RepresentationsManager); }
        //}

        internal bool InternalRolledBackForEdit
        {
            get { return PartComponentDefinitionInstance.RolledBackForEdit; }
        }

        //internal InvSketchBlockDefinitions InternalSketchBlockDefinitions
        //{
        //    get { return InvSketchBlockDefinitions.ByInvSketchBlockDefinitions(PartComponentDefinitionInstance.SketchBlockDefinitions); }
        //}

        //internal InvPlanarSketches InternalSketches
        //{
        //    get { return InvPlanarSketches.ByInvPlanarSketches(PartComponentDefinitionInstance.Sketches); }
        //}

        //internal InvSketches3D InternalSketches3D
        //{
        //    get { return InvSketches3D.ByInvSketches3D(PartComponentDefinitionInstance.Sketches3D); }
        //}

        //internal InvSurfaceBodies InternalSurfaceBodies
        //{
        //    get { return InvSurfaceBodies.ByInvSurfaceBodies(PartComponentDefinitionInstance.SurfaceBodies); }
        //}

        internal InvObjectTypeEnum InternalType
        {
            get { return PartComponentDefinitionInstance.Type.As<InvObjectTypeEnum>(); }
        }

        //internal InvUserCoordinateSystems InternalUserCoordinateSystems
        //{
        //    get { return InvUserCoordinateSystems.ByInvUserCoordinateSystems(PartComponentDefinitionInstance.UserCoordinateSystems); }
        //}

        //internal InvWorkAxes InternalWorkAxes
        //{
        //    get { return InvWorkAxes.ByInvWorkAxes(PartComponentDefinitionInstance.WorkAxes); }
        //}

        internal InvWorkPlanes InternalWorkPlanes
        {
            get { return InvWorkPlanes.ByInvWorkPlanes(PartComponentDefinitionInstance.WorkPlanes); }
        }

        internal InvWorkPoints InternalWorkPoints
        {
            get { return InvWorkPoints.ByInvWorkPoints(PartComponentDefinitionInstance.WorkPoints); }
        }

        //internal InvWorkSurfaces InternalWorkSurfaces
        //{
        //    get { return InvWorkSurfaces.ByInvWorkSurfaces(PartComponentDefinitionInstance.WorkSurfaces); }
        //}

        internal BOMStructureEnum InternalBOMStructure { get; set; }

        internal bool InternalCompactModelHistory { get; set; }

        internal bool InternalCompactModelHistoryOnNextSave { get; set; }

        internal Material InternalMaterial { get; set; }
        #endregion

        #region Private constructors
        private InvPartComponentDefinition(InvPartComponentDefinition invPartComponentDefinition)
        {
            InternalPartComponentDefinition = invPartComponentDefinition.InternalPartComponentDefinition;
        }

        private InvPartComponentDefinition(Inventor.PartComponentDefinition invPartComponentDefinition)
        {
            InternalPartComponentDefinition = invPartComponentDefinition;
        }
        #endregion

        #region Private methods
        private void InternalClearAppearanceOverrides(Object appearanceOverrideObjects)
        {
            PartComponentDefinitionInstance.ClearAppearanceOverrides( appearanceOverrideObjects);
        }

        private iPartFactory InternalCreateFactory()
        {
            return PartComponentDefinitionInstance.CreateFactory();
        }

        private GeometryIntent InternalCreateGeometryIntent(Object geometry, Object intent)
        {
            return PartComponentDefinitionInstance.CreateGeometryIntent( geometry,  intent);
        }

        private void InternalDeleteObjects(ObjectCollection objects, bool retainConsumedSketches, bool retainDepFeatsAndSketches, bool retainDepWorkFeats)
        {
            PartComponentDefinitionInstance.DeleteObjects( objects,  retainConsumedSketches,  retainDepFeatsAndSketches,  retainDepWorkFeats);
        }

        private void InternalExportObjects(ObjectCollection objects)
        {
            PartComponentDefinitionInstance.ExportObjects( objects);
        }

        //private ObjectsEnumerator InternalFindUsingPoint(Point point, SelectionFilterEnum[] objectTypes, Object proximityTolerance, bool visibleObjectsOnly)
        //{
        //    return PartComponentDefinitionInstance.FindUsingPoint( point,  objectTypes,  proximityTolerance,  visibleObjectsOnly);
        //}

        //private void InternalFindUsingRay(Point rayStartPoint, UnitVector rayDirection, double radius, out ObjectsEnumerator foundEntities, out ObjectsEnumerator locationPoints, bool findFirstOnly)
        //{
        //    PartComponentDefinitionInstance.FindUsingRay( rayStartPoint,  rayDirection,  radius, out  foundEntities, out  locationPoints,  findFirstOnly);
        //}

        //private ObjectsEnumerator InternalFindUsingVector(Point originPoint, UnitVector direction, SelectionFilterEnum[] objectTypes, bool useCylinder, Object proximityTolerance, bool visibleObjectsOnly, out Object locationPoints)
        //{
        //    return PartComponentDefinitionInstance.FindUsingVector( originPoint,  direction,  objectTypes,  useCylinder,  proximityTolerance,  visibleObjectsOnly, out  locationPoints);
        //}

        private void InternalGetEndOfPartPosition(out Object after, out Object before)
        {
            PartComponentDefinitionInstance.GetEndOfPartPosition(out  after, out  before);
        }

        private void InternalSetEndOfPartToTopOrBottom(bool setToTop)
        {
            PartComponentDefinitionInstance.SetEndOfPartToTopOrBottom( setToTop);
        }

        private void InternalSuppressFeatures(ObjectCollection features)
        {
            PartComponentDefinitionInstance.SuppressFeatures( features);
        }

        private void InternalUnsuppressFeatures(ObjectCollection features)
        {
            PartComponentDefinitionInstance.UnsuppressFeatures( features);
        }

        #endregion

        #region Public properties
        public Inventor.PartComponentDefinition PartComponentDefinitionInstance
        {
            get { return InternalPartComponentDefinition; }
            set { InternalPartComponentDefinition = value; }
        }

        //public InvAnalysisManager AnalysisManager
        //{
        //    get { return InternalAnalysisManager; }
        //}

        //public InvAnnotationPlanes AnnotationPlanes
        //{
        //    get { return InternalAnnotationPlanes; }
        //}

        //public InvObjectsEnumerator AppearanceOverridesObjects
        //{
        //    get { return InternalAppearanceOverridesObjects; }
        //}

        public Object Application
        {
            get { return InternalApplication; }
        }

        public InvAttributeSets AttributeSets
        {
            get { return InternalAttributeSets; }
        }

        //public InvBIMComponent BIMComponent
        //{
        //    get { return InternalBIMComponent; }
        //}

        //public InvBOMQuantity BOMQuantity
        //{
        //    get { return InternalBOMQuantity; }
        //}

        //public InvClientGraphicsCollection ClientGraphicsCollection
        //{
        //    get { return InternalClientGraphicsCollection; }
        //}

        //public InvDataIO DataIO
        //{
        //    get { return InternalDataIO; }
        //}

        public Object Document
        {
            get { return InternalDocument; }
        }

        //public InvPartFeatures Features
        //{
        //    get { return InternalFeatures; }
        //}

        public bool HasMultipleSolidBodies
        {
            get { return InternalHasMultipleSolidBodies; }
        }

        //public InviMateDefinitions iMateDefinitions
        //{
        //    get { return InternaliMateDefinitions; }
        //}

        //public InvComponentDefinitionReferences ImmediateReferencedDefinitions
        //{
        //    get { return InternalImmediateReferencedDefinitions; }
        //}

        //public InviPartFactory iPartFactory
        //{
        //    get { return InternaliPartFactory; }
        //}

        //public InviPartMember iPartMember
        //{
        //    get { return InternaliPartMember; }
        //}

        public bool IsContentMember
        {
            get { return InternalIsContentMember; }
        }

        public bool IsiPartFactory
        {
            get { return InternalIsiPartFactory; }
        }

        public bool IsiPartMember
        {
            get { return InternalIsiPartMember; }
        }

        //public InvMassProperties MassProperties
        //{
        //    get { return InternalMassProperties; }
        //}

        //public InvModelAnnotations ModelAnnotations
        //{
        //    get { return InternalModelAnnotations; }
        //}

        public string ModelGeometryVersion
        {
            get { return InternalModelGeometryVersion; }
        }

        public InvComponentOccurrences Occurrences
        {
            get { return InternalOccurrences; }
        }

        //public InvParameters Parameters
        //{
        //    get { return InternalParameters; }
        //}

        //public InvPartEvents PartEvents
        //{
        //    get { return InternalPartEvents; }
        //}

        //public InvBox RangeBox
        //{
        //    get { return InternalRangeBox; }
        //}

        //public InvReferenceComponents ReferenceComponents
        //{
        //    get { return InternalReferenceComponents; }
        //}

        //public InvRepresentationsManager RepresentationsManager
        //{
        //    get { return InternalRepresentationsManager; }
        //}

        public bool RolledBackForEdit
        {
            get { return InternalRolledBackForEdit; }
        }

        //public InvSketchBlockDefinitions SketchBlockDefinitions
        //{
        //    get { return InternalSketchBlockDefinitions; }
        //}

        //public InvPlanarSketches Sketches
        //{
        //    get { return InternalSketches; }
        //}

        //public InvSketches3D Sketches3D
        //{
        //    get { return InternalSketches3D; }
        //}

        //public InvSurfaceBodies SurfaceBodies
        //{
        //    get { return InternalSurfaceBodies; }
        //}

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        //public InvUserCoordinateSystems UserCoordinateSystems
        //{
        //    get { return InternalUserCoordinateSystems; }
        //}

        //public InvWorkAxes WorkAxes
        //{
        //    get { return InternalWorkAxes; }
        //}

        public InvWorkPlanes WorkPlanes
        {
            get { return InternalWorkPlanes; }
        }

        public InvWorkPoints WorkPoints
        {
            get { return InternalWorkPoints; }
        }

        //public InvWorkSurfaces WorkSurfaces
        //{
        //    get { return InternalWorkSurfaces; }
        //}

        //public InvBOMStructureEnum BOMStructure
        //{
        //    get { return InternalBOMStructure; }
        //    set { InternalBOMStructure = value; }
        //}

        public bool CompactModelHistory
        {
            get { return InternalCompactModelHistory; }
            set { InternalCompactModelHistory = value; }
        }

        public bool CompactModelHistoryOnNextSave
        {
            get { return InternalCompactModelHistoryOnNextSave; }
            set { InternalCompactModelHistoryOnNextSave = value; }
        }

        //public InvMaterial Material
        //{
        //    get { return InternalMaterial; }
        //    set { InternalMaterial = value; }
        //}

        #endregion

        #region Public static constructors
        public static InvPartComponentDefinition ByInvPartComponentDefinition(InvPartComponentDefinition invPartComponentDefinition)
        {
            return new InvPartComponentDefinition(invPartComponentDefinition);
        }

        public static InvPartComponentDefinition ByInvPartComponentDefinition(Inventor.PartComponentDefinition invPartComponentDefinition)
        {
            return new InvPartComponentDefinition(invPartComponentDefinition);
        }
        #endregion

        #region Public methods
        public void ClearAppearanceOverrides(Object appearanceOverrideObjects)
        {
            InternalClearAppearanceOverrides( appearanceOverrideObjects);
        }

        //public InviPartFactory CreateFactory()
        //{
        //    return InternalCreateFactory();
        //}

        //public InvGeometryIntent CreateGeometryIntent(Object geometry, Object intent)
        //{
        //    return InternalCreateGeometryIntent( geometry,  intent);
        //}

        public void DeleteObjects(ObjectCollection objects, bool retainConsumedSketches, bool retainDepFeatsAndSketches, bool retainDepWorkFeats)
        {
            InternalDeleteObjects( objects,  retainConsumedSketches,  retainDepFeatsAndSketches,  retainDepWorkFeats);
        }

        public void ExportObjects(ObjectCollection objects)
        {
            InternalExportObjects( objects);
        }

        //public InvObjectsEnumerator FindUsingPoint(Point point, SelectionFilterEnum[] objectTypes, Object proximityTolerance, bool visibleObjectsOnly)
        //{
        //    return InternalFindUsingPoint( point,  objectTypes,  proximityTolerance,  visibleObjectsOnly);
        //}

        //public void FindUsingRay(Point rayStartPoint, UnitVector rayDirection, double radius, out ObjectsEnumerator foundEntities, out ObjectsEnumerator locationPoints, bool findFirstOnly)
        //{
        //    InternalFindUsingRay( rayStartPoint,  rayDirection,  radius, out  foundEntities, out  locationPoints,  findFirstOnly);
        //}

        //public InvObjectsEnumerator FindUsingVector(Point originPoint, UnitVector direction, SelectionFilterEnum[] objectTypes, bool useCylinder, Object proximityTolerance, bool visibleObjectsOnly, out Object locationPoints)
        //{
        //    return InternalFindUsingVector( originPoint,  direction,  objectTypes,  useCylinder,  proximityTolerance,  visibleObjectsOnly, out  locationPoints);
        //}

        public void GetEndOfPartPosition(out Object after, out Object before)
        {
            InternalGetEndOfPartPosition(out  after, out  before);
        }

        public void SetEndOfPartToTopOrBottom(bool setToTop)
        {
            InternalSetEndOfPartToTopOrBottom( setToTop);
        }

        public void SuppressFeatures(ObjectCollection features)
        {
            InternalSuppressFeatures( features);
        }

        public void UnsuppressFeatures(ObjectCollection features)
        {
            InternalUnsuppressFeatures( features);
        }

        #endregion
    }
}
