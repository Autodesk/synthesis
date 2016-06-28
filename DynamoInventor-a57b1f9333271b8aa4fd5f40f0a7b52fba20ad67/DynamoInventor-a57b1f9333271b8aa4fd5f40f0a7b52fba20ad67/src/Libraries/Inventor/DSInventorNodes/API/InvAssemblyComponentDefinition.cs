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
    public class InvAssemblyComponentDefinition
    {
        #region Internal properties
        internal Inventor.AssemblyComponentDefinition InternalAssemblyComponentDefinition { get; set; }

        //internal InvComponentOccurrence InternalActiveOccurrence
        //{
        //    get { return InvComponentOccurrence.ByInvComponentOccurrence(AssemblyComponentDefinitionInstance.ActiveOccurrence); }
        //}


        //internal InvObjectsEnumerator InternalAppearanceOverridesObjects
        //{
        //    get { return InvObjectsEnumerator.ByInvObjectsEnumerator(AssemblyComponentDefinitionInstance.AppearanceOverridesObjects); }
        //}


        internal Object InternalApplication
        {
            get { return AssemblyComponentDefinitionInstance.Application; }
        }

        //internal InvAssemblyEvents InternalAssemblyEvents
        //{
        //    get { return InvAssemblyEvents.ByInvAssemblyEvents(AssemblyComponentDefinitionInstance.AssemblyEvents); }
        //}


        internal InvAttributeSets InternalAttributeSets
        {
            get { return InvAttributeSets.ByInvAttributeSets(AssemblyComponentDefinitionInstance.AttributeSets); }
        }


        //internal InvBIMComponent InternalBIMComponent
        //{
        //    get { return InvBIMComponent.ByInvBIMComponent(AssemblyComponentDefinitionInstance.BIMComponent); }
        //}


        //internal InvBOM InternalBOM
        //{
        //    get { return InvBOM.ByInvBOM(AssemblyComponentDefinitionInstance.BOM); }
        //}


        //internal InvBOMQuantity InternalBOMQuantity
        //{
        //    get { return InvBOMQuantity.ByInvBOMQuantity(AssemblyComponentDefinitionInstance.BOMQuantity); }
        //}


        //internal InvClientGraphicsCollection InternalClientGraphicsCollection
        //{
        //    get { return InvClientGraphicsCollection.ByInvClientGraphicsCollection(AssemblyComponentDefinitionInstance.ClientGraphicsCollection); }
        //}


        //internal InvAssemblyConstraints InternalConstraints
        //{
        //    get { return InvAssemblyConstraints.ByInvAssemblyConstraints(AssemblyComponentDefinitionInstance.Constraints); }
        //}


        //internal InvDataIO InternalDataIO
        //{
        //    get { return InvDataIO.ByInvDataIO(AssemblyComponentDefinitionInstance.DataIO); }
        //}


        internal Object InternalDocument
        {
            get { return AssemblyComponentDefinitionInstance.Document; }
        }

        //internal InvFeatures InternalFeatures
        //{
        //    get { return InvFeatures.ByInvFeatures(AssemblyComponentDefinitionInstance.Features); }
        //}


        //internal InviAssemblyFactory InternaliAssemblyFactory
        //{
        //    get { return InviAssemblyFactory.ByInviAssemblyFactory(AssemblyComponentDefinitionInstance.iAssemblyFactory); }
        //}


        //internal InviAssemblyMember InternaliAssemblyMember
        //{
        //    get { return InviAssemblyMember.ByInviAssemblyMember(AssemblyComponentDefinitionInstance.iAssemblyMember); }
        //}


        //internal InviMateDefinitions InternaliMateDefinitions
        //{
        //    get { return InviMateDefinitions.ByInviMateDefinitions(AssemblyComponentDefinitionInstance.iMateDefinitions); }
        //}


        //internal InviMateResults InternaliMateResults
        //{
        //    get { return InviMateResults.ByInviMateResults(AssemblyComponentDefinitionInstance.iMateResults); }
        //}


        //internal InvComponentDefinitionReferences InternalImmediateReferencedDefinitions
        //{
        //    get { return InvComponentDefinitionReferences.ByInvComponentDefinitionReferences(AssemblyComponentDefinitionInstance.ImmediateReferencedDefinitions); }
        //}


        internal bool InternalIsiAssemblyFactory
        {
            get { return AssemblyComponentDefinitionInstance.IsiAssemblyFactory; }
        }

        internal bool InternalIsiAssemblyMember
        {
            get { return AssemblyComponentDefinitionInstance.IsiAssemblyMember; }
        }

        //internal InvAssemblyJoints InternalJoints
        //{
        //    get { return InvAssemblyJoints.ByInvAssemblyJoints(AssemblyComponentDefinitionInstance.Joints); }
        //}


        //internal InvMassProperties InternalMassProperties
        //{
        //    get { return InvMassProperties.ByInvMassProperties(AssemblyComponentDefinitionInstance.MassProperties); }
        //}


        internal string InternalMaster
        {
            get { return AssemblyComponentDefinitionInstance.Master; }
        }

        internal bool InternalMasterActive
        {
            get { return AssemblyComponentDefinitionInstance.MasterActive; }
        }

        internal string InternalModelGeometryVersion
        {
            get { return AssemblyComponentDefinitionInstance.ModelGeometryVersion; }
        }

        //internal InvOccurrencePatterns InternalOccurrencePatterns
        //{
        //    get { return InvOccurrencePatterns.ByInvOccurrencePatterns(AssemblyComponentDefinitionInstance.OccurrencePatterns); }
        //}


        //internal InvComponentOccurrences InternalOccurrences
        //{
        //    get { return InvComponentOccurrences.ByInvComponentOccurrences(AssemblyComponentDefinitionInstance.Occurrences); }
        //}


        //internal InvParameters InternalParameters
        //{
        //    get { return InvParameters.ByInvParameters(AssemblyComponentDefinitionInstance.Parameters); }
        //}


        //internal Inv_AssemblyDocument InternalParent
        //{
        //    get { return Inv_AssemblyDocument.ByInv_AssemblyDocument(AssemblyComponentDefinitionInstance.Parent); }
        //}


        //internal InvBox InternalRangeBox
        //{
        //    get { return InvBox.ByInvBox(AssemblyComponentDefinitionInstance.RangeBox); }
        //}


        //internal InvRepresentationsManager InternalRepresentationsManager
        //{
        //    get { return InvRepresentationsManager.ByInvRepresentationsManager(AssemblyComponentDefinitionInstance.RepresentationsManager); }
        //}


        //internal InvSimulationManager InternalSimulationManager
        //{
        //    get { return InvSimulationManager.ByInvSimulationManager(AssemblyComponentDefinitionInstance.SimulationManager); }
        //}


        //internal InvPlanarSketches InternalSketches
        //{
        //    get { return InvPlanarSketches.ByInvPlanarSketches(AssemblyComponentDefinitionInstance.Sketches); }
        //}


        //internal InvSurfaceBodies InternalSurfaceBodies
        //{
        //    get { return InvSurfaceBodies.ByInvSurfaceBodies(AssemblyComponentDefinitionInstance.SurfaceBodies); }
        //}


        internal InvObjectTypeEnum InternalType
        {
            get { return AssemblyComponentDefinitionInstance.Type.As<InvObjectTypeEnum>(); }
        }


        //internal InvUserCoordinateSystems InternalUserCoordinateSystems
        //{
        //    get { return InvUserCoordinateSystems.ByInvUserCoordinateSystems(AssemblyComponentDefinitionInstance.UserCoordinateSystems); }
        //}


        //internal InvWorkAxes InternalWorkAxes
        //{
        //    get { return InvWorkAxes.ByInvWorkAxes(AssemblyComponentDefinitionInstance.WorkAxes); }
        //}


        internal InvWorkPlanes InternalWorkPlanes
        {
            get { return InvWorkPlanes.ByInvWorkPlanes(AssemblyComponentDefinitionInstance.WorkPlanes); }
        }


        internal InvWorkPoints InternalWorkPoints
        {
            get { return InvWorkPoints.ByInvWorkPoints(AssemblyComponentDefinitionInstance.WorkPoints); }
        }


        internal string InternalActivePositionalState { get; set; }

        internal BOMStructureEnum InternalBOMStructure { get; set; }

        internal Material InternalDefaultMaterial { get; set; }

        internal Asset InternalDefaultVirtualComponentMaterial { get; set; }
        #endregion

        #region Private constructors
        private InvAssemblyComponentDefinition(InvAssemblyComponentDefinition invAssemblyComponentDefinition)
        {
            InternalAssemblyComponentDefinition = invAssemblyComponentDefinition.InternalAssemblyComponentDefinition;
        }

        private InvAssemblyComponentDefinition(Inventor.AssemblyComponentDefinition invAssemblyComponentDefinition)
        {
            InternalAssemblyComponentDefinition = invAssemblyComponentDefinition;
        }
        #endregion

        #region Private methods
        private Object InternalAdjustProxyContext(Object objectProxy)
        {
            return AssemblyComponentDefinitionInstance.AdjustProxyContext( objectProxy);
        }

        private InterferenceResults InternalAnalyzeInterference(ObjectCollection set1, Object set2)
        {
            return AssemblyComponentDefinitionInstance.AnalyzeInterference( set1,  set2);
        }

        private void InternalClearAppearanceOverrides(Object appearanceOverrideObjects)
        {
            AssemblyComponentDefinitionInstance.ClearAppearanceOverrides( appearanceOverrideObjects);
        }

        private iAssemblyFactory InternalCreateFactory()
        {
            return AssemblyComponentDefinitionInstance.CreateFactory();
        }

        private GeometryIntent InternalCreateGeometryIntent(Object geometry, Object intent)
        {
            return AssemblyComponentDefinitionInstance.CreateGeometryIntent( geometry,  intent);
        }

        private VisibleOccurrenceFinder InternalCreateVisibleOccurrenceFinder(bool visible, double percentageVisible, bool compact)
        {
            return AssemblyComponentDefinitionInstance.CreateVisibleOccurrenceFinder( visible,  percentageVisible,  compact);
        }

        private void InternalDeleteObjects(ObjectCollection objects)
        {
            AssemblyComponentDefinitionInstance.DeleteObjects( objects);
        }

        private void InternalExportObjects(ObjectCollection objects)
        {
            AssemblyComponentDefinitionInstance.ExportObjects( objects);
        }

        //Ambiguous
        //private ObjectsEnumerator InternalFindUsingPoint(Inventor.Point point, SelectionFilterEnum[] objectTypes, Object proximityTolerance, bool visibleObjectsOnly)
        //{
        //    return AssemblyComponentDefinitionInstance.FindUsingPoint( point,  objectTypes,  proximityTolerance,  visibleObjectsOnly);
        //}

        //private void InternalFindUsingRay(Inventor.Point rayStartPoint, UnitVector rayDirection, double radius, out ObjectsEnumerator foundEntities, out ObjectsEnumerator locationPoints, bool findFirstOnly)
        //{
        //    AssemblyComponentDefinitionInstance.FindUsingRay( rayStartPoint,  rayDirection,  radius, out  foundEntities, out  locationPoints,  findFirstOnly);
        //}

        //private ObjectsEnumerator InternalFindUsingVector(Inventor.Point originPoint, UnitVector direction, SelectionFilterEnum[] objectTypes, bool useCylinder, Object proximityTolerance, bool visibleObjectsOnly, out Object locationPoints)
        //{
        //    return AssemblyComponentDefinitionInstance.FindUsingVector( originPoint,  direction,  objectTypes,  useCylinder,  proximityTolerance,  visibleObjectsOnly, out  locationPoints);
        //}

        private void InternalGetEndOfFeaturesPosition(out Object after, out Object before)
        {
            AssemblyComponentDefinitionInstance.GetEndOfFeaturesPosition(out  after, out  before);
        }

        private void InternalGetPositionalStates(ref string[] positionalStates)
        {
            AssemblyComponentDefinitionInstance.GetPositionalStates(ref  positionalStates);
        }

        private void InternalHideAllRelationships()
        {
            AssemblyComponentDefinitionInstance.HideAllRelationships();
        }

        private RigidBodyResults InternalRigidBodyAnalysis(NameValueMap options, Object uniqueOccurrences, Object uniqueOccurrenceSettings)
        {
            return AssemblyComponentDefinitionInstance.RigidBodyAnalysis( options,  uniqueOccurrences,  uniqueOccurrenceSettings);
        }

        private void InternalSetEndOfFeaturesToTopOrBottom(bool setToTop)
        {
            AssemblyComponentDefinitionInstance.SetEndOfFeaturesToTopOrBottom( setToTop);
        }

        private void InternalSuppressFeatures(ObjectCollection features)
        {
            AssemblyComponentDefinitionInstance.SuppressFeatures( features);
        }

        private void InternalTransformOccurrences(ObjectCollection occurrences, ObjectCollection transforms, bool ignoreConstraints)
        {
            AssemblyComponentDefinitionInstance.TransformOccurrences( occurrences,  transforms,  ignoreConstraints);
        }

        private void InternalUnsuppressFeatures(ObjectCollection features)
        {
            AssemblyComponentDefinitionInstance.UnsuppressFeatures( features);
        }

        #endregion

        #region Public properties
        public Inventor.AssemblyComponentDefinition AssemblyComponentDefinitionInstance
        {
            get { return InternalAssemblyComponentDefinition; }
            set { InternalAssemblyComponentDefinition = value; }
        }

        //public InvComponentOccurrence ActiveOccurrence
        //{
        //    get { return InternalActiveOccurrence; }
        //}

        //public InvObjectsEnumerator AppearanceOverridesObjects
        //{
        //    get { return InternalAppearanceOverridesObjects; }
        //}

        public Object Application
        {
            get { return InternalApplication; }
        }

        //public InvAssemblyEvents AssemblyEvents
        //{
        //    get { return InternalAssemblyEvents; }
        //}

        public InvAttributeSets AttributeSets
        {
            get { return InternalAttributeSets; }
        }

        //public InvBIMComponent BIMComponent
        //{
        //    get { return InternalBIMComponent; }
        //}

        //public InvBOM BOM
        //{
        //    get { return InternalBOM; }
        //}

        //public InvBOMQuantity BOMQuantity
        //{
        //    get { return InternalBOMQuantity; }
        //}

        //public InvClientGraphicsCollection ClientGraphicsCollection
        //{
        //    get { return InternalClientGraphicsCollection; }
        //}

        //public InvAssemblyConstraints Constraints
        //{
        //    get { return InternalConstraints; }
        //}

        //public InvDataIO DataIO
        //{
        //    get { return InternalDataIO; }
        //}

        public Object Document
        {
            get { return InternalDocument; }
        }

        //public InvFeatures Features
        //{
        //    get { return InternalFeatures; }
        //}

        //public InviAssemblyFactory iAssemblyFactory
        //{
        //    get { return InternaliAssemblyFactory; }
        //}

        //public InviAssemblyMember iAssemblyMember
        //{
        //    get { return InternaliAssemblyMember; }
        //}

        //public InviMateDefinitions iMateDefinitions
        //{
        //    get { return InternaliMateDefinitions; }
        //}

        //public InviMateResults iMateResults
        //{
        //    get { return InternaliMateResults; }
        //}

        //public InvComponentDefinitionReferences ImmediateReferencedDefinitions
        //{
        //    get { return InternalImmediateReferencedDefinitions; }
        //}

        public bool IsiAssemblyFactory
        {
            get { return InternalIsiAssemblyFactory; }
        }

        public bool IsiAssemblyMember
        {
            get { return InternalIsiAssemblyMember; }
        }

        //public InvAssemblyJoints Joints
        //{
        //    get { return InternalJoints; }
        //}

        //public InvMassProperties MassProperties
        //{
        //    get { return InternalMassProperties; }
        //}

        public string Master
        {
            get { return InternalMaster; }
        }

        public bool MasterActive
        {
            get { return InternalMasterActive; }
        }

        public string ModelGeometryVersion
        {
            get { return InternalModelGeometryVersion; }
        }

        //public InvOccurrencePatterns OccurrencePatterns
        //{
        //    get { return InternalOccurrencePatterns; }
        //}

        //public InvComponentOccurrences Occurrences
        //{
        //    get { return InternalOccurrences; }
        //}

        //public InvParameters Parameters
        //{
        //    get { return InternalParameters; }
        //}

        //public Inv_AssemblyDocument Parent
        //{
        //    get { return InternalParent; }
        //}

        //public InvBox RangeBox
        //{
        //    get { return InternalRangeBox; }
        //}

        //public InvRepresentationsManager RepresentationsManager
        //{
        //    get { return InternalRepresentationsManager; }
        //}

        //public InvSimulationManager SimulationManager
        //{
        //    get { return InternalSimulationManager; }
        //}

        //public InvPlanarSketches Sketches
        //{
        //    get { return InternalSketches; }
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

        //public Invstring ActivePositionalState
        //{
        //    get { return InternalActivePositionalState; }
        //    set { InternalActivePositionalState = value; }
        //}

        //public InvBOMStructureEnum BOMStructure
        //{
        //    get { return InternalBOMStructure; }
        //    set { InternalBOMStructure = value; }
        //}

        //public InvMaterial DefaultMaterial
        //{
        //    get { return InternalDefaultMaterial; }
        //    set { InternalDefaultMaterial = value; }
        //}

        //public InvAsset DefaultVirtualComponentMaterial
        //{
        //    get { return InternalDefaultVirtualComponentMaterial; }
        //    set { InternalDefaultVirtualComponentMaterial = value; }
        //}

        #endregion

        #region Public static constructors
        public static InvAssemblyComponentDefinition ByInvAssemblyComponentDefinition(InvAssemblyComponentDefinition invAssemblyComponentDefinition)
        {
            return new InvAssemblyComponentDefinition(invAssemblyComponentDefinition);
        }
        public static InvAssemblyComponentDefinition ByInvAssemblyComponentDefinition(Inventor.AssemblyComponentDefinition invAssemblyComponentDefinition)
        {
            return new InvAssemblyComponentDefinition(invAssemblyComponentDefinition);
        }
        #endregion

        #region Public methods
        public Object AdjustProxyContext(Object objectProxy)
        {
            return InternalAdjustProxyContext( objectProxy);
        }

        public InterferenceResults AnalyzeInterference(ObjectCollection set1, Object set2)
        {
            return InternalAnalyzeInterference( set1,  set2);
        }

        public void ClearAppearanceOverrides(Object appearanceOverrideObjects)
        {
            InternalClearAppearanceOverrides( appearanceOverrideObjects);
        }

        public iAssemblyFactory CreateFactory()
        {
            return InternalCreateFactory();
        }

        public GeometryIntent CreateGeometryIntent(Object geometry, Object intent)
        {
            return InternalCreateGeometryIntent( geometry,  intent);
        }

        public VisibleOccurrenceFinder CreateVisibleOccurrenceFinder(bool visible, double percentageVisible, bool compact)
        {
            return InternalCreateVisibleOccurrenceFinder( visible,  percentageVisible,  compact);
        }

        public void DeleteObjects(ObjectCollection objects)
        {
            InternalDeleteObjects( objects);
        }

        public void ExportObjects(ObjectCollection objects)
        {
            InternalExportObjects( objects);
        }

        //Can't import Inventor.Point
        //public ObjectsEnumerator FindUsingPoint(Inventor.Point point, SelectionFilterEnum[] objectTypes, Object proximityTolerance, bool visibleObjectsOnly)
        //{
        //    return InternalFindUsingPoint( point,  objectTypes,  proximityTolerance,  visibleObjectsOnly);
        //}

        //public void FindUsingRay(Inventor.Point rayStartPoint, UnitVector rayDirection, double radius, out ObjectsEnumerator foundEntities, out ObjectsEnumerator locationPoints, bool findFirstOnly)
        //{
        //    InternalFindUsingRay( rayStartPoint,  rayDirection,  radius, out  foundEntities, out  locationPoints,  findFirstOnly);
        //}

        //public ObjectsEnumerator FindUsingVector(Inventor.Point originPoint, UnitVector direction, SelectionFilterEnum[] objectTypes, bool useCylinder, Object proximityTolerance, bool visibleObjectsOnly, out Object locationPoints)
        //{
        //    return InternalFindUsingVector( originPoint,  direction,  objectTypes,  useCylinder,  proximityTolerance,  visibleObjectsOnly, out  locationPoints);
        //}

        public void GetEndOfFeaturesPosition(out Object after, out Object before)
        {
            InternalGetEndOfFeaturesPosition(out  after, out  before);
        }

        public void GetPositionalStates(ref string[] positionalStates)
        {
            InternalGetPositionalStates(ref  positionalStates);
        }

        public void HideAllRelationships()
        {
            InternalHideAllRelationships();
        }

        public RigidBodyResults RigidBodyAnalysis(NameValueMap options, Object uniqueOccurrences, Object uniqueOccurrenceSettings)
        {
            return InternalRigidBodyAnalysis( options,  uniqueOccurrences,  uniqueOccurrenceSettings);
        }

        public void SetEndOfFeaturesToTopOrBottom(bool setToTop)
        {
            InternalSetEndOfFeaturesToTopOrBottom( setToTop);
        }

        public void SuppressFeatures(ObjectCollection features)
        {
            InternalSuppressFeatures( features);
        }

        public void TransformOccurrences(ObjectCollection occurrences, ObjectCollection transforms, bool ignoreConstraints)
        {
            InternalTransformOccurrences( occurrences,  transforms,  ignoreConstraints);
        }

        public void UnsuppressFeatures(ObjectCollection features)
        {
            InternalUnsuppressFeatures( features);
        }

        #endregion
    }
}
