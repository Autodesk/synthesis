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
    public class InvDocumentInterests
    {
        #region Internal properties
        internal Inventor.DocumentInterests InternalDocumentInterests { get; set; }

        internal Object InternalApplication
        {
            get { return DocumentInterestsInstance.Application; }
        }

        internal int InternalCount
        {
            get { return DocumentInterestsInstance.Count; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return DocumentInterestsInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        #region Private constructors
        private InvDocumentInterests(InvDocumentInterests invDocumentInterests)
        {
            InternalDocumentInterests = invDocumentInterests.InternalDocumentInterests;
        }

        private InvDocumentInterests(Inventor.DocumentInterests invDocumentInterests)
        {
            InternalDocumentInterests = invDocumentInterests;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.DocumentInterests DocumentInterestsInstance
        {
            get { return InternalDocumentInterests; }
            set { InternalDocumentInterests = value; }
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
        public static InvDocumentInterests ByInvDocumentInterests(InvDocumentInterests invDocumentInterests)
        {
            return new InvDocumentInterests(invDocumentInterests);
        }
        public static InvDocumentInterests ByInvDocumentInterests(Inventor.DocumentInterests invDocumentInterests)
        {
            return new InvDocumentInterests(invDocumentInterests);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
