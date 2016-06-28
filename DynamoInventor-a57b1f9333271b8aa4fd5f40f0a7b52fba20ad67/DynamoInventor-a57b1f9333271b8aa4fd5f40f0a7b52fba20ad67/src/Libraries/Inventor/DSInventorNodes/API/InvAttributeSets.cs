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
    public class InvAttributeSets
    {
        #region Internal properties
        internal Inventor.AttributeSets InternalAttributeSets { get; set; }

        internal int InternalCount
        {
            get { return AttributeSetsInstance.Count; }
        }

        //internal InvDataIO InternalDataIO
        //{
        //    get { return InvDataIO.ByInvDataIO(AttributeSetsInstance.DataIO); }
        //}


        internal Object InternalParent
        {
            get { return AttributeSetsInstance.Parent; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return AttributeSetsInstance.Type.As<InvObjectTypeEnum>(); }
        }


        #endregion

        #region Private constructors
        private InvAttributeSets(InvAttributeSets invAttributeSets)
        {
            InternalAttributeSets = invAttributeSets.InternalAttributeSets;
        }

        private InvAttributeSets(Inventor.AttributeSets invAttributeSets)
        {
            InternalAttributeSets = invAttributeSets;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.AttributeSets AttributeSetsInstance
        {
            get { return InternalAttributeSets; }
            set { InternalAttributeSets = value; }
        }

        public int Count
        {
            get { return InternalCount; }
        }

        //public InvDataIO DataIO
        //{
        //    get { return InternalDataIO; }
        //}

        public Object Parent
        {
            get { return InternalParent; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        #endregion
        #region Public static constructors
        public static InvAttributeSets ByInvAttributeSets(InvAttributeSets invAttributeSets)
        {
            return new InvAttributeSets(invAttributeSets);
        }
        public static InvAttributeSets ByInvAttributeSets(Inventor.AttributeSets invAttributeSets)
        {
            return new InvAttributeSets(invAttributeSets);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
