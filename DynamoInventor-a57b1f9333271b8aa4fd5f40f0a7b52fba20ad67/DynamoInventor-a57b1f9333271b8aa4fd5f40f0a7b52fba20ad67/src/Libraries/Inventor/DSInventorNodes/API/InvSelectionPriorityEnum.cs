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
    public class InvSelectionPriorityEnum
    {
        #region Internal properties
        internal Inventor.SelectionPriorityEnum InternalSelectionPriorityEnum { get; set; }

        #endregion

        //internal SelectionPriorityEnum InternalkNoOwnership
        //{
        //    get { return Inventor.SelectionPriorityEnum.kNoOwnership; }
        //}
        //internal SelectionPriorityEnum InternalkSaveOwnership
        //{
        //    get { return Inventor.SelectionPriorityEnum.kSaveOwnership; }
        //}
        //internal SelectionPriorityEnum InternalkExclusiveOwnership
        //{
        //    get { return Inventor.SelectionPriorityEnum.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvSelectionPriorityEnum(InvSelectionPriorityEnum invSelectionPriorityEnum)
        {
            InternalSelectionPriorityEnum = invSelectionPriorityEnum.InternalSelectionPriorityEnum;
        }

        private InvSelectionPriorityEnum(Inventor.SelectionPriorityEnum invSelectionPriorityEnum)
        {
            InternalSelectionPriorityEnum = invSelectionPriorityEnum;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.SelectionPriorityEnum SelectionPriorityEnumInstance
        {
            get { return InternalSelectionPriorityEnum; }
            set { InternalSelectionPriorityEnum = value; }
        }

        #endregion
        //public SelectionPriorityEnum kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public SelectionPriorityEnum kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public SelectionPriorityEnum kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvSelectionPriorityEnum ByInvSelectionPriorityEnum(InvSelectionPriorityEnum invSelectionPriorityEnum)
        {
            return new InvSelectionPriorityEnum(invSelectionPriorityEnum);
        }
        public static InvSelectionPriorityEnum ByInvSelectionPriorityEnum(Inventor.SelectionPriorityEnum invSelectionPriorityEnum)
        {
            return new InvSelectionPriorityEnum(invSelectionPriorityEnum);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
