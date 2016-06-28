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
    public class InvThumbnailSaveOptionEnum
    {
        #region Internal properties
        internal Inventor.ThumbnailSaveOptionEnum InternalThumbnailSaveOptionEnum { get; set; }

        #endregion

        //internal ThumbnailSaveOptionEnum InternalkNoOwnership
        //{
        //    get { return Inventor.ThumbnailSaveOptionEnum.kNoOwnership; }
        //}
        //internal ThumbnailSaveOptionEnum InternalkSaveOwnership
        //{
        //    get { return Inventor.ThumbnailSaveOptionEnum.kSaveOwnership; }
        //}
        //internal ThumbnailSaveOptionEnum InternalkExclusiveOwnership
        //{
        //    get { return Inventor.ThumbnailSaveOptionEnum.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvThumbnailSaveOptionEnum(InvThumbnailSaveOptionEnum invThumbnailSaveOptionEnum)
        {
            InternalThumbnailSaveOptionEnum = invThumbnailSaveOptionEnum.InternalThumbnailSaveOptionEnum;
        }

        private InvThumbnailSaveOptionEnum(Inventor.ThumbnailSaveOptionEnum invThumbnailSaveOptionEnum)
        {
            InternalThumbnailSaveOptionEnum = invThumbnailSaveOptionEnum;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.ThumbnailSaveOptionEnum ThumbnailSaveOptionEnumInstance
        {
            get { return InternalThumbnailSaveOptionEnum; }
            set { InternalThumbnailSaveOptionEnum = value; }
        }

        #endregion
        //public ThumbnailSaveOptionEnum kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public ThumbnailSaveOptionEnum kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public ThumbnailSaveOptionEnum kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvThumbnailSaveOptionEnum ByInvThumbnailSaveOptionEnum(InvThumbnailSaveOptionEnum invThumbnailSaveOptionEnum)
        {
            return new InvThumbnailSaveOptionEnum(invThumbnailSaveOptionEnum);
        }
        public static InvThumbnailSaveOptionEnum ByInvThumbnailSaveOptionEnum(Inventor.ThumbnailSaveOptionEnum invThumbnailSaveOptionEnum)
        {
            return new InvThumbnailSaveOptionEnum(invThumbnailSaveOptionEnum);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
