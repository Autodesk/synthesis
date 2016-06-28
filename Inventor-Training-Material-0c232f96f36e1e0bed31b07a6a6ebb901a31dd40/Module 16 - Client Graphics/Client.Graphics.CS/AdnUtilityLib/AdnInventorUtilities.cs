////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved 
// Written by Philippe Leefsma 2011 - ADN/Developer Technical Services
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Text;
using Microsoft.Win32;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Inventor;

namespace Autodesk.ADN.Utility.InventorUtils
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    // Description: A general purpose Inventor API utility class
    //
    //////////////////////////////////////////////////////////////////////////////////////////////
    public class AdnInventorUtilities
    {
        private static double _Tolerance = 0.0001;

        public static string AddInGuid
        {
            get;
            internal set;
        }

        public static Inventor.Application InvApplication
        {
            get;
            internal set;
        }

        public static void Initialize(Inventor.Application Application, Type addinType)
        {
            InvApplication = Application;

            AddInGuid = addinType.GUID.ToString("B");
        }


        public static double[] ToArray(Point point)
        {
            return new double[] { point.X, point.Y, point.Z };
        }
         
        /////////////////////////////////////////////////////////////
        // Use: Late-binded method to retrieve ObjectType property
        //
        /////////////////////////////////////////////////////////////
        public static ObjectTypeEnum GetInventorType(Object obj)
        {
            try
            {
                System.Object objType = obj.GetType().InvokeMember(
                    "Type",
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    obj,
                    null,
                    null, null, null);

                return (ObjectTypeEnum)objType;
            }
            catch
            {
                //error... 
                return ObjectTypeEnum.kGenericObject;
            }
        }

        /////////////////////////////////////////////////////////////
        // Use: Late-binded method to get object property based
        //      on name.
        /////////////////////////////////////////////////////////////
        public static System.Object GetProperty(System.Object obj,
            string property)
        {
            try
            {
                System.Object objType = obj.GetType().InvokeMember(
                    property,
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    obj,
                    null,
                    null, null, null);

                return objType;
            }
            catch
            {
                return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Use: Returns ComponentDefinition for a part or assembly
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static ComponentDefinition GetCompDefinition(Document document)
        {
            switch (document.DocumentType)
            {
                case DocumentTypeEnum.kAssemblyDocumentObject:

                    AssemblyDocument asm = document as AssemblyDocument;
                    return asm.ComponentDefinition as ComponentDefinition;

                case DocumentTypeEnum.kPartDocumentObject:

                    PartDocument part = document as PartDocument;
                    return part.ComponentDefinition as ComponentDefinition;

                default:
                    return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Use: Returns Point object from input entity. Supports Vertex, WorkPoint
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static Point GetPoint(object entity)
        {
            ObjectTypeEnum type = GetInventorType(entity);

            switch (type)
            {
                case ObjectTypeEnum.kVertexObject:
                case ObjectTypeEnum.kVertexProxyObject:

                    Vertex vertex = entity as Vertex;
                    return vertex.Point;

                case ObjectTypeEnum.kWorkPointObject:
                case ObjectTypeEnum.kWorkPointProxyObject:

                    WorkPoint workpoint = entity as WorkPoint;
                    return workpoint.Point;

                default:
                    return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Use: Returns Plane object from input entity. Supports Face, Workplane and Faces.
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static Plane GetPlane(object planarEntity)
        {
            ObjectTypeEnum type = GetInventorType(planarEntity);

            switch(type)
            {
                case ObjectTypeEnum.kFaceObject:
                case ObjectTypeEnum.kFaceProxyObject:

                    Face face = planarEntity as Face;
                    return face.Geometry as Plane;

                case ObjectTypeEnum.kWorkPlaneObject:
                case ObjectTypeEnum.kWorkPlaneProxyObject:

                    WorkPlane workplane = planarEntity as WorkPlane;
                    return workplane.Plane;

                case ObjectTypeEnum.kFacesObject:

                    Face face1 = (planarEntity as Faces)[1];
                    return face1.Geometry as Plane;

                default:
                    return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Use: Returns Normal as UnitVetor for different type of input entities.
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static UnitVector GetNormal(object planarEntity)
        {
            ObjectTypeEnum type = GetInventorType(planarEntity);

            switch (type)
            {
                case ObjectTypeEnum.kFaceObject:
                case ObjectTypeEnum.kFaceProxyObject:

                    Face face = planarEntity as Face;
                    return GetFaceNormal(face);

                case ObjectTypeEnum.kWorkPlaneObject:
                case ObjectTypeEnum.kWorkPlaneProxyObject:

                    WorkPlane workplane = planarEntity as WorkPlane;
                    return workplane.Plane.Normal;

                case ObjectTypeEnum.kFacesObject:

                    Face face1 = (planarEntity as Faces)[1];
                    return GetFaceNormal(face1);

                default:
                    return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Use: Returns Normal as UnitVector for input Face.
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static UnitVector GetFaceNormal(Face face, Point point)
        {
            SurfaceEvaluator evaluator = face.Evaluator;

            double[] points = { point.X, point.Y, point.Z };

            double[] guessParams = new double[2];
            double[] maxDev = new double[2];
            double[] Params = new double[2];
            SolutionNatureEnum[] sol = new SolutionNatureEnum[2];

            evaluator.GetParamAtPoint(ref points, ref guessParams, ref maxDev, ref Params, ref sol);

            double[] normal = new double[3];

            evaluator.GetNormal(ref Params, ref normal);

            return InvApplication.TransientGeometry.CreateUnitVector(
                normal[0], normal[1], normal[2]);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Use: Returns Normal as UnitVector for input Face.
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static UnitVector GetFaceNormal(Face face)
        {
            SurfaceEvaluator evaluator = face.Evaluator;

            double[] points = { face.PointOnFace.X, face.PointOnFace.Y, face.PointOnFace.Z };

            double[] guessParams = new double[2];
            double[] maxDev = new double[2];
            double[] Params = new double[2];
            SolutionNatureEnum[] sol = new SolutionNatureEnum[2];

            evaluator.GetParamAtPoint(ref points, ref guessParams, ref maxDev, ref Params, ref sol);

            double[] normal = new double[3];

            evaluator.GetNormal(ref Params, ref normal);

            return InvApplication.TransientGeometry.CreateUnitVector(
                normal[0], normal[1], normal[2]);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Use: Returns an UnitVector orthogonal to input vector
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static UnitVector GetOrthoVector(UnitVector vector)
        {
            if (Math.Abs(vector.Z) < _Tolerance)
            {
                return InvApplication.TransientGeometry.CreateUnitVector(0, 0, 1);
            }
            else if (Math.Abs(vector.Y) < _Tolerance)
            {
                return InvApplication.TransientGeometry.CreateUnitVector(0, 1, 0);
            }
            else
            {
                //Expr: - xx'/y = y'
                return InvApplication.TransientGeometry.CreateUnitVector(1, -vector.X / vector.Y, 0);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Use: Returns two orthogonal vectors depending on the input normal
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void GetOrthoBase(UnitVector normal,
            out UnitVector xAxis,
            out UnitVector yAxis)
        {
            xAxis = GetOrthoVector(normal);

            yAxis = normal.CrossProduct(xAxis);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Use: Projects input point onto input plane and returns projected point
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static Point ProjectOnPlane(Point point, Plane plane)
        {
            try
            {
                MeasureTools measureTools = InvApplication.MeasureTools;

                double minDist;
                object contextObj = null;

                minDist = measureTools.GetMinimumDistance((object)point,
                    plane,
                    InferredTypeEnum.kNoInference,
                    InferredTypeEnum.kNoInference,
                    ref contextObj);

                NameValueMap context = contextObj as NameValueMap;

                Point projectedPoint = context.get_Item(context.Count < 3 ? 2 : 3) as Point;

                return projectedPoint;
            }
            catch
            {
                return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Use: Displays Open Dialog and returns filename selected by user
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string ShowOpenDialog(string title, string filter)
        {
            try
            {
                Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

                string pathInit = InvApp.DesignProjectManager.ActiveDesignProject.WorkspacePath;

                if (pathInit == string.Empty)
                {
                    pathInit = System.Environment.GetFolderPath(
                        System.Environment.SpecialFolder.Desktop);
                }

                System.IO.FileInfo fi = new System.IO.FileInfo(pathInit);

                Inventor.FileDialog fileDlg = null;
                InvApp.CreateFileDialog(out fileDlg);

                fileDlg.Filter = filter;
                fileDlg.FilterIndex = 1;
                fileDlg.DialogTitle = title;
                fileDlg.InitialDirectory = fi.DirectoryName;
                fileDlg.FileName = "";
                fileDlg.MultiSelectEnabled = false;
                fileDlg.OptionsEnabled = false;
                fileDlg.CancelError = true;
                fileDlg.SuppressResolutionWarnings = true;

                fileDlg.ShowOpen();

                return fileDlg.FileName;
            }
            catch
            {
                return string.Empty;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Use: Read attribute and returns its value in out parameter. 
        //      Returns true if attribute exists, false otherwise
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool ReadAttribute(object target,
            string setName,
            string attName,
            out object value,
            out ValueTypeEnum type)
        {
            value = null;
            type = ValueTypeEnum.kIntegerType;

            try
            {
                AttributeSets sets = AdnInventorUtilities.GetProperty(target, "AttributeSets") as AttributeSets;

                if (sets == null)
                    return false;

                if (!sets.get_NameIsUsed(setName))
                    return false;

                AttributeSet set = sets[setName];

                if (!set.get_NameIsUsed(attName))
                    return false;

                Inventor.Attribute att = set[attName];

                type = att.ValueType;

                value = att.Value;

                return true;
            }
            catch
            {
                return false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Use: Returns parameter value as string
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string GetStringFromValue(Parameter parameter)
        {
            try
            {
                if (parameter.Value is string || parameter.Value is bool)
                    return parameter.Value.ToString();

                return InvApplication.UnitsOfMeasure.GetStringFromValue(parameter.ModelValue, parameter.get_Units());
            }
            catch
            {
                return "*Error*";
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Use: Return string from API value
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string GetStringFromAPILength(double value)
        {
            try
            {
                UnitsOfMeasure uom = InvApplication.ActiveDocument.UnitsOfMeasure;

                string strValue = uom.GetStringFromValue(value, UnitsTypeEnum.kDefaultDisplayLengthUnits);

                return strValue;
            }
            catch
            {
                return "*Error*";
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Use: Create a new derived PartDocument from a ComponentDefinition (asm or part)
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static PartDocument DeriveComponent(ComponentDefinition compDef)
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            PartDocument derivedDoc = InvApp.Documents.Add(
                    DocumentTypeEnum.kPartDocumentObject,
                    InvApp.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject,
                       SystemOfMeasureEnum.kDefaultSystemOfMeasure,
                       DraftingStandardEnum.kDefault_DraftingStandard,
                       null),
                   false) as PartDocument;

            if (compDef.Type == ObjectTypeEnum.kAssemblyComponentDefinitionObject)
            {
                DerivedAssemblyComponents derAsmComps =
                    derivedDoc.ComponentDefinition.ReferenceComponents.DerivedAssemblyComponents;

                DerivedAssemblyDefinition derAsmDef = derAsmComps.CreateDefinition(
                        (compDef.Document as Document).FullFileName);

                derAsmDef.InclusionOption = DerivedComponentOptionEnum.kDerivedIncludeAll;

                derAsmComps.Add(derAsmDef);

                return derivedDoc;
            }

            if (compDef.Type == ObjectTypeEnum.kPartComponentDefinitionObject)
            {
                DerivedPartComponents derPartComps =
                    derivedDoc.ComponentDefinition.ReferenceComponents.DerivedPartComponents;

                DerivedPartUniformScaleDef derPartDef = derPartComps.CreateDefinition(
                       (compDef.Document as Document).FullFileName);

                derPartDef.IncludeAll();

                derPartComps.Add(derPartDef as DerivedPartDefinition);

                return derivedDoc;
            }

            derivedDoc.Close(true);

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Use: Returns a collection of transient bodies transformed in the context of the assembly 
        //      (if compDef is an assembly CompDef).
        //      The Key of the dictionary is the original body, the value is the transformed transient body
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static Dictionary<SurfaceBody, SurfaceBody> GetTransientBodies(ComponentDefinition compDef)
        {
            Dictionary<SurfaceBody, SurfaceBody> bodies = new Dictionary<SurfaceBody, SurfaceBody>();

            if (compDef.Type == ObjectTypeEnum.kAssemblyComponentDefinitionObject)
            {
                foreach (ComponentOccurrence occurrence in compDef.Occurrences)
                {
                    if (occurrence.DefinitionDocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
                    {
                        Dictionary<SurfaceBody, SurfaceBody> bodiesRec =
                            GetTransientBodies(occurrence.Definition);

                        foreach (SurfaceBody key in bodiesRec.Keys)
                        {
                            SurfaceBody bodyCpy = AdnInventorUtilities.InvApplication.TransientBRep.Copy(bodiesRec[key]);

                            AdnInventorUtilities.InvApplication.TransientBRep.Transform(
                                bodyCpy,
                                occurrence.Transformation);

                            bodies.Add(key, bodyCpy);
                        }
                    }
                    else
                    {
                        foreach (SurfaceBody body in occurrence.SurfaceBodies)
                        {
                            SurfaceBody bodyCpy = AdnInventorUtilities.InvApplication.TransientBRep.Copy(body);

                            AdnInventorUtilities.InvApplication.TransientBRep.Transform(
                                bodyCpy,
                                occurrence.Transformation);

                            bodies.Add(body, bodyCpy);
                        }
                    }
                }
            }
            else
            {
                foreach (SurfaceBody body in compDef.SurfaceBodies)
                {
                    SurfaceBody bodyCpy =
                        AdnInventorUtilities.InvApplication.TransientBRep.Copy(body);

                    bodies.Add(body, bodyCpy);
                }
            }

            return bodies;
        }
    }

    public enum SupportedSoftwareVersionEnum
    {
        kSupportedSoftwareVersionLessThan,
        kSupportedSoftwareVersionGreaterThan,
        kSupportedSoftwareVersionEqualTo,
        kSupportedSoftwareVersionNotEqualTo
    }

    public class AdnAddInRegistration
    {
        private class RegInfo
        {
            #region Fields

            private string _addinGuid;
            private string _description;
            private string _title;

            #endregion

            public RegInfo(Type t)
            {
                _addinGuid = t.GUID.ToString("B");
                object[] attrs = t.Assembly.GetCustomAttributes(false);

                _title = string.Empty;
                _description = string.Empty;

                foreach (object attr in attrs)
                {
                    AssemblyTitleAttribute titleAttr = attr as AssemblyTitleAttribute;

                    if (null != titleAttr)
                    {
                        _title = titleAttr.Title;
                    }
                    AssemblyDescriptionAttribute descriptionAttr = attr as AssemblyDescriptionAttribute;

                    if (null != descriptionAttr)
                    {
                        _description = descriptionAttr.Description;
                    }
                }
            }

            public string AddInGuid
            {
                get { return _addinGuid; }
            }

            public string Description
            {
                get { return _description; }
            }

            public string Title
            {
                get { return _title; }
            }
        }

        public static void RegisterAddIn(
            Type t,
            SupportedSoftwareVersionEnum supportedSoftwareVersion,
            string softwareVersion,
            bool loadOnStartup,
            string addinVersion)
        {
            RegInfo info = new RegInfo(t);

            using (RegistryKey keyClsid = Registry.ClassesRoot.CreateSubKey(@"CLSID\" + info.AddInGuid))
            {
                keyClsid.SetValue(null, info.Title);
                keyClsid.CreateSubKey(@"Implemented Categories\{39AD2B5C-7A29-11D6-8E0A-0010B541CAA8}");

                using (RegistryKey keySettings = keyClsid.CreateSubKey("Settings"))
                {
                    keySettings.SetValue("AddInType", "Standard");
                    keySettings.SetValue("LoadOnStartup", (loadOnStartup ? "1" : "0"));

                    switch (supportedSoftwareVersion)
                    {
                        case SupportedSoftwareVersionEnum.kSupportedSoftwareVersionEqualTo:
                            keySettings.SetValue("SupportedSoftwareVersionEqualTo", softwareVersion);
                            break;
                        case SupportedSoftwareVersionEnum.kSupportedSoftwareVersionGreaterThan:
                            keySettings.SetValue("SupportedSoftwareVersionGreaterThan", softwareVersion);
                            break;
                        case SupportedSoftwareVersionEnum.kSupportedSoftwareVersionLessThan:
                            keySettings.SetValue("SupportedSoftwareVersionLessThan", softwareVersion);
                            break;
                        case SupportedSoftwareVersionEnum.kSupportedSoftwareVersionNotEqualTo:
                            keySettings.SetValue("SupportedSoftwareVersionNotEqualTo", softwareVersion);
                            break;
                        default:
                            keySettings.SetValue("SupportedSoftwareVersionGreaterThan", softwareVersion);
                            break;
                    }

                    keySettings.SetValue("Version", addinVersion);
                }

                using (RegistryKey keyDescription = keyClsid.CreateSubKey("Description"))
                {
                    keyDescription.SetValue(null, info.Description);
                }
            }
        }

        public static void UnregisterAddIn(Type t)
        {
            RegInfo info = new RegInfo(t);

            using (RegistryKey keyClsid = Registry.ClassesRoot.OpenSubKey(@"CLSID\" + info.AddInGuid, true))
            {
                keyClsid.DeleteSubKey(@"Implemented Categories\{39AD2B5C-7A29-11D6-8E0A-0010B541CAA8}");
                keyClsid.DeleteSubKey("Settings");
                keyClsid.DeleteSubKey("Description");
            }
        }
    }

    // An extra utility class for iLogic
    // 
    // Requires extra reference to "Autodesk.iLogic.Interfaces.dll"

    //class iLogicUtilities
    //{
    //    private static string iLogicAddinGuid = "{3BDD8D79-2179-4B11-8A5A-257B1C0263AC}";

    //    private static IiLogicAutomation _iLogicAutomation = null;

    //    public static IiLogicAutomation GetiLogicAutomation()
    //    {
    //        try
    //        {
    //            if (_iLogicAutomation == null)
    //            {
    //                Inventor.ApplicationAddIn addin =
    //                    AdnInventorUtilities.InventorApplication.ApplicationAddIns.get_ItemById(iLogicAddinGuid);

    //                if (addin.Activated == false)
    //                    addin.Activate();

    //                _iLogicAutomation = (IiLogicAutomation)addin.Automation;
    //            }

    //            return _iLogicAutomation;
    //        }
    //        catch
    //        {
    //            return null;
    //        }
    //    }
    //}
}
