using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Inventor;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using DSNodeServices;
using Dynamo.Models;
using Dynamo.Utilities;
using DSInventorNodes.GeometryConversion;
using InventorServices.Persistence;

namespace DSInventorNodes
{
    [RegisterForTrace]
    public class InvOGSSceneNode
    {
        #region Internal properties
        internal Inventor.OGSSceneNode InternalOGSSceneNode { get; set; }

        internal Inv_Document InternalDocument
        {
            get { return Inv_Document.ByInv_Document(OGSSceneNodeInstance.Document); }
        }


        internal string InternalName
        {
            get { return OGSSceneNodeInstance.Name; }
        }

        internal Object InternalParent
        {
            get { return OGSSceneNodeInstance.Parent; }
        }

        internal InvOGSRenderItemsEnumerator InternalRenderItems
        {
            get { return InvOGSRenderItemsEnumerator.ByInvOGSRenderItemsEnumerator(OGSSceneNodeInstance.RenderItems); }
        }


        internal InvOGSSceneNodeTypeEnum InternalSceneNodeType
        {
            get { return InvOGSSceneNodeTypeEnum.ByInvOGSSceneNodeTypeEnum(OGSSceneNodeInstance.SceneNodeType); }
        }


        internal InvOGSSceneNodesEnumerator InternalSubSceneNodes
        {
            get { return InvOGSSceneNodesEnumerator.ByInvOGSSceneNodesEnumerator(OGSSceneNodeInstance.SubSceneNodes); }
        }


        internal InvMatrix InternalTransformation
        {
            get { return InvMatrix.ByInvMatrix(OGSSceneNodeInstance.Transformation); }
        }


        internal InvObjectTypeEnum InternalType
        {
            get { return InvObjectTypeEnum.ByInvObjectTypeEnum(OGSSceneNodeInstance.Type); }
        }


        internal bool InternalVisible
        {
            get { return OGSSceneNodeInstance.Visible; }
        }

        #endregion

        internal OGSSceneNode InternalkNoOwnership
        {
            get { return Inventor.OGSSceneNode.kNoOwnership; }
        }
        internal OGSSceneNode InternalkSaveOwnership
        {
            get { return Inventor.OGSSceneNode.kSaveOwnership; }
        }
        internal OGSSceneNode InternalkExclusiveOwnership
        {
            get { return Inventor.OGSSceneNode.kExclusiveOwnership; }
        }
        #region Private constructors
        private InvOGSSceneNode(InvOGSSceneNode invOGSSceneNode)
        {
            InternalOGSSceneNode = invOGSSceneNode.InternalOGSSceneNode;
        }

        private InvOGSSceneNode(Inventor.OGSSceneNode invOGSSceneNode)
        {
            InternalOGSSceneNode = invOGSSceneNode;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.OGSSceneNode OGSSceneNodeInstance
        {
            get { return InternalOGSSceneNode; }
            set { InternalOGSSceneNode = value; }
        }

        public Inv_Document Document
        {
            get { return InternalDocument; }
        }

        public string Name
        {
            get { return InternalName; }
        }

        public Object Parent
        {
            get { return InternalParent; }
        }

        public InvOGSRenderItemsEnumerator RenderItems
        {
            get { return InternalRenderItems; }
        }

        public InvOGSSceneNodeTypeEnum SceneNodeType
        {
            get { return InternalSceneNodeType; }
        }

        public InvOGSSceneNodesEnumerator SubSceneNodes
        {
            get { return InternalSubSceneNodes; }
        }

        public InvMatrix Transformation
        {
            get { return InternalTransformation; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public bool Visible
        {
            get { return InternalVisible; }
        }

        #endregion
        public OGSSceneNode kNoOwnership
        {
            get { return InternalkNoOwnership; }
        }
        public OGSSceneNode kSaveOwnership
        {
            get { return InternalkSaveOwnership; }
        }
        public OGSSceneNode kExclusiveOwnership
        {
            get { return InternalkExclusiveOwnership; }
        }
        #region Public static constructors
        public static InvOGSSceneNode ByInvOGSSceneNode(InvOGSSceneNode invOGSSceneNode)
        {
            return new InvOGSSceneNode(invOGSSceneNode);
        }
        public static InvOGSSceneNode ByInvOGSSceneNode(Inventor.OGSSceneNode invOGSSceneNode)
        {
            return new InvOGSSceneNode(invOGSSceneNode);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
