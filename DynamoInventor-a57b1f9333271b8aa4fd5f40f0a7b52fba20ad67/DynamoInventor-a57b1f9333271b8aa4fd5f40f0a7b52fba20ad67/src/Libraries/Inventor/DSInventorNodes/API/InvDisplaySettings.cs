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
    public class InvDisplaySettings
    {
        #region Internal properties
        internal Inventor.DisplaySettings InternalDisplaySettings { get; set; }

        internal Object InternalApplication
        {
            get { return DisplaySettingsInstance.Application; }
        }

        //internal InvGroundPlaneSettings InternalGroundPlaneSettings
        //{
        //    get { return InvGroundPlaneSettings.ByInvGroundPlaneSettings(DisplaySettingsInstance.GroundPlaneSettings); }
        //}


        internal Object InternalParent
        {
            get { return DisplaySettingsInstance.Parent; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return DisplaySettingsInstance.Type.As<InvObjectTypeEnum>(); }
        }

        internal bool InternalAreTexturesOn { get; set; }

        internal bool InternalDepthDimming { get; set; }

        internal bool InternalDisplaySilhouettes { get; set; }

        //Ambiguous
        //internal Inventor.Color InternalEdgeColor { get; set; }

        internal int InternalHiddenLineDimmingPercent { get; set; }

        internal DisplayModeEnum InternalNewWindowDisplayMode { get; set; }

        internal ProjectionTypeEnum InternalNewWindowProjectionType { get; set; }

        internal bool InternalNewWindowShowAmbientShadows { get; set; }

        internal bool InternalNewWindowShowGroundPlane { get; set; }

        internal bool InternalNewWindowShowGroundReflections { get; set; }

        internal bool InternalNewWindowShowGroundShadows { get; set; }

        internal bool InternalNewWindowShowObjectShadows { get; set; }

        internal RayTracingQualityEnum InternalRayTracingQuality { get; set; }

        internal bool InternalSolidLinesForHiddenEdges { get; set; }

        internal bool InternalUseRayTracingForRealisticDisplay { get; set; }
        #endregion

        #region Private constructors
        private InvDisplaySettings(InvDisplaySettings invDisplaySettings)
        {
            InternalDisplaySettings = invDisplaySettings.InternalDisplaySettings;
        }

        private InvDisplaySettings(Inventor.DisplaySettings invDisplaySettings)
        {
            InternalDisplaySettings = invDisplaySettings;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.DisplaySettings DisplaySettingsInstance
        {
            get { return InternalDisplaySettings; }
            set { InternalDisplaySettings = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        //public InvGroundPlaneSettings GroundPlaneSettings
        //{
        //    get { return InternalGroundPlaneSettings; }
        //}

        public Object Parent
        {
            get { return InternalParent; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public bool AreTexturesOn
        {
            get { return InternalAreTexturesOn; }
            set { InternalAreTexturesOn = value; }
        }

        public bool DepthDimming
        {
            get { return InternalDepthDimming; }
            set { InternalDepthDimming = value; }
        }

        public bool DisplaySilhouettes
        {
            get { return InternalDisplaySilhouettes; }
            set { InternalDisplaySilhouettes = value; }
        }

        //public Inventor.Color EdgeColor
        //{
        //    get { return InternalEdgeColor; }
        //    set { InternalEdgeColor = value; }
        //}

        public int HiddenLineDimmingPercent
        {
            get { return InternalHiddenLineDimmingPercent; }
            set { InternalHiddenLineDimmingPercent = value; }
        }

        //public InvDisplayModeEnum NewWindowDisplayMode
        //{
        //    get { return InternalNewWindowDisplayMode; }
        //    set { InternalNewWindowDisplayMode = value; }
        //}

        //public InvProjectionTypeEnum NewWindowProjectionType
        //{
        //    get { return InternalNewWindowProjectionType; }
        //    set { InternalNewWindowProjectionType = value; }
        //}

        public bool NewWindowShowAmbientShadows
        {
            get { return InternalNewWindowShowAmbientShadows; }
            set { InternalNewWindowShowAmbientShadows = value; }
        }

        public bool NewWindowShowGroundPlane
        {
            get { return InternalNewWindowShowGroundPlane; }
            set { InternalNewWindowShowGroundPlane = value; }
        }

        public bool NewWindowShowGroundReflections
        {
            get { return InternalNewWindowShowGroundReflections; }
            set { InternalNewWindowShowGroundReflections = value; }
        }

        public bool NewWindowShowGroundShadows
        {
            get { return InternalNewWindowShowGroundShadows; }
            set { InternalNewWindowShowGroundShadows = value; }
        }

        public bool NewWindowShowObjectShadows
        {
            get { return InternalNewWindowShowObjectShadows; }
            set { InternalNewWindowShowObjectShadows = value; }
        }

        //public InvRayTracingQualityEnum RayTracingQuality
        //{
        //    get { return InternalRayTracingQuality; }
        //    set { InternalRayTracingQuality = value; }
        //}

        public bool SolidLinesForHiddenEdges
        {
            get { return InternalSolidLinesForHiddenEdges; }
            set { InternalSolidLinesForHiddenEdges = value; }
        }

        public bool UseRayTracingForRealisticDisplay
        {
            get { return InternalUseRayTracingForRealisticDisplay; }
            set { InternalUseRayTracingForRealisticDisplay = value; }
        }

        #endregion

        #region Public static constructors
        public static InvDisplaySettings ByInvDisplaySettings(InvDisplaySettings invDisplaySettings)
        {
            return new InvDisplaySettings(invDisplaySettings);
        }
        public static InvDisplaySettings ByInvDisplaySettings(Inventor.DisplaySettings invDisplaySettings)
        {
            return new InvDisplaySettings(invDisplaySettings);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
