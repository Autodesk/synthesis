using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Inventor;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using DSNodeServices;
using Dynamo.Models;
using Dynamo.Utilities;
using InventorLibrary.GeometryConversion;
using InventorServices.Persistence;
using InventorServices.Utilities;
using Point = Autodesk.DesignScript.Geometry.Point;
using Application = Autodesk.DesignScript.Geometry.Application;


namespace InventorLibrary.ModulePlacement
{
    [Browsable(false)]
    [IsVisibleInDynamoLibrary(false)]
    //[RegisterForTrace]
	internal class ModuleOldDelete
    {
        #region Private Fields
        private bool firstTime = false;
        private List<Inventor.WorkPoint> layoutWorkPoints = new List<Inventor.WorkPoint>();
        private List<WorkPointProxy> layoutWorkPointProxies = new List<WorkPointProxy>();
        #endregion

        #region Internal properties    
        internal bool CreateAllCopies { get; set; }

        internal List<WorkPointProxy> LayoutWorkPointProxies
        {
            get { return layoutWorkPointProxies; }
            set { layoutWorkPointProxies = value; }
        }

        internal bool FirstTime
        {
            get { return firstTime; }
            set { firstTime = value; }
        }

        internal Inventor.Application InventorApplication
        {
            get { return InventorServices.Persistence.PersistenceManager.InventorApplication; }
        }

        internal int GeometryMapIndex { get; set; }

        internal string LayoutPartPath { get; set; }

        internal WorkPlane LayoutWorkPlane { get; set; }

        internal List<WorkPoint> LayoutWorkPoints
        {
            get { return layoutWorkPoints; }
            set { layoutWorkPoints = value; }
        }

        internal string ModulePath { get; set; }

        internal List<Inventor.Point> ModulePoints { get; set; }

        internal string TemplateAssemblyPath { get; set; }

        internal string TemplateDrawingPath { get; set; }

        internal Matrix TransformationMatrix { get; set; }

        internal UniqueModuleEvaluator UniqueModules { get; set; }

        internal WorkPlaneProxy ModuleWorkPlaneProxyAssembly { get; set; }


        #endregion

        #region Private constructors

        private ModuleOldDelete(List<Point> pointList)
        {
            ModulePoints = pointList.Select(p => p.ToPoint()).ToList();
        }

        #endregion

        #region Private mutators
        private ModuleOldDelete InternalPlaceModule()
        {
            CreateInvLayout();
            return this;
        }

        private void CreateInvLayout()
        {
            CreateLayoutPartFile();
           
            Inventor.AssemblyComponentDefinition componentDefinition = InventorServices.Persistence.PersistenceManager.ActiveAssemblyDoc.ComponentDefinition;
            TransformationMatrix = InventorApplication.TransientGeometry.CreateMatrix();
            ComponentOccurrence componentOccurrence = componentDefinition.Occurrences.Add(LayoutPartPath, TransformationMatrix);
            ComponentOccurrences occurrences = componentDefinition.Occurrences;

            //TODO This is janky.  Don't need to assume that we are starting in an empty assembly file.  
            ComponentOccurrence layoutOccurrence = occurrences[1];
            PartComponentDefinition layoutComponentDefinition = (PartComponentDefinition)layoutOccurrence.Definition;

            for (int i = 0; i < ModulePoints.Count; i++)
            {
                WorkPoint workPoint = layoutComponentDefinition.WorkPoints.AddFixed(ModulePoints[i], false);
                workPoint.Grounded = true;
                workPoint.Visible = false;
                //Inventor's API documentation is so bad!
                object workPointProxyObject;
                layoutOccurrence.CreateGeometryProxy(workPoint, out workPointProxyObject);
                LayoutWorkPointProxies.Add((WorkPointProxy)workPointProxyObject);
                LayoutWorkPoints.Add(workPoint);
            }

            LayoutWorkPlane = layoutComponentDefinition.WorkPlanes.AddByThreePoints(layoutWorkPoints[0], layoutWorkPoints[1], layoutWorkPoints[2]);
            LayoutWorkPlane.Grounded = true;
            LayoutWorkPlane.Visible = false;
            object wPlaneProxyObject;
            layoutOccurrence.CreateGeometryProxy(LayoutWorkPlane, out wPlaneProxyObject);
            ModuleWorkPlaneProxyAssembly = (WorkPlaneProxy)wPlaneProxyObject;
        }

        private void CreateLayoutPartFile()
        {
            string partTemplateFile = @"C:\Users\Public\Documents\Autodesk\Inventor 2014\Templates\Standard.ipt";
            LayoutPartPath = "C:\\Users\\frankfralick\\Documents\\Inventor\\Dynamo 2014\\Layout.ipt";
            //TODO This is just for early testing of everything.  This will get set and managed elsewhere.
            if (!System.IO.File.Exists(LayoutPartPath))
            {
                PartDocument layoutPartDoc = (PartDocument)InventorApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject, partTemplateFile, true);
                layoutPartDoc.SaveAs(LayoutPartPath, false);
                layoutPartDoc.Close();
            }
            
        }

        //TODO: MakeInvCopy is going to be called over and over again by DesignScript, not us, so there is no opportunity to pass the count into this method.  
        //UniqueModuleEvaluator needs to be modified each time this is called so we know which module we are on.

        //TODO:  ApprenticeServer instance creation and lifetime management needs to be handled by InventorServices.Persistance

        //TODO: OccurrenceList needs to be set on each Module instance during UniqueModuleEvaluator's construction.

        //TODO: Refactor this method, it is so big.
        private void MakeInvCopy(ApprenticeServer appServ, 
                                 string templateAssemblyPath, 
                                 string templateDrawingPath, 
                                 string targetDirectory, 
                                 OccurrenceList occList, 
                                 int count, 
                                 UniqueModuleEvaluator uniqueModuleEvaluator)
        {
            // TODO Test for the existance of folders and assemblies.
            ApprenticeServer oAppServ = appServ;
            int panelID = count;
            OccurrenceList oOccs = occList;
            string topFileFullName;
            string targetPath = targetDirectory;
            TemplateAssemblyPath = templateAssemblyPath;
            TemplateDrawingPath = templateDrawingPath;
            string panelIDString = System.Convert.ToString(panelID);
            UniqueModules = uniqueModuleEvaluator;

            //Instead of using "panelID" to create unique folders for all instances, redirect to the GeometryMapIndex
            string geoMapString = System.Convert.ToString(GeometryMapIndex);
            string folderName;
            if (CreateAllCopies == false)
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
                if (panelID < 10)
                {
                    folderName = System.IO.Path.GetFileNameWithoutExtension(TemplateAssemblyPath) + " 00" + panelIDString;
                }
                else if (10 <= panelID && panelID < 100)
                {
                    folderName = System.IO.Path.GetFileNameWithoutExtension(TemplateAssemblyPath) + " 0" + panelIDString;
                }
                else
                {
                    folderName = System.IO.Path.GetFileNameWithoutExtension(TemplateAssemblyPath) + " " + panelIDString;
                }
            }
            //if(panelID < 10){
            //Need to get number of the parent occ, top level name as foldername
            string pathString = System.IO.Path.Combine(targetPath, folderName);

            topFileFullName = oOccs.TargetAssembly.FullDocumentName;
            string topFileNameOnly = System.IO.Path.GetFileName(topFileFullName);
            ModulePath = System.IO.Path.Combine(pathString, topFileNameOnly);


            TupleList<string, string> filePathPair = new TupleList<string, string>();

            for (int i = 0; i < occList.Items.Count; i++)
            {
                string targetOccPath = occList.Items[i].ReferencedFileDescriptor.FullFileName;
                string newCopyName = System.IO.Path.GetFileName(targetOccPath);
                string newFullCopyName = System.IO.Path.Combine(pathString, newCopyName);
                filePathPair.Add(targetOccPath, newFullCopyName);
            }

            //Check if an earlier panel already made the folder, if not, create it.
            if (!System.IO.Directory.Exists(pathString))
            {
                firstTime = true;
                System.IO.Directory.CreateDirectory(pathString);
                //AssemblyReplaceRef(oAppServ, oOccs.TargetAssembly, filePathPair, pathString);
                ApprenticeServerDocument oAssDoc;
                oAssDoc = oAppServ.Open(TemplateAssemblyPath);
                FileSaveAs fileSaver;
                fileSaver = oAppServ.FileSaveAs;
                fileSaver.AddFileToSave(oAssDoc, ModulePath);
                fileSaver.ExecuteSaveCopyAs();

                //Need to copy presentation files if there are any.  For now this is only going to work with the top assembly.
                string templateDirectory = System.IO.Path.GetDirectoryName(TemplateAssemblyPath);
                string[] presentationFiles = System.IO.Directory.GetFiles(templateDirectory, "*.ipn");
                //If we want the ability to have subassemblies with .ipn files or multiple ones, this will have to be changed
                //to iterate over all the .ipn files.
                if (presentationFiles.Length != 0)
                {
                    string newCopyPresName = System.IO.Path.GetFileName(presentationFiles[0]);
                    string newFullCopyPresName = System.IO.Path.Combine(pathString, newCopyPresName);

                    ApprenticeServerDocument presentationDocument = oAppServ.Open(presentationFiles[0]);
                    DocumentDescriptorsEnumerator presFileDescriptors = presentationDocument.ReferencedDocumentDescriptors;
                    foreach (DocumentDescriptor refPresDocDescriptor in presFileDescriptors)
                    {
                        if (refPresDocDescriptor.FullDocumentName == TemplateAssemblyPath)
                        {
                            refPresDocDescriptor.ReferencedFileDescriptor.ReplaceReference(ModulePath);
                            FileSaveAs fileSavePres;
                            fileSavePres = oAppServ.FileSaveAs;
                            fileSavePres.AddFileToSave(presentationDocument, newFullCopyPresName);
                        }
                    }
                }

                string newCopyDrawingName = System.IO.Path.GetFileName(TemplateDrawingPath);
                string newFullCopyDrawingName = System.IO.Path.Combine(pathString, newCopyDrawingName);

                if (TemplateDrawingPath != "")
                {
                    ApprenticeServerDocument drawingDoc = oAppServ.Open(TemplateDrawingPath);
                    DocumentDescriptorsEnumerator drawingFileDescriptors = drawingDoc.ReferencedDocumentDescriptors;
                    //This needs to be fixed.  It was written with the assumption that only the template assembly would be in 
                    //the details and be first in the collection of document descriptors.  Need to iterate through 
                    //drawingFileDescriptors and match names and replace correct references.
                    //Possibly can use the "filePathPair" object for name matching/reference replacing.
                    //drawingFileDescriptors[1].ReferencedFileDescriptor.ReplaceReference(topAssemblyNewLocation);
                    foreach (DocumentDescriptor refDocDescriptor in drawingFileDescriptors)
                    {
                        foreach (Tuple<string, string> pathPair in filePathPair)
                        {
                            string newFileNameLower = System.IO.Path.GetFileName(pathPair.Item2);
                            string drawingReferenceLower = System.IO.Path.GetFileName(refDocDescriptor.FullDocumentName);
                            string topAssemblyLower = System.IO.Path.GetFileName(ModulePath);
                            if (topAssemblyLower == drawingReferenceLower)
                            {
                                refDocDescriptor.ReferencedFileDescriptor.ReplaceReference(ModulePath);
                            }
                            if (newFileNameLower == drawingReferenceLower)
                            {
                                refDocDescriptor.ReferencedFileDescriptor.ReplaceReference(pathPair.Item2);
                            }
                        }
                    }

                    FileSaveAs fileSaveDrawing;
                    fileSaveDrawing = oAppServ.FileSaveAs;
                    fileSaveDrawing.AddFileToSave(drawingDoc, newFullCopyDrawingName);
                    fileSaveDrawing.ExecuteSaveCopyAs();
                    firstTime = true;

                    if (!UniqueModules.DetailDocumentPaths.Contains(newFullCopyDrawingName))
                    {
                        UniqueModules.DetailDocumentPaths.Add(newFullCopyDrawingName);
                    }
                }
            }
        }
        #endregion


        #region Public properties

        #endregion

        #region Public static constructors
        public static ModuleOldDelete ByPoints(List<Point> points)
        {
            return new ModuleOldDelete(points);
        }
        #endregion

        #region Public methods
        //This will have arguments to specify template, target directory for copies to go, template assembly, etc.
        //For development purposes these are being hard coded to defaults.
        public ModuleOldDelete PlaceModule()
        {
            return InternalPlaceModule();
        }

        #endregion

        #region Internal static constructors

        #endregion


        //List<WorkPoint> ModuleWorkPointListAssembly;
        //List<WorkPoint> ModuleWorkPointListTarget;
        //WorkPlane moduleWorkPlaneAssembly;
        //WorkPlane moduleWorkPlaneTarget;
        //List<WorkPointProxy> moduleWorkPointProxyAssembly;
        //List<WorkPointProxy> moduleWorkPointProxyTarget;
        //WorkPlaneProxy moduleWorkPlaneProxyAssembly;
        //WorkPlaneProxy moduleWorkPlaneProxyTarget;
        //PartComponentDefinition layoutCompDef;

        //TupleList<string, object> scopePairs = new TupleList<string, object>();
        //List<ComponentOccurrence> topOccSubsList = new List<ComponentOccurrence>();



        //public PartComponentDefinition LayoutCompDef
        //{
        //    get { return layoutCompDef; }
        //    set { layoutCompDef = value; }
        //}

        //public AssemblyComponentDefinition FrameCompDef
        //{
        //    get { return frameCompDef; }
        //    set { frameCompDef = value; }
        //}

        //public WorkPlane ModuleWorkPlaneTarget
        //{
        //    get{return moduleWorkPlaneTarget;}
        //    set{moduleWorkPlaneAssembly = value;}
        //}



        //public List<WorkPoint> ModuleWorkPointsTarget
        //{
        //    get{return ModuleWorkPointListTarget;}
        //    set{ModuleWorkPointListTarget = value;}
        //}

        //public List<WorkPointProxy> ModuleWorkPointsProxyTarget
        //{
        //    get{return moduleWorkPointProxyTarget;}
        //    set{moduleWorkPointProxyTarget = value;}
        //}



        //public WorkPlaneProxy ModuleWorkPlaneProxyTarget
        //{
        //    get{return moduleWorkPlaneProxyTarget;}
        //    set{moduleWorkPlaneProxyTarget = value;}
        //}

        //public bool CreateAllCopies
        //{
        //    get { return createAllCopies; }
        //    set { createAllCopies = value; }
        //}

        //public TupleList<string, object> ScopeNamePairs
        //{
        //    get { return scopePairs; }
        //    set { scopePairs = value; }
        //}

        //public List<ComponentOccurrence> TopOccurrences
        //{
        //    get{return topOccSubsList;}
        //    set{topOccSubsList = value;}
        //}

        ////public TupleList<string, double> OriginalParameters
        ////{
        ////    get
        ////    {
        ////        MMDB db = new MMDB();
        ////        TupleList<string, double> oldParams = db.Params(TargetPath);
        ////        return oldParams;
        ////    }
        ////}


        //public void PlacePanel(AssemblyDocument assDoc, int constraintVal, string layoutName)
        //{
        //    AssemblyDocument oAssDoc = assDoc;
        //    int constraint = constraintVal;
        //    ComponentOccurrence topOcc;
        //    string layoutDisplayName = layoutName;
        //    topOcc = oCompDef.Occurrences.Add(ModulePath, oMatrix);
        //    ComponentOccurrencesEnumerator topOccSubs = topOcc.SubOccurrences;
        //    int topOccSubsCount = topOccSubs.Count;
        //    ComponentOccurrence layoutOcc = null;
        //    ComponentOccurrence frameOcc = null;
        //    layoutCompDef = null;
        //    for (int i = 0; i < topOccSubsCount; i++)
        //    {
        //        ComponentOccurrence currentSub = topOccSubs[i + 1];
        //        if (currentSub.Name == layoutDisplayName)
        //        {
        //            layoutOcc = currentSub;
        //            layoutCompDef = (PartComponentDefinition)layoutOcc.Definition;

        //        }
        //        if (currentSub.Name == "Frame0001:1")
        //        {
        //            frameOcc = currentSub;
        //            frameCompDef = (AssemblyComponentDefinition)frameOcc.Definition;

        //        }
        //        else
        //        {
        //            topOccSubsList.Add(currentSub);
        //        }
        //    }
        //    int workPointCount = layoutCompDef.WorkPoints.Count;
        //    List<WorkPointProxy> targetProxyList = new List<WorkPointProxy>();
        //    List<WorkPoint> targetWorkPointList = new List<WorkPoint>();
        //    for (int j = 0; j < workPointCount; j++)

        //     //TODO  Need to put logic in that test a layout file for derivedparametertables collection.Count != 0
        //     //then do the copy of the layout file, get the layout file and swap out the document descriptor IN APPRENTICE.
        //    {
        //        WorkPoint currentWP;
        //        currentWP = layoutCompDef.WorkPoints[j+1];
        //        targetWorkPointList.Add(currentWP);
        //        object currentWPProxyObject;
        //        WorkPointProxy currentWPProxy;
        //        layoutOcc.CreateGeometryProxy(currentWP, out currentWPProxyObject);
        //        currentWPProxy = (WorkPointProxy)currentWPProxyObject;
        //        targetProxyList.Add(currentWPProxy);
        //    }
        //    //TODO Fix this to be more intellegent.  What if assembly had two planes (rooms etc.).
        //    WorkPlane targetWorkPlane;
        //    targetWorkPlane = (WorkPlane)layoutCompDef.WorkPlanes[4];
        //    ModuleWorkPointsTarget = targetWorkPointList;
        //    ModuleWorkPointsProxyTarget = targetProxyList;
        //    object wPlaneProxyObject;
        //    WorkPlaneProxy wPlaneProxy;
        //    layoutOcc.CreateGeometryProxy(targetWorkPlane, out wPlaneProxyObject);
        //    wPlaneProxy = (WorkPlaneProxy)wPlaneProxyObject;
        //    ModuleWorkPlaneProxyTarget = wPlaneProxy;

        //    //Workplane constraints needed or not?
        //    //oAssDoc.ComponentDefinition.Constraints.AddMateConstraint(PanelWorkPlaneProxyTarget, layoutCompDef.WorkPlanes[1], 0);

        //    if (firstTime==true)
        //    {
        //        for (int f = 0; f < constraint; f++)
        //        {
        //            //TODO this is uncertain.  It changes from test to test, need to get better handle on the indexing of points.
        //            //oAssDoc.ComponentDefinition.Constraints.AddMateConstraint(PanelWorkPointsProxyTarget[f+1], PanelWorkPointsProxyAssembly[f],0);
        //            oAssDoc.ComponentDefinition.Constraints.AddMateConstraint(ModuleWorkPointsProxyTarget[f+1], ModuleWorkPointsProxyAssembly[f], 0);
        //        }
        //        topOcc.Adaptive = true;
        //        oAssDoc.Update2();
        //        topOcc.Adaptive = false;
        //        targetWorkPlane.Visible = false;
        //        layoutOcc.Visible = false;
        //    }

        //    else
        //    {
        //        //oAssDoc.ComponentDefinition.Constraints.AddMateConstraint(panelWorkPlaneProxyTarget, PanelWorkPlaneProxyAssembly, 0);
        //        //for (int f = 0; f < constraint-2; f++)
        //        for (int f = 0; f < constraint; f++)
        //        {
        //            //TODO this is uncertain.  It changes from test to test, need to get better handle on the indexing of points.
        //            //oAssDoc.ComponentDefinition.Constraints.AddMateConstraint(PanelWorkPointsProxyTarget[f+1], PanelWorkPointsProxyAssembly[f],0);
        //            MateConstraint oMate = oAssDoc.ComponentDefinition.Constraints.AddMateConstraint(ModuleWorkPointsProxyTarget[f+1], ModuleWorkPointsProxyAssembly[f], 0);
        //            if (f>0)
        //            {
        //                //These mate constraints will fail out in space because of double accuracy issues unless they are relaxed some.
        //                oMate.ConstraintLimits.MaximumEnabled = true;
        //                oMate.ConstraintLimits.Maximum.Expression = ".5 in";
        //            }
                    
        //        }
        //        targetWorkPlane.Visible = false;
        //        layoutOcc.Visible = false;
        //    }
        //}

        //public void UpdateParameters()
        //{
        //    //TODO:  This whole method is terrible.  This must be refactored and split up.  The call
        //    //site should also change to be somewhere other than the main program control.
        //    //Gets a list of the Layout parameters from the database
        //    //TupleList<string, double> oldParams = OriginalParameters;
        //    Parameters layoutParams = LayoutCompDef.Parameters;
        //    scopePairs.Add("layoutParams", layoutParams);
        //    string testParam1 = "Module_Width";
        //    string testParam2 = "Module_Height";
        //    List<string> testParams = new List<string>() { testParam1, testParam2 };
        //    double testParamValue = 0;
        //    //if (FrameCompDef != null)
        //    //{
        //        //Parameters frameParams = FrameCompDef.Parameters;
        //        //scopePairs.Add("frameParams", frameParams);
        //    for (int b = 0; b < layoutParams.Count; b++)
        //    {
        //        foreach (string testParam in testParams)
        //        {
        //            if (layoutParams[b + 1].Name == testParam)
        //            {
        //                //changed this from b to b+1?  
        //                testParamValue = System.Convert.ToDouble(layoutParams[b + 1].Value);
        //                //TODO make unit conversion more intelligent, can't always assume standard inches template.
        //                double inchTestValue = testParamValue / 2.54;
        //                if (FrameCompDef != null)
        //                {
        //                    Parameters frameParams = FrameCompDef.Parameters;
        //                    scopePairs.Add("frameParams", frameParams);
        //                    for (int m = 0; m < frameParams.Count; m++)
        //                    {
        //                        string currentExpression = frameParams[m + 1].Expression;
        //                        string[] expressionWords = currentExpression.Split(new Char[] { ' ' });
        //                        for (int e = 0; e < expressionWords.Length; e++)
        //                        {
        //                            if (expressionWords[e] == testParam)
        //                            {
        //                                frameParams[m + 1].Expression = Regex.Replace(currentExpression, testParam, inchTestValue.ToString() + " in");
        //                            }
        //                        }
        //                    }
        //                }

        //                if (TopOccurrences != null)
        //                {
        //                    foreach (ComponentOccurrence topOcc in TopOccurrences)
        //                    {
        //                        if (topOcc.DefinitionDocumentType == DocumentTypeEnum.kPartDocumentObject)
        //                        {
        //                            PartComponentDefinition topOccPartDef = (PartComponentDefinition)topOcc.Definition;
        //                            Parameters topOccPartParams = topOccPartDef.Parameters;
        //                            scopePairs.Add("topOccPartParams", topOccPartParams);
        //                            for (int u = 0; u < topOccPartParams.Count; u++)
        //                            {
        //                                string currentExpression = topOccPartParams[u + 1].Expression;
        //                                string[] expressionWords = currentExpression.Split(new Char[] { ' ' });
        //                                for (int p = 0; p < expressionWords.Length; p++)
        //                                {
        //                                    if (expressionWords[p] == testParam)
        //                                    {
        //                                        topOccPartParams[u + 1].Expression = Regex.Replace(currentExpression, testParam, inchTestValue.ToString() + " in");
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public void AssemblyReplaceRef(ApprenticeServer appServ, ApprenticeServerDocument assDoc,TupleList<string,string> namePair,string folderPath)
        //{
        //    ApprenticeServer oAppServ = appServ;
        //    ApprenticeServerDocument oAssDoc = assDoc;
        //    OccurrenceList newOccs = new OccurrenceList(oAppServ,oAssDoc);
        //    string pathString = folderPath;
        //    List<string> patternComponentsList = new List<string>();
        //    for (int i = 0; i < newOccs.Items.Count; i++) {
        //        if(newOccs.Items[i].DefinitionDocumentType == DocumentTypeEnum.kPartDocumentObject){
        //            //if (newOccs.Items[i].ParentOccurrence == null) {
        //                for (int f = 0; f < namePair.Count; f++) {
        //                    if(namePair[f].Item1 == newOccs.Items[i].ReferencedFileDescriptor.FullFileName){
        //                        if(patternComponentsList.Contains(namePair[f].Item1)){
        //                            newOccs.Items[i].ReferencedDocumentDescriptor.ReferencedFileDescriptor.ReplaceReference(namePair[f].Item2);
        //                        }
        //                        else{
        //                            if (!System.IO.File.Exists(namePair[f].Item2))
        //                            {
        //                                oAppServ.FileManager.CopyFile(namePair[f].Item1,namePair[f].Item2);
        //                                newOccs.Items[i].ReferencedDocumentDescriptor.ReferencedFileDescriptor.ReplaceReference(namePair[f].Item2);
        //                            }
								    
        //                            patternComponentsList.Add(namePair[f].Item1);
        //                        }
        //                    }
        //                }
        //        }
        //        else if(newOccs.Items[i].DefinitionDocumentType == DocumentTypeEnum.kAssemblyDocumentObject){
        //            for (int n = 0; n < namePair.Count; n++) {
        //                if(namePair[n].Item1 == newOccs.Items[i].ReferencedFileDescriptor.FullFileName){
        //                    ApprenticeServerDocument oSubAssDoc = oAppServ.Open(newOccs.Items[i].ReferencedFileDescriptor.FullFileName);
        //                    AssemblyReplaceRef(oAppServ,oSubAssDoc,namePair,pathString);
        //                    FileSaveAs fileSave;
        //                    fileSave = oAppServ.FileSaveAs;
        //                    string newFilePath = namePair[n].Item2;
        //                    fileSave.AddFileToSave(oSubAssDoc,newFilePath);

        //                    //TODO Need to code a check at the beginning (copyutilmain.xaml.cs) that determines if the target directory is in the Project or not. 
        //                    //Apprentice fails otherwise.
        //                    //TODO Also, Inventor needs to be running for this not to fail.
        //                    fileSave.ExecuteSaveCopyAs();
        //                    newOccs.Items[i].ReferencedDocumentDescriptor.ReferencedFileDescriptor.ReplaceReference(namePair[n].Item2);
        //                }
        //            }
        //        }
        //    }
        //}

        //TODO Refactor "Panel" to "Module"
        //TODO Add "UpdateLocation" method (if using db locations from Revit)

	}
}
