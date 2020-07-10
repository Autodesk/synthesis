using System;
using System.Dynamic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.VirtualFileSystem;
using glTFLoader;
using SharpGLTF;
using SharpGLTF.Schema2;
using System.Threading;
using SynthesisAPI.Utilities;
using System.Xml.Serialization;
using SynthesisAPI.EnvironmentManager;
using static SynthesisAPI.EnvironmentManager.Design;
using System.Linq;

namespace SynthesisAPI.AssetManager
{
    public class GltfAsset : Asset
    {
        public GltfAsset(string name, Guid owner, Permissions perm, string sourcePath)
        {
            Init(name, owner, perm, sourcePath);
        }

        public override IEntry Load(byte[] data)
        {
            var stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;

            ModelRoot model = null;
            bool tryFix = false;
            model = GetModelInfo(model, stream, tryFix);

            return this;
        }

        private ModelRoot GetModelInfo(ModelRoot model, MemoryStream stream, bool tryFix = false)
        {
            try
            {
                var settings = tryFix ? SharpGLTF.Validation.ValidationMode.TryFix : SharpGLTF.Validation.ValidationMode.Strict;

                model = ModelRoot.ReadGLB(stream, settings);
                ImportDesign(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed");
            }

            return model;
        }

        public Design ImportDesign(ModelRoot modelRoot)
        {
            Design design = new Design();

            // this is the root ---> modelRoot.DefaultScence.VisualChildren
            foreach (Node child in modelRoot.DefaultScene.VisualChildren)
            {
                design.RootOccurence = ExportOccurrenceFromNode(child);
            }

            return design;
        }

        public Occurrence ExportOccurrenceFromNode(Node node)
        {
            //// if node is in the Components maps, return it
            //if (Components[node.LogicalIndex].Equals(node.LogicalIndex))
            //{
            //    // return map component
            //} // else export the node

            Occurrence occurrence = new Occurrence();

            // todo:
            // OccurenceHeader
            // IsGrounded
            // Transform
            // ComponentUuid
            // Attributes

            occurrence.AComponent = ExportComponentsFromMesh(node.Mesh);

            foreach (Node child in node.VisualChildren)
            {
                occurrence.ChildOccurences.Add(ExportOccurrenceFromNode(child));
            }

            return occurrence;
        }

        private Design.Component ExportComponentsFromMesh(SharpGLTF.Schema2.Mesh mesh)
        {
            // todo: 
            // export mesh ONLY IF it hasn't been exported yet

            Design.Component component = new Design.Component();

            // todo:
            // ComponentHeader
            // PartNumber
            // BoundingBox
            // MaterialId
            // ComponentPhysicalProperties
            // Attributes

            // MeshBodies
            foreach (SharpGLTF.Schema2.MeshPrimitive meshBody in mesh.Primitives)
            {
                component.MeshBodies.Add(ExportMeshBodiesFromPrimitives(meshBody));
            }

            Components.Add(mesh.LogicalIndex, component);

            return component;
        }

        private MeshBody ExportMeshBodiesFromPrimitives(MeshPrimitive primitive)
        {
            MeshBody meshBody = new MeshBody();

            //Vertices = new List<double>();
            //Normals = new List<double>();
            //Uvs = new List<double>();
            //Indices = new List<int>();

            meshBody.TriangleMesh.Indices = (IList<int>)primitive.GetPoin();
            // triangle mesh

            return meshBody;
        }
    }
}
