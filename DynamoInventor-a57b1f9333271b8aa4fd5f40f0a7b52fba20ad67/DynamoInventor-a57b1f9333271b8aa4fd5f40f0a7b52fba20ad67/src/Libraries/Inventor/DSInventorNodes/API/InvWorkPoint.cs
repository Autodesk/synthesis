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
using SimpleInjector;

namespace InventorLibrary.API
{
    [IsVisibleInDynamoLibrary(false)]
    public class InvWorkPoint 
    {
        #region Private fields
        private IObjectBinder _binder;
        #endregion


        #region Internal properties
        internal Inventor.WorkPoint InternalWorkPoint { get; set; }

        internal Object InternalApplication
        {
            get { return WorkPointInstance.Application; }
        }

        internal InvAttributeSets InternalAttributeSets
        {
            get { return InvAttributeSets.ByInvAttributeSets(WorkPointInstance.AttributeSets); }
        }

        internal bool InternalConstruction
        {
            get { return WorkPointInstance.Construction; }
        }

        internal bool InternalConsumed
        {
            get { return WorkPointInstance.Consumed; }
        }
        //this is temporary, will only work in assemblies
        internal InvAssemblyWorkPointDef InternalDefinition
        {
            get { return InvAssemblyWorkPointDef.ByInvAssemblyWorkPointDef((Inventor.AssemblyWorkPointDef)WorkPointInstance.Definition); }
        }

        //internal InvWorkPointDefinitionEnum InternalDefinitionType
        //{
        //    get { return WorkPointInstance.DefinitionType.As<InvWorkPointDefinitionEnum>(); }
        //}

        //internal InvObjectCollection InternalDependents
        //{
        //    get { return InvObjectCollection.ByInvObjectCollection(WorkPointInstance.Dependents); }
        //}

        //internal InvObjectCollection InternalDrivenBy
        //{
        //    get { return InvObjectCollection.ByInvObjectCollection(WorkPointInstance.DrivenBy); }
        //}

        internal bool InternalHasReferenceComponent
        {
            get { return WorkPointInstance.HasReferenceComponent; }
        }

        //internal InvHealthStatusEnum InternalHealthStatus
        //{
        //    get { return WorkPointInstance.HealthStatus.As<InvHealthStatusEnum>(); }
        //}

        internal bool InternalIsCoordinateSystemElement
        {
            get { return WorkPointInstance.IsCoordinateSystemElement; }
        }

        internal bool InternalIsOwnedByFeature
        {
            get { return WorkPointInstance.IsOwnedByFeature; }
        }

        internal bool InternalIsParentSketch
        {
            get { return WorkPointInstance.IsParentSketch; }
        }

        internal bool InternalIsPatternElement
        {
            get { return WorkPointInstance.IsPatternElement; }
        }

        //internal InvPartFeature InternalOwnedBy
        //{
        //    get { return InvPartFeature.ByInvPartFeature(WorkPointInstance.OwnedBy); }
        //}

        internal InvComponentDefinition InternalParent
        {
            get { return InvComponentDefinition.ByInvComponentDefinition(WorkPointInstance.Parent); }
        }

        //internal InvSketch3D InternalParentSketch
        //{
        //    get { return InvSketch3D.ByInvSketch3D(WorkPointInstance.ParentSketch); }
        //}

        internal InvPoint InternalPoint
        {
            get { return InvPoint.ByInvPoint(WorkPointInstance.Point); }
        }

        //internal InvReferenceComponent InternalReferenceComponent
        //{
        //    get { return InvReferenceComponent.ByInvReferenceComponent(WorkPointInstance.ReferenceComponent); }
        //}

        internal InvWorkPoint InternalReferencedEntity
        {
            get { return InvWorkPoint.ByInvWorkPoint(WorkPointInstance.ReferencedEntity); }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return WorkPointInstance.Type.As<InvObjectTypeEnum>(); }
        }

        internal bool InternalAdaptive { get; set; }

        internal bool InternalConsumeInputs { get; set; }

        internal bool InternalExported { get; set; }

        internal bool InternalGrounded { get; set; }

        internal string InternalName { get; set; }

        internal bool InternalShared { get; set; }

        internal bool InternalVisible { get; set; }
        #endregion

        #region Private constructors
        private InvWorkPoint(InvWorkPoint invWorkPoint)
        {
            InternalWorkPoint = invWorkPoint.InternalWorkPoint;
        }

        private InvWorkPoint(Inventor.WorkPoint invWorkPoint)
        {
            InternalWorkPoint = invWorkPoint;
        }

        private InvWorkPoint(Point point, IObjectBinder binder)
        {
            _binder = binder;
            Inventor.WorkPoint wp;
            if (_binder.GetObjectFromTrace<Inventor.WorkPoint>(out wp))
            {
                InternalWorkPoint = wp;
                AssemblyWorkPointDef wpDef = (AssemblyWorkPointDef)wp.Definition;               
                wpDef.Point = point.ToPoint();
            }

            else
            {
                wp = PersistenceManager.ActiveAssemblyDoc.ComponentDefinition.WorkPoints.AddFixed(point.ToPoint(), false);
                InternalWorkPoint = wp;
                _binder.SetObjectForTrace<WorkPoint>(this.InternalWorkPoint);
            }
        }
        #endregion

        #region Private methods
        private void InternalSetWorkPoint(Inventor.WorkPoint wp)
        {
            InternalWorkPoint = wp;
            //this.InternalElementId = InternalReferencePoint.Id;
            byte[] refKey = new byte[] { };
            if (ReferenceManager.KeyManager == null)
            {
                ReferenceManager.KeyManager = PersistenceManager.ActiveAssemblyDoc.ReferenceKeyManager;
            }
            ReferenceManager.KeyContext = PersistenceManager.ActiveAssemblyDoc.ReferenceKeyManager.CreateKeyContext();

            wp.GetReferenceKey(ref refKey, (int)ReferenceManager.KeyContext);
            //this.InternalRefKey = refKey;
        }

        private void InternalDelete(bool retainDependents)
        {
            WorkPointInstance.Delete( retainDependents);
        }

        private void InternalGetReferenceKey(ref byte[] referenceKey, int keyContext)
        {
            WorkPointInstance.GetReferenceKey(ref  referenceKey,  keyContext);
        }

        private void InternalSetAtCentroid(Object entities)
        {
            WorkPointInstance.SetAtCentroid( entities);
        }

        private void InternalSetByCurveAndEntity(Object curve, Object entity, Object proximityPoint)
        {
            WorkPointInstance.SetByCurveAndEntity( curve,  entity,  proximityPoint);
        }

        //private void InternalSetByMidPoint(Edge edge)
        //{
        //    WorkPointInstance.SetByMidPoint( edge);
        //}

        private void InternalSetByPoint(Object point)
        {
            WorkPointInstance.SetByPoint( point);
        }

        //private void InternalSetBySphereCenterPoint(Face face)
        //{
        //    WorkPointInstance.SetBySphereCenterPoint( face);
        //}

        private void InternalSetByThreePlanes(Object plane1, Object plane2, Object plane3)
        {
            WorkPointInstance.SetByThreePlanes( plane1,  plane2,  plane3);
        }

        //private void InternalSetByTorusCenterPoint(Face face)
        //{
        //    WorkPointInstance.SetByTorusCenterPoint( face);
        //}

        private void InternalSetByTwoLines(Object line1, Object line2)
        {
            WorkPointInstance.SetByTwoLines( line1,  line2);
        }

        private void InternalSetEndOfPart(bool before)
        {
            WorkPointInstance.SetEndOfPart( before);
        }

        private void InternalSetFixed(Point point)
        {
            WorkPointInstance.SetFixed( point.ToPoint());
        }

        #endregion

        #region Public properties
        public Inventor.WorkPoint WorkPointInstance
        {
            get { return InternalWorkPoint; }
            set { InternalWorkPoint = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        public InvAttributeSets AttributeSets
        {
            get { return InternalAttributeSets; }
        }

        public bool Construction
        {
            get { return InternalConstruction; }
        }

        public bool Consumed
        {
            get { return InternalConsumed; }
        }

        public InvAssemblyWorkPointDef Definition
        {
            get { return InternalDefinition; }
        }

        //public InvWorkPointDefinitionEnum DefinitionType
        //{
        //    get { return InternalDefinitionType; }
        //}

        //public InvObjectCollection Dependents
        //{
        //    get { return InternalDependents; }
        //}

        //public InvObjectCollection DrivenBy
        //{
        //    get { return InternalDrivenBy; }
        //}

        public bool HasReferenceComponent
        {
            get { return InternalHasReferenceComponent; }
        }

        //public InvHealthStatusEnum HealthStatus
        //{
        //    get { return InternalHealthStatus; }
        //}

        public bool IsCoordinateSystemElement
        {
            get { return InternalIsCoordinateSystemElement; }
        }

        public bool IsOwnedByFeature
        {
            get { return InternalIsOwnedByFeature; }
        }

        public bool IsParentSketch
        {
            get { return InternalIsParentSketch; }
        }

        public bool IsPatternElement
        {
            get { return InternalIsPatternElement; }
        }

        //public InvPartFeature OwnedBy
        //{
        //    get { return InternalOwnedBy; }
        //}

        public InvComponentDefinition Parent
        {
            get { return InternalParent; }
        }

        //public InvSketch3D ParentSketch
        //{
        //    get { return InternalParentSketch; }
        //}

        public InvPoint Point
        {
            get { return InternalPoint; }
        }

        //public InvReferenceComponent ReferenceComponent
        //{
        //    get { return InternalReferenceComponent; }
        //}

        public InvWorkPoint ReferencedEntity
        {
            get { return InternalReferencedEntity; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public bool Adaptive
        {
            get { return InternalAdaptive; }
            set { InternalAdaptive = value; }
        }

        public bool ConsumeInputs
        {
            get { return InternalConsumeInputs; }
            set { InternalConsumeInputs = value; }
        }

        public bool Exported
        {
            get { return InternalExported; }
            set { InternalExported = value; }
        }

        public bool Grounded
        {
            get { return InternalGrounded; }
            set { InternalGrounded = value; }
        }

        public string Name
        {
            get { return InternalName; }
            set { InternalName = value; }
        }

        public bool Shared
        {
            get { return InternalShared; }
            set { InternalShared = value; }
        }

        public bool Visible
        {
            get { return InternalVisible; }
            set { InternalVisible = value; }
        }

        #endregion

        #region Public static constructors
        public static InvWorkPoint ByInvWorkPoint(InvWorkPoint invWorkPoint)
        {
            return new InvWorkPoint(invWorkPoint);
        }

        public static InvWorkPoint ByInvWorkPoint(Inventor.WorkPoint invWorkPoint)
        {
            return new InvWorkPoint(invWorkPoint);
        }

        public static InvWorkPoint ByPoint(Point point)
        {
            var binder = PersistenceManager.IoC.GetInstance<IObjectBinder>();
            return new InvWorkPoint(point, binder);
        }
        #endregion

        #region Public methods
        public void Delete(bool retainDependents)
        {
            InternalDelete( retainDependents);
        }

        public void GetReferenceKey(ref byte[] referenceKey, int keyContext)
        {
            InternalGetReferenceKey(ref  referenceKey,  keyContext);
        }

        public void SetAtCentroid(Object entities)
        {
            InternalSetAtCentroid( entities);
        }

        public void SetByCurveAndEntity(Object curve, Object entity, Object proximityPoint)
        {
            InternalSetByCurveAndEntity( curve,  entity,  proximityPoint);
        }

        //public void SetByMidPoint(Edge edge)
        //{
        //    InternalSetByMidPoint( edge);
        //}

        //what is going on here is that this, and other methods accepting "Object" point or plane, etc
        //is that these methods accept certain varieties of geometries.  This is how the api constrains 
        //what can be done.  I can't programattically place work points in a part file unless it is 
        //on some geometry like a sketch point or vertex.  Of course, the workaround is to place
        //a sketch line with one endpoint at the location you want, place the work point, then delete the 
        //sketch without deleting consuming work features.
        public void SetByPoint(Object point)
        {
            InternalSetByPoint( point);
        }

        //public void SetBySphereCenterPoint(Face face)
        //{
        //    InternalSetBySphereCenterPoint( face);
        //}

        public void SetByThreePlanes(Object plane1, Object plane2, Object plane3)
        {
            InternalSetByThreePlanes( plane1,  plane2,  plane3);
        }

        //public void SetByTorusCenterPoint(Face face)
        //{
        //    InternalSetByTorusCenterPoint( face);
        //}

        public void SetByTwoLines(Object line1, Object line2)
        {
            InternalSetByTwoLines( line1,  line2);
        }

        public void SetEndOfPart(bool before)
        {
            InternalSetEndOfPart( before);
        }

        public void SetFixed(Point point)
        {
            InternalSetFixed( point);
        }

        #endregion
    }
}
