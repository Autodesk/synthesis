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
    public class InvEnvironmentManager
    {
        #region Internal properties
        internal Inventor.EnvironmentManager InternalEnvironmentManager { get; set; }

        //internal InvApplication InternalApplication
        //{
        //    get { return InvApplication.ByInvApplication(EnvironmentManagerInstance.Application); }
        //}

        //internal InvEnvironment InternalBaseEnvironment
        //{
        //    get { return InvEnvironment.ByInvEnvironment(EnvironmentManagerInstance.BaseEnvironment); }
        //}

        //internal InvEnvironment InternalEditObjectEnvironment
        //{
        //    get { return InvEnvironment.ByInvEnvironment(EnvironmentManagerInstance.EditObjectEnvironment); }
        //}

        //internal Inv_Document InternalParent
        //{
        //    get { return Inv_Document.ByInv_Document(EnvironmentManagerInstance.Parent); }
        //}

        internal InvObjectTypeEnum InternalType
        {
            get { return EnvironmentManagerInstance.Type.As<InvObjectTypeEnum>(); }
        }

        //Need to add Inventor.Environment
        //internal Environment InternalOverrideEnvironment { get; set; }
        #endregion

        #region Private constructors
        private InvEnvironmentManager(InvEnvironmentManager invEnvironmentManager)
        {
            InternalEnvironmentManager = invEnvironmentManager.InternalEnvironmentManager;
        }

        private InvEnvironmentManager(Inventor.EnvironmentManager invEnvironmentManager)
        {
            InternalEnvironmentManager = invEnvironmentManager;
        }
        #endregion

        #region Private methods
        private void InternalGetCurrentEnvironment(out Inventor.Environment environment, out string editTargetId)
        {
            EnvironmentManagerInstance.GetCurrentEnvironment(out  environment, out  editTargetId);
        }

        private void InternalSetCurrentEnvironment(Inventor.Environment environment, string editObjectId)
        {
            EnvironmentManagerInstance.SetCurrentEnvironment( environment,  editObjectId);
        }

        #endregion

        #region Public properties
        public Inventor.EnvironmentManager EnvironmentManagerInstance
        {
            get { return InternalEnvironmentManager; }
            set { InternalEnvironmentManager = value; }
        }

        //public InvApplication Application
        //{
        //    get { return InternalApplication; }
        //}

        //public InvEnvironment BaseEnvironment
        //{
        //    get { return InternalBaseEnvironment; }
        //}

        //public InvEnvironment EditObjectEnvironment
        //{
        //    get { return InternalEditObjectEnvironment; }
        //}

        //public Inv_Document Parent
        //{
        //    get { return InternalParent; }
        //}

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        //public InvEnvironment OverrideEnvironment
        //{
        //    get { return InternalOverrideEnvironment; }
        //    set { InternalOverrideEnvironment = value; }
        //}

        #endregion
        #region Public static constructors
        public static InvEnvironmentManager ByInvEnvironmentManager(InvEnvironmentManager invEnvironmentManager)
        {
            return new InvEnvironmentManager(invEnvironmentManager);
        }
        public static InvEnvironmentManager ByInvEnvironmentManager(Inventor.EnvironmentManager invEnvironmentManager)
        {
            return new InvEnvironmentManager(invEnvironmentManager);
        }
        #endregion

        #region Public methods
        public void GetCurrentEnvironment(out Inventor.Environment environment, out string editTargetId)
        {
            InternalGetCurrentEnvironment(out  environment, out  editTargetId);
        }

        public void SetCurrentEnvironment(Inventor.Environment environment, string editObjectId)
        {
            InternalSetCurrentEnvironment( environment,  editObjectId);
        }

        #endregion
    }
}
