using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inventor;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Dynamo.Models;
using Dynamo.Utilities;
using InventorLibrary.API;
using InventorLibrary.GeometryConversion;

using InventorServices.Persistence;
using InventorServices.Utilities;
using Point = Autodesk.DesignScript.Geometry.Point;
using Application = Autodesk.DesignScript.Geometry.Application;

namespace InventorLibrary.ModulePlacement
{
    [IsVisibleInDynamoLibrary(false)]
    public class Module
    {
        #region Private fields
        private bool? firstTime = null;
        private TupleList<string, string> filePathPair = new TupleList<string, string>();
        private bool reuseDuplicates = true;
        private List<ComponentOccurrence> topOccurrences = new List<ComponentOccurrence>();
        private List<WorkPointProxy> layoutWorkPointProxies = new List<WorkPointProxy>();
        private List<WorkPoint> layoutWorkPoints = new List<WorkPoint>();
        private List<WorkPointProxy> targetWorkPointProxies = new List<WorkPointProxy>();
        private List<WorkPoint> targetWorkPoints = new List<WorkPoint>();
        private Matrix transfomationMatrix = PersistenceManager.InventorApplication.TransientGeometry.CreateMatrix();
        private List<IBindableObject> bindablePointObjects = new List<IBindableObject>();
        private List<IBindableObject> bindablePlaneObjects = new List<IBindableObject>();
        private string modulePath;
        #endregion

        #region Private constructors
        private Module(List<Point> points)
        {
            //InternalModulePoints = points;
            List<Point> tempScaledPointsList = new List<Point>();
            foreach (Point point in points)
            {
                double newX = point.X * 30.48;
                double newY = point.Y * 30.48;
                double newZ = point.Z * 30.48;
                Point newPoint = Point.ByCoordinates(newX, newY, newZ);
                tempScaledPointsList.Add(newPoint);
            }
            InternalModulePoints = tempScaledPointsList;
        }
        #endregion

        #region Internal methods
        internal void PlaceWorkGeometryForContsraints(PartComponentDefinition layoutComponentDefinition, ComponentOccurrence layoutOccurrence, int moduleNumber)
        {
            PartDocument partDoc = (PartDocument)layoutComponentDefinition.Document;
            ReferenceKeyManager refKeyManager = partDoc.ReferenceKeyManager;
            
            //TODO: Property ModuleId can be factored out and _binder.ContextData can be used instead.
            ModuleId = moduleNumber;

            for (int i = 0; i < PointObjects.Count; i++)
            {
                //Each ModuleObject needs to have its ContextManager set.
                PointObjects[i].Binder.ContextManager.BindingContextManager = refKeyManager;
                WorkPoint workPoint;
                if (PointObjects[i].Binder.GetObjectFromTrace<WorkPoint>(out workPoint))
                {
                    Inventor.Point newLocation = PersistenceManager.InventorApplication.TransientGeometry.CreatePoint(InternalModulePoints[i].X,
                                                                                                                              InternalModulePoints[i].Y,
                                                                                                                              InternalModulePoints[i].Z);

                    workPoint.SetFixed(InternalModulePoints[i].ToPoint());
                }

                else
                {
                    workPoint = layoutComponentDefinition.WorkPoints.AddFixed(InternalModulePoints[i].ToPoint(), false);
                    PointObjects[i].Binder.SetObjectForTrace<WorkPoint>(workPoint, ModuleUtilities.ReferenceKeysSorter);
                }

                //workPoint.Visible = false;

                object workPointProxyObject;
                layoutOccurrence.CreateGeometryProxy(workPoint, out workPointProxyObject);
                LayoutWorkPointProxies.Add((WorkPointProxy)workPointProxyObject);
                LayoutWorkPoints.Add(workPoint);
            }

            //If we will have more than 2 constraints, it will help assembly stability later
            //if we have a plane to constrain to first.
            if (InternalModulePoints.Count > 2)
            {
                
                WorkPlane workPlane;
                //TODO: Is this a good idea? Why is this a list? Will we ever have more
                //than work plane?  
                PlaneObjects[0].Binder.ContextManager.BindingContextManager = refKeyManager;
                if (PlaneObjects[0].Binder.GetObjectFromTrace<WorkPlane>(out workPlane))
                {
                    if (workPlane.DefinitionType == WorkPlaneDefinitionEnum.kThreePointsWorkPlane)
                    {
                        workPlane.SetByThreePoints(LayoutWorkPoints[0], LayoutWorkPoints[1], LayoutWorkPoints[2]);
                        LayoutWorkPlane = workPlane;
                        object wPlaneProxyObject;
                        layoutOccurrence.CreateGeometryProxy(workPlane, out wPlaneProxyObject);
                        LayoutWorkPlaneProxy = (WorkPlaneProxy)wPlaneProxyObject;
                    }
                }
                else
                {
                    //If the first three points are colinear, adding a workplane will fail.  We will check the area of a triangle 
                    //described by the first three points. If the area is very close to 0, we can assume these points are colinear, and we should
                    //not attempt to construct a work plane from them.
                    Inventor.Point pt1 = LayoutWorkPoints[0].Point;
                    Inventor.Point pt2 = LayoutWorkPoints[1].Point;
                    Inventor.Point pt3 = LayoutWorkPoints[2].Point;
                    if (Math.Abs(pt1.X * (pt2.Y - pt3.Y) + pt2.X * (pt3.Y - pt1.Y) + pt3.X * (pt1.Y - pt2.Y)) > .0000001)
                    {
                        workPlane = layoutComponentDefinition.WorkPlanes.AddByThreePoints(LayoutWorkPoints[0], LayoutWorkPoints[1], LayoutWorkPoints[2], false);
                        PlaneObjects[0].Binder.SetObjectForTrace<WorkPlane>(workPlane, ModuleUtilities.ReferenceKeysSorter);
                        workPlane.Grounded = true;
                        //workPlane.Visible = false;
                        LayoutWorkPlane = workPlane;
                        object wPlaneProxyObject;
                        layoutOccurrence.CreateGeometryProxy(workPlane, out wPlaneProxyObject);
                        LayoutWorkPlaneProxy = (WorkPlaneProxy)wPlaneProxyObject;
                    }
                }
            }
        }

        internal void MakeInvCopy(string templateAssemblyPath,
                         string targetDirectory,
                         OccurrenceList occurrenceList,
                         UniqueModuleEvaluator uniqueModuleEvaluator)
        {
            // TODO Test for the existance of folders and assemblies.
            TemplateAssemblyPath = templateAssemblyPath;        
            UniqueModules = uniqueModuleEvaluator;

            
            //Get the folder name that will be used to store the files associated with this Module.
            string folderName = GetModuleFolderPath();

            //Need to get number of the parent occ, top level name as foldername
            ModulePath = System.IO.Path.Combine(targetDirectory, folderName);

            string topFileFullName = occurrenceList.TargetAssembly.FullDocumentName;
            string topFileNameOnly = System.IO.Path.GetFileName(topFileFullName);
            ModuleAssemblyPath = System.IO.Path.Combine(ModulePath, topFileNameOnly);

            //If this file already exists in the current location, for now we are
            //going to just skip the file creation, and assume it was previously done
            //correctly.  Probably need to give the user the option to redo and 
            //overwrite files if they want to.
            if (!System.IO.File.Exists(ModuleAssemblyPath))
            {
                FilePathPair = new TupleList<string, string>();

                for (int i = 0; i < occurrenceList.Items.Count; i++)
                {
                    string targetOccPath = occurrenceList.Items[i].ReferencedFileDescriptor.FullFileName;
                    string newCopyName = System.IO.Path.GetFileName(targetOccPath);
                    string newFullCopyName = System.IO.Path.Combine(ModulePath, newCopyName);
                    FilePathPair.Add(targetOccPath, newFullCopyName);
                }

                //Check if an earlier module already made the folder, if not, create it.
                if (!System.IO.Directory.Exists(ModulePath))
                {
                    //This property is needed later when placing occurrences of the assembly this Module instance 
                    //refers to.  If FirstTime is false, we will want to have a slightly different strategry for constraint
                    //placement.  If FirstTime is true, all constraints are important and we need not relax them.  If 
                    //FirstTime is false, then we need tolerance in the constraints because of double precision.  When
                    //FirstTime is false, we really just want to position the occurrence correctly, not drive its 
                    //geometry.

                    if (FirstTime == null)
                    {
                        FirstTime = true;
                    }

                    System.IO.Directory.CreateDirectory(ModulePath);
                    ReplaceReferences(occurrenceList.TargetAssembly, FilePathPair, ModulePath);
                    AssemblyDocument oAssDoc = (AssemblyDocument)PersistenceManager.InventorApplication.Documents.Open(TemplateAssemblyPath, false);
                    oAssDoc.SaveAs(ModuleAssemblyPath, true);
                    oAssDoc.Close(true);


                    //Need to copy presentation files if there are any.  For now this is only going to work with the top assembly.
                    string templateDirectory = System.IO.Path.GetDirectoryName(TemplateAssemblyPath);
                    string[] presentationFiles = System.IO.Directory.GetFiles(templateDirectory, "*.ipn");
                    //If we want the ability to have subassemblies with .ipn files or multiple ones, this will have to be changed
                    //to iterate over all the .ipn files.
                    if (presentationFiles.Length != 0)
                    {
                        string newCopyPresName = System.IO.Path.GetFileName(presentationFiles[0]);
                        string newFullCopyPresName = System.IO.Path.Combine(ModulePath, newCopyPresName);
                        PresentationDocument presentationDocument = (PresentationDocument)PersistenceManager.InventorApplication.Documents.Open(presentationFiles[0], false);
                        DocumentDescriptorsEnumerator presFileDescriptors = presentationDocument.ReferencedDocumentDescriptors;
                        foreach (DocumentDescriptor refPresDocDescriptor in presFileDescriptors)
                        {
                            if (refPresDocDescriptor.FullDocumentName == TemplateAssemblyPath)
                            {
                                refPresDocDescriptor.ReferencedFileDescriptor.ReplaceReference(ModuleAssemblyPath);
                                presentationDocument.SaveAs(newFullCopyPresName, true);
                                presentationDocument.Close(true);
                            }
                        }
                    }
                }

                else
                {
                    FirstTime = false;
                }
            }
        }

        internal void GenerateDrawings(string templateDrawingPath)
        {
            TemplateDrawingPath = templateDrawingPath;
            if (FirstTime == true && TemplateDrawingPath != null && TemplateDrawingPath != "")
            {
                string newCopyDrawingName = System.IO.Path.GetFileName(TemplateDrawingPath);
                string newFullCopyDrawingName = System.IO.Path.Combine(ModulePath, newCopyDrawingName);
                if (!System.IO.File.Exists(newFullCopyDrawingName))
                {
                    DrawingDocument drawingDoc = (DrawingDocument)PersistenceManager.InventorApplication.Documents.Open(TemplateDrawingPath, false);
                    DocumentDescriptorsEnumerator drawingFileDescriptors = drawingDoc.ReferencedDocumentDescriptors;
                    //This needs to be fixed.  It was written with the assumption that only the template assembly would be in 
                    //the details and be first in the collection of document descriptors.  This was a safe assumption when
                    //I was the only user of this code. Need to iterate through drawingFileDescriptors and match names 
                    //and replace correct references.  Possibly can use the "filePathPair" object for name 
                    //matching/reference replacing.
                    //drawingFileDescriptors[1].ReferencedFileDescriptor.ReplaceReference(topAssemblyNewLocation);
                    foreach (DocumentDescriptor refDocDescriptor in drawingFileDescriptors)
                    {
                        foreach (Tuple<string, string> pathPair in FilePathPair)
                        {
                            string newFileNameLower = System.IO.Path.GetFileName(pathPair.Item2);
                            string drawingReferenceLower = System.IO.Path.GetFileName(refDocDescriptor.FullDocumentName);
                            string topAssemblyLower = System.IO.Path.GetFileName(ModuleAssemblyPath);
                            if (topAssemblyLower == drawingReferenceLower)
                            {
                                refDocDescriptor.ReferencedFileDescriptor.ReplaceReference(ModuleAssemblyPath);
                            }
                            if (newFileNameLower == drawingReferenceLower)
                            {
                                refDocDescriptor.ReferencedFileDescriptor.ReplaceReference(pathPair.Item2);
                            }
                        }
                    }

                    drawingDoc.SaveAs(newFullCopyDrawingName, false);
                    drawingDoc.Close(true);

                    if (!UniqueModules.DetailDocumentPaths.Contains(newFullCopyDrawingName))
                    {
                        UniqueModules.DetailDocumentPaths.Add(newFullCopyDrawingName);
                    }
                }
            }
        }

        private string GetModuleFolderPath()
        {
            string folderName;
            string geoMapString = System.Convert.ToString(GeometryMapIndex);
            if (ReuseDuplicates == true)
            {
                if (GeometryMapIndex < 10)
                {
                    folderName = System.IO.Path.GetFileNameWithoutExtension(TemplateAssemblyPath) + " 00" + geoMapString;
                }

                else if (10 <= GeometryMapIndex && GeometryMapIndex < 100)
                {
                    folderName = System.IO.Path.GetFileNameWithoutExtension(TemplateAssemblyPath) + " 0" + geoMapString;
                }
                else
                {
                    folderName = System.IO.Path.GetFileNameWithoutExtension(TemplateAssemblyPath) + " " + geoMapString;
                }
            }

            else
            {
                string moduleIdString = System.Convert.ToString(ModuleId);
                if (ModuleId < 10)
                {
                    folderName = System.IO.Path.GetFileNameWithoutExtension(TemplateAssemblyPath) + " 00" + moduleIdString;
                }
                else if (10 <= ModuleId && ModuleId < 100)
                {
                    folderName = System.IO.Path.GetFileNameWithoutExtension(TemplateAssemblyPath) + " 0" + moduleIdString;
                }
                else
                {
                    folderName = System.IO.Path.GetFileNameWithoutExtension(TemplateAssemblyPath) + " " + moduleIdString;
                }
            }
            return folderName;
        }

        internal void PlaceModule()
        {
            ComponentOccurrence topOccurrence;
            if(AssemblyOccurrenceObject.Binder.GetObjectFromTrace<ComponentOccurrence>(out topOccurrence))
            {
                topOccurrence.Adaptive = true;
                PersistenceManager.ActiveAssemblyDoc.Update2(true);
                topOccurrence.Adaptive = false;
                if (topOccurrence.DefinitionDocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
                {
                    var doc = (AssemblyDocument)topOccurrence.Definition.Document;
                    doc.Save2();
                }
                else if (topOccurrence.DefinitionDocumentType == DocumentTypeEnum.kPartDocumentObject)
                {
                    var doc = (PartDocument)topOccurrence.Definition.Document;
                    doc.Save2();
                }
            }

            else
            {
                AssemblyComponentDefinition assemblyCompDef = PersistenceManager.ActiveAssemblyDoc.ComponentDefinition;
                topOccurrence = assemblyCompDef.Occurrences.Add(ModuleAssemblyPath, TransformationMatrix);
                ComponentOccurrencesEnumerator topOccSubs = topOccurrence.SubOccurrences;
                int topOccSubsCount = topOccSubs.Count;
                ComponentOccurrence layoutOcc = null;
                ComponentOccurrence frameOcc = null;
                for (int i = 0; i < topOccSubsCount; i++)
                {
                    ComponentOccurrence currentSub = topOccSubs[i + 1];
                    //This is by convention.  Possible better alternative is to have this be 
                    //explicitly set in the user interface.
                    if (currentSub.Name == "Template Layout:1")
                    {
                        layoutOcc = currentSub;
                        LayoutComponentDefinition = (PartComponentDefinition)layoutOcc.Definition;
                    }

                    //What did I do this for? I think for old flea flicker parameter updating.
                    if (currentSub.Name == "Frame0001:1")
                    {
                        frameOcc = currentSub;
                        FrameComponentDefinition = (AssemblyComponentDefinition)frameOcc.Definition;
                    }

                    else
                    {
                        TopOccurrences.Add(currentSub);
                    }
                }
                int workPointCount = LayoutComponentDefinition.WorkPoints.Count;
                for (int j = 0; j < workPointCount; j++)

                //TODO  Need to put logic in that test a layout file for derivedparametertables collection.Count != 0
                //then do the copy of the layout file, get the layout file and swap out the document descriptor IN APPRENTICE.
                {
                    WorkPoint currentWP;
                    currentWP = LayoutComponentDefinition.WorkPoints[j + 1];
                    TargetWorkPoints.Add(currentWP);
                    object currentWPProxyObject;
                    WorkPointProxy currentWPProxy;
                    layoutOcc.CreateGeometryProxy(currentWP, out currentWPProxyObject);
                    currentWPProxy = (WorkPointProxy)currentWPProxyObject;
                    TargetWorkPointProxies.Add(currentWPProxy);
                }
                //TODO Fix this to be more intellegent.  What if assembly had two planes (rooms etc.).
                WorkPlane targetWorkPlane;
                if (LayoutComponentDefinition.WorkPlanes.Count > 3)
                {
                    targetWorkPlane = (WorkPlane)LayoutComponentDefinition.WorkPlanes[4];
                    object wPlaneProxyObject;
                    layoutOcc.CreateGeometryProxy(targetWorkPlane, out wPlaneProxyObject);
                    TargetWorkPlaneProxy = (WorkPlaneProxy)wPlaneProxyObject;
                    targetWorkPlane.Visible = false;
                }
                //Workplane constraints needed or not?
                //Make sure the target assembly's layout is adaptive.
                layoutOcc.Adaptive = true;
                PersistenceManager.ActiveAssemblyDoc.ComponentDefinition.Constraints.AddMateConstraint(TargetWorkPlaneProxy, LayoutWorkPlaneProxy, 0);

                if (FirstTime == true)
                {
                    for (int f = 0; f < PointObjects.Count; f++)
                    {
                        //TODO this is uncertain.  It changes from test to test, need to get better handle on the indexing of points.
                        PersistenceManager.ActiveAssemblyDoc
                                          .ComponentDefinition
                                          .Constraints
                                          .AddMateConstraint(TargetWorkPointProxies[f + 1], LayoutWorkPointProxies[f], 0);
                    }

                    topOccurrence.Adaptive = true;
                    PersistenceManager.ActiveAssemblyDoc.Update2(true);
                    topOccurrence.Adaptive = false;
                    //layoutOcc.Visible = false;
                }

                else
                {
                    for (int f = 0; f < PointObjects.Count; f++)
                    {
                        //TODO this is uncertain.  It changes from test to test, need to get better handle on the indexing of points.
                        MateConstraint mateConstraint = PersistenceManager.ActiveAssemblyDoc
                                                                          .ComponentDefinition
                                                                          .Constraints
                                                                          .AddMateConstraint(TargetWorkPointProxies[f + 1], LayoutWorkPointProxies[f], 0);
                        if (f > 0)
                        {
                            //These mate constraints will fail out in space because of double accuracy issues unless they are relaxed some.
                            mateConstraint.ConstraintLimits.MaximumEnabled = true;
                            mateConstraint.ConstraintLimits.Maximum.Expression = ".5 in";
                        }
                    }
                    //layoutOcc.Visible = false;
                }                
                AssemblyOccurrenceObject.Binder.SetObjectForTrace<ComponentOccurrence>(topOccurrence, ModuleUtilities.ReferenceKeysSorter);
                if (topOccurrence.DefinitionDocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
                {
                    var doc = (AssemblyDocument)topOccurrence.Definition.Document;
                    doc.Save2();
                }
                else if (topOccurrence.DefinitionDocumentType == DocumentTypeEnum.kPartDocumentObject)
                {
                    var doc = (PartDocument)topOccurrence.Definition.Document;
                    doc.Save2();
                }
            }
        }

        public void ReplaceReferences(AssemblyDocument targetAssembly, TupleList<string, string> namePair, string folderPath)
        {
            OccurrenceList newOccs = new OccurrenceList(targetAssembly);
            string pathString = folderPath;
            List<string> patternComponentsList = new List<string>();
            for (int i = 0; i < newOccs.Items.Count; i++)
            {
                if (newOccs.Items[i].DefinitionDocumentType == DocumentTypeEnum.kPartDocumentObject)
                {
                    for (int f = 0; f < namePair.Count; f++)
                    {
                        if (namePair[f].Item1 == newOccs.Items[i].ReferencedFileDescriptor.FullFileName)
                        {
                            if (patternComponentsList.Contains(namePair[f].Item1))
                            {
                                newOccs.Items[i].ReferencedDocumentDescriptor.ReferencedFileDescriptor.ReplaceReference(namePair[f].Item2);
                            }

                            else
                            {
                                if (!System.IO.File.Exists(namePair[f].Item2))
                                {
                                    PartDocument partDoc = (PartDocument)PersistenceManager.InventorApplication.Documents.Open(namePair[f].Item1, false);
                                    partDoc.SaveAs(namePair[f].Item2, true);
                                    partDoc.Close(true);
                                    newOccs.Items[i].ReferencedDocumentDescriptor.ReferencedFileDescriptor.ReplaceReference(namePair[f].Item2);
                                }
                                patternComponentsList.Add(namePair[f].Item1);
                            }
                        }
                    }
                }

                else if (newOccs.Items[i].DefinitionDocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
                {
                    for (int n = 0; n < namePair.Count; n++)
                    {
                        if (namePair[n].Item1 == newOccs.Items[i].ReferencedFileDescriptor.FullFileName)
                        {
                            AssemblyDocument subAssembly = (AssemblyDocument)PersistenceManager.InventorApplication.Documents.Open(newOccs.Items[i].ReferencedFileDescriptor.FullFileName,false);
                            ReplaceReferences(subAssembly, namePair, pathString);
                            string newFilePath = namePair[n].Item2;
                            subAssembly.SaveAs(namePair[n].Item2, true);
                            subAssembly.Close(true);
                            newOccs.Items[i].ReferencedDocumentDescriptor.ReferencedFileDescriptor.ReplaceReference(namePair[n].Item2);
                        }
                    }
                }
            }
        }

        #endregion

        #region Internal properties
        internal IBindableObject AssemblyOccurrenceObject { get; set; }

        internal TupleList<string, string> FilePathPair
        {
            get { return filePathPair; }
            set { filePathPair = value; }
        }

        internal bool? FirstTime
        {
            get { return firstTime; }
            set { firstTime = value; }
        }

        internal AssemblyComponentDefinition FrameComponentDefinition { get; set; }

        internal int GeometryMapIndex { get; set; }

        internal List<Point> InternalModulePoints { get; set; }

        internal PartComponentDefinition LayoutComponentDefinition { get; set; }

        internal WorkPlaneProxy LayoutWorkPlaneProxy { get; set; }

        internal WorkPlane LayoutWorkPlane { get; set; }

        internal List<WorkPointProxy> LayoutWorkPointProxies
        {
            get { return layoutWorkPointProxies; }
            set { layoutWorkPointProxies = value; }
        }

        internal List<WorkPoint> LayoutWorkPoints
        {
            get { return layoutWorkPoints; }
            set { layoutWorkPoints = value; }
        }

        internal List<IBindableObject> PointObjects
        {
            get { return bindablePointObjects; }
        }

        internal List<IBindableObject> PlaneObjects
        {
            get { return bindablePlaneObjects; }
        }

        internal string ModulePath 
        {
            get { return modulePath; }
            set { modulePath = value; }
        }

        internal int ModuleId { get; set; }

        internal string ModuleAssemblyPath { get; set; }

        internal bool ReuseDuplicates
        {
            get { return reuseDuplicates; }
            set { reuseDuplicates = value; }
        }

        internal List<WorkPointProxy> TargetWorkPointProxies
        {
            get { return targetWorkPointProxies; }
            set { targetWorkPointProxies = value; }
        }

        internal List<WorkPoint> TargetWorkPoints
        {
            get { return targetWorkPoints; }
            set { targetWorkPoints = value; }
        }

        internal WorkPlaneProxy TargetWorkPlaneProxy { get; set; }

        internal WorkPlane TargetWorkPlane { get; set; }

        internal string TemplateAssemblyPath { get; set; }

        internal string TemplateDrawingPath { get; set; }

        internal List<ComponentOccurrence> TopOccurrences
        {
            get { return topOccurrences; }
            set { topOccurrences = value; }
        }

        internal Matrix TransformationMatrix 
        {
            get { return transfomationMatrix; }
            set { transfomationMatrix = value; }
        }

        internal UniqueModuleEvaluator UniqueModules { get; set; }
        #endregion

        #region Public static constructors
        public static Module ByPoints(List<Point> points)
        {
            return new Module(points);
        }
        #endregion

        #region Public methods

        #endregion
    }
}
