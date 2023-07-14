#define DEBUG_MIRABUF

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirabuf;
using Mirabuf.Joint;
using System.IO;
using System.IO.Compression;
using System;
using System.Linq;
using System.Threading.Tasks;
using SynthesisAPI.Utilities;
using Google.Protobuf;
using Mirabuf.Material;
using Newtonsoft.Json;
using Logger              = SynthesisAPI.Utilities.Logger;
using MPhysicalProperties = Mirabuf.PhysicalProperties;
using Vector3             = Mirabuf.Vector3;
using UVector3            = UnityEngine.Vector3;

namespace Synthesis.Import {
    public class MirabufLive {
#region Framework

        public const UInt32 CURRENT_MIRA_EXPORTER_VERSION = 5;
        public const UInt32 OLDEST_MIRA_EXPORTER_VERSION  = 4;

        public const int FIELD_LAYER     = 7;
        public const int DYNAMIC_1_LAYER = 8;
        public const int DYNAMIC_2_LAYER = 9;
        public const int DYNAMIC_3_LAYER = 10;
        public const int DYNAMIC_4_LAYER = 11;
        public const int DYNAMIC_5_LAYER = 12;
        public const int DYNAMIC_6_LAYER = 13;

        private static Queue<int> dynamicLayers = new Queue<int>(new int[] { DYNAMIC_1_LAYER, DYNAMIC_2_LAYER,
            DYNAMIC_3_LAYER, DYNAMIC_4_LAYER, DYNAMIC_5_LAYER, DYNAMIC_6_LAYER });

#endregion

        private string _path;
        public string MiraPath => _path;
        public Assembly MiraAssembly;

        public enum MirabufFileState {
            Valid          = 1,
            DuplicateParts = 2
        }

        ;

        private MirabufFileState _state = MirabufFileState.Valid;

        private Task<RigidbodyDefinitions> _findDefinitions = null;
        public RigidbodyDefinitions Definitions {
            get {
                if (_findDefinitions.Status < TaskStatus.RanToCompletion) {
                    _findDefinitions.Wait();
                }
                return _findDefinitions.Result;
            }
        }

        public MirabufLive(string path) {
            _path = path;
            Load();

            _findDefinitions = Task<RigidbodyDefinitions>.Factory.StartNew(() => FindRigidbodyDefinitions(this));
        }

        public static MirabufLive OpenMirabufFile(string path) => MirabufCache.Get(path);

#region File Management

        private void Load() {
            byte[] buff = File.ReadAllBytes(_path);

            if (buff[0] == 0x1f && buff[1] == 0x8b) {
                int originalLength = BitConverter.ToInt32(buff, buff.Length - 4);

                MemoryStream mem        = new MemoryStream(buff);
                byte[] res              = new byte[originalLength];
                GZipStream decompresser = new GZipStream(mem, CompressionMode.Decompress);
                decompresser.Read(res, 0, res.Length);
                decompresser.Close();
                mem.Close();
                buff = res;
            }

            MiraAssembly = Assembly.Parser.ParseFrom(buff);
        }

        public void Save() {
            if (MiraAssembly == null) {
                File.Delete(_path);
                return;
            }
            byte[] buff = new byte[MiraAssembly.CalculateSize()];
            MiraAssembly.WriteTo(new CodedOutputStream(buff));
            string backupPath = $"{_path}.bak";
            File.Delete(backupPath);
            File.Move(_path, backupPath);
            File.WriteAllBytes(_path, buff);
        }

#endregion

#region Creation

        public Dictionary<string, GameObject> GenerateDefinitionObjects(
            GameObject assemblyContainer, bool physics = true) {
            Dictionary<string, GameObject> groupObjects = new Dictionary<string, GameObject>();

            int dynamicLayer = 0;

            if (physics && MiraAssembly.Dynamic) {
                if (dynamicLayers.Count == 0)
                    throw new Exception("No more dynamic layers");
                dynamicLayer = dynamicLayers.Dequeue();

                assemblyContainer.layer = dynamicLayer;
                assemblyContainer.AddComponent<DynamicLayerReserver>();
            }

            foreach (var group in Definitions.Definitions.Values) {
                GameObject groupObject = new GameObject(group.Name);
                var isGamepiece        = group.IsGamepiece;
                var isStatic           = group.IsStatic;
                // Import Parts

#region Parts

                foreach (var part in group.Parts) {
                    var partInstance   = part.Value;
                    var partDefinition = MiraAssembly.Data.Parts.PartDefinitions[partInstance.PartDefinitionReference];
                    GameObject partObject = new GameObject(partInstance.Info.Name);

                    MakePartDefinition(partObject, partDefinition, partInstance, MiraAssembly.Data,
                        !physics ? ColliderGenType.NoCollider
                                 : (isStatic ? ColliderGenType.Concave : ColliderGenType.Convex));
                    partObject.transform.parent        = groupObject.transform;
                    var gt                             = partInstance.GlobalTransform.UnityMatrix;
                    partObject.transform.localPosition = gt.GetPosition();
                    partObject.transform.localRotation = gt.rotation;
                }

                groupObject.transform.parent = assemblyContainer.transform;

#endregion

                if (!MiraAssembly.Dynamic && !isGamepiece) {
                    groupObject.transform.GetComponentsInChildren<UnityEngine.Transform>().ForEach(
                        x => x.gameObject.layer = FIELD_LAYER);
                } else if (MiraAssembly.Dynamic && physics) {
                    groupObject.transform.GetComponentsInChildren<UnityEngine.Transform>().ForEach(
                        x => x.gameObject.layer = dynamicLayer);
                }

                if (physics) {
                    // Combine all physical data for grouping
                    var rb = groupObject.AddComponent<Rigidbody>();
                    if (isStatic)
                        rb.isKinematic = true;
                    rb.mass         = (float) group.CollectivePhysicalProperties.Mass;
                    rb.centerOfMass = group.CollectivePhysicalProperties.Com; // I actually don't need to flip this
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                }

                // TODO: Do this in importer
                // if (isGamepiece) {
                // 	var gpSim = new GamepieceSimObject(group.Name, groupObject);
                // 	try {
                // 		// SimulationManager.RegisterSimObject(gpSim);
                // 	} catch (Exception e) {
                // 		// TODO: Fix
                // 		throw e;
                // 		Logger.Log($"Gamepiece with name {gpSim.Name} already exists.");
                // 		UnityEngine.Object.Destroy(groupObject);
                // 	}
                // 	gamepieces.Add(gpSim);
                // } else {
                //
                // }
                groupObjects.Add(group.GUID, groupObject);
            }

            return groupObjects;
        }

        private enum ColliderGenType {
            NoCollider = 0,
            Convex     = 1,
            Concave    = 2
        }

        private static void MakePartDefinition(GameObject container, PartDefinition definition, PartInstance instance,
            AssemblyData assemblyData, ColliderGenType colliderGenType = ColliderGenType.Convex) {
            PhysicMaterial physMat = new PhysicMaterial {
                dynamicFriction = 0.6f, // definition.PhysicalData.,
                staticFriction  = 0.6f  // definition.PhysicalData.Friction
            };
            foreach (var body in definition.Bodies) {
                var bodyObject    = new GameObject(body.Info.Name);
                var filter        = bodyObject.AddComponent<MeshFilter>();
                var renderer      = bodyObject.AddComponent<MeshRenderer>();
                filter.sharedMesh = body.TriangleMesh.UnityMesh;
                renderer.material = assemblyData.Materials.Appearances.ContainsKey(instance.Appearance)
                                        ? assemblyData.Materials.Appearances[instance.Appearance].UnityMaterial
                                    : assemblyData.Materials.Appearances.ContainsKey(body.AppearanceOverride)
                                        ? assemblyData.Materials.Appearances[body.AppearanceOverride].UnityMaterial
                                        : Appearance.DefaultAppearance.UnityMaterial; // Setup the override
                // renderer.material = assemblyData.Materials.Appearances.ContainsKey(instance.Appearance)
                // 	? assemblyData.Materials.Appearances[instance.Appearance].UnityMaterial
                // 	: Appearance.DefaultAppearance.UnityMaterial;
                if (!instance.SkipCollider && colliderGenType > ColliderGenType.NoCollider) {
                    MeshCollider collider = null;
                    try {
                        collider = bodyObject.AddComponent<MeshCollider>();
                        if (colliderGenType == ColliderGenType.Convex) {
                            collider.convex = true;
                            collider.sharedMesh =
                                body.TriangleMesh.ColliderMesh; // Again, not sure if this actually works
                        } else {
                            collider.convex     = false;
                            collider.sharedMesh = body.TriangleMesh.UnityMesh;
                        }
                    } catch (Exception e) {
                        if (collider != null) {
                            GameObject.Destroy(collider);
                            collider = null;
                        }
                    }

                    if (collider != null)
                        collider.material = physMat;
                    // if (addToColliderIgnore)
                    // 	_collidersToIgnore.Add(collider);
                }
                bodyObject.transform.parent = container.transform;
                // Ensure all transformations are zeroed after assigning parent
                bodyObject.transform.localPosition = UVector3.zero;
                bodyObject.transform.localRotation = Quaternion.identity;
                bodyObject.transform.localScale    = UVector3.one;
                // bodyObject.transform.ApplyMatrix(body.);
            }
        }

#endregion

#region Rigidbody Identification

        /// <summary>
        /// I really don't like how I made this. It gets the job done but I feel like it
        /// could use a ton of optimizations.
        /// TODO: Maybe get rid of the dictionary in the rigidbodyDefinitions and just
        /// 	store keys. I feel like that would be a bit better.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static RigidbodyDefinitions FindRigidbodyDefinitions(MirabufLive live) {
            Assembly assembly = live.MiraAssembly;

#if DEBUG_MIRABUF
            MemoryStream ms = new MemoryStream();
            ms.Seek(0, SeekOrigin.Begin);
            assembly.WriteTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var debugAssembly = Assembly.Parser.ParseFrom(ms);

            debugAssembly.Data.Parts.PartDefinitions.ForEach(x => x.Value.Bodies.ForEach(x => x.TriangleMesh = null));
            var jFormatter = new JsonFormatter(JsonFormatter.Settings.Default);
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                  $"{Path.AltDirectorySeparatorChar}{debugAssembly.Info.Name}.json",
                jFormatter.Format(debugAssembly));

            bool allUnique = false;
            {
                var allNodes                = debugAssembly.Data.Joints.RigidGroups.Select(x => x.Name);
                HashSet<string> uniqueGuids = new HashSet<string>((int) (allNodes.Count() * (1f / 0.75f)));
                allUnique                   = allNodes.All(x => uniqueGuids.Add(x));
            }
            if (!allUnique)
                Logger.Log("Not all unique", LogLevel.Warning);
#endif

            var defs    = new Dictionary<string, RigidbodyDefinition>();
            var partMap = new Dictionary<string, string>();

            // Create grounded node
            // var groundedJoint = assembly.Data.Joints.JointInstances["grounded"];

            // I'm using lambda functions so I can reuse repeated logic while taking advantage of variables in the
            // current scope
            Action<RigidbodyDefinition, RigidbodyDefinition> MergeDefinitions = (keep, delete) => {
                delete.Parts.ForEach((k, v) => keep.Parts.Add(k, v));
                for (int i = 0; i < partMap.Keys.Count; i++) {
                    if (partMap[partMap.Keys.ElementAt(i)].Equals(delete.GUID))
                        partMap[partMap.Keys.ElementAt(i)] = keep.GUID;
                }
                defs.Remove(delete.GUID);
            };
            Action<string, string, string> MoveToDef = (part, from, to) => {
                if (from != string.Empty)
                    defs[from].Parts.Remove(part);
                var _a = assembly.Data.Parts.PartInstances[part];
                defs[to].Parts.Add(part, _a);
                partMap[part] = to;
            };

            var paths = LoadPartTreePaths(assembly.DesignHierarchy);

            var discoveredRigidGroups = new List<RigidGroup>();

            (string guid, JointInstance joint) groundJoint = default;
            int counter                                    = 0;

            // Create initial definitions
            foreach (var jInst in assembly.Data.Joints.JointInstances) {
                RigidbodyDefinition mainDef;
                RigidbodyDefinition stubDef;
                bool isGrounded = jInst.Key.Equals("grounded");
                if (isGrounded) {
                    groundJoint = (jInst.Key, jInst.Value);
                    continue; // Skip grounded to apply it last
                }

                mainDef            = new RigidbodyDefinition { GUID = $"{counter}",
                    Name                                 = $"{jInst.Value.Info.Name}_Rigidgroup{counter}",
                    Parts                                = new Dictionary<string, PartInstance>() };
                defs[mainDef.GUID] = mainDef;

                stubDef            = new RigidbodyDefinition { GUID = $"{counter}-2",
                    Name                                 = $"{jInst.Value.Info.Name}_Rigidgroup{counter}-2",
                    Parts                                = new Dictionary<string, PartInstance>() };
                defs[stubDef.GUID] = stubDef;

                // I'm slowly turning into Nick
                // Grab all the parts listed under the joint
                if ((jInst.Value.Parts != null && jInst.Value.Parts.Nodes != null) &&
                    jInst.Value.Parts.Nodes.Count > 0) {
                    // I don't know why AllTreeElements sometimes returns null elements
                    var tmpParts = jInst.Value.Parts.Nodes.AllTreeElements()
                                       .Where(x => x.Value != null && x.Value != string.Empty)
                                       .Select(x => (x.Value, assembly.Data.Parts.PartInstances[x.Value]));
                    var tmpRG = new RigidGroup { Name = $"discovered_{counter}" };
                    tmpRG.Occurrences.Add(tmpParts.Select(x => x.Value));
                    discoveredRigidGroups.Add(tmpRG);
                    // tmpParts.ForEach(x => MoveToDef(x.Value, string.Empty, mainDef.GUID));
                }

                // Grab all parts eligable via design hierarchy
                var parentSiblings = IdentifyParentSiblings(paths, jInst.Value.ParentPart, jInst.Value.ChildPart);
                // Gonna make an action so I don't have to have the same code written twice
                Action<string, string> inherit = (part, definition) => {
                    List<Node> toCheck = new List<Node>();
                    List<Node> tmp     = new List<Node>();
                    string originalDef;
                    var path = paths[part];
                    // Logger.Log($"{part}: Path length {path.Count}");
                    Node n = assembly.DesignHierarchy.Nodes[0];
                    for (int i = 0; i < path.Count; i++) {
                        n = n.Children.Find(y => y.Value.Equals(path[i]));
                    }
                    toCheck.Add(n.Children.Find(y => y.Value.Equals(part)));
                    // if (path.Count == 0) {
                    // 	toCheck.Add(assembly.DesignHierarchy.Nodes.Find(y => y.Value == part));
                    // } else {

                    var defObj = defs[definition];
                    defObj.Name += $"_{assembly.Data.Parts.PartInstances[part]}";

                    // }
                    if (partMap.ContainsKey(part))
                        originalDef = partMap[part];
                    else
                        originalDef = string.Empty;

                    while (toCheck.Count > 0) {
                        toCheck.ForEach(y => {
                            partMap.TryGetValue(y.Value, out string existingDef);
                            if (existingDef == null) {
                                var _partInst = assembly.Data.Parts.PartInstances[y.Value];
                                defs[definition].Parts.Add(y.Value, _partInst);
                                partMap[y.Value] = definition;
                                tmp.AddRange(y.Children.Where(z => z != null && z.Value != null)); // Just to make sure
                            } else if (existingDef.Equals(originalDef)) {
                                MoveToDef(y.Value, originalDef, definition);
                                tmp.AddRange(y.Children.Where(z => z != null && z.Value != null)); // Just to make sure
                            }
                        });
                        toCheck.Clear();
                        toCheck.AddRange(tmp);
                        tmp.Clear();
                    }
                };
                inherit(parentSiblings.child, stubDef.GUID);
                inherit(parentSiblings.parent, mainDef.GUID);
                counter++;
            }

            // Apply Ground Last
            if (groundJoint == default) {
                Logger.Log("Not sure this is possible but failed to find grounded joint", LogLevel.Error);
                throw new Exception();
            }
            RigidbodyDefinition groundDef = new RigidbodyDefinition { Name = "grounded", GUID = "grounded",
                Parts = new Dictionary<string, PartInstance>() };
            defs.Add(groundDef.GUID, groundDef);
            groundJoint.joint.Parts?.Nodes?.AllTreeElements()
                .Where(x => x.Value != null && x.Value != string.Empty)
                .ForEach(x => {
                    if (!partMap.ContainsKey(x.Value))
                        MoveToDef(x.Value, string.Empty, groundDef.GUID);
                });

            // Add disjointed to grounded
            assembly.Data.Parts.PartInstances.Where(x => !partMap.ContainsKey(x.Key)).ForEach(x => {
                MoveToDef(x.Key, string.Empty, "grounded");
            });

            // Check if the original grounded object has been eaten by one of the joints
            var swallower = partMap[assembly.Data.Joints.JointInstances["grounded"].Parts.Nodes.ElementAt(0).Value];
            if (swallower != groundDef.GUID) {
                var def = defs[swallower];
                MergeDefinitions(groundDef, def);
            }

            // Apply RigidGroups
            discoveredRigidGroups.AddRange(assembly.Data.Joints.RigidGroups);
            discoveredRigidGroups.Where(x => x.Occurrences.Count > 1).ForEach(x => {
                RigidbodyDefinition rigidDef = new RigidbodyDefinition { Name = $"{x.Name}", GUID = $"{counter}",
                    Parts = new Dictionary<string, PartInstance>() };
                defs.Add(rigidDef.GUID, rigidDef);
                x.Occurrences.ForEach(y => {
                    partMap.TryGetValue(y, out string existingDef);
                    if (existingDef == null) {
                        MoveToDef(y, string.Empty, rigidDef.GUID);
                    } else if (!existingDef.Equals(rigidDef.GUID)) {
                        if (existingDef.Equals("grounded")) {
                            MergeDefinitions(groundDef, rigidDef);
                            rigidDef = groundDef;
                        } else {
                            // Logger.Log($"MERGING DEFINITION: {defs[existingDef].Name} based on {x.Name}",
                            // LogLevel.Debug);
                            rigidDef.Name += $"_{defs[existingDef].Name}";
                            MergeDefinitions(rigidDef, defs[existingDef]);
                        }
                    }
                });

                counter++;
            });

            bool wasRemoved                 = false;
            RigidbodyDefinition newGrounded = default;
            // Clear excess rigidbodies
            for (int i = 0; i < defs.Keys.Count; i++) {
                if (defs[defs.Keys.ElementAt(i)].Parts.Count == 0) {
                    if (defs.Keys.ElementAt(i).Equals("grounded")) {
                        var groundedPartGuid = assembly.Data.Joints.JointInstances.Find(x => x.Key.Equals("grounded"))
                                                   .Value.Parts.Nodes[0]
                                                   .Value;
                        newGrounded = defs[partMap[groundedPartGuid]];
                        wasRemoved  = true;
                    }
                    defs.Remove(defs.Keys.ElementAt(i));
                    i -= 1;
                }
            }

            // Re-add grounded if it was removed
            if (wasRemoved) {
                defs.Remove(newGrounded.GUID);
                List<string> originalKeys = new List<string>(partMap.Keys);
                foreach (string partKey in originalKeys) {
                    if (partMap[partKey].Equals(newGrounded.GUID))
                        partMap[partKey] = "grounded";
                }
                newGrounded.GUID = "grounded";
                newGrounded.Name = "grounded";
                defs.Add(newGrounded.GUID, newGrounded);
            }

            // Move gamepieces to separate groupings
            int gamepieceCounter = 0;
            if (!assembly.Dynamic) {
                assembly.Data.Parts.PartDefinitions.Where(x => x.Value.Dynamic)
                    .ForEach(
                        x => assembly.Data.Parts.PartInstances.Where(y => y.Value.PartDefinitionReference.Equals(x.Key))
                                 .ForEach(y => {
                                     RigidbodyDefinition rigidDef =
                                         new RigidbodyDefinition { Name = $"gamepiece_{gamepieceCounter}",
                                             GUID = $"{counter}", Parts = new Dictionary<string, PartInstance>() };
                                     defs.Add(rigidDef.GUID, rigidDef);
                                     GetAllPartsInBranch(y.Key, paths, assembly.DesignHierarchy.Nodes.ElementAt(0))
                                         .ForEach(z => MoveToDef(z, partMap[z], rigidDef.GUID));

                                     gamepieceCounter++;
                                     counter++;
                                 }));
            }

            // Make names reasonable
            int shift = 0;
            for (int i = 0; i < defs.Keys.Count; i++) {
                var def = defs[defs.Keys.ElementAt(i)];
                if (def.Name.Equals("grounded") || def.Name.Contains("gamepiece")) {
                    shift++;
                } else {
                    def.Name = $"node_{i - shift}";
                }
                defs[defs.Keys.ElementAt(i)] = def;
            }

            // =====================================
            // === Data setup for ease of access ===
            // =====================================

            MakeGlobalTransformations(assembly);

            float totalMass                      = 0f;
            HashSet<string> partDuplicateChecker = new HashSet<string>();
            foreach (var group in defs.Values.Select(x => x.GUID)) {
                var collectivePhysData = new List<(Matrix4x4, Mirabuf.PhysicalProperties)>();

                foreach (var part in defs[group].Parts) {
                    if (!partDuplicateChecker.Add(part.Value.Info.GUID)) {
                        live._state = MirabufFileState.DuplicateParts;
                        continue;
                    }

                    var partInstance   = part.Value;
                    var partDefinition = assembly.Data.Parts.PartDefinitions[partInstance.PartDefinitionReference];
                    collectivePhysData.Add((partInstance.GlobalTransform.UnityMatrix, partDefinition.PhysicalData));
                }

                var combPhysProps = CombinePhysicalProperties(collectivePhysData);
                totalMass += (float) combPhysProps.Mass;
                { // Ugh, structs...
                    var groupInstance                          = defs[group];
                    groupInstance.CollectivePhysicalProperties = combPhysProps;
                    groupInstance.IsGamepiece = !assembly.Dynamic && groupInstance.Name.Contains("gamepiece");
                    groupInstance.IsStatic    = !assembly.Dynamic && groupInstance.Name.Contains("grounded");
                    defs[group]               = groupInstance;
                }
            }

            var definitions =
                new RigidbodyDefinitions { Mass = totalMass, Definitions = defs, PartToDefinitionMap = partMap };

            return definitions;
        }

        private static (string parent, string child)
            IdentifyParentSiblings(Dictionary<string, List<string>> paths, string parent, string child) {
            var parentAncestors = new List<string>();
            var childAncestors  = new List<string>();
            parentAncestors.AddRange(paths[parent]);
            childAncestors.AddRange(paths[child]);
            if (parentAncestors.Count == 0 || childAncestors.Count == 0) {
                return (parentAncestors.Count == 0 ? parent : parentAncestors[0],
                    childAncestors.Count == 0 ? child : childAncestors[0]);
            }
            while (parentAncestors[0].Equals(childAncestors[0])) {
                parentAncestors.RemoveAt(0);
                childAncestors.RemoveAt(0);

                if (parentAncestors.Count == 0 || childAncestors.Count == 0) {
                    return (parentAncestors.Count == 0 ? parent : parentAncestors[0],
                        childAncestors.Count == 0 ? child : childAncestors[0]);
                }
            }
            return (parentAncestors[0], childAncestors[0]);
        }

        private static Dictionary<string, List<string>> LoadPartTreePaths(GraphContainer designHierarchy) {
            Dictionary<string, List<string>> paths = new Dictionary<string, List<string>>();
            foreach (Node n in designHierarchy.Nodes[0].Children) {
                LoadPartTreePaths(n, paths);
            }
            return paths;
        }

        private static void LoadPartTreePaths(
            Node n, Dictionary<string, List<string>> paths, List<string> currentPath = null) {
            paths[n.Value] = new List<string>();
            if (currentPath != null && currentPath.Count > 0)
                paths[n.Value].AddRange(currentPath);
            // paths[n.Value] = currentPath == null ? new List<string>() : new List<string>(currentPath);
            if (currentPath == null)
                currentPath = new List<string>();
            currentPath.Add(n.Value);
            foreach (Node child in n.Children) {
                LoadPartTreePaths(child, paths, currentPath);
            }
            currentPath.RemoveAt(currentPath.Count - 1);
        }

        private static List<string> GetAllPartsInBranch(
            string rootPart, Dictionary<string, List<string>> paths, Node rootNode) {
            var parts   = new List<string>();
            var toCheck = new List<Node>();
            toCheck.Add(NavigateDHPath(paths[rootPart].Append(rootPart).ToList(), rootNode));
            while (toCheck.Count > 0) {
                var tmp = new List<Node>();
                toCheck.ForEach(x => {
                    parts.Add(x.Value);
                    x.Children.ForEach(y => {
                        if (y.Value != string.Empty)
                            tmp.Add(y);
                    });
                });
                toCheck.Clear();
                toCheck = tmp;
            }
            return parts;
        }

        private static Node NavigateDHPath(List<string> path, Node rootNode) {
            var current = rootNode;
            while (path.Count > 0) {
                current = current.Children.First(x => x.Value.Equals(path[0]));
                path.RemoveAt(0);
            }
            return current;
        }

        private static MPhysicalProperties CombinePhysicalProperties(
            List<(Matrix4x4 trans, MPhysicalProperties prop)> props) {
            var total = 0.0f;
            var com   = new UVector3();
            props.ForEach(x => total += (float) x.prop.Mass);
            props.ForEach(x => com += (x.trans.MultiplyPoint(x.prop.Com)) * (float) x.prop.Mass);
            com /= total;
            return new MPhysicalProperties { Mass = total, Com = com };
        }

#endregion

#region Global Transformations

        private static Dictionary<string, Matrix4x4> MakeGlobalTransformations(Assembly assembly) {
            var map = new Dictionary<string, Matrix4x4>();
            foreach (Node n in assembly.DesignHierarchy.Nodes) {
                Matrix4x4 trans =
                    assembly.Data.Parts.PartInstances[n.Value].Transform == null
                        ? assembly.Data.Parts
                                      .PartDefinitions[assembly.Data.Parts.PartInstances[n.Value]
                                                           .PartDefinitionReference]
                                      .BaseTransform == null
                              ? Matrix4x4.identity
                              : assembly.Data.Parts
                                    .PartDefinitions[assembly.Data.Parts.PartInstances[n.Value].PartDefinitionReference]
                                    .BaseTransform.MirabufMatrix
                        : assembly.Data.Parts.PartInstances[n.Value].Transform.MirabufMatrix;
                map.Add(n.Value, trans);
                MakeGlobalTransformations(map, map[n.Value], assembly.Data.Parts, n);
            }

            foreach (var kvp in map) {
                assembly.Data.Parts.PartInstances[kvp.Key].GlobalTransform = map[kvp.Key];
            }

            return map;
        }

        private static void MakeGlobalTransformations(
            Dictionary<string, Matrix4x4> map, Matrix4x4 parent, Parts parts, Node node) {
            foreach (var child in node.Children) {
                if (!map.ContainsKey(child.Value)) {
                    map.Add(child.Value, parent * parts.PartInstances[child.Value].Transform.MirabufMatrix);
                    MakeGlobalTransformations(map, map[child.Value], parts, child);
                } else {
                    Logger.Log($"Key \"{child.Value}\" already present in map; ignoring", LogLevel.Error);
                }
            }
        }

#endregion

        /// <summary>
        /// Collection of parts that move together
        /// </summary>
        public struct RigidbodyDefinition {
            public string GUID;
            public string Name;
            public bool IsGamepiece;
            public bool IsStatic;
            public MPhysicalProperties CollectivePhysicalProperties;
            public Dictionary<string, PartInstance> Parts; // Using a dictionary to make Key searches faster
        }

        public struct RigidbodyDefinitions {
            public float Mass;
            public Dictionary<string, RigidbodyDefinition> Definitions;
            public Dictionary<string, string> PartToDefinitionMap;
        }

        public class DynamicLayerReserver : MonoBehaviour {
            private void OnDestroy() {
                dynamicLayers.Enqueue(gameObject.layer);
            }
        }
    }
}