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
    public class InvWorkPlanes
    {
        #region Internal properties
        internal Inventor.WorkPlanes InternalWorkPlanes { get; set; }

        internal Object InternalApplication
        {
            get { return WorkPlanesInstance.Application; }
        }

        internal int InternalCount
        {
            get { return WorkPlanesInstance.Count; }
        }

        internal InvComponentDefinition InternalParent
        {
            get { return InvComponentDefinition.ByInvComponentDefinition(WorkPlanesInstance.Parent); }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return WorkPlanesInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        #region Private constructors
        private InvWorkPlanes(InvWorkPlanes invWorkPlanes)
        {
            InternalWorkPlanes = invWorkPlanes.InternalWorkPlanes;
        }

        private InvWorkPlanes(Inventor.WorkPlanes invWorkPlanes)
        {
            InternalWorkPlanes = invWorkPlanes;
        }
        #endregion

        #region Private methods
        //private InvWorkPlane InternalAddByLineAndPoint(Object line, Object point, bool construction)
        //{
        //    return WorkPlanesInstance.AddByLineAndPoint( line,  point,  construction);
        //}

        //private InvWorkPlane InternalAddByLineAndTangent(Object line, Face face, Point proximityPoint, bool construction)
        //{
        //    return WorkPlanesInstance.AddByLineAndTangent( line,  face,  proximityPoint,  construction);
        //}

        //private InvWorkPlane InternalAddByLinePlaneAndAngle(Object line, Object plane, Object angle, bool construction)
        //{
        //    return WorkPlanesInstance.AddByLinePlaneAndAngle( line,  plane,  angle,  construction);
        //}

        //private InvWorkPlane InternalAddByNormalToCurve(Object curveEntity, Object point, bool construction)
        //{
        //    return WorkPlanesInstance.AddByNormalToCurve( curveEntity,  point,  construction);
        //}

        //private InvWorkPlane InternalAddByPlaneAndOffset(Object plane, Object offset, bool construction)
        //{
        //    return WorkPlanesInstance.AddByPlaneAndOffset( plane,  offset,  construction);
        //}

        //private InvWorkPlane InternalAddByPlaneAndPoint(Object plane, Object point, bool construction)
        //{
        //    return WorkPlanesInstance.AddByPlaneAndPoint( plane,  point,  construction);
        //}

        //private InvWorkPlane InternalAddByPlaneAndTangent(Object plane, Face face, Point proximityPoint, bool construction)
        //{
        //    return WorkPlanesInstance.AddByPlaneAndTangent( plane,  face,  proximityPoint,  construction);
        //}

        //private InvWorkPlane InternalAddByPointAndTangent(Object point, Face face, bool construction)
        //{
        //    return WorkPlanesInstance.AddByPointAndTangent( point,  face,  construction);
        //}

        //private InvWorkPlane InternalAddByThreePoints(InvWorkPoint point1, InvWorkPoint point2, InvWorkPoint point3, bool construction)
        //{
        //    //WorkPlane workPlane;
        //    //if (ReferenceKeyBinder.GetObjectFromTrace<Inventor.WorkPlane>(out workPlane))
        //    //{
        //    //    if (workPlane.DefinitionType == WorkPlaneDefinitionEnum.kThreePointsWorkPlane)
        //    //    {
        //    //        workPlane.SetByThreePoints(point1.WorkPointInstance, point2.WorkPointInstance, point3.WorkPointInstance);
        //    //    }
        //    //    return InvWorkPlane.ByInvWorkPlane(workPlane);
        //    //}

        //    //else
        //    //{
        //    //    WorkPoint wp1 = point1.WorkPointInstance;
        //    //    WorkPoint wp2 = point2.WorkPointInstance;
        //    //    WorkPoint wp3 = point3.WorkPointInstance;
        //    //    workPlane = WorkPlanesInstance.AddByThreePoints(wp1, wp2, wp3);
        //    //    ReferenceKeyBinder.SetObjectForTrace(workPlane);
        //    //    return InvWorkPlane.ByInvWorkPlane(workPlane);
        //    }
        //}

        //private InvWorkPlane InternalAddByTorusMidPlane(Face face, bool construction)
        //{
        //    return WorkPlanesInstance.AddByTorusMidPlane( face,  construction);
        //}

        //private InvWorkPlane InternalAddByTwoLines(Object line1, Object line2, bool construction)
        //{
        //    return WorkPlanesInstance.AddByTwoLines( line1,  line2,  construction);
        //}

        //private InvWorkPlane InternalAddByTwoPlanes(Object plane1, Object plane2, bool construction)
        //{
        //    return WorkPlanesInstance.AddByTwoPlanes( plane1,  plane2,  construction);
        //}

        //private InvWorkPlane InternalAddFixed(Point originPoint, UnitVector xAxis, UnitVector yAxis, bool construction)
        //{
        //    return WorkPlanesInstance.AddFixed( originPoint.ToPoint(),  xAxis,  yAxis,  construction);
        //}

        //private IEnumerator<InvWorkPlane> InternalGetEnumerator()
        //{
        //    return WorkPlanesInstance.GetEnumerator();
        //}

        #endregion

        #region Public properties
        public Inventor.WorkPlanes WorkPlanesInstance
        {
            get { return InternalWorkPlanes; }
            set { InternalWorkPlanes = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        public int Count
        {
            get { return InternalCount; }
        }

        public InvComponentDefinition Parent
        {
            get { return InternalParent; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        #endregion

        #region Public static constructors
        public static InvWorkPlanes ByInvWorkPlanes(InvWorkPlanes invWorkPlanes)
        {
            return new InvWorkPlanes(invWorkPlanes);
        }

        public static InvWorkPlanes ByInvWorkPlanes(Inventor.WorkPlanes invWorkPlanes)
        {
            return new InvWorkPlanes(invWorkPlanes);
        }
        #endregion

        #region Public methods
        //public InvWorkPlane AddByLineAndPoint(Object line, Object point, bool construction)
        //{
        //    return InternalAddByLineAndPoint( line,  point,  construction);
        //}

        //public InvWorkPlane AddByLineAndTangent(Object line, Face face, Point proximityPoint, bool construction)
        //{
        //    return InternalAddByLineAndTangent( line,  face,  proximityPoint,  construction);
        //}

        //public InvWorkPlane AddByLinePlaneAndAngle(Object line, Object plane, Object angle, bool construction)
        //{
        //    return InternalAddByLinePlaneAndAngle( line,  plane,  angle,  construction);
        //}

        //public InvWorkPlane AddByNormalToCurve(Object curveEntity, Object point, bool construction)
        //{
        //    return InternalAddByNormalToCurve( curveEntity,  point,  construction);
        //}

        //public InvWorkPlane AddByPlaneAndOffset(Object plane, Object offset, bool construction)
        //{
        //    return InternalAddByPlaneAndOffset( plane,  offset,  construction);
        //}

        //public InvWorkPlane AddByPlaneAndPoint(Object plane, Object point, bool construction)
        //{
        //    return InternalAddByPlaneAndPoint( plane,  point,  construction);
        //}

        //public InvWorkPlane AddByPlaneAndTangent(Object plane, Face face, Point proximityPoint, bool construction)
        //{
        //    return InternalAddByPlaneAndTangent( plane,  face,  proximityPoint,  construction);
        //}

        //public InvWorkPlane AddByPointAndTangent(Object point, Face face, bool construction)
        //{
        //    return InternalAddByPointAndTangent( point,  face,  construction);
        //}

        ////public InvWorkPlane AddByThreePoints(InvWorkPoint point1, InvWorkPoint point2, InvWorkPoint point3, bool construction)
        ////{
        ////    return InternalAddByThreePoints(point1, point2, point3, construction);
        ////}

        //public InvWorkPlane AddByTorusMidPlane(Face face, bool construction)
        //{
        //    return InternalAddByTorusMidPlane( face,  construction);
        //}

        //public InvWorkPlane AddByTwoLines(Object line1, Object line2, bool construction)
        //{
        //    return InternalAddByTwoLines( line1,  line2,  construction);
        //}

        //public InvWorkPlane AddByTwoPlanes(Object plane1, Object plane2, bool construction)
        //{
        //    return InternalAddByTwoPlanes( plane1,  plane2,  construction);
        //}

        //public InvWorkPlane AddFixed(Point originPoint, UnitVector xAxis, UnitVector yAxis, bool construction)
        //{
        //    return InternalAddFixed( originPoint,  xAxis,  yAxis,  construction);
        //}

        //public IEnumerator GetEnumerator()
        //{
        //    return InternalGetEnumerator();
        //}

        #endregion
    }
}
