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
    public class InvAssets
    {
        #region Internal properties
        internal Inventor.Assets InternalAssets { get; set; }

        internal Object InternalApplication
        {
            get { return AssetsInstance.Application; }
        }

        internal int InternalCount
        {
            get { return AssetsInstance.Count; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return AssetsInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        #region Private constructors
        private InvAssets(InvAssets invAssets)
        {
            InternalAssets = invAssets.InternalAssets;
        }

        private InvAssets(Inventor.Assets invAssets)
        {
            InternalAssets = invAssets;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.Assets AssetsInstance
        {
            get { return InternalAssets; }
            set { InternalAssets = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        public int Count
        {
            get { return InternalCount; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        #endregion

        #region Public static constructors
        public static InvAssets ByInvAssets(InvAssets invAssets)
        {
            return new InvAssets(invAssets);
        }
        public static InvAssets ByInvAssets(Inventor.Assets invAssets)
        {
            return new InvAssets(invAssets);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
