using System;
using System.Dynamic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using SynthesisAPI.VirtualFileSystem;
using SharpGLTF;
using SharpGLTF.Schema2;
using System.Threading;
using SynthesisAPI.Utilities;
using System.Xml.Serialization;
using SynthesisAPI.EnvironmentManager;
using static SynthesisAPI.EnvironmentManager.Design;
using System.Linq;
using SharpGLTF.Memory;
using UnityEngine.Assertions.Must;
using SharpGLTF.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.Runtime;
using Logger = SynthesisAPI.Utilities.Logger;

namespace SynthesisAPI.AssetManager
{
    public class GltfAsset : Asset
    {
        private ModelRoot model = null;

        public GltfAsset(string name, Permissions perm, string sourcePath)
        {
            Init(name, perm, sourcePath);
        }

        public override IEntry Load(byte[] data)
        {
            var stream = new MemoryStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;

            GetModelInfo(stream, true);

            return this;
        }

        private void GetModelInfo(MemoryStream stream, bool tryFix = false)
        {
            try
            {
                var settings = tryFix ? SharpGLTF.Validation.ValidationMode.TryFix : SharpGLTF.Validation.ValidationMode.Strict;

                model = ModelRoot.ReadGLB(stream, settings);
            }
            catch (Exception ex)
            {
                ApiProvider.Log("GLTF file cannot be read");
            }
        }

        #region Object Bundle

        public static implicit operator Bundle(GltfAsset gltfAsset) => gltfAsset.Parse();

        public Bundle Parse()
        {
            if (model == null) return null;
            return CreateBundle(model.DefaultScene.VisualChildren.First()); 
        }

        /// <summary>
        /// Parses the individual elements of a gltf model.
        /// </summary>
        /// <param name="modelRoot"></param>
        /// <returns></returns>
        private Bundle CreateBundle(Node root)
        {
            Bundle bundle = new Bundle();
            AddComponents(bundle, root);
            foreach (Node child in root.VisualChildren)
                bundle.ChildBundles.Add(CreateBundle(child));
            return bundle;
        }

        private void AddComponents(Bundle bundle, Node node)
        {
            bundle.Components.Add(ParseTransform(node.LocalTransform));
            if (node.Mesh != null) bundle.Components.Add(ParseMesh(node.Mesh, node.LocalTransform.Scale.ToMathNet()));
        }

        private EnvironmentManager.Components.Mesh ParseMesh(SharpGLTF.Schema2.Mesh nodeMesh, Vector3D scaleFactor)
        {
            // Logger.Log($"Scale Factor: {scaleFactor.X}, {scaleFactor.Y}, {scaleFactor.Z}");

            EnvironmentManager.Components.Mesh m = new EnvironmentManager.Components.Mesh();
            foreach (SharpGLTF.Schema2.MeshPrimitive primitive in nodeMesh.Primitives)
            {
                int c = m.Vertices.Count();
                // checks for POSITION or NORMAL vertex as not all designs have both
                if (primitive.VertexAccessors.ContainsKey("POSITION"))
                {
                    Vector3Array vertices = primitive.GetVertices("POSITION").AsVector3Array();
                    foreach (System.Numerics.Vector3 vertex in vertices)
                        m.Vertices.Add(new Vector3D(vertex.X * scaleFactor.X, vertex.Y * scaleFactor.Y, vertex.Z * scaleFactor.Z));
                }

                var triangles = primitive.GetIndices();
                for (int i = 0; i < triangles.Count; i++)
                    m.Triangles.Add((int)triangles[i] + c);
            }
            return m;
        }
        private EnvironmentManager.Components.Transform ParseTransform(SharpGLTF.Transforms.AffineTransform nodeTransform)
        {
            EnvironmentManager.Components.Transform t = new EnvironmentManager.Components.Transform();

            t.Rotation = new MathNet.Spatial.Euclidean.Quaternion(nodeTransform.Rotation.W, nodeTransform.Rotation.X,
                nodeTransform.Rotation.Y, nodeTransform.Rotation.Z);
            // t.Scale = new MathNet.Spatial.Euclidean.Vector3D(nodeTransform.Scale.X, nodeTransform.Scale.Y, nodeTransform.Scale.Z);
            t.Position = new MathNet.Spatial.Euclidean.Vector3D(nodeTransform.Translation.X * nodeTransform.Scale.X,
                nodeTransform.Translation.Y * nodeTransform.Scale.Y, nodeTransform.Translation.Z * nodeTransform.Scale.Z);

            return t;
        }
        #endregion
    }
}
