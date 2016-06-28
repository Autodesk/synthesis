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
    public class InvRenderStyles
    {
        #region Internal properties
        internal Inventor.RenderStyles InternalRenderStyles { get; set; }

        internal Object InternalApplication
        {
            get { return RenderStylesInstance.Application; }
        }

        internal int InternalCount
        {
            get { return RenderStylesInstance.Count; }
        }

        internal Object InternalParent
        {
            get { return RenderStylesInstance.Parent; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return RenderStylesInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        //internal RenderStyles InternalkNoOwnership
        //{
        //    get { return Inventor.RenderStyles.kNoOwnership; }
        //}
        //internal RenderStyles InternalkSaveOwnership
        //{
        //    get { return Inventor.RenderStyles.kSaveOwnership; }
        //}
        //internal RenderStyles InternalkExclusiveOwnership
        //{
        //    get { return Inventor.RenderStyles.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvRenderStyles(InvRenderStyles invRenderStyles)
        {
            InternalRenderStyles = invRenderStyles.InternalRenderStyles;
        }

        private InvRenderStyles(Inventor.RenderStyles invRenderStyles)
        {
            InternalRenderStyles = invRenderStyles;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.RenderStyles RenderStylesInstance
        {
            get { return InternalRenderStyles; }
            set { InternalRenderStyles = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        public int Count
        {
            get { return InternalCount; }
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
        //public RenderStyles kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public RenderStyles kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public RenderStyles kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvRenderStyles ByInvRenderStyles(InvRenderStyles invRenderStyles)
        {
            return new InvRenderStyles(invRenderStyles);
        }
        public static InvRenderStyles ByInvRenderStyles(Inventor.RenderStyles invRenderStyles)
        {
            return new InvRenderStyles(invRenderStyles);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
