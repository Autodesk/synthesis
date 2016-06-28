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
    public class InvComponentDefinition
    {
        #region Internal properties
        internal Inventor.ComponentDefinition InternalComponentDefinition { get; set; }

        internal Object InternalApplication
        {
            get { return ComponentDefinitionInstance.Application; }
        }

        internal InvAttributeSets InternalAttributeSets
        {
            get { return InvAttributeSets.ByInvAttributeSets(ComponentDefinitionInstance.AttributeSets); }
        }


        ////internal InvBOMQuantity InternalBOMQuantity
        ////{
        ////    get { return InvBOMQuantity.ByInvBOMQuantity(ComponentDefinitionInstance.BOMQuantity); }
        ////}


        ////internal InvClientGraphicsCollection InternalClientGraphicsCollection
        ////{
        ////    get { return InvClientGraphicsCollection.ByInvClientGraphicsCollection(ComponentDefinitionInstance.ClientGraphicsCollection); }
        ////}


        ////internal InvDataIO InternalDataIO
        ////{
        ////    get { return InvDataIO.ByInvDataIO(ComponentDefinitionInstance.DataIO); }
        ////}


        internal Object InternalDocument
        {
            get { return ComponentDefinitionInstance.Document; }
        }

        ////internal InvComponentDefinitionReferences InternalImmediateReferencedDefinitions
        ////{
        ////    get { return InvComponentDefinitionReferences.ByInvComponentDefinitionReferences(ComponentDefinitionInstance.ImmediateReferencedDefinitions); }
        ////}


        internal string InternalModelGeometryVersion
        {
            get { return ComponentDefinitionInstance.ModelGeometryVersion; }
        }

        ////internal InvComponentOccurrences InternalOccurrences
        ////{
        ////    get { return InvComponentOccurrences.ByInvComponentOccurrences(ComponentDefinitionInstance.Occurrences); }
        ////}


        ////internal InvBox InternalRangeBox
        ////{
        ////    get { return InvBox.ByInvBox(ComponentDefinitionInstance.RangeBox); }
        ////}


        ////internal InvSurfaceBodies InternalSurfaceBodies
        ////{
        ////    get { return InvSurfaceBodies.ByInvSurfaceBodies(ComponentDefinitionInstance.SurfaceBodies); }
        ////}


        internal InvObjectTypeEnum InternalType
        {
            get { return ComponentDefinitionInstance.Type.As<InvObjectTypeEnum>(); }
        }


        internal BOMStructureEnum InternalBOMStructure { get; set; }
        #endregion

        #region Private constructors
        private InvComponentDefinition(InvComponentDefinition invComponentDefinition)
        {
            InternalComponentDefinition = invComponentDefinition.InternalComponentDefinition;
        }

        private InvComponentDefinition(Inventor.ComponentDefinition invComponentDefinition)
        {
            InternalComponentDefinition = invComponentDefinition;
        }
        #endregion

        #region Private methods
        private ObjectsEnumerator InternalFindUsingPoint(Point point, SelectionFilterEnum[] objectTypes, Object proximityTolerance, bool visibleObjectsOnly)
        {
            return ComponentDefinitionInstance.FindUsingPoint( point.ToPoint(),  objectTypes,  proximityTolerance,  visibleObjectsOnly);
        }

        private ObjectsEnumerator InternalFindUsingVector(Point originPoint, UnitVector direction, SelectionFilterEnum[] objectTypes, bool useCylinder, Object proximityTolerance, bool visibleObjectsOnly, out Object locationPoints)
        {
            return ComponentDefinitionInstance.FindUsingVector( originPoint.ToPoint(),  direction,  objectTypes,  useCylinder,  proximityTolerance,  visibleObjectsOnly, out  locationPoints);
        }

        #endregion

        #region Public properties
        public Inventor.ComponentDefinition ComponentDefinitionInstance
        {
            get { return InternalComponentDefinition; }
            set { InternalComponentDefinition = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        public InvAttributeSets AttributeSets
        {
            get { return InternalAttributeSets; }
        }

        ////public InvBOMQuantity BOMQuantity
        ////{
        ////    get { return InternalBOMQuantity; }
        ////}

        ////public InvClientGraphicsCollection ClientGraphicsCollection
        ////{
        ////    get { return InternalClientGraphicsCollection; }
        ////}

        ////public InvDataIO DataIO
        ////{
        ////    get { return InternalDataIO; }
        ////}

        public Object Document
        {
            get { return InternalDocument; }
        }

        ////public InvComponentDefinitionReferences ImmediateReferencedDefinitions
        ////{
        ////    get { return InternalImmediateReferencedDefinitions; }
        ////}

        public string ModelGeometryVersion
        {
            get { return InternalModelGeometryVersion; }
        }

        ////public InvComponentOccurrences Occurrences
        ////{
        ////    get { return InternalOccurrences; }
        ////}

        ////public InvBox RangeBox
        ////{
        ////    get { return InternalRangeBox; }
        ////}

        ////public InvSurfaceBodies SurfaceBodies
        ////{
        ////    get { return InternalSurfaceBodies; }
        ////}

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        ////public InvBOMStructureEnum BOMStructure
        ////{
        ////    get { return InternalBOMStructure; }
        ////    set { InternalBOMStructure = value; }
        ////}

        #endregion

        #region Public static constructors
        public static InvComponentDefinition ByInvComponentDefinition(InvComponentDefinition invComponentDefinition)
        {
            return new InvComponentDefinition(invComponentDefinition);
        }
        public static InvComponentDefinition ByInvComponentDefinition(Inventor.ComponentDefinition invComponentDefinition)
        {
            return new InvComponentDefinition(invComponentDefinition);
        }
        #endregion

        #region Public methods
        public ObjectsEnumerator FindUsingPoint(Point point, SelectionFilterEnum[] objectTypes, Object proximityTolerance, bool visibleObjectsOnly)
        {
            return InternalFindUsingPoint( point,  objectTypes,  proximityTolerance,  visibleObjectsOnly);
        }

        public ObjectsEnumerator FindUsingVector(Point originPoint, UnitVector direction, SelectionFilterEnum[] objectTypes, bool useCylinder, Object proximityTolerance, bool visibleObjectsOnly, out Object locationPoints)
        {
            return InternalFindUsingVector( originPoint,  direction,  objectTypes,  useCylinder,  proximityTolerance,  visibleObjectsOnly, out  locationPoints);
        }

        #endregion
    }
}
