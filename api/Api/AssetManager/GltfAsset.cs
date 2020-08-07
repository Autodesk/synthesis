using System.Collections.Specialized;
using System.Collections.Generic;
using System;
using System.IO;
using SynthesisAPI.VirtualFileSystem;
using SharpGLTF.Schema2;
using SynthesisAPI.Utilities;
using SynthesisAPI.EnvironmentManager;
using System.Linq;
using SharpGLTF.Memory;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.Runtime;
using SharpGLTF.IO;
using Newtonsoft.Json.Linq;

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
            catch (Exception)
            {
                Logger.Log("GLTF file cannot be read", LogLevel.Warning);
            }
        }

        #region Object Bundle

        public static implicit operator Bundle(GltfAsset gltfAsset) => gltfAsset.Parse();

        public Bundle Parse()
        {
            if (model == null) return null;
            var bundle = CreateBundle(model.DefaultScene.VisualChildren.First());

            bundle.Components.Add(ParseJoints());

            // ParseJoints(ref bundle);
            return bundle;
        }

        private Dictionary<string, EnvironmentManager.Components.Rigidbody> rigidbodies = new Dictionary<string, EnvironmentManager.Components.Rigidbody>();

        private Bundle CreateBundle(Node node, Node parent = null)
        {
            Bundle bundle = new Bundle();

            AddComponents(bundle, node, parent);

            foreach (Node child in node.VisualChildren)
                bundle.ChildBundles.Add(CreateBundle(child, node));

            return bundle;
        }

        /*private void AddExtras(Bundle bundle)
        {
            JsonDictionary extras = (JsonDictionary)model.Extras;

            //todo: Joints
            EnvironmentManager.Components.JointCollection jl = new EnvironmentManager.Components.JointCollection();

            foreach (JsonDictionary joint in (JsonList)extras["joints"])
                jl.Add(ParseJoints(joint));
            bundle.Components.Add(jl);
        }*/

        private void AddComponents(Bundle bundle, Node node, Node parent = null)
        {
            if (parent != null) {
                var scale = node.LocalTransform.Scale;
                var parentScale = parent.LocalTransform.Scale;
                scale = new System.Numerics.Vector3(scale.X * parentScale.X, scale.Y * parentScale.Y, scale.Z * parentScale.Z);
                var localTransform = node.LocalTransform;
                localTransform.Scale = scale;
                node.LocalTransform = localTransform;
            }

            bundle.Components.Add(ParseTransform(node.LocalTransform, node.Name));
            if (node.Mesh != null)
            {
                var sc = node.LocalTransform.Scale;
                bundle.Components.Add(ParseMesh(node.Mesh, new Vector3D(sc.X, sc.Y, sc.Z)));
                bundle.Components.Add(ParseMeshCollider());
                var rigid = ParseRigidbody();
                bundle.Components.Add(rigid);
                rigidbodies.Add(node.Name, rigid);
            }
        }

        private EnvironmentManager.Components.Rigidbody ParseRigidbody() // TODO: Get physical properties (Mass)
        {
            EnvironmentManager.Components.Rigidbody rigidbody = new EnvironmentManager.Components.Rigidbody();
            return rigidbody;
        }

        private EnvironmentManager.Components.MeshCollider ParseMeshCollider()
        {
            EnvironmentManager.Components.MeshCollider collider = new EnvironmentManager.Components.MeshCollider();
            return collider;
        }

        private EnvironmentManager.Components.Mesh ParseMesh(Mesh nodeMesh, Vector3D scaleFactor)
        {
            EnvironmentManager.Components.Mesh m = new EnvironmentManager.Components.Mesh();
            foreach (MeshPrimitive primitive in nodeMesh.Primitives)
            {
                int c = m.Vertices.Count();
                // checks for POSITION or NORMAL vertex as not all designs have both (TODO: This doesn't trip, if it did would we screw up the triangles?)
                if (primitive.VertexAccessors.ContainsKey("POSITION"))
                {
                    var vertices = primitive.GetVertices("POSITION").AsVector3Array();
                    foreach (var vertex in vertices)
                        m.Vertices.Add(new Vector3D(vertex.X * scaleFactor.X, vertex.Y * scaleFactor.Y, vertex.Z * scaleFactor.Z));
                }

                var triangles = primitive.GetIndices();
                for (int i = 0; i < triangles.Count; i++)
                    m.Triangles.Add((int)triangles[i] + c);
            }
            return m;
        }

        private EnvironmentManager.Components.Transform ParseTransform(SharpGLTF.Transforms.AffineTransform nodeTransform, string name)
        {
            EnvironmentManager.Components.Transform t = new EnvironmentManager.Components.Transform();
            // t.Name = name;

            //var quat = .Inversed;

            t.Rotation = new Quaternion(nodeTransform.Rotation.W,
                nodeTransform.Rotation.X, nodeTransform.Rotation.Y, nodeTransform.Rotation.Z);
            t.Position = new Vector3D(nodeTransform.Translation.X * nodeTransform.Scale.X,
                nodeTransform.Translation.Y * nodeTransform.Scale.Y, nodeTransform.Translation.Z * nodeTransform.Scale.Z);
            //scale is applied directly to vertices -> default 1x

            // t.Rotate(new Vector3D(180, 0, 0));

            return t;
        }

        private EnvironmentManager.Components.Joints ParseJoints()
        {
            var joints = (model.Extras as JsonDictionary)["joints"] as JsonList;
            joints.ToString();
            // Logger.Log(joints.GetType().FullName);

            var allJoints = new EnvironmentManager.Components.Joints();

            foreach (var jointObj in joints) {
                var joint = jointObj as JsonDictionary;
                string name = joint.Get("header").Get<string>("name"); // Probably not gonna be used
                var originObj = joint.Get("origin");
                Vector3D anchor = new Vector3D((double)originObj.TryGet<decimal>("x", 0),
                    (double)originObj.TryGet<decimal>("y", 0), (double)originObj.TryGet<decimal>("z", 0));
                EnvironmentManager.Components.Rigidbody parent = rigidbodies[joint.Get<string>("occurrenceOneUUID")];
                EnvironmentManager.Components.Rigidbody child = rigidbodies[joint.Get<string>("occurrenceTwoUUID")];
                if (joint.ContainsKey("revoluteJointMotion")) {
                    var rotAxisVec = joint.Get("revoluteJointMotion").Get("rotationAxisVector"); // If any of these fail, it is an invalid joint export
                    Vector3D axis = new Vector3D((double)rotAxisVec.TryGet<decimal>("x", 0),
                        (double)rotAxisVec.TryGet<decimal>("y", 0), (double)rotAxisVec.TryGet<decimal>("z", 0));
                    // TODO: Support limits
                    var result = new EnvironmentManager.Components.HingeJoint();
                    result.Anchor = anchor;
                    result.Axis = axis;
                    result.ConnectedParent = parent;
                    result.ConnectedChild = child;
                    allJoints.Add(result);
                } else {
                    Logger.Log("Joint type not found, defaulting to a fixed joint", LogLevel.Warning);
                    var result = new EnvironmentManager.Components.FixedJoint();
                    result.Anchor = anchor;
                    result.ConnectedParent = parent;
                    result.ConnectedChild = child;
                    allJoints.Add(result);
                }
            }

            return allJoints;
        }
        #endregion
    }

    public static class JsonDictionaryExtensions {
        public static T Get<T>(this JsonDictionary dict, string key) => (T)dict[key];
        public static T TryGet<T>(this JsonDictionary dict, string key, T defaultObj) {
            if (dict.ContainsKey(key))
                return (T)dict[key];
            else
                return defaultObj;
        }
        public static JsonDictionary Get(this JsonDictionary dict, string key) => (JsonDictionary)dict[key];
    }
}
