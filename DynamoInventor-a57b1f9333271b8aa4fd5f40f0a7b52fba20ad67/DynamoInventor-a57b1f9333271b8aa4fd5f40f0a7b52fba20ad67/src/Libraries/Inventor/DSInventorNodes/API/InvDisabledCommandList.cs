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
    public class InvDisabledCommandList
    {
        #region Internal properties
        internal Inventor.DisabledCommandList InternalDisabledCommandList { get; set; }

        //internal InvApplication InternalApplication
        //{
        //    get { return InvApplication.ByInvApplication(DisabledCommandListInstance.Application); }
        //}


        internal int InternalCount
        {
            get { return DisabledCommandListInstance.Count; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return DisabledCommandListInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        #region Private constructors
        private InvDisabledCommandList(InvDisabledCommandList invDisabledCommandList)
        {
            InternalDisabledCommandList = invDisabledCommandList.InternalDisabledCommandList;
        }

        private InvDisabledCommandList(Inventor.DisabledCommandList invDisabledCommandList)
        {
            InternalDisabledCommandList = invDisabledCommandList;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.DisabledCommandList DisabledCommandListInstance
        {
            get { return InternalDisabledCommandList; }
            set { InternalDisabledCommandList = value; }
        }

        //public InvApplication Application
        //{
        //    get { return InternalApplication; }
        //}

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
        public static InvDisabledCommandList ByInvDisabledCommandList(InvDisabledCommandList invDisabledCommandList)
        {
            return new InvDisabledCommandList(invDisabledCommandList);
        }
        public static InvDisabledCommandList ByInvDisabledCommandList(Inventor.DisabledCommandList invDisabledCommandList)
        {
            return new InvDisabledCommandList(invDisabledCommandList);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
