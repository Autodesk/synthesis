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
    public class InvWorkPlane
    {
        #region Internal properties
        internal Inventor.WorkPlane InternalWorkPlane { get; set; }

        internal Object InternalApplication
        {
            get { return WorkPlaneInstance.Application; }
        }

        internal InvAttributeSets InternalAttributeSets
        {
            get { return InvAttributeSets.ByInvAttributeSets(WorkPlaneInstance.AttributeSets); }
        }

        internal bool InternalConstruction
        {
            get { return WorkPlaneInstance.Construction; }
        }

        internal bool InternalConsumed
        {
            get { return WorkPlaneInstance.Consumed; }
        }

        internal Object InternalDefinition
        {
            get { return WorkPlaneInstance.Definition; }
        }

        //internal InvWorkPlaneDefinitionEnum InternalDefinitionType
        //{
        //    get { return WorkPlaneInstance.DefinitionType.As<InvWorkPlaneDefinitionEnum>(); }
        //}

        //internal InvObjectCollection InternalDependents
        //{
        //    get { return InvObjectCollection.ByInvObjectCollection(WorkPlaneInstance.Dependents); }
        //}

        //internal InvObjectCollection InternalDrivenBy
        //{
        //    get { return InvObjectCollection.ByInvObjectCollection(WorkPlaneInstance.DrivenBy); }
        //}

        internal bool InternalHasReferenceComponent
        {
            get { return WorkPlaneInstance.HasReferenceComponent; }
        }

        //internal InvHealthStatusEnum InternalHealthStatus
        //{
        //    get { return WorkPlaneInstance.HealthStatus.As<InvHealthStatusEnum>(); }
        //}

        internal bool InternalIsCoordinateSystemElement
        {
            get { return WorkPlaneInstance.IsCoordinateSystemElement; }
        }

        internal bool InternalIsOwnedByFeature
        {
            get { return WorkPlaneInstance.IsOwnedByFeature; }
        }

        internal bool InternalIsParentSketch
        {
            get { return WorkPlaneInstance.IsParentSketch; }
        }

        internal bool InternalIsPatternElement
        {
            get { return WorkPlaneInstance.IsPatternElement; }
        }

        //internal InvPartFeature InternalOwnedBy
        //{
        //    get { return InvPartFeature.ByInvPartFeature(WorkPlaneInstance.OwnedBy); }
        //}

        internal InvComponentDefinition InternalParent
        {
            get { return InvComponentDefinition.ByInvComponentDefinition(WorkPlaneInstance.Parent); }
        }

        //internal InvSketch3D InternalParentSketch
        //{
        //    get { return InvSketch3D.ByInvSketch3D(WorkPlaneInstance.ParentSketch); }
        //}

        //internal InvPlane InternalPlane
        //{
        //    get { return InvPlane.ByInvPlane(WorkPlaneInstance.Plane); }
        //}

        //internal InvReferenceComponent InternalReferenceComponent
        //{
        //    get { return InvReferenceComponent.ByInvReferenceComponent(WorkPlaneInstance.ReferenceComponent); }
        //}

        internal InvWorkPlane InternalReferencedEntity
        {
            get { return InvWorkPlane.ByInvWorkPlane(WorkPlaneInstance.ReferencedEntity); }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return WorkPlaneInstance.Type.As<InvObjectTypeEnum>(); }
        }

        internal bool InternalAdaptive { get; set; }

        internal bool InternalAutoResize { get; set; }

        internal bool InternalConsumeInputs { get; set; }

        internal bool InternalExported { get; set; }

        internal bool InternalGrounded { get; set; }

        internal string InternalName { get; set; }

        internal bool InternalShared { get; set; }

        internal bool InternalVisible { get; set; }
        #endregion

        #region Private constructors
        private InvWorkPlane(InvWorkPlane invWorkPlane)
        {
            InternalWorkPlane = invWorkPlane.InternalWorkPlane;
        }

        private InvWorkPlane(Inventor.WorkPlane invWorkPlane)
        {
            InternalWorkPlane = invWorkPlane;
        }
        #endregion

        #region Private methods
        private void InternalDelete(bool retainDependents)
        {
            WorkPlaneInstance.Delete( retainDependents);
        }

        private void InternalFlipNormal()
        {
            WorkPlaneInstance.FlipNormal();
        }

        //private void InternalGetPosition(out Point origin, out UnitVector xAxis, out UnitVector yAxis)
        //{
        //    WorkPlaneInstance.GetPosition(out  origin, out  xAxis, out  yAxis);
        //}

        private void InternalGetReferenceKey(ref byte[] referenceKey, int keyContext)
        {
            WorkPlaneInstance.GetReferenceKey(ref  referenceKey,  keyContext);
        }

        //private void InternalGetSize(out Point point1, out Point point2)
        //{
        //    WorkPlaneInstance.GetSize(out  point1, out  point2);
        //}

        //private void InternalSetByLineAndTangent(Object line, Face face, Point proximityPoint)
        //{
        //    WorkPlaneInstance.SetByLineAndTangent( line,  face,  proximityPoint);
        //}

        private void InternalSetByLinePlaneAndAngle(Object line, Object plane, Object angle)
        {
            WorkPlaneInstance.SetByLinePlaneAndAngle( line,  plane,  angle);
        }

        private void InternalSetByNormalToCurve(Object curveEntity, Object point)
        {
            WorkPlaneInstance.SetByNormalToCurve( curveEntity,  point);
        }

        private void InternalSetByPlaneAndOffset(Object plane, Object offset)
        {
            WorkPlaneInstance.SetByPlaneAndOffset( plane,  offset);
        }

        private void InternalSetByPlaneAndPoint(Object plane, Object point)
        {
            WorkPlaneInstance.SetByPlaneAndPoint( plane,  point);
        }

        //private void InternalSetByPlaneAndTangent(Object plane, Face face, Point proximityPoint)
        //{
        //    WorkPlaneInstance.SetByPlaneAndTangent( plane,  face,  proximityPoint);
        //}

        //private void InternalSetByPointAndTangent(Object point, Face face)
        //{
        //    WorkPlaneInstance.SetByPointAndTangent( point,  face);
        //}

        private void InternalSetByThreePoints(Object point1, Object point2, Object point3)
        {
            WorkPlaneInstance.SetByThreePoints( point1,  point2,  point3);
        }

        //private void InternalSetByTorusMidPlane(Face face)
        //{
        //    WorkPlaneInstance.SetByTorusMidPlane( face);
        //}

        private void InternalSetByTwoLines(Object line1, Object line2)
        {
            WorkPlaneInstance.SetByTwoLines( line1,  line2);
        }

        private void InternalSetByTwoPlanes(Object plane1, Object plane2)
        {
            WorkPlaneInstance.SetByTwoPlanes( plane1,  plane2);
        }

        private void InternalSetEndOfPart(bool before)
        {
            WorkPlaneInstance.SetEndOfPart( before);
        }

        //private void InternalSetFixed(Point originPoint, UnitVector xAxis, UnitVector yAxis)
        //{
        //    WorkPlaneInstance.SetFixed( originPoint,  xAxis,  yAxis);
        //}

        //private void InternalSetSize(Point point1, Point point2)
        //{
        //    WorkPlaneInstance.SetSize( point1,  point2);
        //}

        #endregion

        #region Public properties
        public Inventor.WorkPlane WorkPlaneInstance
        {
            get { return InternalWorkPlane; }
            set { InternalWorkPlane = value; }
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

        public Object Definition
        {
            get { return InternalDefinition; }
        }

        //public InvWorkPlaneDefinitionEnum DefinitionType
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

        //public InvPlane Plane
        //{
        //    get { return InternalPlane; }
        //}

        //public InvReferenceComponent ReferenceComponent
        //{
        //    get { return InternalReferenceComponent; }
        //}

        public InvWorkPlane ReferencedEntity
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

        public bool AutoResize
        {
            get { return InternalAutoResize; }
            set { InternalAutoResize = value; }
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
        public static InvWorkPlane ByInvWorkPlane(InvWorkPlane invWorkPlane)
        {
            return new InvWorkPlane(invWorkPlane);
        }

        public static InvWorkPlane ByInvWorkPlane(Inventor.WorkPlane invWorkPlane)
        {
            return new InvWorkPlane(invWorkPlane);
        }
        #endregion

        #region Public methods
        public void Delete(bool retainDependents)
        {
            InternalDelete( retainDependents);
        }

        public void FlipNormal()
        {
            InternalFlipNormal();
        }

        //public void GetPosition(out Point origin, out UnitVector xAxis, out UnitVector yAxis)
        //{
        //    InternalGetPosition(out  origin, out  xAxis, out  yAxis);
        //}

        public void GetReferenceKey(ref byte[] referenceKey, int keyContext)
        {
            InternalGetReferenceKey(ref  referenceKey,  keyContext);
        }

        //public void GetSize(out Point point1, out Point point2)
        //{
        //    InternalGetSize(out  point1, out  point2);
        //}

        //public void SetByLineAndTangent(Object line, Face face, Point proximityPoint)
        //{
        //    InternalSetByLineAndTangent( line,  face,  proximityPoint);
        //}

        public void SetByLinePlaneAndAngle(Object line, Object plane, Object angle)
        {
            InternalSetByLinePlaneAndAngle( line,  plane,  angle);
        }

        public void SetByNormalToCurve(Object curveEntity, Object point)
        {
            InternalSetByNormalToCurve( curveEntity,  point);
        }

        public void SetByPlaneAndOffset(Object plane, Object offset)
        {
            InternalSetByPlaneAndOffset( plane,  offset);
        }

        public void SetByPlaneAndPoint(Object plane, Object point)
        {
            InternalSetByPlaneAndPoint( plane,  point);
        }

        //public void SetByPlaneAndTangent(Object plane, Face face, Point proximityPoint)
        //{
        //    InternalSetByPlaneAndTangent( plane,  face,  proximityPoint);
        //}

        //public void SetByPointAndTangent(Object point, Face face)
        //{
        //    InternalSetByPointAndTangent( point,  face);
        //}

        public void SetByThreePoints(Object point1, Object point2, Object point3)
        {
            InternalSetByThreePoints( point1,  point2,  point3);
        }

        //public void SetByTorusMidPlane(Face face)
        //{
        //    InternalSetByTorusMidPlane( face);
        //}

        public void SetByTwoLines(Object line1, Object line2)
        {
            InternalSetByTwoLines( line1,  line2);
        }

        public void SetByTwoPlanes(Object plane1, Object plane2)
        {
            InternalSetByTwoPlanes( plane1,  plane2);
        }

        public void SetEndOfPart(bool before)
        {
            InternalSetEndOfPart( before);
        }

        //public void SetFixed(Point originPoint, UnitVector xAxis, UnitVector yAxis)
        //{
        //    InternalSetFixed( originPoint,  xAxis,  yAxis);
        //}

        //public void SetSize(Point point1, Point point2)
        //{
        //    InternalSetSize( point1,  point2);
        //}

        #endregion
    }
}
