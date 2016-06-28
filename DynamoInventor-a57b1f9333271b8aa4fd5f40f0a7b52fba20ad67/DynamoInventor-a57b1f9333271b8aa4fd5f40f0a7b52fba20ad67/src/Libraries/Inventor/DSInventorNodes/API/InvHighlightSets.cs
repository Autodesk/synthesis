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
    public class InvHighlightSets
    {
        #region Internal properties
        internal Inventor.HighlightSets InternalHighlightSets { get; set; }

        internal int InternalCount
        {
            get { return HighlightSetsInstance.Count; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return HighlightSetsInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        #region Private constructors
        private InvHighlightSets(InvHighlightSets invHighlightSets)
        {
            InternalHighlightSets = invHighlightSets.InternalHighlightSets;
        }

        private InvHighlightSets(Inventor.HighlightSets invHighlightSets)
        {
            InternalHighlightSets = invHighlightSets;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.HighlightSets HighlightSetsInstance
        {
            get { return InternalHighlightSets; }
            set { InternalHighlightSets = value; }
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
        public static InvHighlightSets ByInvHighlightSets(InvHighlightSets invHighlightSets)
        {
            return new InvHighlightSets(invHighlightSets);
        }
        public static InvHighlightSets ByInvHighlightSets(Inventor.HighlightSets invHighlightSets)
        {
            return new InvHighlightSets(invHighlightSets);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
