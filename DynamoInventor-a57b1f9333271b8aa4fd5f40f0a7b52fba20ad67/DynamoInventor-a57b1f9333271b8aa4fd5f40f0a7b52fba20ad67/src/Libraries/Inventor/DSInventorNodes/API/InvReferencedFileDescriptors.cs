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
    public class InvReferencedFileDescriptors
    {
        #region Internal properties
        internal Inventor.ReferencedFileDescriptors InternalReferencedFileDescriptors { get; set; }

        internal Object InternalApplication
        {
            get { return ReferencedFileDescriptorsInstance.Application; }
        }

        internal int InternalCount
        {
            get { return ReferencedFileDescriptorsInstance.Count; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return ReferencedFileDescriptorsInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        //internal ReferencedFileDescriptors InternalkNoOwnership
        //{
        //    get { return Inventor.ReferencedFileDescriptors.kNoOwnership; }
        //}
        //internal ReferencedFileDescriptors InternalkSaveOwnership
        //{
        //    get { return Inventor.ReferencedFileDescriptors.kSaveOwnership; }
        //}
        //internal ReferencedFileDescriptors InternalkExclusiveOwnership
        //{
        //    get { return Inventor.ReferencedFileDescriptors.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvReferencedFileDescriptors(InvReferencedFileDescriptors invReferencedFileDescriptors)
        {
            InternalReferencedFileDescriptors = invReferencedFileDescriptors.InternalReferencedFileDescriptors;
        }

        private InvReferencedFileDescriptors(Inventor.ReferencedFileDescriptors invReferencedFileDescriptors)
        {
            InternalReferencedFileDescriptors = invReferencedFileDescriptors;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.ReferencedFileDescriptors ReferencedFileDescriptorsInstance
        {
            get { return InternalReferencedFileDescriptors; }
            set { InternalReferencedFileDescriptors = value; }
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
        //public ReferencedFileDescriptors kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public ReferencedFileDescriptors kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public ReferencedFileDescriptors kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvReferencedFileDescriptors ByInvReferencedFileDescriptors(InvReferencedFileDescriptors invReferencedFileDescriptors)
        {
            return new InvReferencedFileDescriptors(invReferencedFileDescriptors);
        }
        public static InvReferencedFileDescriptors ByInvReferencedFileDescriptors(Inventor.ReferencedFileDescriptors invReferencedFileDescriptors)
        {
            return new InvReferencedFileDescriptors(invReferencedFileDescriptors);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
