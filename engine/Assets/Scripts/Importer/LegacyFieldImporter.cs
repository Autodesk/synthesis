using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SynthesisAPI.Proto;
using UnityEngine;
using Material = SynthesisAPI.Proto.Material;
using Mesh = SynthesisAPI.Proto.Mesh;
using SynthesisAPI.Translation;
using UMaterial = UnityEngine.Material;
using UMesh = UnityEngine.Mesh;
using Logger = SynthesisAPI.Utilities.Logger;

namespace Synthesis.Import
{
	public static class LegacyFieldImporter
	{

        #region ProtoBuf Importer

        #region ProtoField

        /// <summary>
        /// Import function for importing a ProtoField
        /// </summary>
        /// <param name="path">Path to the serialized ProtoField</param>
        /// <returns>Root GameObject of the imported ProtoField</returns>
        public static GameObject ProtoFieldImport(string path) => ProtoFieldImport(File.ReadAllBytes(path));
        /// <summary>
        /// Import function for importing a ProtoField
        /// </summary>
        /// <param name="buf">Serialized ProtoField</param>
        /// <returns>Root GameObject of the imported ProtoField</returns>
        public static GameObject ProtoFieldImport(byte[] buf) => ProtoFieldImport(ProtoField.Parser.ParseFrom(buf));
        /// <summary>
        /// Import function for importing a ProtoField
        /// </summary>
        /// <param name="protoField">ProtoField data</param>
        /// <returns>Root GameObject of the imported ProtoField</returns>
        public static GameObject ProtoFieldImport(ProtoField protoField) {
            var dynoUnity = DynamicObjectImport(protoField.Object);
            var gamepieces = GamepiecesImport(protoField.Gamepieces);
            gamepieces.transform.parent = dynoUnity.transform;
            return dynoUnity;
        }

        #endregion

        /// <summary>
        /// Import assist function for a DynamicObject
        /// </summary>
        /// <param name="dyno">Deserialized DynamicObject</param>
        /// <returns>Root GameObject of the imported DynamicObject</returns>
        /// <exception cref="Exception"></exception>
        public static GameObject DynamicObjectImport(DynamicObject dyno) {
            var dynoUnity = new GameObject();
            var dynamicObjectMeta = dynoUnity.AddComponent<DynamicObjectMeta>();

            var nodeDict = new Dictionary<Guid, GameObject>();
            var nodeSources = new Dictionary<Guid, Node>();
            var ignoreCollisions = new List<Collider>();

            try {

                Logger.Log("Starting Dynamic Object import");
                dynoUnity.name = dyno.Name;
                for (int i = 0; i < dyno.Nodes.Count; i++) {
                    Logger.Log($"Node {i + 1} Started");
                    var node = dyno.Nodes[i];
                    var gNode = new GameObject($"node_{i}");
                    gNode.transform.parent = dynoUnity.transform;

                    #region Visual Mesh

                    // Create the Filter and Renderer and add the mesh and material data
                    var filter = gNode.AddComponent<MeshFilter>();
                    var renderer = gNode.AddComponent<MeshRenderer>();
                    filter.mesh = node.VisualMesh;
                    var materials = new List<UMaterial>();
                    for (int j = 0; j < node.VisualMaterials.Count; j++) {
                        materials.Add(node.VisualMaterials[j]);
                    }
                    renderer.materials = materials.ToArray();

                    #endregion
                    Logger.Log($"Created Visual Mesh for Node {i + 1}");

                    #region Colliders

                    // To reduce lag when verifying colliders in Unity, colliders are added to child GameObjects
                    foreach (var mesh in node.MeshColliders) {
                        var colObj = new GameObject("Mesh_Collider");
                        var col = colObj.AddComponent<MeshCollider>();
                        col.convex = true;
                        col.sharedMesh = mesh;
                        ignoreCollisions.Add(col);
                        colObj.transform.parent = gNode.transform;
                        col.material = new PhysicMaterial() {
                            staticFriction = node.PhysicalProperties.StaticFriction,
                            dynamicFriction = node.PhysicalProperties.DynamicFriction
                        };
                    }
                    // TODO: Box collider support. I can't get rotations to be true
                    foreach (var sphere in node.SphereColliders) {
                        var colObj = new GameObject("Sphere_Collider");
                        var col = colObj.AddComponent<SphereCollider>();
                        col.center = sphere.Center;
                        col.radius = sphere.Radius;
                        ignoreCollisions.Add(col);
                        colObj.transform.parent = gNode.transform;
                        col.material = new PhysicMaterial() {
                            staticFriction = node.PhysicalProperties.StaticFriction,
                            dynamicFriction = node.PhysicalProperties.DynamicFriction
                        };
                    }

                    #endregion
                    Logger.Log($"Created Colliders for Node {i + 1}");

                    #region Physics Data

                    var rb = gNode.AddComponent<Rigidbody>();
                    rb.mass = node.IsStatic ? 0 : node.PhysicalProperties.Mass;
                    rb.centerOfMass = node.PhysicalProperties.CenterOfMass;
                    if (node.IsStatic) {
                        rb.isKinematic = true;
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                    }

                    #endregion
                    Logger.Log($"Phys done for Node {i + 1}");

                    // nodeSrc.collectiveMass = node.PhysicalProperties.CollectiveMass;
                    nodeDict.Add(node.Guid, gNode);
                    nodeSources.Add(node.Guid, node);
                }

                // Joints need to be added after all the rigidbodies have been created
                for (int i = 0; i < dyno.Nodes.Count; i++) {
                    #region Joint

                    if (dyno.Nodes[i].ParentGuid == null)
                        continue;
                    var parent = nodeDict[dyno.Nodes[i].ParentGuid].GetComponent<Rigidbody>();
                    var current = nodeDict[dyno.Nodes[i].Guid].GetComponent<Rigidbody>();

                    switch (dyno.Nodes[i].JointCase) {
                        case Node.JointOneofCase.RotationalJoint:

                            var rotJoint = dyno.Nodes[i].RotationalJoint;
                            var minAngle = rotJoint.CurrentAngle - rotJoint.UpperLimit;
                            var maxAngle = rotJoint.CurrentAngle - rotJoint.LowerLimit;
                            var midAngle = (maxAngle + minAngle) / 2.0f;
                            Logger.Log($"Upper {rotJoint.UpperLimit}\nLower {rotJoint.LowerLimit}\nCurrent {rotJoint.CurrentAngle}\nMin {minAngle}\nMax {maxAngle}\nMid {midAngle}");
                            if (dyno.Nodes[i].RotationalJoint.UseLimits)
                                current.transform.RotateAround(rotJoint.Anchor, rotJoint.Axis, midAngle);

                            CreateHingeJoint(current, dyno.Nodes[i].RotationalJoint, parent, angleRange: midAngle);
                            CreateHingeJoint(parent, dyno.Nodes[i].RotationalJoint, current, useConnectedMassScale: true, angleRange: midAngle);

                            dynamicObjectMeta.AddFlag(dyno.Nodes[i].Guid, EntityFlag.Hinge);
                            // Wheels are the same hinge joint, but their colliders are spheres and the engine associates them separately
                            if (rotJoint.IsWheel) {
                                dynamicObjectMeta.AddFlag(dyno.Nodes[i].Guid, EntityFlag.Wheel);
                            }
                            break;
                        case Node.JointOneofCase.OtherJoint:
                            CreateFixedJoint(current, dyno.Nodes[i].OtherJoint, parent);
                            CreateFixedJoint(current, dyno.Nodes[i].OtherJoint, parent, true);
                            break;
                            // TODO: Linear joints
                    }

                    #endregion
                    Logger.Log($"Created joint for Node {i + 1}");
                }

            } catch (Exception e) {
                Logger.Log(e.Message);
                Logger.Log(e.StackTrace);
                throw new Exception();
            }

            // Ignore collisions between all the colliders within the Dynamic Object
            for (int i = 0; i < ignoreCollisions.Count - 1; i++) {
                for (int j = i + 1; j < ignoreCollisions.Count; j++) {
                    UnityEngine.Physics.IgnoreCollision(ignoreCollisions[i], ignoreCollisions[j], true);
                }
            }

            // DynamicObjectMeta
            dynamicObjectMeta.Init(dyno.Name, nodeDict, nodeSources);

            // Parent all the nodes correctly
            foreach (var kvp in nodeSources) {
                if (kvp.Value.ParentGuid != null)
                    nodeDict[kvp.Key].transform.parent = nodeDict[kvp.Value.ParentGuid].transform;
            }

            return dynoUnity;
        }

        /// <summary>
        /// Import assist function for Gamepieces
        /// </summary>
        /// <param name="gps">A collection of the Gamepiece instances</param>
        /// <returns>Root GameObject containing all the imported Gamepieces</returns>
        public static GameObject GamepiecesImport(IEnumerable<Gamepiece> gps) {

            var container = new GameObject("Gamepieces");

            foreach (var gp in gps) {

                var obj = new GameObject(gp.Definition.Name);
                var tag = obj.AddComponent<GamepieceTag>();
                tag.Definition = gp.Definition;

                #region Visual Mesh

                var filter = obj.AddComponent<MeshFilter>();
                var renderer = obj.AddComponent<MeshRenderer>();
                filter.mesh = gp.VisualMesh;
                var materials = new List<UMaterial>();
                for (int j = 0; j < gp.VisualMaterials.Count; j++) {
                    materials.Add(gp.VisualMaterials[j]);
                }
                renderer.materials = materials.ToArray();

                #endregion

                switch (gp.ColliderCase) {
                    case Gamepiece.ColliderOneofCase.MeshCollider:
                        var meshCol = obj.AddComponent<MeshCollider>();
                        meshCol.convex = true;
                        meshCol.sharedMesh = gp.MeshCollider;
                        break;
                    case Gamepiece.ColliderOneofCase.SphereCollider:
                        var sphereCol = obj.AddComponent<SphereCollider>();
                        sphereCol.center = gp.SphereCollider.Center;
                        sphereCol.radius = gp.SphereCollider.Radius;
                        break;
                }

                var rb = obj.AddComponent<Rigidbody>();
                rb.mass = gp.PhysicalProperties.Mass;
                rb.centerOfMass = gp.PhysicalProperties.CenterOfMass;

                obj.transform.position = gp.Position;

                obj.transform.parent = container.transform;
            }
            return container;
        }

        /// <summary>
        /// Import assist function for creating FixedJoints
        /// </summary>
        /// <param name="c">The owner of the joint component</param>
        /// <param name="jointData">Protobuf joint data</param>
        /// <param name="parent">The connected body of the joint</param>
        /// <param name="useConnectedMassScale">Use connected mass scale</param>
        private static void CreateFixedJoint(Rigidbody c, OtherJoint jointData, Rigidbody parent, bool useConnectedMassScale = false) {
            var joint = c.gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = parent;
            if (useConnectedMassScale)
                joint.connectedMassScale = parent.mass / c.mass;
            else
                joint.massScale = parent.mass / c.mass;
        }

        /// <summary>
        /// Import assist function for creating HingeJoints
        /// </summary>
        /// <param name="c">The owner of the joint component</param>
        /// <param name="jointData">Protobuf joint data</param>
        /// <param name="parent">The connected body of the joint</param>
        /// <param name="useConnectedMassScale">Use connected mass scale</param>
        /// <param name="angleRange">Range of motion for the hinge</param>
        private static void CreateHingeJoint(Rigidbody c, RotationalJoint jointData, Rigidbody parent, bool useConnectedMassScale = false, float angleRange = float.NaN) {
            var hinge = c.gameObject.AddComponent<HingeJoint>();
            hinge.connectedBody = parent;
            hinge.anchor = jointData.Anchor;
            hinge.axis = ((Vector3)jointData.Axis).normalized;
            if (jointData.UseLimits) {
                hinge.useLimits = true;
                float min, max;
                if (float.IsNaN(angleRange)) {
                    min = jointData.CurrentAngle - jointData.UpperLimit;
                    max = jointData.CurrentAngle - jointData.LowerLimit;
                } else {
                    min = -angleRange;
                    max = angleRange;
                }
                hinge.limits = new JointLimits() { // TODO: Bounciness?
                    min = min, // Mathf.Clamp(, -175, 175), // TODO: Fix this?
                    max = max // Mathf.Clamp(, -175, 175)
                };
            }
            if (useConnectedMassScale)
                hinge.connectedMassScale = parent.mass / c.mass;
            else
                hinge.massScale = parent.mass / c.mass;
        }

        #endregion

	}
}