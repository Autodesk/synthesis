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
    public class InvFile
    {
        #region Internal properties
        internal Inventor.File InternalFile { get; set; }

        //internal InvFilesEnumerator InternalAllReferencedFiles
        //{
        //    get { return InvFilesEnumerator.ByInvFilesEnumerator(FileInstance.AllReferencedFiles); }
        //}


        internal Object InternalApplication
        {
            get { return FileInstance.Application; }
        }

        internal Object InternalAvailableDocuments
        {
            get { return FileInstance.AvailableDocuments; }
        }

        internal int InternalFileSaveCounter
        {
            get { return FileInstance.FileSaveCounter; }
        }

        internal string InternalInternalName
        {
            get { return FileInstance.InternalName; }
        }

        internal InvFileOwnershipEnum InternalOwnershipType
        {
            get { return InvFileOwnershipEnum.ByInvFileOwnershipEnum(FileInstance.OwnershipType); }
        }


        //internal InvFileDescriptorsEnumerator InternalReferencedFileDescriptors
        //{
        //    get { return InvFileDescriptorsEnumerator.ByInvFileDescriptorsEnumerator(FileInstance.ReferencedFileDescriptors); }
        //}


        //internal InvFilesEnumerator InternalReferencedFiles
        //{
        //    get { return InvFilesEnumerator.ByInvFilesEnumerator(FileInstance.ReferencedFiles); }
        //}


        //internal InvFilesEnumerator InternalReferencingFiles
        //{
        //    get { return InvFilesEnumerator.ByInvFilesEnumerator(FileInstance.ReferencingFiles); }
        //}


        internal InvObjectTypeEnum InternalType
        {
            get { return FileInstance.Type.As<InvObjectTypeEnum>(); }
        }


        internal string InternalFullFileName { get; set; }
        #endregion

        #region Private constructors
        private InvFile(InvFile invFile)
        {
            InternalFile = invFile.InternalFile;
        }

        private InvFile(Inventor.File invFile)
        {
            InternalFile = invFile;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.File FileInstance
        {
            get { return InternalFile; }
            set { InternalFile = value; }
        }

        //public InvFilesEnumerator AllReferencedFiles
        //{
        //    get { return InternalAllReferencedFiles; }
        //}

        public Object Application
        {
            get { return InternalApplication; }
        }

        public Object AvailableDocuments
        {
            get { return InternalAvailableDocuments; }
        }

        public int FileSaveCounter
        {
            get { return InternalFileSaveCounter; }
        }

        public string InternalName
        {
            get { return InternalInternalName; }
        }

        public InvFileOwnershipEnum OwnershipType
        {
            get { return InternalOwnershipType; }
        }

        //public InvFileDescriptorsEnumerator ReferencedFileDescriptors
        //{
        //    get { return InternalReferencedFileDescriptors; }
        //}

        //public InvFilesEnumerator ReferencedFiles
        //{
        //    get { return InternalReferencedFiles; }
        //}

        //public InvFilesEnumerator ReferencingFiles
        //{
        //    get { return InternalReferencingFiles; }
        //}

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public string FullFileName
        {
            get { return InternalFullFileName; }
            set { InternalFullFileName = value; }
        }

        #endregion
        #region Public static constructors
        public static InvFile ByInvFile(InvFile invFile)
        {
            return new InvFile(invFile);
        }
        public static InvFile ByInvFile(Inventor.File invFile)
        {
            return new InvFile(invFile);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
