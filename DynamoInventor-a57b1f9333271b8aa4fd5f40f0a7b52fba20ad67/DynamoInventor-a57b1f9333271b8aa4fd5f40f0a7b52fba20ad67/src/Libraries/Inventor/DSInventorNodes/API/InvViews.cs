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
    public class InvViews
    {
        #region Internal properties
        internal Inventor.Views InternalViews { get; set; }

        internal int InternalCount
        {
            get { return ViewsInstance.Count; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return ViewsInstance.Type.As<InvObjectTypeEnum>(); }
        }


        #endregion

        //internal Views InternalkNoOwnership
        //{
        //    get { return Inventor.Views.kNoOwnership; }
        //}
        //internal Views InternalkSaveOwnership
        //{
        //    get { return Inventor.Views.kSaveOwnership; }
        //}
        //internal Views InternalkExclusiveOwnership
        //{
        //    get { return Inventor.Views.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvViews(InvViews invViews)
        {
            InternalViews = invViews.InternalViews;
        }

        private InvViews(Inventor.Views invViews)
        {
            InternalViews = invViews;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.Views ViewsInstance
        {
            get { return InternalViews; }
            set { InternalViews = value; }
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
        //public Views kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public Views kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public Views kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvViews ByInvViews(InvViews invViews)
        {
            return new InvViews(invViews);
        }
        public static InvViews ByInvViews(Inventor.Views invViews)
        {
            return new InvViews(invViews);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
