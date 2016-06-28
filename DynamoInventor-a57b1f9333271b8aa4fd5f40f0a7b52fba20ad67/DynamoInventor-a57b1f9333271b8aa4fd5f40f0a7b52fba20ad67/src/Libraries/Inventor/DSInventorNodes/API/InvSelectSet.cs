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
    public class InvSelectSet
    {
        #region Internal properties
        internal Inventor.SelectSet InternalSelectSet { get; set; }

        internal int InternalCount
        {
            get { return SelectSetInstance.Count; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return SelectSetInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        //internal SelectSet InternalkNoOwnership
        //{
        //    get { return Inventor.SelectSet.kNoOwnership; }
        //}
        //internal SelectSet InternalkSaveOwnership
        //{
        //    get { return Inventor.SelectSet.kSaveOwnership; }
        //}
        //internal SelectSet InternalkExclusiveOwnership
        //{
        //    get { return Inventor.SelectSet.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvSelectSet(InvSelectSet invSelectSet)
        {
            InternalSelectSet = invSelectSet.InternalSelectSet;
        }

        private InvSelectSet(Inventor.SelectSet invSelectSet)
        {
            InternalSelectSet = invSelectSet;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.SelectSet SelectSetInstance
        {
            get { return InternalSelectSet; }
            set { InternalSelectSet = value; }
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
        //public SelectSet kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public SelectSet kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public SelectSet kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}

        #region Public static constructors
        public static InvSelectSet ByInvSelectSet(InvSelectSet invSelectSet)
        {
            return new InvSelectSet(invSelectSet);
        }
        public static InvSelectSet ByInvSelectSet(Inventor.SelectSet invSelectSet)
        {
            return new InvSelectSet(invSelectSet);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
