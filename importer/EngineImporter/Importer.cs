using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Synthesis.Proto;
using UnityEngine;
using Material = Synthesis.Proto.Material;
using Mesh = Synthesis.Proto.Mesh;
using UMaterial = UnityEngine.Material;
using UMesh = UnityEngine.Mesh;

namespace Synthesis.Import {
    
    /// <summary>
    /// The Importer class connected functions with string parameters to import an Entity/Model into the Engine
    /// NOTE: This may be moved into the Engine instead of in it's own project.
    /// </summary>
    public class Importer {

        #region Importer Framework
        
        public delegate GameObject ImportFunc(string path);

        /// <summary>
        /// Default Importers that come stock with Synthesis. Leaves ability to add one post release
        /// </summary>
        public static Dictionary<string, ImportFunc> Importers = new Dictionary<string, ImportFunc>() {
            { SourceType.PROTOBUF_ROBOT, ProtoRobotImport },
            { SourceType.PROTOBUF_FIELD, ProtoFieldImport }
        };

        /// <summary>
        /// Import a serialized DynamicObject into a Unity Environment
        /// </summary>
        /// <param name="path">Path to serialized data (this could be a directory or a file depending on your import/translate function)</param>
        /// <param name="type">Type of import to conduct</param>
        /// <param name="transType">Optional translation type to use before importing</param>
        /// <param name="forceTranslate">Force a translation of source data regardless if a temp file exists. TODO: Probably remove in the future</param>
        /// <returns>Root GameObject of whatever Entity/Model you imported</returns>
        public static GameObject Import(string path, string type, string transType, bool forceTranslate = false)
            => Import(path, type, Translator.Translations[transType], forceTranslate);

        /// <summary>
        /// Import an Entity/Model into the Engine
        /// </summary>
        /// <param name="path">Path to serialized data (this could be a directory or a file depending on your import/translate function)</param>
        /// <param name="type">Type of import to conduct</param>
        /// <param name="translation">Optional translation to use before importing</param>
        /// <param name="forceTranslate">Force a translation of source data regardless if a temp file exists. TODO: Probably remove in the future</param>
        /// <returns>Root GameObject of whatever Entity/Model you imported</returns>
        public static GameObject Import(string path, string type, Translator.TranslationFunc translation = null, bool forceTranslate = false) {

            Logger.Init();

            if (!Importers.ContainsKey(type))
                throw new Exception($"{Enum.GetName(type.GetType(), type)} importer doesn't exist");

            if (translation == null) {
                return Importers[type](path);
            } else {
                if (!forceTranslate) {
                    Debug.Log("Checking for cached translation");
                    string newPath = path;
                    string name = newPath.Substring(newPath.LastIndexOf(Path.AltDirectorySeparatorChar) + 1);
                    string tempPath = Path.GetTempPath() + Path.AltDirectorySeparatorChar + "synth_temp";
                    if (Directory.Exists(tempPath)) {
                        newPath = tempPath + Path.AltDirectorySeparatorChar + $"{name}.{type}";
                        if (File.Exists(newPath)) {
                            Debug.Log("Importing from cache");
                            return Importers[type](newPath);
                        } else {
                            Debug.Log($"No file: {newPath}");
                        }
                    } else {
                        Debug.Log($"No directory: {tempPath}");
                    }
                }
                return Importers[type](Translator.Translate(path, translation));
            }
        }

        #endregion
        
        #region ProtoBuf Importer

        /// <summary>
        /// Import function for importing a ProtoField
        /// </summary>
        /// <param name="path">Path to the serialized ProtoField</param>
        /// <returns>Root GameObject of the imported ProtoField</returns>
        public static GameObject ProtoFieldImport(string path) {
            var protoField = ProtoField.Parser.ParseFrom(File.ReadAllBytes(path));
            var dynoUnity = DynamicObjectImport(protoField.Object, protoField.Name);
            var gamepieces = GamepiecesImport(protoField.Gamepieces);
            gamepieces.transform.parent = dynoUnity.transform;
            return dynoUnity;
        }

        /// <summary>
        /// Import function for importing a ProtoRobot
        /// TODO:
        ///     1) Get rid of massScale and collectiveMass
        ///     2) Add Meta components
        /// </summary>
        /// <param name="path">Path to the serialized ProtoRobot</param>
        /// <returns>Root GameObject of the imported ProtoRobot</returns>
        public static GameObject ProtoRobotImport(string path) {
            var protoRobot = ProtoRobot.Parser.ParseFrom(File.ReadAllBytes(path));
            var dynoUnity = DynamicObjectImport(protoRobot.Object, protoRobot.Name);
            return dynoUnity;
        }

        /// <summary>
        /// Import assist function for a DynamicObject
        /// TODO: Make the name apart of the DynamicObject
        /// </summary>
        /// <param name="dyno">Deserialized DynamicObject</param>
        /// <param name="name">Name of the DynamicObject</param>
        /// <returns>Root GameObject of the imported DynamicObject</returns>
        /// <exception cref="Exception"></exception>
        public static GameObject DynamicObjectImport(DynamicObject dyno, string name) {
            var dynoUnity = new GameObject();
            var dynamicObjectMeta = dynoUnity.AddComponent<DynamicObjectMeta>();

            Dictionary<Guid, GameObject> nodeDict = new Dictionary<Guid, GameObject>();

            List<Collider> ignoreCollisions = new List<Collider>();

            try {

                Logger.Log("Starting Dynamic Object import");
                dynoUnity.name = name;
                for (int i = 0; i < dyno.Nodes.Count; i++) {
                    Logger.Log($"Node {i + 1} Started");
                    Node node = dyno.Nodes[i];
                    GameObject gNode = new GameObject($"node_{i}");
                    gNode.transform.parent = dynoUnity.transform;

                    #region Visual Mesh
                    var filter = gNode.AddComponent<MeshFilter>();
                    var renderer = gNode.AddComponent<MeshRenderer>();
                    filter.mesh = node.VisualMesh;
                    List<UMaterial> materials = new List<UMaterial>();
                    for (int j = 0; j < node.VisualMaterials.Count; j++) {
                        var mat = node.VisualMaterials[j];
                        // materials.Add(node.VisualMaterials[j]);
                        materials.Add(mat);
                    }
                    renderer.materials = materials.ToArray();
                    #endregion
                    Logger.Log($"Created Visual Mesh for Node {i + 1}");

                    #region Colliders
                    foreach (var mesh in node.MeshColliders) {
                        var colObj = new GameObject("Mesh_Collider");
                        var col = colObj.AddComponent<MeshCollider>();
                        col.convex = true;
                        col.sharedMesh = mesh;
                        ignoreCollisions.Add(col);
                        colObj.transform.parent = gNode.transform;
                        col.material = new PhysicMaterial() { staticFriction = node.PhysicalProperties.Friction, dynamicFriction = node.PhysicalProperties.Friction };
                    }
                    // TODO: Box collider support. Rotations be wack
                    foreach (var sphere in node.SphereColliders) {
                        var colObj = new GameObject("Sphere_Collider");
                        var col = colObj.AddComponent<SphereCollider>();
                        col.center = sphere.Center;
                        col.radius = sphere.Radius;
                        ignoreCollisions.Add(col);
                        colObj.transform.parent = gNode.transform;
                        col.material = new PhysicMaterial() { staticFriction = node.PhysicalProperties.Friction, dynamicFriction = node.PhysicalProperties.Friction };
                    }
                    #endregion
                    Logger.Log($"Created Colliders for Node {i + 1}");

                    var rb = gNode.AddComponent<Rigidbody>();
                    rb.mass = node.IsStatic ? 0 : node.PhysicalProperties.Mass;
                    rb.centerOfMass = node.PhysicalProperties.CenterOfMass;
                    if (node.IsStatic) {
                        rb.isKinematic = true;
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                    }
                    Logger.Log($"Phys done for {i + 1}");

                    var nodeSrc = gNode.AddComponent<DynamicObjectMeta.NodeSource>();
                    nodeSrc.SourceNode = node;
                    nodeSrc.collectiveMass = node.PhysicalProperties.CollectiveMass;
                    nodeDict.Add(node.Guid, gNode);
                }

                for (int i = 0; i < dyno.Nodes.Count; i++) {
                    #region Joint

                    if (dyno.Nodes[i].ParentGuid == null)
                        continue;
                    Rigidbody parent = nodeDict[dyno.Nodes[i].ParentGuid].GetComponent<Rigidbody>();
                    Rigidbody current = nodeDict[dyno.Nodes[i].Guid].GetComponent<Rigidbody>();

                    switch (dyno.Nodes[i].JointCase) {
                        case Node.JointOneofCase.RotationalJoint:

                            var rotJoint = dyno.Nodes[i].RotationalJoint;
                            float min = rotJoint.CurrentAngle - rotJoint.UpperLimit;
                            float max = rotJoint.CurrentAngle - rotJoint.LowerLimit;
                            float mid = (max + min) / 2.0f;
                            Logger.Log($"Upper {rotJoint.UpperLimit}\nLower {rotJoint.LowerLimit}\nCurrent {rotJoint.CurrentAngle}\nMin {min}\nMax {max}\nMid {mid}");
                            if (dyno.Nodes[i].RotationalJoint.UseLimits)
                                current.transform.RotateAround(rotJoint.Anchor, rotJoint.Axis, mid);

                            CreateHingeJoint(current, dyno.Nodes[i].RotationalJoint, parent, angleRange: mid);
                            CreateHingeJoint(parent, dyno.Nodes[i].RotationalJoint, current, useConnectedMassScale: true, angleRange: mid);
                            var req = current.gameObject.AddComponent<DynamicObjectMeta.ComponentRequest>();
                            req.ComponentName = "moveable"; // Am I spelling movable wrong? Probably...
                            req.ComponentProperties = new Dictionary<string, object>();
                            req.ComponentProperties.Add("forward", KeyCode.A);
                            req.ComponentProperties.Add("backward", KeyCode.D);
                            
                            dynamicObjectMeta.AddFlag(dyno.Nodes[i].Guid, EntityFlag.Hinge);
                            if (rotJoint.IsWheel) {
                                dynamicObjectMeta.AddFlag(dyno.Nodes[i].Guid, EntityFlag.Wheel);
                            }

                            //RotationalJoint rotationalJoint = dyno.Nodes[i].RotationalJoint;
                            //var hinge = nodeDict[dyno.Nodes[i].Guid].AddComponent<HingeJoint>();
                            //hinge.connectedBody = parent;
                            //hinge.anchor = rotationalJoint.Anchor;
                            //hinge.axis = ((Vector3)rotationalJoint.Axis).normalized;
                            //hinge.useLimits = true;
                            //hinge.limits = new JointLimits() { // TODO: Bounciness?
                            //    min = rotationalJoint.CurrentAngle - rotationalJoint.UpperLimit,
                            //    max = rotationalJoint.CurrentAngle - rotationalJoint.LowerLimit
                            //};
                            //hinge.massScale = rotationalJoint.MassScale;
                            // TODO: ComponentRequest for driver support
                            break;
                        case Node.JointOneofCase.OtherJoint:
                            CreateFixedJoint(current, dyno.Nodes[i].OtherJoint, parent);
                            CreateFixedJoint(current, dyno.Nodes[i].OtherJoint, parent, true);
                            //OtherJoint otherJoint = dyno.Nodes[i].OtherJoint;
                            //var joint = current.gameObject.AddComponent<FixedJoint>();
                            //joint.connectedBody = parent;
                            //joint.massScale = otherJoint.MassScale;
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

            for (int i = 0; i < ignoreCollisions.Count - 1; i++) {
                for (int j = i + 1; j < ignoreCollisions.Count; j++) {
                    Physics.IgnoreCollision(ignoreCollisions[i], ignoreCollisions[j], true);
                }
            }

            foreach (var kvp in nodeDict) {
                Node n = kvp.Value.GetComponent<DynamicObjectMeta.NodeSource>().SourceNode;
                if (n.ParentGuid != null)
                    kvp.Value.transform.parent = nodeDict[n.ParentGuid].transform;
            }

            // DynamicObjectMeta
            dynamicObjectMeta.Init(name, nodeDict);

            return dynoUnity;
        }

        /// <summary>
        /// Import assist function for Gamepieces
        /// </summary>
        /// <param name="gps">A collection of the Gamepiece instances</param>
        /// <returns>Root GameObject containing all the imported Gamepieces</returns>
        public static GameObject GamepiecesImport(IEnumerable<Gamepiece> gps) {
            GameObject container = new GameObject("Gamepieces");
            foreach (var gp in gps) {
                var obj = new GameObject(gp.Definition.Name);
                
                var tag = obj.AddComponent<GamepieceTag>();
                tag.Definition = gp.Definition;
                
                var filter = obj.AddComponent<MeshFilter>();
                var renderer = obj.AddComponent<MeshRenderer>();
                filter.mesh = gp.VisualMesh;
                List<UMaterial> materials = new List<UMaterial>();
                for (int j = 0; j < gp.VisualMaterials.Count; j++) {
                    var mat = gp.VisualMaterials[j];
                    // materials.Add(node.VisualMaterials[j]);
                    materials.Add(mat);
                }
                renderer.materials = materials.ToArray();

                switch (gp.ColliderCase) {
                    case Gamepiece.ColliderOneofCase.MeshCollider:
                        var meshCol = obj.AddComponent<MeshCollider>();
                        meshCol.convex = true;
                        meshCol.sharedMesh = gp.MeshCollider;
                        // meshCol.material.dynamicFriction = gp.PhysicalProperties.Friction / 100.0f;
                        // ignoreCollisions.Add(col);
                        break;
                    case Gamepiece.ColliderOneofCase.SphereCollider:
                        var sphereCol = obj.AddComponent<SphereCollider>();
                        sphereCol.center = gp.SphereCollider.Center;
                        sphereCol.radius = gp.SphereCollider.Radius;
                        // sphereCol.material.dynamicFriction = gp.PhysicalProperties.Friction / 100.0f;
                        // ignoreCollisions.Add(col);
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
                if (angleRange == float.NaN) {
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

        /// <summary>
        /// Stock source type definitions.
        /// NOTE: The source type keys are also used as the file extensions for the serialized data atm
        /// TODO: Stop doing what I described above
        /// </summary>
        public static class SourceType {
            public const string PROTOBUF_ROBOT = "spr";
            public const string PROTOBUF_FIELD = "spf";
        }

    }
}
