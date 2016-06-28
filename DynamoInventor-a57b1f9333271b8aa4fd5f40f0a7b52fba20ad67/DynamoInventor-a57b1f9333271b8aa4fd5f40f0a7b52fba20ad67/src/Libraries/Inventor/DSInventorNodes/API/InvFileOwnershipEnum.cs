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
    public class InvFileOwnershipEnum
    {
        #region Internal properties
        internal Inventor.FileOwnershipEnum InternalFileOwnershipEnum { get; set; }

        #endregion

        internal FileOwnershipEnum InternalkNoOwnership
        {
            get { return Inventor.FileOwnershipEnum.kNoOwnership; }
        }
        internal FileOwnershipEnum InternalkSaveOwnership
        {
            get { return Inventor.FileOwnershipEnum.kSaveOwnership; }
        }
        internal FileOwnershipEnum InternalkExclusiveOwnership
        {
            get { return Inventor.FileOwnershipEnum.kExclusiveOwnership; }
        }
        #region Private constructors
        private InvFileOwnershipEnum(InvFileOwnershipEnum invFileOwnershipEnum)
        {
            InternalFileOwnershipEnum = invFileOwnershipEnum.InternalFileOwnershipEnum;
        }

        private InvFileOwnershipEnum(Inventor.FileOwnershipEnum invFileOwnershipEnum)
        {
            InternalFileOwnershipEnum = invFileOwnershipEnum;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.FileOwnershipEnum FileOwnershipEnumInstance
        {
            get { return InternalFileOwnershipEnum; }
            set { InternalFileOwnershipEnum = value; }
        }

        #endregion
        public FileOwnershipEnum kNoOwnership
        {
            get { return InternalkNoOwnership; }
        }
        public FileOwnershipEnum kSaveOwnership
        {
            get { return InternalkSaveOwnership; }
        }
        public FileOwnershipEnum kExclusiveOwnership
        {
            get { return InternalkExclusiveOwnership; }
        }
        #region Public static constructors
        public static InvFileOwnershipEnum ByInvFileOwnershipEnum(InvFileOwnershipEnum invFileOwnershipEnum)
        {
            return new InvFileOwnershipEnum(invFileOwnershipEnum);
        }
        public static InvFileOwnershipEnum ByInvFileOwnershipEnum(Inventor.FileOwnershipEnum invFileOwnershipEnum)
        {
            return new InvFileOwnershipEnum(invFileOwnershipEnum);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
