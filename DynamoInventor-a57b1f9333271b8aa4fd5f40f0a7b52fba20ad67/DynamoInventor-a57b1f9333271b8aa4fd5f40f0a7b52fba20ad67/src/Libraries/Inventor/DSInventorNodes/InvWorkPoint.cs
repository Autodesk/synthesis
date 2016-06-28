using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Inventor;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using DSNodeServices;
using Dynamo.Models;
using Dynamo.Utilities;
using DSInventorNodes.GeometryConversion;
using InventorServices.Persistence;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSInventorNodes.GeometryObjects
{
    [RegisterForTrace]
    [ShortName("workPt")]
    public class InvWorkPoint : AbstractGeometryObject
    {

        #region Internal properties
        internal WorkPoint InternalWorkPoint { get; private set; }

        //public override ComponentOccurrence InternalOccurrence
        //{
        //    get { return InternalWorkPoint.; }
        //}
        
        #endregion

        #region Private constructors
        private InvWorkPoint(Inventor.WorkPoint workPt)
        {
            InternalWorkPoint = workPt;
        }

        private InvWorkPoint(double x, double y, double z)
        {
            //this.VerifyContextSettings();
            AssemblyDocument assDoc = InventorPersistenceManager.ActiveAssemblyDoc;
            AssemblyComponentDefinition compDef = (AssemblyComponentDefinition)assDoc.ComponentDefinition;
            Inventor.Point point = InventorPersistenceManager.InventorApplication.TransientGeometry.CreatePoint(x, y, z);
            Inventor.WorkPoint wp = compDef.WorkPoints.AddFixed(point, false);

            byte[] refKey = new byte[] { };
            //wp.GetReferenceKey(ref refKey, (int)InventorSettings.KeyContext);
            //wp.GetReferenceKey(ref refKey, (int)ReferenceManager.KeyContext);
            //ComponentOccurrenceKeys.Add(refKey);
            //return wp;
            InternalSetWorkPoint(wp);    
        }
        #endregion

        #region Private mutators
        private void InternalSetPosition(double x, double y, double z)
        {
            Inventor.Point newLocation = InventorPersistenceManager.InventorApplication.TransientGeometry.CreatePoint(x, y, z);
            AssemblyWorkPointDef wpDef = (AssemblyWorkPointDef)InternalWorkPoint.Definition;
            wpDef.Point = newLocation;
        }

        private void InternalSetWorkPoint(Inventor.WorkPoint p)
        {
            InternalWorkPoint = p;
        }
        #endregion

        #region Public properties
        public double X
        {
            get { return InternalWorkPoint.Point.X; }
            set { InternalSetPosition(value, Y, Z); }
        }

        public double Y
        {
            get { return InternalWorkPoint.Point.Y; }
            set { InternalSetPosition(X, value, Z); }
        }

        public double Z
        {
            get { return InternalWorkPoint.Point.Z; }
            set { InternalSetPosition(X, Y, value); }
        }

        public Point Point
        {
            get
            {
                return InternalWorkPoint.Point.ToPoint();
            }
        }
        #endregion

        #region Public static constructors

        public static InvWorkPoint ByCoordinates(double x, double y, double z)
        {
            return new InvWorkPoint(x, y, z);
        }

        public static InvWorkPoint ByPoint(Point pt)
        {
            if (pt == null)
            {
                throw new ArgumentNullException("pt");
            }
            return new InvWorkPoint(pt.X, pt.Y, pt.Z);
        }

        #endregion

        #region Internal static constructors
        internal static InvWorkPoint FromExisting(Inventor.WorkPoint pt)
        {
            return new InvWorkPoint(pt);
        }
        #endregion

        #region Tesselation

        #endregion

        //private static void MoveWorkPoint(double x, double y, double z, Inventor.WorkPoint wp)
        //{
        //    Inventor.Point newLocation = DocumentManager.InventorApplication.TransientGeometry.CreatePoint(x, y, z);
        //    AssemblyWorkPointDef wpDef = (AssemblyWorkPointDef)wp.Definition;
        //    wpDef.Point = newLocation;
        //}      
    }
}
