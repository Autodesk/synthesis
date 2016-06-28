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
    public class InvSoftwareVersion
    {
        #region Internal properties
        internal Inventor.SoftwareVersion InternalSoftwareVersion { get; set; }

        internal int Internal_DebugBuildIdentifier
        {
            get { return SoftwareVersionInstance._DebugBuildIdentifier; }
        }

        internal int Internal_InternalBuildIdentifier
        {
            get { return SoftwareVersionInstance._InternalBuildIdentifier; }
        }

        internal int Internal_PatchBuildIdentifier
        {
            get { return SoftwareVersionInstance._PatchBuildIdentifier; }
        }

        internal int InternalBetaVersion
        {
            get { return SoftwareVersionInstance.BetaVersion; }
        }

        internal int InternalBuildIdentifier
        {
            get { return SoftwareVersionInstance.BuildIdentifier; }
        }

        internal string InternalDisplayName
        {
            get { return SoftwareVersionInstance.DisplayName; }
        }

        internal string InternalDisplayVersion
        {
            get { return SoftwareVersionInstance.DisplayVersion; }
        }

        internal bool InternalIs64BitVersion
        {
            get { return SoftwareVersionInstance.Is64BitVersion; }
        }

        internal bool InternalIsEducationVersion
        {
            get { return SoftwareVersionInstance.IsEducationVersion; }
        }

        internal int InternalMajor
        {
            get { return SoftwareVersionInstance.Major; }
        }

        internal int InternalMinor
        {
            get { return SoftwareVersionInstance.Minor; }
        }

        internal bool InternalNotProduction
        {
            get { return SoftwareVersionInstance.NotProduction; }
        }

        //internal InvProductEditionEnum InternalProductEdition
        //{
        //    get { return InvProductEditionEnum.ByInvProductEditionEnum(SoftwareVersionInstance.ProductEdition); }
        //}
        internal string InternalProductName
        {
            get { return SoftwareVersionInstance.ProductName; }
        }

        internal int InternalServicePack
        {
            get { return SoftwareVersionInstance.ServicePack; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return SoftwareVersionInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        //internal SoftwareVersion InternalkNoOwnership
        //{
        //    get { return Inventor.SoftwareVersion.kNoOwnership; }
        //}
        //internal SoftwareVersion InternalkSaveOwnership
        //{
        //    get { return Inventor.SoftwareVersion.kSaveOwnership; }
        //}
        //internal SoftwareVersion InternalkExclusiveOwnership
        //{
        //    get { return Inventor.SoftwareVersion.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvSoftwareVersion(InvSoftwareVersion invSoftwareVersion)
        {
            InternalSoftwareVersion = invSoftwareVersion.InternalSoftwareVersion;
        }

        private InvSoftwareVersion(Inventor.SoftwareVersion invSoftwareVersion)
        {
            InternalSoftwareVersion = invSoftwareVersion;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.SoftwareVersion SoftwareVersionInstance
        {
            get { return InternalSoftwareVersion; }
            set { InternalSoftwareVersion = value; }
        }

        public int _DebugBuildIdentifier
        {
            get { return Internal_DebugBuildIdentifier; }
        }

        public int _InternalBuildIdentifier
        {
            get { return Internal_InternalBuildIdentifier; }
        }

        public int _PatchBuildIdentifier
        {
            get { return Internal_PatchBuildIdentifier; }
        }

        public int BetaVersion
        {
            get { return InternalBetaVersion; }
        }

        public int BuildIdentifier
        {
            get { return InternalBuildIdentifier; }
        }

        public string DisplayName
        {
            get { return InternalDisplayName; }
        }

        public string DisplayVersion
        {
            get { return InternalDisplayVersion; }
        }

        public bool Is64BitVersion
        {
            get { return InternalIs64BitVersion; }
        }

        public bool IsEducationVersion
        {
            get { return InternalIsEducationVersion; }
        }

        public int Major
        {
            get { return InternalMajor; }
        }

        public int Minor
        {
            get { return InternalMinor; }
        }

        public bool NotProduction
        {
            get { return InternalNotProduction; }
        }

        //public InvProductEditionEnum ProductEdition
        //{
        //    get { return InternalProductEdition; }
        //}

        public string ProductName
        {
            get { return InternalProductName; }
        }

        public int ServicePack
        {
            get { return InternalServicePack; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        #endregion
        //public SoftwareVersion kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public SoftwareVersion kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public SoftwareVersion kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvSoftwareVersion ByInvSoftwareVersion(InvSoftwareVersion invSoftwareVersion)
        {
            return new InvSoftwareVersion(invSoftwareVersion);
        }
        public static InvSoftwareVersion ByInvSoftwareVersion(Inventor.SoftwareVersion invSoftwareVersion)
        {
            return new InvSoftwareVersion(invSoftwareVersion);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
