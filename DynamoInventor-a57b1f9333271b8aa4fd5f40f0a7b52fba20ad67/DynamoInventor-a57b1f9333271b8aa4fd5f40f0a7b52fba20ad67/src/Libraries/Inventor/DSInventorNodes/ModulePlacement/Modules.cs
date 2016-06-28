using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Inventor;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Dynamo.Models;
using Dynamo.Utilities;
using InventorLibrary.GeometryConversion;
using InventorServices.Persistence;
using InventorServices.Utilities;
using InventorLibrary.API;
using Point = Autodesk.DesignScript.Geometry.Point;
using Application = Autodesk.DesignScript.Geometry.Application;

namespace InventorLibrary.ModulePlacement
{
    public class Modules : IEnumerable<Module>, IModules
    {
        #region Private fields
        private List<Module> modulesList;
        private IPointsList _modulePoints;
        private PartComponentDefinition layoutComponentDefinition;
        private IModuleBinder _binder;
        private bool _testMode;
        #endregion

        #region Internal properties
        internal ComponentOccurrences AssemblyOccurrences { get; set; }
        internal string DestinationFolder { get; set; }
        internal ComponentOccurrence LayoutOccurrence { get; set; }
        internal PartDocument LayoutPartDocument { get; set; }
        internal string LayoutPartPath { get; set; }
        internal string TemplateAssemblyPath { get; set; }
        internal Matrix TransformationMatrix { get; set; }
        
        internal List<Module> ModulesList
        {
            get { return modulesList; }
            set { modulesList = value; }
        }

        internal IPointsList ModulePoints 
        {
            get { return _modulePoints; }
            set { _modulePoints = value; }
        }

        internal UniqueModuleEvaluator UniqueModules { get; set; }

        #endregion

        #region Private constructors
        public Modules(IPointsList modulePoints, IModuleBinder binder)
        {
            /*
            This class will contain the main program control for creating assemblies,
            placing them, bookkeeping, etc.  The IoC container should only be used
            from this class.  Classes below this one in the hierarchy should not know
            about the container.
            
            Modules is a collection of Module.  Modules also holds top level objects 
            like the master layout part for the assembly we are constructing.  Each 
            Module has a collection of ModuleObject.  ModuleObject is any 
            bindable object at the Module level.  All objects to be bound get 
            their own binder, and they are responsible for setting the IObjectBinder's
            ContextData and ContextManager.  This is a Tuple<int, int>, where Item1 is 
            the Module ID, and Item2 is the ConstraintId.
            
            ContextData will be the lexicographical order of operations for the whole
            collection of module.  Its contents will vary with the number of 
            input constraints per Module.  Because we have to create and keep
            track of very disparate types of objecs, this ordering has to be somewhat
            by convention:
            
            Modules...............ContextData = <0,0-N>.....Item1 = 0 for top level objects
               Module.............ContextData = <1-M,0-P>...<ModuleId, ObjectId>
                  ModuleObject....ContetxData = <1-M,0-P>...Same as Module level objects
            */
            _binder = binder;
            _binder.ContextData.Context = new Tuple<int, int>(0, 0);
            _binder.ContextManager.BindingContextManager = PersistenceManager.ActiveDocument.ReferenceKeyManager;
            _modulePoints = modulePoints;

            //_modulePoints.PropertyChanged += _modulePoints_PropertyChanged;

            //if (ModulePoints.IsDirty)
            //{
             //   CreateModuleCollection(modulePoints);
            //}
            CreateModuleCollection(modulePoints);
        }

        void _modulePoints_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ModulePoints.IsDirty)
            {
                CreateModuleCollection(ModulePoints);
            }
        }

        private void CreateModuleCollection(IPointsList modulePoints)
        {
            modulePoints.OldPointsList = modulePoints.PointsList;
            modulesList = new List<Module>();
            for (int i = 0; i < modulePoints.PointsList.Count; i++)
            {
                int moduleId = i + 1;
                var module = Module.ByPoints(modulePoints.PointsList[i]);
                module.ModuleId = moduleId;
                ModulesList.Add(module);
                if (ModulePoints.PointsList != null && ModulePoints.PointsList[0] != null)
                {
                    for (int j = 0; j < modulePoints.PointsList[i].Count; j++)
                    {
                        int objectId = j;
                        var moduleObject = PersistenceManager.IoC.GetInstance<IBindableObject>();
                        moduleObject.RegisterContextData(moduleId, objectId);
                        module.PointObjects.Add(moduleObject);
                    }

                    if (modulePoints.PointsList[0].Count > 2)
                    {
                        var planeObject = PersistenceManager.IoC.GetInstance<IBindableObject>();
                        planeObject.RegisterContextData(moduleId, modulePoints.PointsList[0].Count);
                        module.PlaneObjects.Add(planeObject);

                        var assemblyOccurrenceObject = PersistenceManager.IoC.GetInstance<IBindableObject>();
                        assemblyOccurrenceObject.RegisterContextData(moduleId, modulePoints.PointsList[0].Count + 1);
                        assemblyOccurrenceObject.Binder.ContextManager.BindingContextManager = PersistenceManager.ActiveAssemblyDoc.ReferenceKeyManager;
                        module.AssemblyOccurrenceObject = assemblyOccurrenceObject;
                    }

                    else
                    {
                        var assemblyOccurrenceObject = PersistenceManager.IoC.GetInstance<IBindableObject>();
                        assemblyOccurrenceObject.RegisterContextData(moduleId, modulePoints.PointsList[0].Count);
                        assemblyOccurrenceObject.Binder.ContextManager.BindingContextManager = PersistenceManager.ActiveAssemblyDoc.ReferenceKeyManager;
                        module.AssemblyOccurrenceObject = assemblyOccurrenceObject;
                    }
                }
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Method checks that both the template assembly, and the folder specified as the destination
        /// for new files is within the active project.
        /// </summary>
        /// <returns></returns>
        private bool CompatibleWithActiveProject()
        {
            //TODO: Flesh this out
            return true;
        }

        /// <summary>
        /// We are creating a new assembly, with new unique subassemblies derived from the template assembly.
        /// The first thing we need in this new assembly is a "layout part".  This will be the first thing we put
        /// in the assembly and it will contain all of the work points that we need to place and contstrain
        /// each module instance.
        /// </summary>
        /// 
        private void CreateLayout(string destinationFolder)
        {
            //TODO: The next line is super brittle.  Need to finish writing out what expected behavior should be for responding to 
            //Document activations/deactivations.  Right now InventorPersistenceManager.ActiveAssemblyDoc is being kept current, but that
            //may be bad.
            Inventor.AssemblyComponentDefinition componentDefinition = PersistenceManager.ActiveAssemblyDoc.ComponentDefinition;
            if (TransformationMatrix == null)
            {
                TransformationMatrix = PersistenceManager.InventorApplication.TransientGeometry.CreateMatrix();
            }
            //We know that there is a Layout.ipt file on the disc if we are here.  
            //Try to bind to the ComponentOccurrence that corresponds to this file in
            //our assembly, otherwise place it, and SetObjectForTrace.
            layoutComponentDefinition = GetLayoutCompDef(componentDefinition);

            for (int i = 0; i < ModulesList.Count; i++)
            {
                //0 is reserved for top level assembly objects that require binding support.
                int moduleNumber = i + 1;
                ModulesList[i].PlaceWorkGeometryForContsraints(layoutComponentDefinition, LayoutOccurrence, moduleNumber);
            }
        }

        private void EnsureLayoutPartExists(string destinationFolder)
        {
            //TODO: Add overload that lets user set this partTemplateFile.  This is a template in the sense of 
            //what the user sees in the UI when creating a new file.
            string partTemplateFile = @"C:\Users\Public\Documents\Autodesk\Inventor 2014\Templates\Standard.ipt";
            LayoutPartPath = destinationFolder + "\\Layout.ipt";
            if (!System.IO.File.Exists(LayoutPartPath))
            {
                LayoutPartDocument = (PartDocument)PersistenceManager.InventorApplication.Documents.Add(DocumentTypeEnum.kPartDocumentObject,
                                                                                                                        partTemplateFile,
                                                                                                                        true);
                LayoutPartDocument.SaveAs(LayoutPartPath, false);
                LayoutPartDocument.Close();
            }
            else
            {
                //We should alert the user that this part exists and ask them what they want to do.
                //Option A: Use the existing file.  Create work geometry in it, even if there is other work geometry
                //          already there.  Maybe they want to add to something they already did.
                //Option B: Delete the file and recreate it.
            }
        }

        private PartComponentDefinition GetLayoutCompDef(Inventor.AssemblyComponentDefinition componentDefinition)
        {
            ComponentOccurrence layoutOccurrence;
            if (_binder.GetObjectFromTrace<ComponentOccurrence>(out layoutOccurrence))
            {
                LayoutOccurrence = layoutOccurrence;
                PartComponentDefinition layoutComponentDefinition = (PartComponentDefinition)layoutOccurrence.Definition;
                AssemblyOccurrences = componentDefinition.Occurrences;
                return layoutComponentDefinition;
            }

            else
            {
                layoutOccurrence = componentDefinition.Occurrences.Add(LayoutPartPath, TransformationMatrix);
                //This delegate should be part of the implementation of some construction dependency.  No need for
                //it to be in here.
                _binder.SetObjectForTrace<ComponentOccurrence>(layoutOccurrence, ModuleUtilities.ReferenceKeysSorter);
                LayoutOccurrence = layoutOccurrence;
                PartComponentDefinition layoutComponentDefinition = (PartComponentDefinition)layoutOccurrence.Definition;
                AssemblyOccurrences = componentDefinition.Occurrences;
                return layoutComponentDefinition;
            }
        }

        private void InternalGenerateDrawings(string templateDrawingPath, string masterDrawingPath)
        {
            PersistenceManager.InventorApplication.Visible = false;
            if (_testMode)
            {
                if (ModulesList.Count < 3)
                {
                    for (int i = 0; i < ModulesList.Count; i++)
                    {
                        ModulesList[i].GenerateDrawings(templateDrawingPath);
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        ModulesList[i].GenerateDrawings(templateDrawingPath);
                    }
                }
            }
            else
            {
                for (int i = 0; i < ModulesList.Count; i++)
                {
                    ModulesList[i].GenerateDrawings(templateDrawingPath);
                }
            }

            
            if (masterDrawingPath != null && masterDrawingPath != "" && System.IO.File.Exists(masterDrawingPath))
            {
                //TODO: test extension is .idw
                DrawingDocument masterDrawing = (DrawingDocument)PersistenceManager.InventorApplication.Documents.Open(masterDrawingPath, true);

                for (int p = 0; p < UniqueModules.DetailDocumentPaths.Count; p++)
                {
                    //Get quantity of this module.
                    //TODO: Look at changing this.  It feels brittle.  
                    int moduleQuantity = UniqueModules.InstanceGeometryMap.Where(q => q.Item2 == p).Count();

                    DrawingDocument moduleDoc = (DrawingDocument)PersistenceManager.InventorApplication.Documents.Open(UniqueModules.DetailDocumentPaths[p], true);
                    //Iterate through the collection in case the template detail contains many sheets.
                    Sheets detailSheets = moduleDoc.Sheets;
                    for (int q = 0; q < detailSheets.Count; q++)
                    {
                        Sheet currentSheet = detailSheets[q+1];
                        currentSheet.Activate();
                        DrawingNotes notes = currentSheet.DrawingNotes;
                        foreach (DrawingNote note in notes)
                        {   
                            //This is an idea.  Having 'template tags' available to the user that lets them place comments in drawing templates that Dynamo can 
                            //swap out for the desired instance value.
                            if (note.Text == "<DynamoUnitNumber>")
                            {
                                int unitNumber = p + 1;
                                string moduleLabelNote = String.Format("<StyleOverride FontSize='.6096'>ITEM: {0}</StyleOverride>", unitNumber.ToString());
                                note.FormattedText=moduleLabelNote;
                            }
                            if (note.Text == "<DynamoModuleQuantity>")
                            {
                                string moduleCountNote = String.Format("<StyleOverride FontSize='.6096'>QUANTITY: {0}</StyleOverride>", moduleQuantity.ToString());
                                note.FormattedText = moduleCountNote;
                            }
                        }
                        currentSheet.CopyTo(masterDrawing as _DrawingDocument);
                    }
                    moduleDoc.Close(true);
                }
                masterDrawing.Save2();
                masterDrawing.Close(true);

            }
            PersistenceManager.InventorApplication.Visible = true;
        }

        /// <summary>
        /// Main program control for copying, placing, constraining template assemblies,
        /// as well as evaluating duplicate geometries.
        /// </summary>
        /// <param name="templateAssemblyPath">Path to template assembly file to be copied and placed in model</param>
        /// <param name="destinationFolder">Path where created files will be saved</param>
        /// <param name="reuseDuplicates">Let the library determine if files can be reused</param>
        /// <param name="testMode">If true will create only three modules</param>
        private void InternalPlaceModules(string templateAssemblyPath, string destinationFolder, bool reuseDuplicates, bool testMode)
        {
            //Do some initial validation that this is going to work.
            //TODO: MOVE THIS INTO THE CONSTRUCTOR
            if (!IsConstraintSetUniform)
            {
                throw new Exception("Each module must have the same number of points.");
            }

            //If the user has chosen to try to reuse files for duplicate geometries, we need
            //evaluate that first before creating any new files.

            //TODO: MOVE THIS TO THE CONSTRUCTOR/ModulePoints property changed event.
            //Create this thing in constructor and set it to null in
            //this method if the user sets reuseDuplicates to false.  We
            //are evaluating this shit every time
            UniqueModules = null;

            if (reuseDuplicates)
            {
                ModulesList.Select(p => { p.ReuseDuplicates = false; return p; }).ToList();
                //UniqueModuleEvaluator needs to be passed into the constructor.
                UniqueModules = UniqueModuleEvaluator.ByModules(ModulesList);
            }

            //We need to get a flattened list of all the ComponentOccurrence objects in the 
            //template assembly.

            //Inventor's API was developed in Russia.
            AssemblyDocument templateAssembly = (AssemblyDocument)PersistenceManager.InventorApplication.Documents.Open(templateAssemblyPath, false);
            OccurrenceList templateOccurrences = new OccurrenceList(templateAssembly);
            _testMode = testMode;
            if (testMode)
            {
                if (ModulesList.Count < 3)
                {
                    for (int i = 0; i < ModulesList.Count; i++)
                    {
                        ModulesList[i].MakeInvCopy(templateAssemblyPath, destinationFolder, templateOccurrences, UniqueModules);
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        ModulesList[i].MakeInvCopy(templateAssemblyPath, destinationFolder, templateOccurrences, UniqueModules);
                    }
                }
            }
            else
            {
                for (int i = 0; i < ModulesList.Count; i++)
                {
                    ModulesList[i].MakeInvCopy(templateAssemblyPath, destinationFolder, templateOccurrences, UniqueModules);
                }
            }

            templateAssembly.Close();

            //ApprenticeServerComponent apprenticeServer = InventorPersistenceManager.ActiveApprenticeServer;
            //ApprenticeServerDocument templateAssemblyApprenticeDoc = apprenticeServer.Open(templateAssemblyPath);
            //OccurrenceList templateOccurrences = new OccurrenceList(apprenticeServer, templateAssemblyApprenticeDoc);
            //apprenticeServer.Close();
            

            //Create Inventor files needed to accommodate this set of "Modules".

            //Create a layout file.  This file will contain all the individual geometries as 
            //work geometry.  It will be placed first in the assembly we are making, and each
            //Module will get constrained to its corresponding set of work geometry.
            //This only needs to happen the first time.
            EnsureLayoutPartExists(destinationFolder);

            //Place the layout part and put work geometry in it.
            CreateLayout(destinationFolder);

            //Place the actual Inventor assemblies.

            //PersistenceManager.InventorApplication.Visible = false;
            try
            {
                if (testMode)
                {
                    if (ModulesList.Count < 3)
                    {
                        for (int i = 0; i < ModulesList.Count; i++)
                        {
                            ModulesList[i].PlaceModule();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            ModulesList[i].PlaceModule();
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < ModulesList.Count; i++)
                    {
                        ModulesList[i].PlaceModule();
                    }
                }
            }
            catch
            {
                PersistenceManager.InventorApplication.Visible = true;
            }
            //PersistenceManager.InventorApplication.Visible = true;

            //Update the view
            PersistenceManager.ActiveAssemblyDoc.Update2();
        }
        #endregion

        #region Internal properties
        internal int InternalCount
        {
            get { return modulesList.Count; }
        }

        private bool IsConstraintSetUniform
        {
            get
            {
                int contstraintCount = ModulesList[0].InternalModulePoints.Count;
                if (ModulesList.Any(p => contstraintCount != p.InternalModulePoints.Count))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private PartComponentDefinition LayoutComponentDefinition { get; set; }
        #endregion

        #region Public properties
        public int Count
        {
            get { return InternalCount; }
        }
        #endregion

        #region Public static constructors
        public static Modules ByPointsList(List<List<Point>> points)
        {
            if (ModuleIoC.HasRegistered == false)
            {
                ModuleIoC.LetThereBeIoC();
            }
            //var modulePoints = PersistenceManager.IoC.GetInstance<IPointsList>();
            ModulePoints modulePoints = new ModulePoints(points);
            //ModulePoints modulePoints = new ModulePoints();
            //modulePoints.PointsList = points;

            //Thread.Sleep(1000);

            var binder = PersistenceManager.IoC.GetInstance<IModuleBinder>();

            //return PersistenceManager.IoC.GetInstance<IModules>() as Modules;
            return new Modules(modulePoints, binder);
        }
        #endregion

        #region Public methods
        public void Add(Module module)
        {
            modulesList.Add(module);
        }

        public Modules GenerateDrawings(string templateDrawingPath, string masterDrawingPath)
        {
            InternalGenerateDrawings(templateDrawingPath, masterDrawingPath);
            return this;
        }

        /// <summary>
        ///Method to pair and place a set of generic module geometries with a real Inventor 
        ///assembly.
        /// </summary>
        /// <param name="templateAssemblyPath"></param>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        public Modules PlaceModules(string templateAssemblyPath, string destinationFolder, bool reuseDuplicates, bool testMode)
        {
            InternalPlaceModules(templateAssemblyPath, destinationFolder, reuseDuplicates, testMode);
            return this;
        }

        #endregion

        public IEnumerator<Module> GetEnumerator()
        {
            return ModulesList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

