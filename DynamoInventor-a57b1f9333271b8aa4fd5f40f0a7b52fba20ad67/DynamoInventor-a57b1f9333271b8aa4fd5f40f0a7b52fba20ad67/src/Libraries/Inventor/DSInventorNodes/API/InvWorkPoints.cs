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
    public class InvWorkPoints : IEnumerable<InvWorkPoint>
    {
        #region Internal properties
        List<InvWorkPoint> workPointList;

        internal Inventor.WorkPoints InternalWorkPoints { get; set; }

        internal Object InternalApplication
        {
            get { return WorkPointsInstance.Application; }
        }

        internal int InternalCount
        {
            get { return workPointList.Count; }
        }

        internal InvComponentDefinition InternalParent
        {
            get { return InvComponentDefinition.ByInvComponentDefinition(WorkPointsInstance.Parent); }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return WorkPointsInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        #region Private constructors
        private InvWorkPoints(InvWorkPoints invWorkPoints)
        {
            InternalWorkPoints = invWorkPoints.InternalWorkPoints;
            //workPointList = new List<InvWorkPoint>();
            //foreach (Inventor.WorkPoint workPoint in InternalWorkPoints)
            //{
            //    workPointList.Add(InvWorkPoint.ByInvWorkPoint(workPoint));
            //}
        }

        private InvWorkPoints(Inventor.WorkPoints invWorkPoints)
        {
            InternalWorkPoints = invWorkPoints;
            workPointList = new List<InvWorkPoint>();
            foreach (Inventor.WorkPoint workPoint in InternalWorkPoints)
            {
                workPointList.Add(InvWorkPoint.ByInvWorkPoint(workPoint));
            }
        }
        #endregion

        #region Private methods
        //private InvWorkPoint InternalAddAtCentroid(Object entities, bool construction)
        //{
        //    return WorkPointsInstance.AddAtCentroid( entities,  construction);
        //}

        //private InvWorkPoint InternalAddByCurveAndEntity(Object curve, Object entity, Object proximityPoint, bool construction)
        //{
        //    return WorkPointsInstance.AddByCurveAndEntity( curve,  entity,  proximityPoint,  construction);
        //}

        //private InvWorkPoint InternalAddByMidPoint(Edge edge, bool construction)
        //{
        //    return WorkPointsInstance.AddByMidPoint( edge,  construction);
        //}

        //private InvWorkPoint InternalAddByPoint(Object point, bool construction)
        //{
        //    return WorkPointsInstance.AddByPoint( point,  construction);
        //}

        //private WorkPoint InternalAddBySphereCenterPoint(Face face, bool construction)
        //{
        //    return WorkPointsInstance.AddBySphereCenterPoint( face,  construction);
        //}

        //private InvWorkPoint InternalAddByThreePlanes(Object plane1, Object plane2, Object plane3, bool construction)
        //{
        //    return WorkPointsInstance.AddByThreePlanes( plane1,  plane2,  plane3,  construction);
        //}

        //private InvWorkPoint InternalAddByTorusCenterPoint(Face face, bool construction)
        //{
        //    return WorkPointsInstance.AddByTorusCenterPoint( face,  construction);
        //}

        //private InvWorkPoint InternalAddByTwoLines(Object line1, Object line2, bool construction)
        //{
        //    return WorkPointsInstance.AddByTwoLines( line1,  line2,  construction);
        //}

        ////private InvWorkPoint InternalAddFixed(Point point, bool construction)
        ////{
        ////    Inventor.WorkPoint wp;
        ////    if (ReferenceKeyBinder.GetObjectFromTrace<Inventor.WorkPoint>(out wp))
        ////    {
        ////        Inventor.Point newLocation = PersistenceManager.InventorApplication.TransientGeometry.CreatePoint(point.X, point.Y, point.Z);
        ////        AssemblyWorkPointDef wpDef = (AssemblyWorkPointDef)wp.Definition;
        ////        wpDef.Point = newLocation;
        ////        return InvWorkPoint.ByInvWorkPoint(wp);
        ////    }

        ////    else
        ////    {
        ////        wp = WorkPointsInstance.AddFixed(point.ToPoint(), construction);
        ////        ReferenceKeyBinder.SetObjectForTrace(wp);
        ////        return InvWorkPoint.ByInvWorkPoint(wp);
        ////    }
        ////}

        //private IEnumerator InternalGetEnumerator()
        //{
        //    return WorkPointsInstance.GetEnumerator();
        //}

        #endregion

        #region Public properties
        public Inventor.WorkPoints WorkPointsInstance
        {
            get { return InternalWorkPoints; }
            set { InternalWorkPoints = value; }
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
        public static InvWorkPoints ByInvWorkPoints(InvWorkPoints invWorkPoints)
        {
            return new InvWorkPoints(invWorkPoints);
        }

        public static InvWorkPoints ByInvWorkPoints(Inventor.WorkPoints invWorkPoints)
        {
            return new InvWorkPoints(invWorkPoints);
        }
        #endregion

        #region Public methods
        //public InvWorkPoint AddAtCentroid(Object entities, bool construction)
        //{
        //    return InternalAddAtCentroid( entities,  construction);
        //}

        //public InvWorkPoint AddByCurveAndEntity(Object curve, Object entity, Object proximityPoint, bool construction)
        //{
        //    return InternalAddByCurveAndEntity( curve,  entity,  proximityPoint,  construction);
        //}

        //public InvWorkPoint AddByMidPoint(Edge edge, bool construction)
        //{
        //    return InternalAddByMidPoint( edge,  construction);
        //}

        //public InvWorkPoint AddByPoint(Object point, bool construction)
        //{
        //    return InternalAddByPoint( point,  construction);
        //}

        //public InvWorkPoint AddBySphereCenterPoint(Face face, bool construction)
        //{
        //    return InternalAddBySphereCenterPoint( face,  construction);
        //}

        //public InvWorkPoint AddByThreePlanes(Object plane1, Object plane2, Object plane3, bool construction)
        //{
        //    return InternalAddByThreePlanes( plane1,  plane2,  plane3,  construction);
        //}

        //public InvWorkPoint AddByTorusCenterPoint(Face face, bool construction)
        //{
        //    return InternalAddByTorusCenterPoint( face,  construction);
        //}

        //public InvWorkPoint AddByTwoLines(Object line1, Object line2, bool construction)
        //{
        //    return InternalAddByTwoLines( line1,  line2,  construction);
        //}

        ////public InvWorkPoint AddFixed(Point point, bool construction)
        ////{
        ////    return InternalAddFixed(point, construction);
        ////}

        //public IEnumerator GetEnumerator()
        //{
        //    return InternalGetEnumerator();
        //}

        #endregion

        public void Add(InvWorkPoint invWorkPoint)
        {
            workPointList.Add(invWorkPoint);
        }

        public IEnumerator<InvWorkPoint> GetEnumerator()
        {
            return workPointList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
