using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Inventor;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Transforms;

namespace SynthesisInventorGltfExporter
{
    public class GLTFDesignExporter
    {
        private MaterialBuilder defaultMaterial = new MaterialBuilder()
            .WithMetallicRoughnessShader()
            .WithChannelParam("BaseColor", Vector4.One);

        private Dictionary<string, MaterialBuilder> materialCache = new Dictionary<string, MaterialBuilder>();
        
        public void ExportDesign(AssemblyDocument assemblyDocument)
        {
            var sceneBuilder = ExportScene(assemblyDocument);
            var filename = assemblyDocument.DisplayName;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }
            sceneBuilder.ToSchema2().SaveGLB("C:/temp/" + filename + ".glb");
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
            ExportNodes(assemblyDocument.ComponentDefinition.Occurrences.Cast<ComponentOccurrence>(), scene, root);
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
            // var assemblyComponentDefinition = (AssemblyComponentDefinition)componentOccurrence.Definition;
            ExportNodes(componentOccurrence.SubOccurrences.Cast<ComponentOccurrence>(), scene, node);
        }

        private void ExportNodePart(ComponentOccurrence componentOccurrence, SceneBuilder scene, NodeBuilder node)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            // var partComponentDefinition = (PartComponentDefinition)componentOccurrence.Definition;
            scene.AddMesh(ExportMesh(componentOccurrence), node);
        }

        private MeshBuilder<VertexPosition> ExportMesh(ComponentOccurrence componentOccurrence)
        {
            var mesh = new MeshBuilder<VertexPosition>("mesh");
            foreach (SurfaceBody surfaceBody in componentOccurrence.SurfaceBodies)
            {
                var surfaceBodyFaces = surfaceBody.Faces;
                foreach (Face face in surfaceBodyFaces)
                {
                    ExportPrimitive(face, mesh.UsePrimitive(ExportAppearanceCached(face.Appearance)));
                }
            }

            return mesh;
        }

        private MaterialBuilder ExportAppearanceCached(Asset appearance)
        {
            if (appearance == null)
                return defaultMaterial;
            if (materialCache.ContainsKey(appearance.Name))
                return materialCache[appearance.Name];
            var colorVector = Vector4.One;
            try
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                Color tempColor = ((ColorAssetValue)appearance["generic_diffuse"]).Value;
                colorVector = new Vector4(tempColor.Red/255f, tempColor.Green/255f, tempColor.Blue/255f, (float) tempColor.Opacity);
            }
            catch (ArgumentException) {}
            var material = new MaterialBuilder()
                .WithMetallicRoughnessShader()
                .WithChannelParam("BaseColor", colorVector )
                .WithChannelParam("MetallicRoughness", new Vector4(1,1,0,0) ); // TODO: metallic roughness
            materialCache[appearance.Name] = material;
            return material;
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
                surfaceBody.CalculateFacets(1, out vertCount, out facetCount, out coords, out norms, out indices);
            else
                surfaceFace.CalculateFacets(1, out vertCount, out facetCount, out coords, out norms, out indices);
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