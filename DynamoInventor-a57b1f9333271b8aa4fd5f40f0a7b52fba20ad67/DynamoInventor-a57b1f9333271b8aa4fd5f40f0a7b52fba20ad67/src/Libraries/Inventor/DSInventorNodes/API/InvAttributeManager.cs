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
    public class InvAttributeManager
    {
        #region Internal properties
        internal Inventor.AttributeManager InternalAttributeManager { get; set; }

        internal Object InternalParent
        {
            get { return AttributeManagerInstance.Parent; }
        }

        internal string InternalRevisionId
        {
            get { return AttributeManagerInstance.RevisionId; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return AttributeManagerInstance.Type.As<InvObjectTypeEnum>(); }
        }


        #endregion

        #region Private constructors
        private InvAttributeManager(InvAttributeManager invAttributeManager)
        {
            InternalAttributeManager = invAttributeManager.InternalAttributeManager;
        }

        private InvAttributeManager(Inventor.AttributeManager invAttributeManager)
        {
            InternalAttributeManager = invAttributeManager;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.AttributeManager AttributeManagerInstance
        {
            get { return InternalAttributeManager; }
            set { InternalAttributeManager = value; }
        }

        public Object Parent
        {
            get { return InternalParent; }
        }

        public string RevisionId
        {
            get { return InternalRevisionId; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        #endregion
        #region Public static constructors
        public static InvAttributeManager ByInvAttributeManager(InvAttributeManager invAttributeManager)
        {
            return new InvAttributeManager(invAttributeManager);
        }
        public static InvAttributeManager ByInvAttributeManager(Inventor.AttributeManager invAttributeManager)
        {
            return new InvAttributeManager(invAttributeManager);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
