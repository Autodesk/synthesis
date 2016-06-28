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
    public class InvDocumentDescriptorsEnumerator
    {
        #region Internal properties
        internal Inventor.DocumentDescriptorsEnumerator InternalDocumentDescriptorsEnumerator { get; set; }

        internal Object InternalApplication
        {
            get { return DocumentDescriptorsEnumeratorInstance.Application; }
        }

        internal int InternalCount
        {
            get { return DocumentDescriptorsEnumeratorInstance.Count; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return DocumentDescriptorsEnumeratorInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        //internal DocumentDescriptorsEnumerator InternalkNoOwnership
        //{
        //    get { return Inventor.DocumentDescriptorsEnumerator.kNoOwnership; }
        //}
        //internal DocumentDescriptorsEnumerator InternalkSaveOwnership
        //{
        //    get { return Inventor.DocumentDescriptorsEnumerator.kSaveOwnership; }
        //}
        //internal DocumentDescriptorsEnumerator InternalkExclusiveOwnership
        //{
        //    get { return Inventor.DocumentDescriptorsEnumerator.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvDocumentDescriptorsEnumerator(InvDocumentDescriptorsEnumerator invDocumentDescriptorsEnumerator)
        {
            InternalDocumentDescriptorsEnumerator = invDocumentDescriptorsEnumerator.InternalDocumentDescriptorsEnumerator;
        }

        private InvDocumentDescriptorsEnumerator(Inventor.DocumentDescriptorsEnumerator invDocumentDescriptorsEnumerator)
        {
            InternalDocumentDescriptorsEnumerator = invDocumentDescriptorsEnumerator;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.DocumentDescriptorsEnumerator DocumentDescriptorsEnumeratorInstance
        {
            get { return InternalDocumentDescriptorsEnumerator; }
            set { InternalDocumentDescriptorsEnumerator = value; }
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
        //public DocumentDescriptorsEnumerator kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public DocumentDescriptorsEnumerator kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public DocumentDescriptorsEnumerator kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvDocumentDescriptorsEnumerator ByInvDocumentDescriptorsEnumerator(InvDocumentDescriptorsEnumerator invDocumentDescriptorsEnumerator)
        {
            return new InvDocumentDescriptorsEnumerator(invDocumentDescriptorsEnumerator);
        }
        public static InvDocumentDescriptorsEnumerator ByInvDocumentDescriptorsEnumerator(Inventor.DocumentDescriptorsEnumerator invDocumentDescriptorsEnumerator)
        {
            return new InvDocumentDescriptorsEnumerator(invDocumentDescriptorsEnumerator);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
