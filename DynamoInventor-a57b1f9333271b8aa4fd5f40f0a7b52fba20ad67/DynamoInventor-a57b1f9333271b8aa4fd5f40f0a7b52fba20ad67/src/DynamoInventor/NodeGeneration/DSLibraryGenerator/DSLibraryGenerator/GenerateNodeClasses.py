import GenerationHelpers as gh
import Inventor

#define list of namespaces these clases will be using
using = ['System',
         'System.Collections.Generic',
         'System.ComponentModel',
         'System.Linq',
         'System.Text',
         'Inventor',
         'Autodesk.DesignScript.Geometry',
         'Autodesk.DesignScript.Interfaces',
         'DSNodeServices',
         'Dynamo.Models',
         'Dynamo.Utilities',
         'InventorLibrary.GeometryConversion',
         'InventorServices.Persistence']

#define a type in the assembly these classes will be generated from.
type_from_assembly = Inventor.Application

#if generating multiple classes at once, limit to types in the same namespace.
types_to_generate = ['Inventor.WorkPlane']

#define the namespace the generated classes will be part of.
destination_namespace = 'InventorLibrary.API'

#define an prefix for the wrapper type name:
prefix = "Inv"

#define the folder to save class files to:
#destination_folder = "C:\\Projects\\Dynamo\\Dynamo\\scripts\\NodeGenerator\\Tests\\"
destination_folder = "C:\\Projects\\Dynamo\\Dynamo\\src\\Libraries\\Inventor\\DSInventorNodes\\API\\"

generator = gh.ClassGenerator(using, type_from_assembly, types_to_generate, destination_namespace, prefix, destination_folder)

#'Inventor.AssemblyDocument'
#'Inventor.PartDocument',
#'Inventor.CommandIDEnum',
#'Inventor._DocPerformanceMonitor',
#'Inventor.Assets',
#'Inventor.AttributeManager',
#'Inventor.AttributeManager',
#'Inventor.AttributeSets',
#'Inventor.AssemblyComponentDefinition',
#'Inventor.AssemblyComponentDefinitions',
#'Inventor.BrowserPanel',
#'Inventor.CachedGraphicsStatus',
#'Inventor.CommandTypesEnum',
#'Inventor.ComponentDefinition',
#'Inventor.ComponentOccurrence',
#'Inventor.DisabledCommandList',
#'Inventor.DisplaySettings',
#'Inventor.DocumentDescriptorsEnumerator',
#'Inventor.DocumentEvents',
#'Inventor.DocumentInterests',
#'Inventor.Documents',
#'Inventor.DocumentSubType',
#'Inventor.DocumentTypeEnum',
#'Inventor.EnvironmentManager',
#'Inventor.File',
#'Inventor.FileOwnershipEnum',
#'Inventor.GraphicDataSetsCollection',
#'Inventor.InventorVBAProject',
#'Inventor.LightingStyle',
#'Inventor.ObjectTypeEnum',
#'Inventor.OGSSceneNode',
#'Inventor.PartComponentDefinition',
#'Inventor.Point',
#'Inventor.PrintManager',
#'Inventor.PropertySets',
#'Inventor.ReferencedFileDescriptors',
#'Inventor.ReferencedOLEFileDescriptors',
#'Inventor.ReferenceKeyManager',
#'Inventor.RenderStyles',
#'Inventor.SelectionPriorityEnum'
#'Inventor.SelectSet',
#'Inventor.SketchSettings',
#'Inventor.SoftwareVersion',
#'Inventor.ThumbnailSaveOptionEnum',
#'Inventor.UnitsOfMeasure',
#'Inventor.Views',
#'Inventor.WorkPlane',
#'Inventor.WorkPlanes',
#'Inventor.WorkPoint',
#'Inventor.WorkPoints',


