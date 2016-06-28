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
    public class InvInventorVBAProject
    {
        #region Internal properties
        internal Inventor.InventorVBAProject InternalInventorVBAProject { get; set; }

        //internal InvInventorVBAComponents InternalInventorVBAComponents
        //{
        //    get { return InvInventorVBAComponents.ByInvInventorVBAComponents(InventorVBAProjectInstance.InventorVBAComponents); }
        //}

        //internal InvApplication InternalParent
        //{
        //    get { return InvApplication.ByInvApplication(InventorVBAProjectInstance.Parent); }
        //}

        //internal InvVBAProjectTypeEnum InternalProjectType
        //{
        //    get { return InvVBAProjectTypeEnum.ByInvVBAProjectTypeEnum(InventorVBAProjectInstance.ProjectType); }
        //}

        internal bool InternalSaved
        {
            get { return InventorVBAProjectInstance.Saved; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return InventorVBAProjectInstance.Type.As<InvObjectTypeEnum>(); }
        }

        internal Object InternalVBProject
        {
            get { return InventorVBAProjectInstance.VBProject; }
        }

        internal string InternalName { get; set; }
        #endregion

        //internal InventorVBAProject InternalkNoOwnership
        //{
        //    get { return Inventor.InventorVBAProject.kNoOwnership; }
        //}
        //internal InventorVBAProject InternalkSaveOwnership
        //{
        //    get { return Inventor.InventorVBAProject.kSaveOwnership; }
        //}
        //internal InventorVBAProject InternalkExclusiveOwnership
        //{
        //    get { return Inventor.InventorVBAProject.kExclusiveOwnership; }
        //}

        #region Private constructors
        private InvInventorVBAProject(InvInventorVBAProject invInventorVBAProject)
        {
            InternalInventorVBAProject = invInventorVBAProject.InternalInventorVBAProject;
        }

        private InvInventorVBAProject(Inventor.InventorVBAProject invInventorVBAProject)
        {
            InternalInventorVBAProject = invInventorVBAProject;
        }
        #endregion

        #region Private methods
        private void InternalClose()
        {
            InventorVBAProjectInstance.Close();
        }

        private void InternalSave()
        {
            InventorVBAProjectInstance.Save();
        }

        private void InternalSaveAs(string fullFileName)
        {
            InventorVBAProjectInstance.SaveAs( fullFileName);
        }

        #endregion

        #region Public properties
        public Inventor.InventorVBAProject InventorVBAProjectInstance
        {
            get { return InternalInventorVBAProject; }
            set { InternalInventorVBAProject = value; }
        }

        //public InvInventorVBAComponents InventorVBAComponents
        //{
        //    get { return InternalInventorVBAComponents; }
        //}

        //public InvApplication Parent
        //{
        //    get { return InternalParent; }
        //}

        //public InvVBAProjectTypeEnum ProjectType
        //{
        //    get { return InternalProjectType; }
        //}

        public bool Saved
        {
            get { return InternalSaved; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public Object VBProject
        {
            get { return InternalVBProject; }
        }

        public string Name
        {
            get { return InternalName; }
            set { InternalName = value; }
        }

        #endregion
        //public InventorVBAProject kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public InventorVBAProject kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public InventorVBAProject kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvInventorVBAProject ByInvInventorVBAProject(InvInventorVBAProject invInventorVBAProject)
        {
            return new InvInventorVBAProject(invInventorVBAProject);
        }
        public static InvInventorVBAProject ByInvInventorVBAProject(Inventor.InventorVBAProject invInventorVBAProject)
        {
            return new InvInventorVBAProject(invInventorVBAProject);
        }
        #endregion

        #region Public methods
        public void Close()
        {
            InternalClose();
        }

        public void Save()
        {
            InternalSave();
        }

        public void SaveAs(string fullFileName)
        {
            InternalSaveAs( fullFileName);
        }

        #endregion
    }
}
