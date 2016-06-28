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
    public class InvPoint
    {
        #region Internal properties
        internal Inventor.Point InternalPoint { get; set; }

        internal double InternalX 
        {
            get { return InternalPoint.X; }
            set { InternalPoint.X = value; }
        }

        internal double InternalY
        {
            get { return InternalPoint.Y; }
            set { InternalPoint.Y = value; }
        }

        internal double InternalZ
        {
            get { return InternalPoint.Z; }
            set { InternalPoint.Z = value; }
        }
        #endregion

        #region Private constructors
        private InvPoint(InvPoint invPoint)
        {
            //this situation should require no binding.
            //Inventor.Point is created through TransientGeometry.CreatePoint,
            //I don't think InvPoint needs to be kept track of, its seems like the only
            //way I move things is by creating a new point, and setting the workpoint or 
            //whatever Point property to the new Inventor.Point.
            //I think this whole class is probably pointless, and the user can 
            //just use ProtoGeometry any time they need to supply a point to an 
            //Inventor method
            InternalPoint = invPoint.InternalPoint;
        }

        private InvPoint(Inventor.Point invPoint)
        {
            InternalPoint = invPoint;
        }

        private InvPoint(Point invPoint)
        {
            InternalPoint = invPoint.ToPoint();
        }
        #endregion

        #region Private methods
        private Point InternalCopy()
        {
            return PointInstance.Copy().ToPoint();
        }

        private double InternalDistanceTo(Point point)
        {
            return PointInstance.DistanceTo(point.ToPoint());
        }

        private void InternalGetPointData(ref double[] coords)
        {
            PointInstance.GetPointData(ref  coords);
        }

        private bool InternalIsEqualTo(Point point, double tolerance)
        {
            return PointInstance.IsEqualTo(point.ToPoint(), tolerance);
        }

        private void InternalPutPointData(ref double[] coords)
        {
            PointInstance.PutPointData(ref  coords);
        }

        private void InternalTransformBy(Matrix matrix)
        {
            PointInstance.TransformBy(matrix);
        }

        //private void InternalTranslateBy(Vector vector)
        //{
        //    PointInstance.TranslateBy( vector);
        //}

        //private Vector InternalVectorTo(Point point)
        //{
        //    return PointInstance.VectorTo(point.ToPoint());
        //}

        #endregion

        #region Public properties
        public Inventor.Point PointInstance
        {
            get { return InternalPoint; }
        }

        public double X
        {
            get { return InternalX; }
            set { InternalX = value; }
        }

        public double Y
        {
            get { return InternalY; }
            set { InternalY = value; }
        }

        public double Z
        {
            get { return InternalZ; }
            set { InternalZ = value; }
        }

        #endregion

        #region Public static constructors
        public static InvPoint ByInvPoint(InvPoint invPoint)
        {
            return new InvPoint(invPoint);
        }

        public static InvPoint ByInvPoint(Inventor.Point invPoint)
        {
            return new InvPoint(invPoint);
        }

        public static InvPoint ByInvPoint(Point invPoint)
        {
            return new InvPoint(invPoint);
        }
        #endregion

        #region Public methods
        public Point Copy()
        {
            return InternalCopy();
        }

        public double DistanceTo(Point point)
        {
            return InternalDistanceTo( point);
        }

        public void GetPointData(ref double[] coords)
        {
            InternalGetPointData(ref  coords);
        }

        public bool IsEqualTo(Point point, double tolerance)
        {
            return InternalIsEqualTo( point,  tolerance);
        }

        public void PutPointData(ref double[] coords)
        {
            InternalPutPointData(ref  coords);
        }

        public void TransformBy(Matrix matrix)
        {
            InternalTransformBy( matrix);
        }

        //public void TranslateBy(Vector vector)
        //{
        //    InternalTranslateBy( vector);
        //}

        //public Vector VectorTo(Point point)
        //{
        //    return InternalVectorTo( point);
        //}

        #endregion
    }
}
