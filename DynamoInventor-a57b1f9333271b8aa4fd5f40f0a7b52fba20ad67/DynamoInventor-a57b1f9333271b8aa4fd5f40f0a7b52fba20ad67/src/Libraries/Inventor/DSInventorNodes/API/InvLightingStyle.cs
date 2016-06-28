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
    public class InvLightingStyle
    {
        #region Internal properties
        internal Inventor.LightingStyle InternalLightingStyle { get; set; }

        internal Object InternalApplication
        {
            get { return LightingStyleInstance.Application; }
        }

        internal InvAttributeSets InternalAttributeSets
        {
            get { return InvAttributeSets.ByInvAttributeSets(LightingStyleInstance.AttributeSets); }
        }

        internal string InternalInternalName
        {
            get { return LightingStyleInstance.InternalName; }
        }

        internal bool InternalInUse
        {
            get { return LightingStyleInstance.InUse; }
        }

        //internal InvLights InternalLights
        //{
        //    get { return InvLights.ByInvLights(LightingStyleInstance.Lights); }
        //}

        internal Object InternalParent
        {
            get { return LightingStyleInstance.Parent; }
        }

        //internal InvStyleLocationEnum InternalStyleLocation
        //{
        //    get { return InvStyleLocationEnum.ByInvStyleLocationEnum(LightingStyleInstance.StyleLocation); }
        //}

        internal InvObjectTypeEnum InternalType
        {
            get { return LightingStyleInstance.Type.As<InvObjectTypeEnum>(); }
        }

        internal bool InternalUpToDate
        {
            get { return LightingStyleInstance.UpToDate; }
        }

        internal double InternalAmbience { get; set; }

        internal double InternalAmbientShadowIntensity { get; set; }

        internal double InternalBrightness { get; set; }

        internal double InternalImageBasedLightingBrightness { get; set; }

        internal double InternalImageBasedLightingRotation { get; set; }

        internal double InternalImageBasedLightingScale { get; set; }

        internal bool InternalImageBasedLightingShowImage { get; set; }

        internal string InternalImageBasedLightingSourceInternalName { get; set; }

        internal string InternalImageBasedLightingSourceName { get; set; }

        internal string InternalName { get; set; }

        internal double InternalShadowDensity { get; set; }

        internal ShadowDirectionEnum InternalShadowDirection { get; set; }

        internal double InternalShadowSoftness { get; set; }
        #endregion

        //internal LightingStyle InternalkNoOwnership
        //{
        //    get { return Inventor.LightingStyle.kNoOwnership; }
        //}
        //internal LightingStyle InternalkSaveOwnership
        //{
        //    get { return Inventor.LightingStyle.kSaveOwnership; }
        //}
        //internal LightingStyle InternalkExclusiveOwnership
        //{
        //    get { return Inventor.LightingStyle.kExclusiveOwnership; }
        //}

        #region Private constructors
        private InvLightingStyle(InvLightingStyle invLightingStyle)
        {
            InternalLightingStyle = invLightingStyle.InternalLightingStyle;
        }

        private InvLightingStyle(Inventor.LightingStyle invLightingStyle)
        {
            InternalLightingStyle = invLightingStyle;
        }
        #endregion

        #region Private methods
        private LightingStyle InternalConvertToLocal()
        {
            return LightingStyleInstance.ConvertToLocal();
        }

        private LightingStyle InternalCopy(string newName)
        {
            return LightingStyleInstance.Copy( newName);
        }

        private void InternalDelete()
        {
            LightingStyleInstance.Delete();
        }

        private void InternalGetReferenceKey(ref byte[] referenceKey, int keyContext)
        {
            LightingStyleInstance.GetReferenceKey(ref  referenceKey,  keyContext);
        }

        private void InternalSaveToGlobal()
        {
            LightingStyleInstance.SaveToGlobal();
        }

        private void InternalUpdateFromGlobal()
        {
            LightingStyleInstance.UpdateFromGlobal();
        }

        #endregion

        #region Public properties
        public Inventor.LightingStyle LightingStyleInstance
        {
            get { return InternalLightingStyle; }
            set { InternalLightingStyle = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        public InvAttributeSets AttributeSets
        {
            get { return InternalAttributeSets; }
        }

        //public string InternalName
        //{
        //    get { return InternalInternalName; }
        //}

        public bool InUse
        {
            get { return InternalInUse; }
        }

        //public InvLights Lights
        //{
        //    get { return InternalLights; }
        //}

        public Object Parent
        {
            get { return InternalParent; }
        }

        //public InvStyleLocationEnum StyleLocation
        //{
        //    get { return InternalStyleLocation; }
        //}

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        public bool UpToDate
        {
            get { return InternalUpToDate; }
        }

        public double Ambience
        {
            get { return InternalAmbience; }
            set { InternalAmbience = value; }
        }

        public double AmbientShadowIntensity
        {
            get { return InternalAmbientShadowIntensity; }
            set { InternalAmbientShadowIntensity = value; }
        }

        public double Brightness
        {
            get { return InternalBrightness; }
            set { InternalBrightness = value; }
        }

        public double ImageBasedLightingBrightness
        {
            get { return InternalImageBasedLightingBrightness; }
            set { InternalImageBasedLightingBrightness = value; }
        }

        public double ImageBasedLightingRotation
        {
            get { return InternalImageBasedLightingRotation; }
            set { InternalImageBasedLightingRotation = value; }
        }

        public double ImageBasedLightingScale
        {
            get { return InternalImageBasedLightingScale; }
            set { InternalImageBasedLightingScale = value; }
        }

        public bool ImageBasedLightingShowImage
        {
            get { return InternalImageBasedLightingShowImage; }
            set { InternalImageBasedLightingShowImage = value; }
        }

        public string ImageBasedLightingSourceInternalName
        {
            get { return InternalImageBasedLightingSourceInternalName; }
            set { InternalImageBasedLightingSourceInternalName = value; }
        }

        public string ImageBasedLightingSourceName
        {
            get { return InternalImageBasedLightingSourceName; }
            set { InternalImageBasedLightingSourceName = value; }
        }

        public string Name
        {
            get { return InternalName; }
            set { InternalName = value; }
        }

        public double ShadowDensity
        {
            get { return InternalShadowDensity; }
            set { InternalShadowDensity = value; }
        }

        public ShadowDirectionEnum ShadowDirection
        {
            get { return InternalShadowDirection; }
            set { InternalShadowDirection = value; }
        }

        public double ShadowSoftness
        {
            get { return InternalShadowSoftness; }
            set { InternalShadowSoftness = value; }
        }

        #endregion
        //public LightingStyle kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public LightingStyle kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public LightingStyle kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvLightingStyle ByInvLightingStyle(InvLightingStyle invLightingStyle)
        {
            return new InvLightingStyle(invLightingStyle);
        }
        public static InvLightingStyle ByInvLightingStyle(Inventor.LightingStyle invLightingStyle)
        {
            return new InvLightingStyle(invLightingStyle);
        }
        #endregion

        #region Public methods
        public LightingStyle ConvertToLocal()
        {
            return InternalConvertToLocal();
        }

        public LightingStyle Copy(string newName)
        {
            return InternalCopy( newName);
        }

        public void Delete()
        {
            InternalDelete();
        }

        public void GetReferenceKey(ref byte[] referenceKey, int keyContext)
        {
            InternalGetReferenceKey(ref  referenceKey,  keyContext);
        }

        public void SaveToGlobal()
        {
            InternalSaveToGlobal();
        }

        public void UpdateFromGlobal()
        {
            InternalUpdateFromGlobal();
        }

        #endregion
    }
}
