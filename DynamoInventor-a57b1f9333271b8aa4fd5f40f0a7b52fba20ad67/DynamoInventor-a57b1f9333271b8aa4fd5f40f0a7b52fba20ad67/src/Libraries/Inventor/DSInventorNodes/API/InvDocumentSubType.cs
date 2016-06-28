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
    public class InvDocumentSubType
    {
        #region Internal properties
        internal Inventor.DocumentSubType InternalDocumentSubType { get; set; }

        internal string InternalDocumentSubTypeID
        {
            get { return DocumentSubTypeInstance.DocumentSubTypeID; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return DocumentSubTypeInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        #region Private constructors
        private InvDocumentSubType(InvDocumentSubType invDocumentSubType)
        {
            InternalDocumentSubType = invDocumentSubType.InternalDocumentSubType;
        }

        private InvDocumentSubType(Inventor.DocumentSubType invDocumentSubType)
        {
            InternalDocumentSubType = invDocumentSubType;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.DocumentSubType DocumentSubTypeInstance
        {
            get { return InternalDocumentSubType; }
            set { InternalDocumentSubType = value; }
        }

        public string DocumentSubTypeID
        {
            get { return InternalDocumentSubTypeID; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        #endregion
        #region Public static constructors
        public static InvDocumentSubType ByInvDocumentSubType(InvDocumentSubType invDocumentSubType)
        {
            return new InvDocumentSubType(invDocumentSubType);
        }
        public static InvDocumentSubType ByInvDocumentSubType(Inventor.DocumentSubType invDocumentSubType)
        {
            return new InvDocumentSubType(invDocumentSubType);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
