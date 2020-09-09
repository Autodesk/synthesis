using System.Collections.Generic;
using System;
using System.IO;
using SynthesisAPI.VirtualFileSystem;
using SharpGLTF.Schema2;
using SynthesisAPI.Utilities;
using SynthesisAPI.EnvironmentManager;
using System.Linq;
using MathNet.Spatial.Euclidean;
using SharpGLTF.IO;
using SynthesisAPI.EventBus;

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
                Logger.Log($"GLTF asset \"{Name}\" either could not be read or could not be interpretted", LogLevel.Error);
            }
        }

        #region Object Bundle

        public static implicit operator Bundle(GltfAsset gltfAsset) => gltfAsset.Parse();

        private Dictionary<string, List<string>> preprocessedJoints;
        private Dictionary<string, EnvironmentManager.Components.Rigidbody> rigidbodies;
        private List<(EnvironmentManager.Components.Rigidbody, EnvironmentManager.Components.Rigidbody)> defaultRigidjoints;
        private List<SharpGLTF.Schema2.Asset> exporterType;
        private bool rigidbodyPresent;
        public Bundle Parse()
        {
            if (model == null) return null;

            preprocessedJoints = new Dictionary<string, List<string>>();
            rigidbodies = new Dictionary<string, EnvironmentManager.Components.Rigidbody>();
            defaultRigidjoints = new List<(EnvironmentManager.Components.Rigidbody, EnvironmentManager.Components.Rigidbody)>();
            exporterType = new List<SharpGLTF.Schema2.Asset>();
            rigidbodyPresent = false;

            PreprocessJoints();
            ExportInfoGathering();

            var bundle = CreateBundle(model.DefaultScene.VisualChildren.First());

            bundle.Components.Add(ParseJoints());

            return bundle;
        }

        private void PreprocessJoints() {
            RecursiveUuidGathering(model.DefaultScene.VisualChildren.First());
            var joints = (model.Extras as JsonDictionary)["joints"] as JsonList;
            foreach (var j in joints) {
                preprocessedJoints[(j as JsonDictionary).Get<string>("occurrenceOneUUID")].Add((j as JsonDictionary).Get<string>("occurrenceTwoUUID"));
                preprocessedJoints[(j as JsonDictionary).Get<string>("occurrenceTwoUUID")].Add((j as JsonDictionary).Get<string>("occurrenceOneUUID"));
            }
        }

        private void RecursiveUuidGathering(Node root) {
            if ((root.Extras as JsonDictionary)?.ContainsKey("uuid") ?? false) {
                preprocessedJoints.Add((string)(root.Extras as JsonDictionary)!["uuid"], new List<string>());
            }
            foreach (Node child in root.VisualChildren)
                RecursiveUuidGathering(child);
        }

        private void ExportInfoGathering()
        {
            Analytics.SetUnityPrefs("177922339", true);
            string generator = model.Asset.Generator;
            Analytics.LogEventAsync(Analytics.EventCategory.ExporterType, Analytics.EventAction.Load, generator, 10);
            Analytics.UploadDump();

            var version = model.Asset.MinVersion;
            Analytics.LogEventAsync(Analytics.EventCategory.ExporterVersion, Analytics.EventAction.Load, version.ToString(), 10);
            Analytics.UploadDump();
        }



        private Bundle CreateBundle(Node node, Node parentNode = null, Bundle parentBundle = null)
        {
            Bundle bundle = new Bundle(parentBundle);

            AddComponents(bundle, node, parentNode);

            foreach (Node child in node.VisualChildren) {
                bundle.ChildBundles.Add(CreateBundle(child, node, bundle));
            }

            return bundle;
        }

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
                bundle.Components.Add(ParseMeshCollider(node.Mesh));

                if ((node.Extras as JsonDictionary)?.ContainsKey("uuid") ?? false) {
                    var rigid = ParseRigidbody(node.Mesh);
                    bundle.Components.Add(rigid);
                    var uuid = (node.Extras as JsonDictionary)?.Get<string>("uuid");
                    rigid.ExportedJointUuid = uuid;
                    rigidbodies.Add(uuid, rigid);

                    // Identify parent physics body (if any)
                    var seniorRB = GetSeniorRigidbody(bundle.ParentBundle);
                    if (seniorRB != null) {
                        if (seniorRB.ExportedJointUuid == string.Empty) {
                            defaultRigidjoints.Add((seniorRB, rigid));
                        } else {
                            if (!preprocessedJoints[uuid].Contains(seniorRB.ExportedJointUuid)) {
                                defaultRigidjoints.Add((seniorRB, rigid));
                            }
                        }
                    } else {
                        Logger.Log("Not rigidbody???", LogLevel.Warning);
                    }
                } else if (parent == null) {
                    var rigid = ParseRigidbody(node.Mesh);
                    bundle.Components.Add(rigid);
                    return;
                }
                else
                {

                    var seniorRB = GetSeniorRigidbody(bundle.ParentBundle);
                    //if(seniorRB != null)
                    //{
                    //    seniorRB.mass += ParseMass(node.Mesh) ?? 0;
                    //}
                }
            } else if (parent == null) {
                var rigid = ParseRigidbody(null);
                bundle.Components.Add(rigid);
            }
        }

        private EnvironmentManager.Components.Rigidbody? GetSeniorRigidbody(Bundle? b) {
            if (b == null)
                return null;
            else if (b.Components.HasType<EnvironmentManager.Components.Rigidbody>())
                return b.Components.Get<EnvironmentManager.Components.Rigidbody>();
            else
                return GetSeniorRigidbody(b.ParentBundle);
        }

        private static float? ParseMass(Mesh nodeMesh, float defaultMass = 1)
        {
            if (nodeMesh != null)
            {
                if ((nodeMesh.Extras as JsonDictionary).TryGetValue("physicalProperties", out object physicalProperties))
                {
                    if ((physicalProperties as JsonDictionary).TryGetValue("mass", out object massStr))
                    {
                        if (float.TryParse(massStr.ToString(), out float mass))
                        {
                            return mass;
                        }
                    }
                }
                return null;
            }
            return defaultMass;
        }

        private EnvironmentManager.Components.Rigidbody ParseRigidbody(Mesh nodeMesh)
        {
            EnvironmentManager.Components.Rigidbody rigidbody = new EnvironmentManager.Components.Rigidbody();
            rigidbody.mass = ParseMass(nodeMesh) ?? throw new SynthesisException($"Failed to parse mass of rigidbody in GLTF asset: {Name}");
            return rigidbody;
        }

        private EnvironmentManager.Components.MeshCollider ParseMeshCollider(Mesh nodeMesh)
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

            t.Rotation = new Quaternion(nodeTransform.Rotation.W,
                nodeTransform.Rotation.X, nodeTransform.Rotation.Y, nodeTransform.Rotation.Z);
            t.Position = new Vector3D(nodeTransform.Translation.X * nodeTransform.Scale.X,
                nodeTransform.Translation.Y * nodeTransform.Scale.Y, nodeTransform.Translation.Z * nodeTransform.Scale.Z);
            //scale is applied directly to vertices -> default 1x

            return t;
        }

        #endregion

        private EnvironmentManager.Components.Joints ParseJoints()
        {
            var joints = (model.Extras as JsonDictionary)["joints"] as JsonList;

            var allJoints = new EnvironmentManager.Components.Joints();

            foreach (var jointObj in joints) {
                var joint = jointObj as JsonDictionary;
                string name = joint.Get("header").Get<string>("name"); // Probably not gonna be used
                Vector3D anchor = ParseJointVector3D(joint.Get("origin"));
                EnvironmentManager.Components.Rigidbody parent = rigidbodies[joint.Get<string>("occurrenceOneUUID")];
                EnvironmentManager.Components.Rigidbody child = rigidbodies[joint.Get<string>("occurrenceTwoUUID")];

                if (joint.ContainsKey("revoluteJointMotion")) {
                    var revoluteData = joint.Get("revoluteJointMotion");
                    Vector3D axis = ParseJointVector3D(revoluteData.Get("rotationAxisVector"));
                    // TODO: Support limits
                    var result = new EnvironmentManager.Components.HingeJoint();
                    result.Anchor = anchor;
                    result.Axis = axis;
                    result.ConnectedParent = parent;
                    result.ConnectedChild = child;
                    allJoints.Add(result);
                } else { // For yet to be supported and fixed motion joints
                    var result = new EnvironmentManager.Components.FixedJoint();
                    result.Anchor = anchor;
                    result.ConnectedParent = parent;
                    result.ConnectedChild = child;
                    allJoints.Add(result);
                }
            }

            // Add the remaining potential joints
            foreach (var x in defaultRigidjoints) {
                var j = new EnvironmentManager.Components.FixedJoint();
                j.Anchor = new Vector3D(0, 0, 0); // Pretty sure this doesn't matter. From what I can tell, Unity FixedJoints only really care about the bodies
                j.ConnectedParent = x.Item1;
                j.ConnectedChild = x.Item2;
                allJoints.Add(j);
            }

            return allJoints;
        }

        private Vector3D ParseJointVector3D(JsonDictionary dict) =>
            new Vector3D((double)dict.TryGet<decimal>("x", 0),
                    (double)dict.TryGet<decimal>("y", 0), (double)dict.TryGet<decimal>("z", 0));
    }

    public static class GltfAssetExtensions {
        public static T Get<T>(this JsonDictionary dict, string key) => (T)dict[key];
        public static T TryGet<T>(this JsonDictionary dict, string key, T defaultObj) {
            if (dict.ContainsKey(key))
                return (T)dict[key];
            else
                return defaultObj;
        }
        public static JsonDictionary Get(this JsonDictionary dict, string key) => (JsonDictionary)dict[key];

        public static void ForEach<T>(this IEnumerable<T> e, Action<int, T> method) {
            for (int i = 0; i < e.Count(); ++i)
                method(i, e.ElementAt(i)); // I really hope this holds reference
        }
    }
}
