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
    public class InvReferenceKeyManager
    {
        #region Internal properties
        internal Inventor.ReferenceKeyManager InternalReferenceKeyManager { get; set; }

        internal Object InternalApplication
        {
            get { return ReferenceKeyManagerInstance.Application; }
        }

        internal Object InternalParent
        {
            get { return ReferenceKeyManagerInstance.Parent; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return ReferenceKeyManagerInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        //internal ReferenceKeyManager InternalkNoOwnership
        //{
        //    get { return Inventor.ReferenceKeyManager.kNoOwnership; }
        //}
        //internal ReferenceKeyManager InternalkSaveOwnership
        //{
        //    get { return Inventor.ReferenceKeyManager.kSaveOwnership; }
        //}
        //internal ReferenceKeyManager InternalkExclusiveOwnership
        //{
        //    get { return Inventor.ReferenceKeyManager.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvReferenceKeyManager(InvReferenceKeyManager invReferenceKeyManager)
        {
            InternalReferenceKeyManager = invReferenceKeyManager.InternalReferenceKeyManager;
        }

        private InvReferenceKeyManager(Inventor.ReferenceKeyManager invReferenceKeyManager)
        {
            InternalReferenceKeyManager = invReferenceKeyManager;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.ReferenceKeyManager ReferenceKeyManagerInstance
        {
            get { return InternalReferenceKeyManager; }
            set { InternalReferenceKeyManager = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
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
        //public ReferenceKeyManager kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public ReferenceKeyManager kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public ReferenceKeyManager kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvReferenceKeyManager ByInvReferenceKeyManager(InvReferenceKeyManager invReferenceKeyManager)
        {
            return new InvReferenceKeyManager(invReferenceKeyManager);
        }
        public static InvReferenceKeyManager ByInvReferenceKeyManager(Inventor.ReferenceKeyManager invReferenceKeyManager)
        {
            return new InvReferenceKeyManager(invReferenceKeyManager);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
