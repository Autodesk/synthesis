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
    public class InvSketchSettings
    {
        #region Internal properties
        internal Inventor.SketchSettings InternalSketchSettings { get; set; }

        internal Object InternalApplication
        {
            get { return SketchSettingsInstance.Application; }
        }

        //internal Inv_Document InternalParent
        //{
        //    get { return Inv_Document.ByInv_Document(SketchSettingsInstance.Parent); }
        //}

        internal InvObjectTypeEnum InternalType
        {
            get { return SketchSettingsInstance.Type.As<InvObjectTypeEnum>(); }
        }

        internal bool InternalDisplayLineWeights { get; set; }

        internal LineWeightTypeEnum InternalLineWeightType { get; set; }

        internal int InternalMinorLinesPerMajorGridLine { get; set; }

        internal int InternalSnapsPerMinorGrid { get; set; }

        internal double InternalUpperLimitForFirstRangeOfLineWeights { get; set; }

        internal double InternalUpperLimitForSecondRangeOfLineWeights { get; set; }

        internal double InternalUpperLimitForThirdRangeOfLineWeights { get; set; }

        internal string InternalXSnapSpacing { get; set; }

        internal string InternalYSnapSpacing { get; set; }
        #endregion

        //internal SketchSettings InternalkNoOwnership
        //{
        //    get { return Inventor.SketchSettings.kNoOwnership; }
        //}
        //internal SketchSettings InternalkSaveOwnership
        //{
        //    get { return Inventor.SketchSettings.kSaveOwnership; }
        //}
        //internal SketchSettings InternalkExclusiveOwnership
        //{
        //    get { return Inventor.SketchSettings.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvSketchSettings(InvSketchSettings invSketchSettings)
        {
            InternalSketchSettings = invSketchSettings.InternalSketchSettings;
        }

        private InvSketchSettings(Inventor.SketchSettings invSketchSettings)
        {
            InternalSketchSettings = invSketchSettings;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.SketchSettings SketchSettingsInstance
        {
            get { return InternalSketchSettings; }
            set { InternalSketchSettings = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        //public Inv_Document Parent
        //{
        //    get { return InternalParent; }
        //}

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public bool DisplayLineWeights
        {
            get { return InternalDisplayLineWeights; }
            set { InternalDisplayLineWeights = value; }
        }

        //public InvLineWeightTypeEnum LineWeightType
        //{
        //    get { return InternalLineWeightType; }
        //    set { InternalLineWeightType = value; }
        //}

        public int MinorLinesPerMajorGridLine
        {
            get { return InternalMinorLinesPerMajorGridLine; }
            set { InternalMinorLinesPerMajorGridLine = value; }
        }

        public int SnapsPerMinorGrid
        {
            get { return InternalSnapsPerMinorGrid; }
            set { InternalSnapsPerMinorGrid = value; }
        }

        public double UpperLimitForFirstRangeOfLineWeights
        {
            get { return InternalUpperLimitForFirstRangeOfLineWeights; }
            set { InternalUpperLimitForFirstRangeOfLineWeights = value; }
        }

        public double UpperLimitForSecondRangeOfLineWeights
        {
            get { return InternalUpperLimitForSecondRangeOfLineWeights; }
            set { InternalUpperLimitForSecondRangeOfLineWeights = value; }
        }

        public double UpperLimitForThirdRangeOfLineWeights
        {
            get { return InternalUpperLimitForThirdRangeOfLineWeights; }
            set { InternalUpperLimitForThirdRangeOfLineWeights = value; }
        }

        public string XSnapSpacing
        {
            get { return InternalXSnapSpacing; }
            set { InternalXSnapSpacing = value; }
        }

        public string YSnapSpacing
        {
            get { return InternalYSnapSpacing; }
            set { InternalYSnapSpacing = value; }
        }

        #endregion
        //public SketchSettings kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public SketchSettings kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public SketchSettings kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}

        #region Public static constructors
        public static InvSketchSettings ByInvSketchSettings(InvSketchSettings invSketchSettings)
        {
            return new InvSketchSettings(invSketchSettings);
        }
        public static InvSketchSettings ByInvSketchSettings(Inventor.SketchSettings invSketchSettings)
        {
            return new InvSketchSettings(invSketchSettings);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
