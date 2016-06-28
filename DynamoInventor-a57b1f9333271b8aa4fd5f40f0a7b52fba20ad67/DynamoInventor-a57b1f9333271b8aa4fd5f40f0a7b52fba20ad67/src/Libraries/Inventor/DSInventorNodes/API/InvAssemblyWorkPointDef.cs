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
    public class InvAssemblyWorkPointDef
    {
        #region Internal properties
        internal Inventor.AssemblyWorkPointDef InternalAssemblyWorkPointDef { get; set; }

        internal Object InternalApplication
        {
            get { return AssemblyWorkPointDefInstance.Application; }
        }

        //internal InvAssemblyConstraintsEnumerator InternalConstraints
        //{
        //    get { return InvAssemblyConstraintsEnumerator.ByInvAssemblyConstraintsEnumerator(AssemblyWorkPointDefInstance.Constraints); }
        //}

        internal InvWorkPoint InternalParent
        {
            get { return InvWorkPoint.ByInvWorkPoint(AssemblyWorkPointDefInstance.Parent); }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return AssemblyWorkPointDefInstance.Type.As<InvObjectTypeEnum>(); }
        }

        internal Point InternalPoint 
        {
            get { return InternalAssemblyWorkPointDef.Point.ToPoint(); }
            set { InternalAssemblyWorkPointDef.Point = value.ToPoint(); }
        }
        #endregion

        #region Private constructors
        private InvAssemblyWorkPointDef(InvAssemblyWorkPointDef invAssemblyWorkPointDef)
        {
            InternalAssemblyWorkPointDef = invAssemblyWorkPointDef.InternalAssemblyWorkPointDef;
        }

        private InvAssemblyWorkPointDef(Inventor.AssemblyWorkPointDef invAssemblyWorkPointDef)
        {
            InternalAssemblyWorkPointDef = invAssemblyWorkPointDef;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.AssemblyWorkPointDef AssemblyWorkPointDefInstance
        {
            get { return InternalAssemblyWorkPointDef; }
            set { InternalAssemblyWorkPointDef = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        ////public InvAssemblyConstraintsEnumerator Constraints
        ////{
        ////    get { return InternalConstraints; }
        ////}

        public InvWorkPoint Parent
        {
            get { return InternalParent; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public Point Point
        {
            get { return InternalPoint; }
            set { InternalPoint = value; }
        }

        #endregion

        #region Public static constructors
        public static InvAssemblyWorkPointDef ByInvAssemblyWorkPointDef(InvAssemblyWorkPointDef invAssemblyWorkPointDef)
        {
            return new InvAssemblyWorkPointDef(invAssemblyWorkPointDef);
        }

        public static InvAssemblyWorkPointDef ByInvAssemblyWorkPointDef(Inventor.AssemblyWorkPointDef invAssemblyWorkPointDef)
        {
            return new InvAssemblyWorkPointDef(invAssemblyWorkPointDef);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
