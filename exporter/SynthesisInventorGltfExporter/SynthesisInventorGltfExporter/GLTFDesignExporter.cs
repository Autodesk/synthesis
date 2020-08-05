using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Google.Protobuf;
using Inventor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using Synthesis.Gltfextras;
using Asset = Inventor.Asset;
using Color = Inventor.Color;
using File = System.IO.File;

namespace SynthesisInventorGltfExporter
{
    /*
     * TODO: Avoid exporting duplicate meshes using occurrences (materials didn't work when I tried to do this)
     * TODO: Figure out a more elegant method to add extras to the gltf object
     * TODO: Figure out how to use dependency redirects in an inventor addin (was not working for some reason) so we can use an updated version of sharpgltf
     */
    public class GLTFDesignExporter
    {
        private MaterialBuilder defaultMaterial = new MaterialBuilder()
            .WithMetallicRoughnessShader()
            .WithChannelParam("BaseColor", Vector4.One);

        private Dictionary<string, MaterialBuilder> materialCache;
        private List<AssemblyJoint> allDocumentJoints;
        
        private List<string> warnings;

        private HashSet<string> visitedDocuments = new HashSet<string>();
        private ProgressBar progressBar;
        private int progressTotal;
        private bool exportMaterials;
        private bool exportFaceMaterials;
        private bool exportHidden;
        private decimal exportTolerance;

        public void ExportDesign(Application application, AssemblyDocument assemblyDocument, string filePath, bool glb, bool checkMaterialsChecked, bool checkFaceMaterials, bool checkHiddenChecked, decimal numericToleranceValue)
        {
            exportTolerance = numericToleranceValue;
            exportHidden = checkHiddenChecked;
            exportFaceMaterials = checkFaceMaterials;
            exportMaterials = checkMaterialsChecked;
            materialCache = new Dictionary<string, MaterialBuilder>();
            allDocumentJoints = new List<AssemblyJoint>();
            warnings = new List<string>();

            progressTotal = 2 + assemblyDocument.AllReferencedDocuments.Count;
            progressBar = application.CreateProgressBar(false, progressTotal, "Exporting " + assemblyDocument.DisplayName + " to glTF");

            progressBar.Message = "Preparing for export...";

            var sceneBuilder = ExportScene(assemblyDocument);

            progressBar.Message = "Exporting joints...";
            progressBar.UpdateProgress();
            // TODO: Figure out a more elegant solution for extras. This is only needed because sharpGLTF (this version anyways) doesn't support writing extras so we need to do it manually. 
            var modelRoot = sceneBuilder.ToSchema2();
            var dictionary = modelRoot.WriteToDictionary("temp");
            var jsonString = Encoding.ASCII.GetString(dictionary["temp.gltf"].Array);
            var parsedJToken = (JObject) JToken.ReadFrom(new JsonTextReader(new StringReader(jsonString)));
            var extras = new JObject();
            extras.Add("joints", ExportJoints(assemblyDocument));
            parsedJToken.Add("extras", extras);
            
            var readSettings = new ReadSettings();
            readSettings.FileReader = assetFileName => dictionary[assetFileName];
            var modifiedGltf = ModelRoot.ReadGLTF(new MemoryStream(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(parsedJToken))), readSettings);
            
            progressBar.Message = "Saving file...";
            progressBar.UpdateProgress();

            if (glb)
                modifiedGltf.SaveGLB(filePath);
            else
                modifiedGltf.SaveGLTF(filePath);

            Debug.WriteLine("-----gltf export warnings-----");
            foreach (var warning in warnings)
            {
                Debug.WriteLine(warning);
            }
            Debug.WriteLine("----------");
            progressBar.Close();
        }
        
        
        private JArray ExportJoints(AssemblyDocument assemblyDocument)
        {
            var jointArray = new JArray();
            
            foreach (AssemblyJoint joint in allDocumentJoints)
            {
                try { jointArray.Add(JObject.Parse(JsonFormatter.Default.Format(ExportJoint(joint)))); } catch {}
            }
        
            return jointArray;
        }
        
        private Joint ExportJoint(AssemblyJoint invJoint)
        {
            var protoJoint = new Joint();
            
            var header = new Header();
            try { header.Name = invJoint.Name; } catch {}
            protoJoint.Header = header;
            
            protoJoint.Origin = GetCenterOrRoot(invJoint.Definition);
            
            try { protoJoint.IsLocked = invJoint.Locked; } catch {}
            try { protoJoint.IsSuppressed = invJoint.Suppressed; } catch {}
        
            protoJoint.OccurrenceOneUUID = GetJointedOccurrenceUUID(invJoint.OccurrenceOne);
            protoJoint.OccurrenceTwoUUID = GetJointedOccurrenceUUID(invJoint.OccurrenceTwo);
            
            var assemblyJointDefinition = invJoint.Definition;
            
            switch (assemblyJointDefinition.JointType)
            {
                case AssemblyJointTypeEnum.kRigidJointType:
                    protoJoint.RigidJointMotion = GetRigidJointMotion(assemblyJointDefinition);
                    break;
                case AssemblyJointTypeEnum.kRotationalJointType:
                    protoJoint.RevoluteJointMotion = GetRevoluteJointMotion(assemblyJointDefinition);
                    break;
                case AssemblyJointTypeEnum.kSlideJointType:
                    protoJoint.SliderJointMotion = GetSliderJointMotion(assemblyJointDefinition);
                    break;
                case AssemblyJointTypeEnum.kCylindricalJointType:
                    protoJoint.CylindricalJointMotion = GetCylindricalJointMotion(assemblyJointDefinition);
                    break;
                // case AssemblyJointTypeEnum.k: // pinSlot
                    // protoJoint.RigidJointMotion = GetRigidJointMotion(assemblyJointDefinition);
                // break;
                case AssemblyJointTypeEnum.kPlanarJointType:
                    protoJoint.PinSlotJointMotion = GetPinSlotJointMotion(assemblyJointDefinition);
                    break;
                case AssemblyJointTypeEnum.kBallJointType:
                    protoJoint.PlanarJointMotion = GetPlanarJointMotion(assemblyJointDefinition);
                    break;
            }
            
            return protoJoint;
        }

        private Vector3D GetNormalOrDirection(AssemblyJointDefinition assemblyJointDefinition)
        {
            try { return GetVector3D(GetNormDirFromGeometry(assemblyJointDefinition.OriginOne)); } catch {}
            try { return GetVector3D(GetNormDirFromGeometry(assemblyJointDefinition.OriginTwo)); } catch {}
            warnings.Add("No joint axis found for joint "+assemblyJointDefinition.Parent.Name);
            return new Vector3D();
        }

        private dynamic GetNormDirFromGeometry(GeometryIntent intent)
        {
            try { return GetNormDir(intent.Geometry); } catch {}
            try { return GetNormDir(intent.Geometry.Geometry); } catch {}
            throw new Exception();
        }

        private dynamic GetNormDir(dynamic var)
        {
            try { return var.Normal; } catch {}
            try { return var.Direction; } catch {}
            throw new Exception();
        }
        private Vector3D GetCenterOrRoot(AssemblyJointDefinition assemblyJointDefinition)
        {
            // TODO: The point returned from getCenter is very often wrong...
            try { return GetVector3DConvertUnits(GetCenterFromGeometry(assemblyJointDefinition.OriginTwo)); } catch {}
            try { return GetVector3DConvertUnits(GetCenterFromGeometry(assemblyJointDefinition.OriginOne)); } catch {}
            warnings.Add("No joint center found for joint "+assemblyJointDefinition.Parent.Name);
            throw new Exception();
        }

        private dynamic GetCenterFromGeometry(GeometryIntent intent)
        {
            try { return GetCenter(intent.Geometry.Geometry); } catch {}
            try { return GetCenter(intent.Geometry); } catch {}
            try
            {
                if (intent.Point != null)
                    return intent.Point; // This seems to always be wrong. hmm...
            } catch {}
            throw new Exception();
        }

        private dynamic GetCenter(dynamic var)
        {
            try { return var.RootPoint; } catch {}
            try { return var.Center; } catch {}
            try { return var.MidPoint; } catch {}
            throw new Exception();
        }

        private JointLimits GetAngularLimits(AssemblyJointDefinition assemblyJointDefinition)
        {
            var limits = new JointLimits();
            limits.IsMaximumValueEnabled = assemblyJointDefinition.HasAngularPositionLimits;
            limits.IsMinimumValueEnabled = assemblyJointDefinition.HasAngularPositionLimits;
            limits.IsRestValueEnabled = false;
            if (limits.IsMaximumValueEnabled)
                limits.MaximumValue = assemblyJointDefinition.AngularPositionEndLimit.Value;
            if (limits.IsMinimumValueEnabled)
                limits.MinimumValue = assemblyJointDefinition.AngularPositionStartLimit.Value;
            // limits.RestValue = assemblyJointDefinition.AngularPosition;
            return limits;
        }

        private JointLimits GetLinearLimits(AssemblyJointDefinition assemblyJointDefinition)
        {
            var limits = new JointLimits();
            limits.IsMaximumValueEnabled = assemblyJointDefinition.HasLinearPositionEndLimit;
            limits.IsMinimumValueEnabled = assemblyJointDefinition.HasLinearPositionStartLimit;
            limits.IsRestValueEnabled = false;
            if (limits.IsMaximumValueEnabled)
                limits.MaximumValue = assemblyJointDefinition.LinearPositionEndLimit.Value;
            if (limits.IsMinimumValueEnabled)
                limits.MinimumValue = assemblyJointDefinition.LinearPositionStartLimit.Value;
            // limits.RestValue = assemblyJointDefinition.LinearPosition;
            return limits;
        }

        private double GetAngularPosition(AssemblyJointDefinition assemblyJointDefinition)
        {
            if (assemblyJointDefinition.AngularPosition != null)
                return assemblyJointDefinition.AngularPosition.Value;
            return 0;
        }

        private double GetLinearPosition(AssemblyJointDefinition assemblyJointDefinition)
        {
            if (assemblyJointDefinition.LinearPosition != null)
                return assemblyJointDefinition.LinearPosition.Value;
            return 0;
        }

        private RigidJointMotion GetRigidJointMotion(AssemblyJointDefinition assemblyJointDefinition)
        {
            var protoJointMotion = new RigidJointMotion();
            return protoJointMotion;
        }

        private RevoluteJointMotion GetRevoluteJointMotion(AssemblyJointDefinition assemblyJointDefinition)
        {
            var protoJointMotion = new RevoluteJointMotion();
            protoJointMotion.RotationAxisVector = GetNormalOrDirection(assemblyJointDefinition);
            protoJointMotion.RotationValue = GetAngularPosition(assemblyJointDefinition);
            protoJointMotion.RotationLimits = GetAngularLimits(assemblyJointDefinition);
            return protoJointMotion;
        }

        private SliderJointMotion GetSliderJointMotion(AssemblyJointDefinition assemblyJointDefinition)
        {
            var protoJointMotion = new SliderJointMotion();
            protoJointMotion.SlideDirectionVector = GetNormalOrDirection(assemblyJointDefinition);
            protoJointMotion.SlideValue = GetLinearPosition(assemblyJointDefinition);
            protoJointMotion.SlideLimits = GetLinearLimits(assemblyJointDefinition);
            return protoJointMotion;
        }
        private CylindricalJointMotion GetCylindricalJointMotion(AssemblyJointDefinition assemblyJointDefinition)
        {
            var protoJointMotion = new CylindricalJointMotion();
            protoJointMotion.RotationAxisVector = GetNormalOrDirection(assemblyJointDefinition);
            protoJointMotion.RotationValue = GetAngularPosition(assemblyJointDefinition);
            protoJointMotion.RotationLimits = GetAngularLimits(assemblyJointDefinition);
            
            protoJointMotion.SlideValue = GetLinearPosition(assemblyJointDefinition);
            protoJointMotion.SlideLimits = GetLinearLimits(assemblyJointDefinition);
            return protoJointMotion;
        }
        private PinSlotJointMotion GetPinSlotJointMotion(AssemblyJointDefinition assemblyJointDefinition)
        {
            var protoJointMotion = new PinSlotJointMotion();
            protoJointMotion.RotationAxisVector = GetNormalOrDirection(assemblyJointDefinition);
            protoJointMotion.RotationValue = GetAngularPosition(assemblyJointDefinition);
            protoJointMotion.RotationLimits = GetAngularLimits(assemblyJointDefinition);
            
            protoJointMotion.SlideDirectionVector = GetNormalOrDirection(assemblyJointDefinition);
            protoJointMotion.SlideValue = GetLinearPosition(assemblyJointDefinition);
            protoJointMotion.SlideLimits = GetLinearLimits(assemblyJointDefinition);
            return protoJointMotion;
        }
        private PlanarJointMotion GetPlanarJointMotion(AssemblyJointDefinition assemblyJointDefinition)
        {
            var protoJointMotion = new PlanarJointMotion();
            warnings.Add("Limits not implemented for planar joints! Joint: "+assemblyJointDefinition.Parent.Name);
            // TODO
            return protoJointMotion;
        }
        private BallJointMotion GetBallJointMotion(AssemblyJointDefinition assemblyJointDefinition)
        {
            var protoJointMotion = new BallJointMotion();
            warnings.Add("Limits not implemented for ball joints! Joint: "+assemblyJointDefinition.Parent.Name);
            // TODO
            return protoJointMotion;
        }

        private string GetJointedOccurrenceUUID(ComponentOccurrence occurrence)
        {
            return string.Join("+", new List<ComponentOccurrence>(occurrence.OccurrencePath.Cast<ComponentOccurrence>()).Select(o => o.Name));
        }

        private static Vector3D GetVector3DConvertUnits(dynamic getJointCenter)
        {
            var protoJointOrigin = new Vector3D();
            protoJointOrigin.X = getJointCenter.X/100;
            protoJointOrigin.Y = getJointCenter.Y/100;
            protoJointOrigin.Z = getJointCenter.Z/100;
            return protoJointOrigin;
        }
        
        private static Vector3D GetVector3D(dynamic hasXYZ)
        {
            var protoJointOrigin = new Vector3D();
            protoJointOrigin.X = hasXYZ.X;
            protoJointOrigin.Y = hasXYZ.Y;
            protoJointOrigin.Z = hasXYZ.Z;
            return protoJointOrigin;
        }
        
        private SceneBuilder ExportScene(AssemblyDocument assemblyDocument)
        {
            var scene = new SceneBuilder();
            ExportNodeRootAssembly(assemblyDocument, scene);
            return scene;
        }

        private void ExportNodeRootAssembly(AssemblyDocument assemblyDocument, SceneBuilder scene)
        {
            var root = new NodeBuilder(assemblyDocument.DisplayName);
            var assemblyComponentDefinition = assemblyDocument.ComponentDefinition;
            allDocumentJoints.AddRange(assemblyComponentDefinition.Joints.Cast<AssemblyJoint>());
            ExportNodes(assemblyComponentDefinition.Occurrences.Cast<ComponentOccurrence>(), scene, root);
        }

        private void ExportNodes(IEnumerable<ComponentOccurrence> occurrences, SceneBuilder scene, NodeBuilder parent)
        {
            foreach (ComponentOccurrence childOccurrence in occurrences)
            {
                ExportNode(childOccurrence, scene, parent.CreateNode());
            }
        }

        private void ExportNode(ComponentOccurrence componentOccurrence, SceneBuilder scene, NodeBuilder node)
        {
            var componentOccurrenceDefinition = componentOccurrence.Definition;

            ExportAppearanceCached(componentOccurrence.Appearance);

            // node.LocalTransform = InvToGltfMatrix4X4(componentOccurrence.Transformation); TODO: mesh caching by using definitions instead of occurrences (materials were an issue)
            switch (componentOccurrenceDefinition.Type)
            {
                case ObjectTypeEnum.kAssemblyComponentDefinitionObject:
                    ExportNodeAssembly(componentOccurrence, scene, node);
                    break;
                case ObjectTypeEnum.kPartComponentDefinitionObject:
                    ExportNodePart(componentOccurrence, scene, node);
                    break;
            }
        }
        private void ExportNodeAssembly(ComponentOccurrence componentOccurrence, SceneBuilder scene, NodeBuilder node)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var assemblyComponentDefinition = (AssemblyComponentDefinition)componentOccurrence.Definition;
            try
            {
                if (visitedDocuments.Add(assemblyComponentDefinition.Document.InternalName()))
                {
                    progressBar.Message = "Exporting mesh " + visitedDocuments.Count + " of " + progressTotal + "...";
                    progressBar.UpdateProgress();
                }
            }
            catch { }

            allDocumentJoints.AddRange(assemblyComponentDefinition.Joints.Cast<AssemblyJoint>());
            ExportNodes(componentOccurrence.SubOccurrences.Cast<ComponentOccurrence>(), scene, node);
        }

        private void ExportNodePart(ComponentOccurrence componentOccurrence, SceneBuilder scene, NodeBuilder node)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var partComponentDefinition = (PartComponentDefinition)componentOccurrence.Definition;
            try
            {
                if (visitedDocuments.Add(partComponentDefinition.Document.InternalName()))
                {
                    progressBar.Message = "Exporting mesh " + visitedDocuments.Count + " of " + progressTotal + "...";
                    progressBar.UpdateProgress();
                }
            }
            catch { }

            if (exportHidden || componentOccurrence.Visible)
            {
                scene.AddMesh(ExportMesh(componentOccurrence), node);
            }
        }

        private MeshBuilder<VertexPosition> ExportMesh(ComponentOccurrence componentOccurrence)
        {
            var mesh = new MeshBuilder<VertexPosition>("mesh");
            foreach (SurfaceBody surfaceBody in componentOccurrence.SurfaceBodies)
            {
                if (exportMaterials && exportFaceMaterials)
                {
                    var surfaceBodyFaces = surfaceBody.Faces;
                    foreach (Face face in surfaceBodyFaces)
                    {
                        ExportPrimitive(face, mesh.UsePrimitive(ExportAppearanceCached(face.Appearance)));
                    }
                }
                else
                {
                    ExportPrimitive(surfaceBody, mesh.UsePrimitive(ExportAppearanceCached(componentOccurrence.Appearance)));
                }
            }

            return mesh;
        }

        private MaterialBuilder ExportAppearanceCached(Asset appearance)
        {
            if (!exportMaterials || appearance == null)
                return defaultMaterial;

            var appearanceName = appearance.Name;
            if (materialCache.ContainsKey(appearanceName))
                return materialCache[appearanceName];
            
            var colorVector = Vector4.One;
            try
            {
                Color tempColor = AttemptGetColor(appearance);
                colorVector = new Vector4(tempColor.Red / 255f, tempColor.Green / 255f, tempColor.Blue / 255f, (float) tempColor.Opacity);
            }
            catch { }
            var material = new MaterialBuilder()
                .WithMetallicRoughnessShader()
                .WithChannelParam("BaseColor", colorVector )
                .WithChannelParam("MetallicRoughness", new Vector4(1,1,0,0) ); // TODO: metallic roughness
            materialCache[appearanceName] = material;
            return material;
        }

        private Color AttemptGetColor(Asset appearance)
        {
            try {return ((ColorAssetValue) appearance["generic_diffuse"]).Value;} catch {}
            try {return ((ColorAssetValue) appearance["masonrycmu_color"]).Value;} catch {}
            try {return ((ColorAssetValue) appearance["solidglass_transmittance_custom_color"]).Value;} catch {}
            try {return ((ColorAssetValue) appearance["hardwood_tint_color"]).Value;} catch {}
            try {return ((ColorAssetValue) appearance["water_tint_color"]).Value;} catch {}
            try {return ((ColorAssetValue) appearance["concrete_color"]).Value;} catch {}
            try {return ((ColorAssetValue) appearance["plasticvinyl_color"]).Value;} catch {}
            try {return ((ColorAssetValue) appearance["metallicpaint_base_color"]).Value;} catch {}
            try {return ((ColorAssetValue) appearance["wallpaint_color"]).Value;} catch {}
            try {return ((ColorAssetValue) appearance["metal_color"]).Value;} catch {}
            try {return ((ColorAssetValue) appearance["ceramic_color"]).Value;} catch {}
            try {return ((ColorAssetValue) appearance["glazing_transmittance_map"]).Value;} catch {}
            throw new Exception("Could not get color!");
        }

        private void PrintAssetLibrary(Application application)
        {
            using (StreamWriter file = new StreamWriter(@"C:\temp\InvAppearances.csv"))
            {
                file.Write("Appearance,");
                file.Write("DisplayName,");
                file.Write("HasTexture,");
                file.Write("IsReadOnly,");
                file.Write("Name,");
                file.WriteLine();

                foreach (AssetLibrary assetLib in application.AssetLibraries)
                {
                    // file.Write("Library" + assetLib.DisplayName);
                    // file.Write("  DisplayName: " + assetLib.DisplayName);
                    // file.Write("  FullFileName: " + assetLib.FullFileName);
                    // file.Write("  InternalName: " + assetLib.InternalName);
                    // file.Write("  IsReadOnly: " + assetLib.IsReadOnly);

                    foreach (Asset appearance in assetLib.AppearanceAssets)
                    {
                        file.Write(appearance.DisplayName + ",");
                        file.Write(appearance.HasTexture + ",");
                        file.Write(appearance.IsReadOnly + ",");
                        file.Write(appearance.Name + ",");
                        file.WriteLine();
                        PrintAsset(appearance, file);
                    }
                }
            }
        }

        private void PrintAsset(Asset appearance, StreamWriter file)
        {
            // file.Write("Appearance,");
            // file.Write("DisplayName,");
            // file.Write("Name,");
            file.Write(appearance.DisplayName+",");
            // file.Write("      HasTexture: " + appearance.HasTexture);
            // file.Write("      IsReadOnly: " + appearance.IsReadOnly);
            file.Write(appearance.Name+",");
            file.WriteLine();
            foreach (AssetValue o in appearance)
            {
                PrintAssetValue(o, file);
            }
            file.WriteLine();
            file.WriteLine();
        }

        private void PrintAssetValue(AssetValue InValue, StreamWriter file)
        {
            
            file.Write(InValue.Name+",");
            file.Write(InValue.IsReadOnly+",");

            switch (InValue.ValueType)
            {
                case AssetValueTypeEnum.kAssetValueTypeBoolean:
                    file.Write("Boolean,");

                    var booleanValue = InValue as BooleanAssetValue;

                    file.Write(booleanValue.Value+",");
                    break;
                case AssetValueTypeEnum.kAssetValueTypeChoice:
                    file.Write("Choice,");

                    var choiceValue = InValue as ChoiceAssetValue;

                    file.Write(choiceValue.Value + ",");

                    string[] names = new string[]{};
                    string[] choices = new string[]{};
                    choiceValue.GetChoices(out names, out choices);
                    for (int i = 0; i < names.Length; i++)
                    {
                        file.Write(" " + names[i] + ":" + choices[i] + ",");
                    }
                    break;
                case AssetValueTypeEnum.kAssetValueTypeColor:
                    file.Write("Color,");

                    var colorValue = InValue as ColorAssetValue;

                    file.Write(colorValue.HasConnectedTexture + ",");
                    file.Write(colorValue.HasMultipleValues + ",");

                    if (!colorValue.HasMultipleValues)
                    {
                        file.Write(ColorString(colorValue.Value, file));
                    }
                    else
                    {
                        file.Write("Colors,");

                        var colors = colorValue.get_Values();

                        foreach (var color in colors)
                        {
                            file.Write(ColorString(color, file));
                        }
                    }
                    break;

                case AssetValueTypeEnum.kAssetValueTypeFilename:
                    file.Write("Filename,");

                    var filenameValue = InValue as FilenameAssetValue;

                    file.Write(filenameValue.Value + ",");
                    break;

                case AssetValueTypeEnum.kAssetValueTypeFloat:
                    file.Write("Float,");

                    var floatValue = InValue as FloatAssetValue;

                    file.Write(floatValue.Value + ",");
                    break;

                case AssetValueTypeEnum.kAssetValueTypeInteger:
                    file.Write("Integer,");

                    var integerValue = InValue as IntegerAssetValue;

                    file.Write(integerValue.Value + ",");
                    break;

                case AssetValueTypeEnum.kAssetValueTypeReference:
                    file.Write("Reference,");

                    var refType = InValue as ReferenceAssetValue;
                    break;

                case AssetValueTypeEnum.kAssetValueTypeString:
                    file.Write("String,");

                    var stringValue = InValue as StringAssetValue;

                    file.Write(stringValue.Value + ",");
                    break;

            }
            file.WriteLine();
        }

        private string ColorString(Color color, StreamWriter file)
        {
            return "("+color.Red + " " + color.Green + " " + color.Blue + " " + color.Opacity+"),";
        }

        private void ExportPrimitive(SurfaceBody surfaceBody, PrimitiveBuilder<MaterialBuilder, VertexPosition, VertexEmpty, VertexEmpty> primitive)
        {
            ExportPrimitive(surfaceBody, null, primitive);
        }
        
        private void ExportPrimitive(Face surfaceFace, PrimitiveBuilder<MaterialBuilder, VertexPosition, VertexEmpty, VertexEmpty> primitive)
        {
            ExportPrimitive(null, surfaceFace, primitive);
        }
        private void ExportPrimitive(SurfaceBody surfaceBody, Face surfaceFace, PrimitiveBuilder<MaterialBuilder, VertexPosition, VertexEmpty, VertexEmpty> primitive)
        {
            int facetCount;
            int vertCount;
            var coords = new double[]{};
            var norms = new double[]{};
            var indices = new int[]{};
            if (surfaceBody != null) 
                surfaceBody.CalculateFacets((double) exportTolerance, out vertCount, out facetCount, out coords, out norms, out indices);
            else
                surfaceFace.CalculateFacets((double) exportTolerance, out vertCount, out facetCount, out coords, out norms, out indices);
            for (var c = 0; c < indices.Length; c += 3)
            {
                var indexA = indices[c];
                var indexB = indices[c+1];
                var indexC = indices[c+2];
                primitive.AddTriangle(
                    GetCoordinates(coords, indexA-1),
                    GetCoordinates(coords, indexB-1),
                    GetCoordinates(coords, indexC-1)
                );
            }
        }
        
        private VertexPosition GetCoordinates(double[] coords, int index)
        {
            return new VertexPosition(
                (float)coords[index * 3],
                (float)coords[index * 3 + 1],
                (float)coords[index * 3 + 2]
            );
        }
        
        
        private Matrix4x4 InvToGltfMatrix4X4(Matrix invTransform)
        {
            var transform = new Matrix4x4(
                (float) invTransform.Cell[1, 1],
                (float) invTransform.Cell[2, 1],
                (float) invTransform.Cell[3, 1],
                (float) invTransform.Cell[4, 1],
                (float) invTransform.Cell[1, 2],
                (float) invTransform.Cell[2, 2],
                (float) invTransform.Cell[3, 2],
                (float) invTransform.Cell[4, 2],
                (float) invTransform.Cell[1, 3],
                (float) invTransform.Cell[2, 3],
                (float) invTransform.Cell[3, 3],
                (float) invTransform.Cell[4, 3],
                (float) invTransform.Cell[1, 4],
                (float) invTransform.Cell[2, 4],
                (float) invTransform.Cell[3, 4],
                (float) invTransform.Cell[4, 4]
            );
            return AffineTransform.Evaluate(transform, null, null, null);
        }

        private void DebugPrintAppearance(Asset appearance)
        {
            Debug.Print("=========================+");
            Debug.Print(appearance.Name);
            Debug.Print(appearance.DisplayName);
            Debug.Print(appearance.CategoryName);
            
            foreach (AssetValue assetValue in appearance)
            {
                Debug.Print("--------------");
                Debug.Print(assetValue.Name);
                Debug.Print(assetValue.DisplayName);
                switch (assetValue.ValueType)
                {
                    case AssetValueTypeEnum.kAssetValueTypeColor:
                        var colorAssetValue = (ColorAssetValue) assetValue;
                        if (colorAssetValue.HasMultipleValues)
                        {
                            foreach (var value in colorAssetValue.get_Values())
                            {
                                Debug.Print(value.Red.ToString());
                                Debug.Print(value.Green.ToString());
                                Debug.Print(value.Blue.ToString());
                                Debug.Print(value.Opacity.ToString());
                            }
                        }
                        else
                        {
                            Debug.Print(colorAssetValue.Value.Red.ToString());
                            Debug.Print(colorAssetValue.Value.Green.ToString());
                            Debug.Print(colorAssetValue.Value.Blue.ToString());
                            Debug.Print(colorAssetValue.Value.Opacity.ToString());
                        }
                        break;
                    case AssetValueTypeEnum.kAssetValueTypeBoolean:
                        var booleanAssetValue = (BooleanAssetValue) assetValue;
                        Debug.Print(booleanAssetValue.Value.ToString());
                        break;
                    case AssetValueTypeEnum.kAssetValueTypeChoice:
                        var choiceAssetValue = (ChoiceAssetValue) assetValue;
                        Debug.Print(choiceAssetValue.Value);
                        break;
                    case AssetValueTypeEnum.kAssetValueTypeFloat:
                        var floatAssetValue = (FloatAssetValue) assetValue;
                        if (floatAssetValue.HasMultipleValues)
                        {
                            foreach (var value in floatAssetValue.get_Values())
                            {
                                Debug.Print(value.ToString());
                            }
                        }
                        else
                        {
                            Debug.Print(floatAssetValue.Value.ToString());
                        }
                        break;
                    case AssetValueTypeEnum.kAssetValueTypeInteger:
                        var integerAssetValue = (IntegerAssetValue) assetValue;
                        if (integerAssetValue.HasMultipleValues)
                        {
                            foreach (var value in integerAssetValue.get_Values())
                            {
                                Debug.Print(value.ToString());
                            }
                        }
                        else
                        {
                            Debug.Print(integerAssetValue.Value.ToString());
                        }
                        break;
                    case AssetValueTypeEnum.kAssetValueTypeString:
                        var stringAssetValue = (StringAssetValue) assetValue;
                        Debug.Print(stringAssetValue.Value);
                        break;
                }
            }
        }
    }
}