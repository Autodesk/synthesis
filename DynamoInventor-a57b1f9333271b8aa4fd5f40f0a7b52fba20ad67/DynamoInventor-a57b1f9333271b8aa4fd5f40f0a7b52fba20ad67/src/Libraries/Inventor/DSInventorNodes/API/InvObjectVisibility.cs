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
    public class InvObjectVisibility
    {
        #region Internal properties
        internal Inventor.ObjectVisibility InternalObjectVisibility { get; set; }

        internal Object InternalApplication
        {
            get { return ObjectVisibilityInstance.Application; }
        }

        //internal Inv_Document InternalParent
        //{
        //    get { return Inv_Document.ByInv_Document(ObjectVisibilityInstance.Parent); }
        //}

        internal InvObjectTypeEnum InternalType
        {
            get { return ObjectVisibilityInstance.Type.As<InvObjectTypeEnum>(); }
        }

        internal bool InternalAllWorkFeatures { get; set; }

        internal bool InternalConstructionSurfaces { get; set; }

        internal bool InternalGroupDiagnostics { get; set; }

        internal bool InternalGroupSolids { get; set; }

        internal bool InternalGroupSurfaces { get; set; }

        internal bool InternalGroupWires { get; set; }

        internal bool InternalOriginWorkAxes { get; set; }

        internal bool InternalOriginWorkPlanes { get; set; }

        internal bool InternalOriginWorkPoints { get; set; }

        internal bool InternalSketches { get; set; }

        internal bool InternalSketches3D { get; set; }

        internal bool InternalUCSTriads { get; set; }

        internal bool InternalUCSWorkAxes { get; set; }

        internal bool InternalUCSWorkPlanes { get; set; }

        internal bool InternalUCSWorkPoints { get; set; }

        internal bool InternalUserWorkAxes { get; set; }

        internal bool InternalUserWorkPlanes { get; set; }

        internal bool InternalUserWorkPoints { get; set; }

        internal bool InternalWeldmentSymbols { get; set; }

        internal bool InternalWelds { get; set; }
        #endregion

        #region Private constructors
        private InvObjectVisibility(InvObjectVisibility invObjectVisibility)
        {
            InternalObjectVisibility = invObjectVisibility.InternalObjectVisibility;
        }

        private InvObjectVisibility(Inventor.ObjectVisibility invObjectVisibility)
        {
            InternalObjectVisibility = invObjectVisibility;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.ObjectVisibility ObjectVisibilityInstance
        {
            get { return InternalObjectVisibility; }
            set { InternalObjectVisibility = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        //public Inv_Document Parent
        //{
        //    get { return InternalParent; }
        //}

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public bool AllWorkFeatures
        {
            get { return InternalAllWorkFeatures; }
            set { InternalAllWorkFeatures = value; }
        }

        public bool ConstructionSurfaces
        {
            get { return InternalConstructionSurfaces; }
            set { InternalConstructionSurfaces = value; }
        }

        public bool GroupDiagnostics
        {
            get { return InternalGroupDiagnostics; }
            set { InternalGroupDiagnostics = value; }
        }

        public bool GroupSolids
        {
            get { return InternalGroupSolids; }
            set { InternalGroupSolids = value; }
        }

        public bool GroupSurfaces
        {
            get { return InternalGroupSurfaces; }
            set { InternalGroupSurfaces = value; }
        }

        public bool GroupWires
        {
            get { return InternalGroupWires; }
            set { InternalGroupWires = value; }
        }

        public bool OriginWorkAxes
        {
            get { return InternalOriginWorkAxes; }
            set { InternalOriginWorkAxes = value; }
        }

        public bool OriginWorkPlanes
        {
            get { return InternalOriginWorkPlanes; }
            set { InternalOriginWorkPlanes = value; }
        }

        public bool OriginWorkPoints
        {
            get { return InternalOriginWorkPoints; }
            set { InternalOriginWorkPoints = value; }
        }

        public bool Sketches
        {
            get { return InternalSketches; }
            set { InternalSketches = value; }
        }

        public bool Sketches3D
        {
            get { return InternalSketches3D; }
            set { InternalSketches3D = value; }
        }

        public bool UCSTriads
        {
            get { return InternalUCSTriads; }
            set { InternalUCSTriads = value; }
        }

        public bool UCSWorkAxes
        {
            get { return InternalUCSWorkAxes; }
            set { InternalUCSWorkAxes = value; }
        }

        public bool UCSWorkPlanes
        {
            get { return InternalUCSWorkPlanes; }
            set { InternalUCSWorkPlanes = value; }
        }

        public bool UCSWorkPoints
        {
            get { return InternalUCSWorkPoints; }
            set { InternalUCSWorkPoints = value; }
        }

        public bool UserWorkAxes
        {
            get { return InternalUserWorkAxes; }
            set { InternalUserWorkAxes = value; }
        }

        public bool UserWorkPlanes
        {
            get { return InternalUserWorkPlanes; }
            set { InternalUserWorkPlanes = value; }
        }

        public bool UserWorkPoints
        {
            get { return InternalUserWorkPoints; }
            set { InternalUserWorkPoints = value; }
        }

        public bool WeldmentSymbols
        {
            get { return InternalWeldmentSymbols; }
            set { InternalWeldmentSymbols = value; }
        }

        public bool Welds
        {
            get { return InternalWelds; }
            set { InternalWelds = value; }
        }

        #endregion

        #region Public static constructors
        public static InvObjectVisibility ByInvObjectVisibility(InvObjectVisibility invObjectVisibility)
        {
            return new InvObjectVisibility(invObjectVisibility);
        }
        public static InvObjectVisibility ByInvObjectVisibility(Inventor.ObjectVisibility invObjectVisibility)
        {
            return new InvObjectVisibility(invObjectVisibility);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
