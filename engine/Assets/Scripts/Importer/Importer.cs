using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Mirabuf;
using Mirabuf.Joint;
using Mirabuf.Material;
using Synthesis.Util;
using SynthesisAPI.Proto;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Material = SynthesisAPI.Proto.Material;
using Mesh = SynthesisAPI.Proto.Mesh;
using UMaterial = UnityEngine.Material;
using UMesh = UnityEngine.Mesh;
using Logger = SynthesisAPI.Utilities.Logger;
using Assembly = Mirabuf.Assembly;
using AssemblyData = Mirabuf.AssemblyData;
using Joint = Mirabuf.Joint.Joint;
using Transform = UnityEngine.Transform;
using Vector3 = Mirabuf.Vector3;
using UVector3 = UnityEngine.Vector3;
using Node = Mirabuf.Node;
using MPhysicalProperties = Mirabuf.PhysicalProperties;

#pragma warning disable 
namespace Synthesis.Import {

    /// <summary>
    /// The Importer class connected functions with string parameters to import an Entity/Model into the Engine
    /// NOTE: This may be moved into the Engine instead of in it's own project.
    /// </summary>
    public static class Importer {

        #region Importer Framework
        
        public delegate GameObject ImportFuncString(string path);
        public delegate GameObject ImportFuncBuffer(byte[] buffer);

        /// <summary>
        /// Default Importers that come stock with Synthesis. Leaves ability to add one post release
        /// </summary>
        // public static Dictionary<SourceType, (ImportFuncString strFunc, ImportFuncBuffer bufFunc)> Importers
        //     = new Dictionary<SourceType, (ImportFuncString strFunc, ImportFuncBuffer bufFunc)>() {
        //     // { SourceType.PROTOBUF_ROBOT, (ProtoRobotImport, ProtoRobotImport) },
        //     { SourceType.PROTOBUF_DYNAMIC_STATIC, (ProtoFieldImport, ProtoFieldImport) }
        // };

        /// <summary>
        /// Import a serialized DynamicObject into a Unity Environment
        /// </summary>
        /// <param name="path">Path to serialized data (this could be a directory or a file depending on your import/translate function)</param>
        /// <param name="type">Type of import to conduct</param>
        /// <param name="transType">Optional translation type to use before importing</param>
        /// <param name="forceTranslate">Force a translation of source data regardless if a temp file exists. TODO: Probably remove in the future</param>
        /// <returns>Root GameObject of whatever Entity/Model you imported</returns>
        // public static GameObject Import(string path, SourceType type, Translator.TranslationType transType, bool forceTranslate = false)
        //     => Import(path, type, Translator.Translations.ContainsKey(transType) ? Translator.Translations[transType].strFunc : null, forceTranslate);
        /// <summary>
        /// Import a serialized DynamicObject into a Unity Environment
        /// </summary>
        /// <param name="buf">Serialized data</param>
        /// <param name="type">Type of import to conduct</param>
        /// <param name="transType">Optional translation type to use before importing</param>
        /// <returns>Root GameObject of whatever Entity/Model you imported</returns>
        // public static GameObject Import(byte[] buf, SourceType type, Translator.TranslationType transType)
        //     => Import(buf, type, Translator.Translations.ContainsKey(transType) ? Translator.Translations[transType].bufFunc : null);

        // /// <summary>
        // /// Import an Entity/Model into the Engine
        // /// </summary>
        // /// <param name="path">Path to serialized data (this could be a directory or a file depending on your import/translate function)</param>
        // /// <param name="type">Type of import to conduct</param>
        // /// <param name="translation">Optional translation to use before importing</param>
        // /// <param name="forceTranslate">Force a translation of source data regardless if a temp file exists. TODO: Probably remove in the future</param>
        // /// <returns>Root GameObject of whatever Entity/Model you imported</returns>
        // public static GameObject Import(string path, SourceType type, Translator.TranslationFuncString translation = null, bool forceTranslate = false) {
        //     if (!Importers.ContainsKey(type))
        //         throw new Exception($"{Enum.GetName(type.GetType(), type)} importer doesn't exist");
        //
        //     if (translation == null) {
        //         return Importers[type].strFunc(path);
        //     } else {
        //         if (!forceTranslate) {
        //             Debug.Log("Checking for cached translation");
        //             string newPath = path;
        //             // string name = newPath.Substring(newPath.LastIndexOf(Path.AltDirectorySeparatorChar) + 1);
        //             string name = Translator.TempFileName(path);
        //             string tempPath = Path.GetTempPath() + Path.AltDirectorySeparatorChar + "synth_temp";
        //             if (Directory.Exists(tempPath)) {
        //                 newPath = tempPath + Path.AltDirectorySeparatorChar + $"{name}.{type.FileEnding}";
        //                 if (File.Exists(newPath)) {
        //                     Debug.Log("Importing from cache");
        //                     return Importers[type].strFunc(newPath);
        //                 } else {
        //                     Debug.Log($"No file: {newPath}");
        //                 }
        //             } else {
        //                 Debug.Log($"No directory: {tempPath}");
        //             }
        //         }
        //         return Importers[type].strFunc(Translator.Translate(path, translation));
        //     }
        // }
        //
        // /// <summary>
        // /// Import an Entity/Model into the Engine
        // /// </summary>
        // /// <param name="buf">Serialized data (this could be a directory or a file depending on your import/translate function)</param>
        // /// <param name="type">Type of import to conduct</param>
        // /// <param name="translation">Optional translation to use before importing</param>
        // /// <returns>Root GameObject of whatever Entity/Model you imported</returns>
        // public static GameObject Import(byte[] buf, SourceType type, Translator.TranslationFuncBuffer translation = null) {
        //     if (!Importers.ContainsKey(type))
        //         throw new Exception($"{Enum.GetName(type.GetType(), type)} importer doesn't exist");
        //
        //     if (translation == null) {
        //         return Importers[type].bufFunc(buf);
        //     } else {
        //         return Importers[type].bufFunc(Translator.Translate(buf, translation));
        //     }
        // }

        #endregion

        #region Mirabuf Importer
        
        /*
         * Questions:
         * - Does the Part transform override or add on top of the PartDefinition transform?
         * - Should Parts have a parent definition?
         * - Are Bodys supposed to have a transform?
         * - Is there currently anything planned for using the value section of an Edge?
         * - Is vector data stored as centimeters or meters?
         */
        
        /*
         * Potential Changes:
         * - Add static/dynamic friction as separate values (if possible to grab from fusion)
         * - Rename TriangleMesh's material reference to appearance
         * - Uvs are float vectors not int vectors
         * - Restitution?
         */
        
        /*
         * Notes for Hunter so he doesn't forget:
         * - For now I'm keeping everything mirrored because Fusion is lovely
         * - May have to compound transformations (specifically rotations) and then flip X and W values of quaternions and flip X value of vectors.
         *      Consider doing that at a SpatialTransformMatrix level when converting to a UnityEngine.Matrix4x4, instead of compounding
         * - Check if changing positions/rotations/scales change recursively or not
         */
        
        public static GameObject AssemblyImport(byte[] buffer)
            => AssemblyImport(Assembly.Parser.ParseFrom(buffer));

        private static List<Collider> _collidersToIgnore;
        public static GameObject AssemblyImport(Assembly assembly) {

            // For debugging the contents of the imported assembly
            var jFormatter = new JsonFormatter(JsonFormatter.Settings.Default);
            File.WriteAllText(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                + $"{Path.AltDirectorySeparatorChar}{assembly.Info.Name}.json",
                jFormatter.Format(assembly));

            // assembly.Data.Joints.JointInstances.ForEach(x => {
            //     var definition = assembly.Data.Joints.JointDefinitions[x.Value.JointReference];
            //     Debug.Log(Enum.GetName(typeof(JointMotion), definition.JointMotionType));
            //     if (x.Value.Parts != null) {
            //         DebugGraph(x.Value.Parts);
            //     }
            //     // Debug.Log($"Part1: {assembly.Data.Parts.PartInstances[x.Value.Part1].Info.Name}");
            //     // Debug.Log($"Part2: {assembly.Data.Parts.PartInstances[x.Value.Part2].Info.Name}");
            //
            // });

            GameObject assemblyObject = new GameObject(assembly.Info.Name);
            var parts = assembly.Data.Parts;
            MakeGlobalTransformations(assembly);
            var partObjects = new Dictionary<string, GameObject>(); // TODO: Do I need this?
            var groupObjects = new Dictionary<string, GameObject>();
            float totalMass = 0;
            _collidersToIgnore = new List<Collider>();
            // Import Rigid Definitions
            #region Rigid Definitions
            var rigidDefinitions = FindRigidbodyDefinitions(assembly);
            foreach (var group in rigidDefinitions.definitions.Values) {
                GameObject groupObject = new GameObject(group.Name);
                var collectivePhysData = new List<MPhysicalProperties>();
                // Import Parts
                #region Parts
                foreach (var part in group.Parts) {
                    var partInstance = part.Value;
                    var partDefinition = parts.PartDefinitions[partInstance.PartDefinitionReference];
                    GameObject partObject = new GameObject(partInstance.Info.Name);
                    MakePartDefinition(partObject, partDefinition, partInstance, assembly.Data);
                    partObjects.Add(partInstance.Info.GUID, partObject);
                    partObject.transform.parent = groupObject.transform;
                    // MARK: If transform changes do work recursively, apply transformations here instead of in a separate loop
                    partObject.transform.ApplyMatrix(partInstance.GlobalTransform);
                    collectivePhysData.Add(partDefinition.PhysicalData);
                }
                #endregion
                // Combine all physical data for grouping
                var combPhysProps = CombinePhysicalProperties(collectivePhysData);
                var rb = groupObject.AddComponent<Rigidbody>();
                rb.mass = (float)combPhysProps.Mass;
                totalMass += rb.mass;
                rb.centerOfMass = combPhysProps.Com; // I actually don't need to flip this
                groupObject.transform.parent = assemblyObject.transform;
                groupObjects.Add(group.GUID, groupObject);
            }
            #endregion

            #region Joints
            foreach (var jointKvp in assembly.Data.Joints.JointInstances) {
                if (jointKvp.Key == "grounded")
                    continue;
                // Debug.Log($"Joint Key: {jointKvp.Key}");
                MakeJoint(
                    groupObjects[jointKvp.Key], 
                    groupObjects[rigidDefinitions.partToDefinitionMap[jointKvp.Value.ChildPart]], 
                    jointKvp.Value, 
                    totalMass, 
                    assembly
                    );
            }
            #endregion
            
            
            
            for (int i = 0; i < _collidersToIgnore.Count - 1; i++) {
                for (int j = i + 1; j < _collidersToIgnore.Count; j++) {
                    UnityEngine.Physics.IgnoreCollision(_collidersToIgnore[i], _collidersToIgnore[j]);
                }
            }
            
            // TODO: Might want to investigate memory usage and see if it could be an issue
            
            return assemblyObject;
        }

        public static void MakeJoint(GameObject a, GameObject b, JointInstance instance, float totalMass, Assembly assembly) {
            // Stuff I'm gonna use for all joints
            var definition = assembly.Data.Joints.JointDefinitions[instance.JointReference];
            var rbA = a.GetComponent<Rigidbody>();
            var rbB = b.GetComponent<Rigidbody>();
            switch (definition.JointMotionType) {
                case JointMotion.Revolute: // Hinge/Revolution joint
                    var revoluteA = a.AddComponent<HingeJoint>();
                    revoluteA.anchor = (definition.Origin == null ? new Vector3() : definition.Origin)
                                       + (instance.Offset == null ? new Vector3() : instance.Offset);
                    revoluteA.axis =
                        (((Matrix4x4)assembly.Data.Parts.PartInstances[instance.ParentPart].GlobalTransform).rotation)
                        * definition.Rotational.RotationalFreedom.Axis; // CHANGE
                    revoluteA.connectedBody = rbB;
                    revoluteA.massScale = Mathf.Pow(totalMass / rbA.mass, 1);
                    var revoluteB = b.AddComponent<HingeJoint>();
                    revoluteB.anchor = (definition.Origin == null ? new Vector3() : definition.Origin)
                                       + (instance.Offset == null ? new Vector3() : instance.Offset);
                    revoluteB.axis = revoluteA.axis; // definition.Rotational.RotationalFreedom.Axis;
                    revoluteB.connectedBody = rbA;
                    revoluteB.connectedMassScale = Mathf.Pow(totalMass / rbA.mass, 1);
                    break;
                case JointMotion.Slider:
                default: // Make a rigid joint
                    var rigidA = a.AddComponent<FixedJoint>();
                    rigidA.anchor = (definition.Origin == null ? new Vector3() : definition.Origin)
                                    + (instance.Offset == null ? new Vector3() : instance.Offset);
                    rigidA.axis = UVector3.forward;
                    rigidA.connectedBody = rbB;
                    rigidA.massScale = Mathf.Pow(totalMass / rbA.mass, 1); // Not sure if this works
                    var rigidB = b.AddComponent<FixedJoint>();
                    rigidB.anchor = (definition.Origin == null ? new Vector3() : definition.Origin)
                                    + (instance.Offset == null ? new Vector3() : instance.Offset);
                    rigidB.axis = UVector3.forward;
                    rigidB.connectedBody = rbA;
                    rigidB.connectedMassScale = Mathf.Pow(totalMass / rbA.mass, 1);
                    break;
            }
        }
        
        #region Assistant Functions

        public static MPhysicalProperties CombinePhysicalProperties(IEnumerable<MPhysicalProperties> props) {
            var total = 0.0;
            var com = new Vector3();
            props.ForEach(x => total += x.Mass);
            props.ForEach(x => com += x.Com * x.Mass);
            com /= total;
            return new MPhysicalProperties { Mass = total, Com = com };
        }

        public static (
            Dictionary<string, RigidbodyDefinition> definitions,
            Dictionary<string, string> partToDefinitionMap
            ) FindRigidbodyDefinitions(Assembly assembly) {
            
            var defs = new Dictionary<string, RigidbodyDefinition>();
            var partMap = new Dictionary<string, string>();

            int counter = 0;
            assembly.Data.Joints.JointInstances.ForEach(x => {
                var def = new RigidbodyDefinition {
                    GUID = $"{x.Value.Info.GUID}",
                    Name = $"Rigidgroup:{counter}",
                    Parts = new Dictionary<string, PartInstance>()
                };
                counter++;
                List<Node> nodes = x.Value.Parts == null ? new List<Node>() : new List<Node>(x.Value.Parts.Nodes);
                while (nodes.Any()) {
                    nodes.ForEach(y => {
                        Debug.Log($"Part GUID: \"{y.Value}\"");
                        if (y.Value != String.Empty)
                            def.Parts.Add(y.Value, assembly.Data.Parts.PartInstances[y.Value]);
                    });
                    var newNodes = new List<Node>();
                    nodes.ForEach(y => newNodes.AddRange(y.Children));
                    nodes = newNodes;
                }
                defs.Add(def.GUID, def);
                def.Parts.ForEach(y => partMap[y.Key] = def.GUID);
            });
            
            return (defs, partMap);
        }
        
        #endregion

        #region Debug Functions
        
        public static void DebugGraph(GraphContainer graph) {
            graph.Nodes.ForEach(x => DebugNode(x, 0));
        }

        public static void DebugNode(Node node, int level) {
            int a = 0;
            string prefix = "";
            while (a != level) {
                prefix += "-";
                a++;
            }
            Debug.Log($"{prefix} {node.Value}");
            node.Children.ForEach(x => DebugNode(x, level + 1));
        }
        
        private static void DebugJoint(Assembly assembly, string joint) {
            var instance = assembly.Data.Joints.JointInstances[joint];
            var definition = assembly.Data.Joints.JointDefinitions[instance.JointReference];
            
            Debug.Log(Enum.GetName(typeof(Joint.JointMotionOneofCase), definition.JointMotionCase));
        }
        
        #endregion

        // public struct Tree<T> {
        //     public T Value;
        //     public List<Tree<T>> Children;
        // }
        
        public struct RigidbodyDefinition {
            public string GUID;
            public string Name;
            public Dictionary<string, PartInstance> Parts; // Using a dictionary to make Key searches faster
        }

        // I think this might work?
        public static Dictionary<string, Matrix4x4> MakeGlobalTransformations(Assembly assembly) {
            var map = new Dictionary<string, Matrix4x4>();
            foreach (Node n in assembly.DesignHierarchy.Nodes) {
                Matrix4x4 trans = assembly.Data.Parts.PartInstances[n.Value].Transform == null
                    ? assembly.Data.Parts.PartDefinitions[assembly.Data.Parts.PartInstances[n.Value].PartDefinitionReference].BaseTransform == null
                        ? Matrix4x4.identity
                        : assembly.Data.Parts.PartDefinitions[assembly.Data.Parts.PartInstances[n.Value].PartDefinitionReference].BaseTransform.UnityMatrix
                    : assembly.Data.Parts.PartInstances[n.Value].Transform.UnityMatrix;
                map.Add(n.Value, trans);
                MakeGlobalTransformations(map, map[n.Value], assembly.Data.Parts, n);
            }
            foreach (var kvp in map) {
                assembly.Data.Parts.PartInstances[kvp.Key].GlobalTransform = map[kvp.Key];
            }
            return map;
        }
        
        public static void MakeGlobalTransformations(Dictionary<string, Matrix4x4> map, Matrix4x4 parent, Parts parts, Node node) {
            foreach (var child in node.Children) {
                map.Add(child.Value, parent * parts.PartInstances[child.Value].Transform.UnityMatrix);
                MakeGlobalTransformations(map, map[child.Value], parts, child);
            }
        }

        // public static void ApplyTransformationsToGraph(Dictionary<string, GameObject> gameObjects, Assembly assembly) {
        //     ApplyTransformationsToGraph(gameObjects, assembly, assembly.DesignHierarchy.Nodes);
        // }

        // This is all wrong whoops
        // public static void ApplyTransformationsToGraph(Dictionary<string, GameObject> gameObjects, Assembly assembly, IEnumerable<Node> nodes) {
        //     foreach (var node in nodes) {
        //         ApplyTransformation(node.Value, gameObjects[node.Value], assembly.Data);
        //         ApplyTransformationsToGraph(gameObjects, assembly, node.Children.UnravelNodes());
        //     }
        // }
        //
        // public static void ApplyTransformation(string part, GameObject gameObject, AssemblyData data) {
        //     gameObject.transform.ApplyMatrix(data.Parts.PartInstances[part].Transform.SpatialMatrix.UnityMatrix);
        // }

        // public static void MakeRigidGroup(GameObject container, IEnumerable<string> parts, AssemblyData assemblyData) {
        //     
        // }
        
        // Keep this
        // TODO: Collider Ignorance (Is that the correct term?)
        public static void MakePartDefinition(GameObject container, PartDefinition definition, PartInstance instance, AssemblyData assemblyData) {
            PhysicMaterial physMat = new PhysicMaterial {
                dynamicFriction = 0.6f, // definition.PhysicalData.,
                staticFriction = 0.6f // definition.PhysicalData.Friction
            };
            foreach (var body in definition.Bodies) {
                GameObject bodyObject = new GameObject(body.Info.Name);
                var filter = bodyObject.AddComponent<MeshFilter>();
                var renderer = bodyObject.AddComponent<MeshRenderer>();
                filter.sharedMesh = body.TriangleMesh.UnityMesh;
                renderer.material = assemblyData.Materials.Appearances.ContainsKey(body.AppearanceOverride)
                    ? assemblyData.Materials.Appearances[body.AppearanceOverride].UnityMaterial
                    : assemblyData.Materials.Appearances.ContainsKey(instance.Appearance)
                        ? assemblyData.Materials.Appearances[instance.Appearance].UnityMaterial
                        : Appearance.DefaultAppearance.UnityMaterial; // Setup the override
                var collider = bodyObject.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = body.TriangleMesh.UnityMesh; // Again, not sure if this actually works
                collider.material = physMat;
                _collidersToIgnore.Add(collider);
                bodyObject.transform.parent = container.transform;
                // Ensure all transformations are zeroed after assigning parent
                bodyObject.transform.localPosition = UVector3.zero;
                bodyObject.transform.localRotation = Quaternion.identity;
                bodyObject.transform.localScale = UVector3.one;
                // bodyObject.transform.ApplyMatrix(body.);
            }
        }
        
        // public static SpatialTransformMatrix CompoundTransformations(Part )
        
        #endregion
        
        #region Protobuf 2.0 Importer

        // public static GameObject DynamicStaticImport(byte[] buf)
        //     => DynamicStaticImport((DynamicStatic)buf);
        //
        // public static GameObject DynamicStaticImport(DynamicStatic dyStat) {
        //
        //     GameObject assembly = new GameObject(dyStat.Name);
        //     
        //     // Occurrences
        //     Occurrence root = dyStat.AssemblyData.OccurrenceMap.Values.ElementAt(0);
        //     while (!root.IsGrounded)
        //         root = dyStat.AssemblyData.OccurrenceMap[root.Parent];
        //     
        //     MakeOccurrence(root, dyStat.AssemblyData);
        //
        //     // Joints
        //     foreach (var kvp in dyStat.AssemblyData.JointMap) {
        //         MakeJoint(kvp.Value, dyStat.AssemblyData);
        //     }
        //     
        //     var values = dyStat.AssemblyData.ImportedOccurrences.Values;
        //     for (int i = 0; i < values.Count - 1; i++) {
        //         for (int j = i + 1; j < values.Count; j++) {
        //             // Consider caching collider to avoid calling GetComponent too much
        //             UnityEngine.Physics.IgnoreCollision(values.ElementAt(0).GetComponent<MeshCollider>(),
        //                 values.ElementAt(1).GetComponent<MeshCollider>());
        //         }
        //     }
        //
        //     return assembly;
        // }
        //
        // public static void MakeJoint(Joint joint, AssemblyData assemDat) {
        //     switch (joint.JointCase) {
        //         case Joint.JointOneofCase.RotationalJoint:
        //             MakeRotationalJoint(joint, assemDat);
        //             break;
        //         case Joint.JointOneofCase.OtherJoint:
        //             MakeOtherJoint(joint, assemDat);
        //             break;
        //     }
        // }
        //
        // public static void MakeOtherJoint(Joint joint, AssemblyData assemDat) {
        //     MakeOtherJoint(joint, assemDat, false);
        //     MakeOtherJoint(joint, assemDat, true);
        // }
        //
        // public static void MakeOtherJoint(Joint joint, AssemblyData assemDat, bool sideB) {
        //     FixedJoint other;
        //     if (sideB) {
        //         other = assemDat.ImportedOccurrences[joint.Occurrence2].AddComponent<FixedJoint>();
        //         other.connectedBody = assemDat.ImportedOccurrences[joint.Occurrence1].GetComponent<Rigidbody>();
        //         other.massScale = assemDat.ComponentMap[assemDat.OccurrenceMap[joint.Occurrence2].ComponentRef].PhysicalProperties.Mass
        //                           / assemDat.ComponentMap[assemDat.OccurrenceMap[joint.Occurrence1].ComponentRef].PhysicalProperties.Mass;
        //     } else {
        //         other = assemDat.ImportedOccurrences[joint.Occurrence1].AddComponent<FixedJoint>();
        //         other.connectedBody = assemDat.ImportedOccurrences[joint.Occurrence2].GetComponent<Rigidbody>();
        //         other.massScale = assemDat.ComponentMap[assemDat.OccurrenceMap[joint.Occurrence1].ComponentRef].PhysicalProperties.Mass 
        //                           / assemDat.ComponentMap[assemDat.OccurrenceMap[joint.Occurrence2].ComponentRef].PhysicalProperties.Mass;
        //     }
        //     other.anchor = joint.Anchor;
        //     other.axis = joint.Axis;
        // }
        //
        // public static void MakeRotationalJoint(Joint joint, AssemblyData assemDat) {
        //     MakeRotationalJoint(joint, assemDat, false);
        //     MakeRotationalJoint(joint, assemDat, true);
        // }
        //
        // public static void MakeRotationalJoint(Joint joint, AssemblyData assemDat, bool sideB) {
        //     HingeJoint hinge;
        //     if (sideB) {
        //         hinge = assemDat.ImportedOccurrences[joint.Occurrence2].gameObject.AddComponent<HingeJoint>();
        //         hinge.connectedBody = assemDat.ImportedOccurrences[joint.Occurrence1].GetComponent<Rigidbody>();
        //     } else {
        //         hinge = assemDat.ImportedOccurrences[joint.Occurrence1].gameObject.AddComponent<HingeJoint>();
        //         hinge.connectedBody = assemDat.ImportedOccurrences[joint.Occurrence2].GetComponent<Rigidbody>();
        //     }
        //     hinge.anchor = joint.Anchor;
        //     hinge.axis = ((Vector3)joint.Axis).normalized;
        //     // float angleRange = joint.RotationalJoint.UpperLimit - joint.RotationalJoint.LowerLimit;
        //     if (joint.UseLimits) {
        //         hinge.useLimits = true;
        //         float min, max;
        //         
        //         min = joint.RotationalJoint.CurrentAngle - joint.RotationalJoint.UpperLimit;
        //         max = joint.RotationalJoint.CurrentAngle - joint.RotationalJoint.LowerLimit;
        //         
        //         var minAngle = joint.RotationalJoint.CurrentAngle - joint.RotationalJoint.UpperLimit;
        //         var maxAngle = joint.RotationalJoint.CurrentAngle - joint.RotationalJoint.LowerLimit;
        //         var midAngle = (maxAngle + minAngle) / 2.0f;
        //         Logger.Log($"Upper {joint.RotationalJoint.UpperLimit}\nLower {joint.RotationalJoint.LowerLimit}"
        //                    + $"\nCurrent {joint.RotationalJoint.CurrentAngle}\nMin {minAngle}\nMax {maxAngle}\nMid {midAngle}");
        //         if (!sideB)
        //             assemDat.ImportedOccurrences[joint.Occurrence1].transform.RotateAround(joint.Anchor, joint.Axis, midAngle);
        //         
        //         hinge.limits = new JointLimits() { // TODO: Bounciness?
        //             min = -midAngle, // Mathf.Clamp(, -175, 175), // TODO: Fix this?
        //             max = midAngle // Mathf.Clamp(, -175, 175)
        //         };
        //     }
        //     // Maybe just get the mass through the Proto data
        //     float mass = assemDat.ComponentMap[assemDat.OccurrenceMap[joint.Occurrence1].ComponentRef].PhysicalProperties.Mass;
        //     if (sideB)
        //         hinge.massScale = mass / hinge.connectedBody.mass;
        //     else
        //         hinge.massScale = hinge.connectedBody.mass / mass;
        // }
        //
        // public static void MakeOccurrence(Occurrence occ, AssemblyData assemDat, Transform parent = null) {
        //     // if (!occ.IsGrounded && !assemDat.ImportedOccurrences.ContainsKey(occ.Parent))
        //     //    MakeOccurrence(assemDat.OccurrenceMap[occ.Parent], assemDat);
        //
        //     var comp = assemDat.ComponentMap[occ.ComponentRef];
        //     
        //     GameObject occurrenceObject = new GameObject(occ.Name);
        //     if (!occ.IsGrounded)
        //         occurrenceObject.transform.parent = parent;
        //     // Load Transformation
        //     var matrix = assemDat.ComponentMap[occ.ComponentRef].Transform.UnityMatrix * occ.Transform.UnityMatrix;
        //     occurrenceObject.transform.ApplyMatrix(matrix);
        //     // Create Bodies
        //     comp.Bodies.ForEachIndex((i, x) => CreateBody(x, occurrenceObject.transform, assemDat, comp.PhysicalProperties));
        //     // Physical Bodies
        //     Rigidbody rb = occurrenceObject.AddComponent<Rigidbody>();
        //     rb.mass = comp.PhysicalProperties.Mass;
        //     rb.centerOfMass = comp.PhysicalProperties.CenterOfMass;
        //     // Make Child Occurrences
        //     assemDat.ImportedOccurrences.Add(occ.GUID, occurrenceObject);
        //     foreach (var child in occ.Children) {
        //         MakeOccurrence(assemDat.OccurrenceMap[child], assemDat, occurrenceObject.transform);
        //     }
        // }
        //
        // public static void CreateBody(Body body, Transform parent, AssemblyData assemDat, PhysProps physicalProperties) {
        //     GameObject bodyObject = new GameObject(body.Name);
        //     bodyObject.transform.parent = parent;
        //     bodyObject.transform.ApplyMatrix(body.Transform); // We should really avoid using scales
        //     var filter = bodyObject.AddComponent<MeshFilter>();
        //     var renderer = bodyObject.AddComponent<MeshRenderer>();
        //     filter.sharedMesh = body.VisualMesh;
        //     renderer.material = assemDat.MaterialMap[body.Material];
        //     var collider = bodyObject.AddComponent<MeshCollider>();
        //     collider.convex = true;
        //     collider.sharedMesh = body.VisualMesh; // Does making it shared do weird stuff?
        //     collider.material = new PhysicMaterial() { dynamicFriction = physicalProperties.DynamicFriction, staticFriction = physicalProperties.StaticFriction };
        // }
        
        #endregion
        
        #region Protobuf Importer

        /*
        
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

        #region ProtoRobot
        
        /// <summary>
        /// Import function for importing a ProtoRobot
        /// </summary>
        /// <param name="path">Path to the serialized ProtoRobot</param>
        /// <returns>Root GameObject of the imported ProtoRobot</returns>
        public static GameObject ProtoRobotImport(string path) => ProtoRobotImport(File.ReadAllBytes(path));
        /// <summary>
        /// Import function for importing a ProtoRobot
        /// </summary>
        /// <param name="buffer">Serialized ProtoRobot</param>
        /// <returns>Root GameObject of the imported ProtoRobot</returns>
        public static GameObject ProtoRobotImport(byte[] buffer) => ProtoRobotImport(ProtoRobot.Parser.ParseFrom(buffer));
        /// <summary>
        /// Import function for importing a ProtoRobot
        /// TODO:
        ///     1) Get rid of massScale and collectiveMass
        ///     2) Add Meta components
        /// </summary>
        /// <param name="protoRobot">ProtoRobot data</param>
        /// <returns>Root GameObject of the imported ProtoRobot</returns>
        public static GameObject ProtoRobotImport(ProtoRobot protoRobot) {
            var dynoUnity = DynamicObjectImport(protoRobot.Object);
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

        */

        #endregion

        /// <summary>
        /// Stock source type definitions
        /// </summary>
        public struct SourceType {

            public static readonly SourceType PROTOBUF_DYNAMIC_STATIC = new SourceType("proto_dynamic_static", "synth");
            // public static readonly SourceType PROTOBUF_FIELD = new SourceType("proto_field", ProtoField.FILE_ENDING);
            
            public string FileEnding { get; private set; }
            public string Indentifier { get; private set; }

            public SourceType(string indentifier, string fileEnding) {
                Indentifier = indentifier;
                FileEnding = fileEnding;
            }
        }

    }
}
