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
    public class InvPropertySets
    {
        #region Internal properties
        internal Inventor.PropertySets InternalPropertySets { get; set; }

        internal int InternalCount
        {
            get { return PropertySetsInstance.Count; }
        }

        internal bool InternalDirty
        {
            get { return PropertySetsInstance.Dirty; }
        }

        internal Object InternalParent
        {
            get { return PropertySetsInstance.Parent; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return PropertySetsInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        //internal PropertySets InternalkNoOwnership
        //{
        //    get { return Inventor.PropertySets.kNoOwnership; }
        //}
        //internal PropertySets InternalkSaveOwnership
        //{
        //    get { return Inventor.PropertySets.kSaveOwnership; }
        //}
        //internal PropertySets InternalkExclusiveOwnership
        //{
        //    get { return Inventor.PropertySets.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvPropertySets(InvPropertySets invPropertySets)
        {
            InternalPropertySets = invPropertySets.InternalPropertySets;
        }

        private InvPropertySets(Inventor.PropertySets invPropertySets)
        {
            InternalPropertySets = invPropertySets;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.PropertySets PropertySetsInstance
        {
            get { return InternalPropertySets; }
            set { InternalPropertySets = value; }
        }

        public int Count
        {
            get { return InternalCount; }
        }

        public bool Dirty
        {
            get { return InternalDirty; }
        }

        public Object Parent
        {
            get { return InternalParent; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        #endregion
        //public PropertySets kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public PropertySets kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public PropertySets kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvPropertySets ByInvPropertySets(InvPropertySets invPropertySets)
        {
            return new InvPropertySets(invPropertySets);
        }
        public static InvPropertySets ByInvPropertySets(Inventor.PropertySets invPropertySets)
        {
            return new InvPropertySets(invPropertySets);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
