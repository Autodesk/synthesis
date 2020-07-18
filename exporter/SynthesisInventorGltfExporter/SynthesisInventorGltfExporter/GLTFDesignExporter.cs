using System;
using System.Diagnostics;
using System.Numerics;
using Inventor;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;

namespace SynthesisInventorGltfExporter
{
    public class GLTFDesignExporter
    {
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
            ExpotNodeRootAssembly(assemblyDocument, scene);
            return scene;
        }

        private void ExpotNodeRootAssembly(AssemblyDocument assemblyDocument, SceneBuilder scene)
        {
            var node = new NodeBuilder(assemblyDocument.DisplayName);
            foreach (ComponentOccurrence childOccurrence in assemblyDocument.ComponentDefinition.Occurrences)
            {
                ExportNode(childOccurrence, scene, node.CreateNode());
            }
        }

        private void ExportNode(ComponentOccurrence componentOccurrence, SceneBuilder scene, NodeBuilder createNode)
        {
            var componentOccurrenceDefinition = componentOccurrence.Definition;

            switch (componentOccurrenceDefinition.Type)
            {
                case ObjectTypeEnum.kAssemblyComponentDefinitionObject:
                    ExportNodeAssembly(componentOccurrence, scene, createNode);
                    break;
                case ObjectTypeEnum.kPartComponentDefinitionObject:
                    ExportNodePart(componentOccurrence, scene, createNode);
                    break;
            }
        }

        private void ExportNodeAssembly(ComponentOccurrence componentOccurrence, SceneBuilder scene, NodeBuilder node)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var assemblyComponentDefinition = (AssemblyComponentDefinition)componentOccurrence.Definition;

            foreach (ComponentOccurrence childOccurrence in assemblyComponentDefinition.Occurrences)
            {
                ExportNode(childOccurrence, scene, node.CreateNode());
            }
        }

        private void ExportNodePart(ComponentOccurrence componentOccurrence, SceneBuilder scene, NodeBuilder node)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            var partComponentDefinition = (PartComponentDefinition)componentOccurrence.Definition;

            scene.AddMesh(ExportMesh(partComponentDefinition), node);
        }

        private MeshBuilder<VertexPosition> ExportMesh(PartComponentDefinition partComponentDefinition)
        {
            var mesh = new MeshBuilder<VertexPosition>("mesh");
            foreach (SurfaceBody surfaceBody in partComponentDefinition.SurfaceBodies)
            {
                ExportPrimitive(surfaceBody, mesh.UsePrimitive(ExportAppearance(surfaceBody.Appearance)));
            }

            return mesh;
        }

        private MaterialBuilder ExportAppearance(Asset surfaceBodyAppearance)
        {
            return defaultMat;
        }

        private static MaterialBuilder defaultMat = new MaterialBuilder()
            .WithMetallicRoughnessShader()
            .WithChannelParam("BaseColor", Vector4.One);

        private void ExportPrimitive(SurfaceBody surfaceBody, PrimitiveBuilder<MaterialBuilder, VertexPosition, VertexEmpty, VertexEmpty> primitive)
        {
            int facetCount;
            int vertCount;
            var coords = new double[]{};
            var norms = new double[]{};
            var indices = new int[]{};
            surfaceBody.CalculateFacets(1, out vertCount, out facetCount, out coords, out norms, out indices);
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

        private static VertexPosition GetCoordinates(double[] coords, int index)
        {
            return new VertexPosition(
                (float)coords[index * 3],
                (float)coords[index * 3 + 1],
                (float)coords[index * 3 + 2]
            );
        }
    }
}