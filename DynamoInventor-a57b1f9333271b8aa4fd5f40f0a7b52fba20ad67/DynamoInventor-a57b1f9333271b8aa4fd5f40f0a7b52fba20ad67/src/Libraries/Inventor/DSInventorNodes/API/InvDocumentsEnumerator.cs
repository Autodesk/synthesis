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
    public class InvDocumentsEnumerator
    {
        #region Internal properties
        internal Inventor.DocumentsEnumerator InternalDocumentsEnumerator { get; set; }

        internal int InternalCount
        {
            get { return DocumentsEnumeratorInstance.Count; }
        }

        //internal ObjectTypeEnum InternalType
        //{
        //    get { return new InvObjectTypeEnum(DocumentsEnumeratorInstance.Type); }
        //}


        #endregion

        #region Private constructors
        private InvDocumentsEnumerator(InvDocumentsEnumerator invDocumentsEnumerator)
        {
            InternalDocumentsEnumerator = invDocumentsEnumerator.InternalDocumentsEnumerator;
        }

        private InvDocumentsEnumerator(Inventor.DocumentsEnumerator invDocumentsEnumerator)
        {
            InternalDocumentsEnumerator = invDocumentsEnumerator;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.DocumentsEnumerator DocumentsEnumeratorInstance
        {
            get { return InternalDocumentsEnumerator; }
            set { InternalDocumentsEnumerator = value; }
        }

        public int Count
        {
            get { return InternalCount; }
        }

        //public ObjectTypeEnum Type
        //{
        //    get { return InternalType; }
        //}

        #endregion
        #region Public static constructors
        public static InvDocumentsEnumerator ByInvDocumentsEnumerator(InvDocumentsEnumerator invDocumentsEnumerator)
        {
            return new InvDocumentsEnumerator(invDocumentsEnumerator);
        }
        public static InvDocumentsEnumerator ByInvDocumentsEnumerator(Inventor.DocumentsEnumerator invDocumentsEnumerator)
        {
            return new InvDocumentsEnumerator(invDocumentsEnumerator);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
