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
    public class InvLightingStyles
    {
        #region Internal properties
        internal Inventor.LightingStyles InternalLightingStyles { get; set; }

        internal Object InternalApplication
        {
            get { return LightingStylesInstance.Application; }
        }

        internal int InternalCount
        {
            get { return LightingStylesInstance.Count; }
        }

        internal Object InternalParent
        {
            get { return LightingStylesInstance.Parent; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return LightingStylesInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        #region Private constructors
        private InvLightingStyles(InvLightingStyles invLightingStyles)
        {
            InternalLightingStyles = invLightingStyles.InternalLightingStyles;
        }

        private InvLightingStyles(Inventor.LightingStyles invLightingStyles)
        {
            InternalLightingStyles = invLightingStyles;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.LightingStyles LightingStylesInstance
        {
            get { return InternalLightingStyles; }
            set { InternalLightingStyles = value; }
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

        #region Public static constructors
        public static InvLightingStyles ByInvLightingStyles(InvLightingStyles invLightingStyles)
        {
            return new InvLightingStyles(invLightingStyles);
        }
        public static InvLightingStyles ByInvLightingStyles(Inventor.LightingStyles invLightingStyles)
        {
            return new InvLightingStyles(invLightingStyles);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
