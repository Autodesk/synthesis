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
    public class InvReferencedOLEFileDescriptors
    {
        #region Internal properties
        internal Inventor.ReferencedOLEFileDescriptors InternalReferencedOLEFileDescriptors { get; set; }

        internal Object InternalApplication
        {
            get { return ReferencedOLEFileDescriptorsInstance.Application; }
        }

        internal int InternalCount
        {
            get { return ReferencedOLEFileDescriptorsInstance.Count; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return ReferencedOLEFileDescriptorsInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        //internal ReferencedOLEFileDescriptors InternalkNoOwnership
        //{
        //    get { return Inventor.ReferencedOLEFileDescriptors.kNoOwnership; }
        //}
        //internal ReferencedOLEFileDescriptors InternalkSaveOwnership
        //{
        //    get { return Inventor.ReferencedOLEFileDescriptors.kSaveOwnership; }
        //}
        //internal ReferencedOLEFileDescriptors InternalkExclusiveOwnership
        //{
        //    get { return Inventor.ReferencedOLEFileDescriptors.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvReferencedOLEFileDescriptors(InvReferencedOLEFileDescriptors invReferencedOLEFileDescriptors)
        {
            InternalReferencedOLEFileDescriptors = invReferencedOLEFileDescriptors.InternalReferencedOLEFileDescriptors;
        }

        private InvReferencedOLEFileDescriptors(Inventor.ReferencedOLEFileDescriptors invReferencedOLEFileDescriptors)
        {
            InternalReferencedOLEFileDescriptors = invReferencedOLEFileDescriptors;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.ReferencedOLEFileDescriptors ReferencedOLEFileDescriptorsInstance
        {
            get { return InternalReferencedOLEFileDescriptors; }
            set { InternalReferencedOLEFileDescriptors = value; }
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
        //public ReferencedOLEFileDescriptors kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public ReferencedOLEFileDescriptors kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public ReferencedOLEFileDescriptors kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvReferencedOLEFileDescriptors ByInvReferencedOLEFileDescriptors(InvReferencedOLEFileDescriptors invReferencedOLEFileDescriptors)
        {
            return new InvReferencedOLEFileDescriptors(invReferencedOLEFileDescriptors);
        }
        public static InvReferencedOLEFileDescriptors ByInvReferencedOLEFileDescriptors(Inventor.ReferencedOLEFileDescriptors invReferencedOLEFileDescriptors)
        {
            return new InvReferencedOLEFileDescriptors(invReferencedOLEFileDescriptors);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
