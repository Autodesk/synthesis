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
    public class InvCommandTypesEnum
    {
        #region Internal properties
        internal Inventor.CommandTypesEnum InternalCommandTypesEnum { get; set; }

        #endregion

        //internal CommandTypesEnum InternalkNoOwnership
        //{
        //    get { return Inventor.CommandTypesEnum.kNoOwnership; }
        //}
        //internal CommandTypesEnum InternalkSaveOwnership
        //{
        //    get { return Inventor.CommandTypesEnum.kSaveOwnership; }
        //}
        //internal CommandTypesEnum InternalkExclusiveOwnership
        //{
        //    get { return Inventor.CommandTypesEnum.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvCommandTypesEnum(InvCommandTypesEnum invCommandTypesEnum)
        {
            InternalCommandTypesEnum = invCommandTypesEnum.InternalCommandTypesEnum;
        }

        private InvCommandTypesEnum(Inventor.CommandTypesEnum invCommandTypesEnum)
        {
            InternalCommandTypesEnum = invCommandTypesEnum;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.CommandTypesEnum CommandTypesEnumInstance
        {
            get { return InternalCommandTypesEnum; }
            set { InternalCommandTypesEnum = value; }
        }

        #endregion
        //public CommandTypesEnum kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public CommandTypesEnum kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public CommandTypesEnum kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvCommandTypesEnum ByInvCommandTypesEnum(InvCommandTypesEnum invCommandTypesEnum)
        {
            return new InvCommandTypesEnum(invCommandTypesEnum);
        }
        public static InvCommandTypesEnum ByInvCommandTypesEnum(Inventor.CommandTypesEnum invCommandTypesEnum)
        {
            return new InvCommandTypesEnum(invCommandTypesEnum);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
