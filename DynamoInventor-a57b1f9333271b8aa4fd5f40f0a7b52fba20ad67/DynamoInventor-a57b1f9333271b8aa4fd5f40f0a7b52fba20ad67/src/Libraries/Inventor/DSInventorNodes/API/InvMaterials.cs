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
    public class InvMaterials
    {
        #region Internal properties
        internal Inventor.Materials InternalMaterials { get; set; }

        internal Object InternalApplication
        {
            get { return MaterialsInstance.Application; }
        }

        internal int InternalCount
        {
            get { return MaterialsInstance.Count; }
        }

        internal Object InternalParent
        {
            get { return MaterialsInstance.Parent; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return MaterialsInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        #region Private constructors
        private InvMaterials(InvMaterials invMaterials)
        {
            InternalMaterials = invMaterials.InternalMaterials;
        }

        private InvMaterials(Inventor.Materials invMaterials)
        {
            InternalMaterials = invMaterials;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.Materials MaterialsInstance
        {
            get { return InternalMaterials; }
            set { InternalMaterials = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        public int Count
        {
            get { return InternalCount; }
        }

        public Object Parent
        {
            get { return InternalParent; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        #endregion

        #region Public static constructors
        public static InvMaterials ByInvMaterials(InvMaterials invMaterials)
        {
            return new InvMaterials(invMaterials);
        }
        public static InvMaterials ByInvMaterials(Inventor.Materials invMaterials)
        {
            return new InvMaterials(invMaterials);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
