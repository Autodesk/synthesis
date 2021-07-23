/*using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using SynthesisAPI.Proto;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using Google.Protobuf.Reflection;
using UnityEngine;
using Material = SynthesisAPI.Proto.Material;
using Mesh = SynthesisAPI.Proto.Mesh;
using UQuat = UnityEngine.Quaternion;
using UVec3 = UnityEngine.Vector3;
using Logger = SynthesisAPI.Utilities.Logger;

namespace SynthesisAPI.Translation {
    
    /// <summary>
    /// The Translator translates other serialized data into another form of serialized data.
    /// We use this to support legacy exported robots and fields
    /// </summary>
    public static class Translator {

        public const string SERIALIZER_SIGNATURE = "PTL";

        public delegate Task<string> TranslationFuncString(string path, string output = null);
        public delegate Task<byte[]> TranslationFuncBuffer(byte[] buf);

        public static Dictionary<TranslationType, (TranslationFuncString strFunc, TranslationFuncBuffer bufFunc)> Translations
            = new Dictionary<TranslationType, (TranslationFuncString strFunc, TranslationFuncBuffer bufFunc)>() {
            { TranslationType.BXDF_TO_PROTO_FIELD, (BXDFToProtoFieldAsync, null) },
            { TranslationType.BXDJ_TO_PROTO_ROBOT, (BXDJToProtoRobotAsync, null) }
        };

        #region Waited Functions
        
        /// <summary>
        /// Loads serialized file and translates it to a different format
        /// </summary>
        /// <param name="source">Path to original file</param>
        /// <param name="output">Optional path to where to write the new data to</param>
        /// <returns>Path to the newly translated file</returns>
        public static string Translate(string path, TranslationType transType, string output = null) {
            var a = TranslateAsync(path, transType, output);
            a.Start();
            a.Wait();
            return a.Result;
        }

        public static string Translate(string path, TranslationFuncString transFunc, string output = null) {
            var a = TranslateAsync(path, transFunc, output);
            a.Start();
            a.Wait();
            return a.Result;
        }
        public static byte[] Translate(byte[] buf, TranslationType transType) {
            var a = TranslateAsync(buf, transType);
            a.Start();
            a.Wait();
            return a.Result;
        }

        public static byte[] Translate(byte[] buf, TranslationFuncBuffer transFunc) {
            var a = TranslateAsync(buf, transFunc);
            a.Start();
            a.Wait();
            return a.Result;
        }
        
        #endregion

        #region Asynchronous Functions

        /// <summary>
        /// Loads serialized file and translates it to a different format
        /// </summary>
        /// <param name="source">Path to original file</param>
        /// <param name="output">Optional path to where to write the new data to</param>
        /// <returns>Task that results in a path to the newly translated file</returns>
        public static Task<string> TranslateAsync(string path, TranslationType transType, string output = null) {
            if (!Translations.ContainsKey(transType))
                return Task.FromResult(string.Empty);

            return TranslateAsync(path, Translations[transType].strFunc, output);
        }

        public static Task<string> TranslateAsync(string path, TranslationFuncString transFunc, string output = null) {
            return transFunc(path, output);
        }
        
        public static Task<byte[]> TranslateAsync(byte[] buf, TranslationType transType) {
            if (!Translations.ContainsKey(transType))
                return Task.FromResult(new byte[0]);

            return TranslateAsync(buf, Translations[transType].bufFunc);
        }

        public static Task<byte[]> TranslateAsync(byte[] buf, TranslationFuncBuffer transFunc) {
            return transFunc(buf);
        }
        
        #endregion

        #region ToProto
        private static bool CPU_OPTIMIZE = false;

        public static Task<string> BXDFToProtoFieldAsync(string source, string output = null) {
            var myTask = new Task<string>(() => {

                var protoField = new ProtoField();
                var protoNode = new Node();

                BXDAMesh mesh = new BXDAMesh();
                mesh.ReadFromFile(source + Path.AltDirectorySeparatorChar + "mesh.bxda");

                var fieldDef =
                    BXDFProperties.ReadProperties(source + Path.AltDirectorySeparatorChar + "definition.bxdf");

                var fieldData = new XmlDocument();
                fieldData.LoadXml(File.ReadAllText(source + Path.AltDirectorySeparatorChar + "field_data.xml"));
                var gamepieceDefinitions = ParseFieldData(fieldData);
                protoField.GamepieceDefinitions.AddRange(gamepieceDefinitions);

                var instances = new Dictionary<int, List<(FieldNode node, BXDVector3 position, BXDQuaternion rotation)>>();

                foreach (var node in fieldDef.NodeGroup.EnumerateAllLeafFieldNodes()) {
                    if (node.SubMeshID != -1) {
                        if (!instances.ContainsKey(node.SubMeshID))
                            instances[node.SubMeshID] = new List<(FieldNode node, BXDVector3 position, BXDQuaternion rotation)>();

                        instances[node.SubMeshID].Add((node, node.Position, node.Rotation));
                    }
                }

                #region VisualMesh & Colliders

                var meshColliders = new List<Mesh>();
                // var boxColliders = new List<Box>(); // TODO: Box collider support
                var sphereColliders = new List<Sphere>();
                var v = new List<Vec3>();
                var t = new List<int>();
                var materials = new List<Material>();
                var subMeshes = new List<SubMeshDescription>();
                var materialsAndSubMeshes = new Dictionary<Material, List<(int start, int length)>>();
                for (int j = 0; j < mesh.meshes.Count; j++) {
                    var sub = mesh.meshes[j];

                    for (int h = 0; h < instances[j].Count; h++) {

                        var colV = new List<Vec3>();
                        var colT = new List<int>();
                        var gpMats = new List<Material>();
                        var gpSubMeshes = new List<SubMeshDescription>();

                        int initVertCount = v.Count;

                        for (int k = 0; k < sub.surfaces.Count; k++) {
                            var surf = sub.surfaces[k];
                            var cpy = new int[surf.indicies.Length];
                            Array.Copy(surf.indicies, cpy, cpy.Length);
                            Array.Reverse(cpy);
                            var start = t.Count + colT.Count;
                            foreach (int a in cpy) {
                                colT.Add(a);
                            }

                            var mat = (Material)surf;
                            if (CPU_OPTIMIZE) {
                                if (!materialsAndSubMeshes.ContainsKey(mat))
                                    materialsAndSubMeshes[mat] = new List<(int start, int length)>();
                                materialsAndSubMeshes[mat].Add((start, (t.Count + colT.Count) - start));
                            }

                            var subMesh = new SubMeshDescription() { Start = start, Count = (t.Count + colT.Count) - start };
                            gpSubMeshes.Add(subMesh);
                            gpMats.Add(mat);
                        }

                        for (int k = 0; k < sub.verts.Length; k += 3) {
                            var vec = new BXDVector3() {
                                x = (float)sub.verts[k], y = (float)sub.verts[k + 1], z = (float)sub.verts[k + 2]
                            };
                            var moddedVec = ModVec(instances[j][h].rotation, vec, instances[j][h].position);
                            colV.Add(moddedVec);
                        }

                        Gamepiece gamepiece = null;
                        var isGamepiece = false;

                        if (fieldDef.GetPropertySets().ContainsKey(instances[j][h].node.PropertySetID)) {
                            var propertySet = fieldDef.GetPropertySets()[instances[j][h].node.PropertySetID];
                            var colliderType = propertySet
                                .Collider
                                .CollisionType;
                            
                            gamepiece = new Gamepiece();
                            isGamepiece = gamepieceDefinitions.Exists(x => x.Name == propertySet.PropertySetID);
                            if (isGamepiece) {
                                gamepiece.Definition = gamepieceDefinitions.Find(x => x.Name == propertySet.PropertySetID);
                                var bounds = colV.Bounds();
                                gamepiece.Position = (bounds.min + bounds.max) / 2;
                                colV = colV.Map<Vec3>(x => x - gamepiece.Position);
                                gpSubMeshes = gpSubMeshes.Map<SubMeshDescription>(x => new SubMeshDescription() {
                                    Start = x.Start - t.Count,
                                    Count = x.Count
                                });
                                gamepiece.VisualMaterials.AddRange(gpMats);
                                Mesh vm = new Mesh();
                                vm.Vertices.AddRange(colV);
                                vm.Triangles.AddRange(colT);
                                vm.SubMeshes.AddRange(gpSubMeshes);
                                gamepiece.VisualMesh = vm;
                                gamepiece.PhysicalProperties = new PhysProps {
                                    Mass = propertySet.Mass, DynamicFriction = (propertySet.Friction / 100.0f),
                                    StaticFriction = (propertySet.Friction / 100.0f), CenterOfMass = new Vec3()
                                };

                                // Logger.Log()
                                Logger.Log("Found Existing Gamepiece");
                            }
                            
                            switch (colliderType) {
                                case PropertySet.PropertySetCollider.PropertySetCollisionType.BOX: // MARK: Wow, are box colliders annoying
                                case PropertySet.PropertySetCollider.PropertySetCollisionType.MESH:
                                    Mesh colliderMesh = new Mesh();
                                    colliderMesh.Vertices.AddRange(colV);
                                    colliderMesh.Triangles.AddRange(colT);
                                    colliderMesh.SubMeshes.Add(new SubMeshDescription()
                                        { Start = 0, Count = colT.Count });
                                    if (isGamepiece) {
                                        // gamepiece.ColliderCase = Gamepiece.ColliderOneofCase.MeshCollider;
                                        gamepiece.MeshCollider = colliderMesh;
                                    } else {
                                        meshColliders.Add(colliderMesh);
                                    }
                                    break;
                                case PropertySet.PropertySetCollider.PropertySetCollisionType.SPHERE:
                                    if (isGamepiece) {
                                        gamepiece.SphereCollider = Sphere.CreateFromVerts(colV);
                                    } else {
                                        sphereColliders.Add(Sphere.CreateFromVerts(colV));
                                    }
                                    break;
                            }
                        }

                        if (isGamepiece) {
                            protoField.Gamepieces.Add(gamepiece);
                        } else {
                            v.AddRange(colV);
                            t.AddRange(colT.Map<int>(x => x + initVertCount));
                            subMeshes.AddRange(gpSubMeshes);
                            materials.AddRange(gpMats);
                        }
                    }
                }

                Mesh m = new Mesh();

                // Organize indices and materials. This should hopefully perform better
                if (CPU_OPTIMIZE) {
                    var organizedIndices = new List<int>();
                    var organizedSubMeshes = new List<SubMeshDescription>();
                    var organizedMaterials = new List<Material>();
                    int vertTotal = 0;
                    foreach (var kvp in materialsAndSubMeshes) {
                        organizedMaterials.Add(kvp.Key);
                        var subMesh = new SubMeshDescription() { Start = organizedIndices.Count };
                        int indicesCountForMaterial = 0;

                        kvp.Value.ForEach(x => {
                            indicesCountForMaterial += x.length;
                            vertTotal += x.length / 3;
                            organizedIndices.AddRange(t.Skip(x.start).Take(x.length));
                        });
                        subMesh.Count = indicesCountForMaterial;
                        Logger.Log($"{subMesh.Count / 3} verts in sub mesh");
                        organizedSubMeshes.Add(subMesh);
                    }

                    Logger.Log($"{vertTotal} total vertices");

                    m.Vertices.AddRange(v);
                    // Logger.Log($"{m.Vertices.Count} verts");
                    m.Triangles.AddRange(organizedIndices);
                    m.SubMeshes.AddRange(organizedSubMeshes);
                    protoNode.VisualMaterials.AddRange(organizedMaterials);
                } else {
                    m.Vertices.AddRange(v);
                    Logger.Log($"{m.Vertices.Count} verts");
                    m.Triangles.AddRange(t);
                    m.SubMeshes.AddRange(subMeshes);
                    protoNode.VisualMaterials.AddRange(materials);
                }

                protoNode.VisualMesh = m;

                protoNode.MeshColliders.AddRange(meshColliders);
                protoNode.SphereColliders.AddRange(sphereColliders);

                Logger.Log($"{meshColliders.Count} Mesh Colliders");
                Logger.Log($"{sphereColliders.Count} Sphere Colliders");
                #endregion

                protoNode.PhysicalProperties = new PhysProps { Mass = 1, CenterOfMass = new Vec3() };
                protoNode.Guid = mesh.GUID;
                protoNode.ParentGuid = null;
                protoNode.IsStatic = true;

                #region Serialization
                DynamicObject dyno = new DynamicObject();
                dyno.Nodes.Add(protoNode);
                string name = source.Substring(source.LastIndexOf(Path.AltDirectorySeparatorChar) + 1);
                dyno.Name = name;

                protoField.Object = dyno;
                protoField.SerializerSignature = SERIALIZER_SIGNATURE;

                return protoField.Serialize(output);

                #endregion
            });
            return myTask;
        }

        public static Task<string> BXDJToProtoRobotAsync(string source, string output = null) {
            Logger.Log("Creating Task");

            var myTask = new Task<string>(() => {

                Logger.Log("Starting Translation from BXDJ to Proto");
                var rootNode = ReadSkeletonSafe(source + Path.AltDirectorySeparatorChar + "skeleton");
                var nodes = rootNode.ListAllNodes();

                var protoNodes = new Node[nodes.Count];
                var nodeTasks = new List<Task>();

                var bxdMeshes = new Dictionary<Guid, BXDAMesh>();
                var collectivePhysProps = new Dictionary<Guid, PhysProps>();
                var childrenDirectory = new Dictionary<Guid, List<Guid>>();
                var collectiveMasses = new Dictionary<Guid, float>();

                for (int i = 0; i < nodes.Count; i++) {
                    var mesh = new BXDAMesh();
                    mesh.ReadFromFile(source + Path.AltDirectorySeparatorChar + nodes[i].ModelFileName);
                    bxdMeshes.Add(nodes[i].GUID, mesh);

                    var physProps = new PhysProps {
                        Mass = mesh.physics.mass, CenterOfMass = mesh.physics.centerOfMass
                    };

                    if (nodes[i].GetParent() != null) {
                        if (childrenDirectory.ContainsKey(nodes[i].GetParent().GUID))
                            childrenDirectory[nodes[i].GetParent().GUID].Add(nodes[i].GUID);
                        else
                            childrenDirectory.Add(nodes[i].GetParent().GUID, new List<Guid>(new Guid[] { nodes[i].GUID }));
                    }

                    collectivePhysProps.Add(nodes[i].GUID, physProps);
                    Logger.Log($"All physical properties saved for node {i + 1}");
                }
                foreach (var n in nodes) {
                    float collectMass = bxdMeshes[n.GUID].physics.mass + GetCollectiveMass(n.GUID, bxdMeshes, childrenDirectory);
                    collectiveMasses[n.GUID] = collectMass;
                }
                Logger.Log("Cached all mesh data");

                object wheelAxisLock = new object();
                Vec3 wheelAxis = null;

                for (int _i = 0; _i < nodes.Count; _i++) {
                    int i = _i;
                    Logger.Log($"Creating node task {_i + 1}");
                    nodeTasks.Add(new Task(() => {

                        try {

                            var mesh = bxdMeshes.Values.ElementAt(i);
                            Logger.Log($"Starting node {i + 1} translation");
                            var protoNode = new Node {
                                Guid = nodes[i].GUID
                            };
                            if (nodes[i].GetParent() == null)
                                protoNode.ParentGuid = null;
                            else
                                protoNode.ParentGuid = nodes[i].GetParent().GUID;

                            protoNode.PhysicalProperties = collectivePhysProps[nodes[i].GUID];

                            #region Visual Mesh
                            
                            var v = new List<Vec3>();
                            var t = new List<int>();
                            var materials = new List<Material>();
                            var subMeshes = new List<SubMeshDescription>();
                            for (int j = 0; j < mesh.meshes.Count; j++) {
                                var sub = mesh.meshes[j];

                                for (int k = 0; k < sub.surfaces.Count; k++) {
                                    var surf = sub.surfaces[k];
                                    var cpy = new int[surf.indicies.Length];
                                    Array.Copy(surf.indicies, cpy, cpy.Length);
                                    Array.Reverse(cpy);
                                    var start = t.Count;
                                    foreach (int a in cpy) {
                                        t.Add(a + v.Count);
                                    }
                                    subMeshes.Add(new SubMeshDescription() { Start = start, Count = t.Count - start });

                                    materials.Add((Material)surf);
                                }
                                for (int k = 0; k < sub.verts.Length; k += 3) {
                                    v.Add(new Vec3 { X = (float)(sub.verts[k] * -0.01), Y = (float)(sub.verts[k + 1] * 0.01), Z = (float)(sub.verts[k + 2] * 0.01) });
                                }
                            }
                            Mesh m = new Mesh();
                            m.Vertices.AddRange(v);
                            m.Triangles.AddRange(t);
                            m.SubMeshes.AddRange(subMeshes);
                            protoNode.VisualMesh = m;
                            protoNode.VisualMaterials.AddRange(materials);
                            #endregion
                            Logger.Log($"Completed Visual Mesh for node {i + 1}");

                            #region Collision Meshes
                            
                            var colliders = new List<Mesh>();
                            for (int j = 0; j < mesh.colliders.Count; j++) {
                                var sub = mesh.colliders[j];
                                v.Clear();
                                t.Clear();
                                subMeshes.Clear();

                                for (int k = 0; k < sub.surfaces.Count; k++) {
                                    var cpy = new int[sub.surfaces[k].indicies.Length];
                                    Array.Copy(sub.surfaces[k].indicies, cpy, cpy.Length);
                                    Array.Reverse(cpy);
                                    var start = t.Count;
                                    t.AddRange(cpy);
                                    subMeshes.Add(new SubMeshDescription() { Start = start, Count = t.Count - start });
                                }
                                for (int k = 0; k < sub.verts.Length; k += 3) {
                                    v.Add(new Vec3 { X = (float)(sub.verts[k] * -0.01), Y = (float)(sub.verts[k + 1] * 0.01), Z = (float)(sub.verts[k + 2] * 0.01) });
                                }

                                m = new Mesh();
                                m.Vertices.AddRange(v);
                                m.Triangles.AddRange(t);
                                m.SubMeshes.AddRange(subMeshes);
                                colliders.Add(m);
                            }
                            protoNode.MeshColliders.AddRange(colliders);
                            #endregion
                            Logger.Log($"Completed Colliders for node {i + 1}");

                            #region Joint
                            // Joint
                            var skeletalJoint = nodes[i].GetSkeletalJoint();
                            if (skeletalJoint != null) {
                                var jointType = skeletalJoint.GetJointType();

                                switch (jointType) {
                                    case SkeletalJointType.ROTATIONAL:

                                        var jointData = skeletalJoint as RotationalJoint_Base;
                                        var rotationalJoint = new RotationalJoint();
                                        rotationalJoint.ConnectedBody = nodes[i].GetParent().GUID;
                                        rotationalJoint.Anchor = jointData.basePoint;
                                        rotationalJoint.Axis = ((Vec3)jointData.axis).Normalize();
                                        if (jointData.hasAngularLimit) {
                                            rotationalJoint.UseLimits = true;
                                            rotationalJoint.CurrentAngle = RadiansToDegrees(jointData.currentAngularPosition);
                                            rotationalJoint.LowerLimit = RadiansToDegrees(jointData.angularLimitLow);
                                            rotationalJoint.UpperLimit = RadiansToDegrees(jointData.angularLimitHigh);
                                        } else {
                                            rotationalJoint.UseLimits = false;
                                        }
                                        rotationalJoint.MassScale = collectiveMasses[nodes[i].GetParent().GUID] / collectiveMasses[nodes[i].GUID];

                                        rotationalJoint.IsWheel = HasDriverMeta<WheelDriverMeta>(nodes[i]) && GetDriverMeta<WheelDriverMeta>(nodes[i]).type != WheelType.NOT_A_WHEEL;
                                        if (rotationalJoint.IsWheel) {
                                            // TODO: Axis correction?
                                            switch (GetDriverMeta<WheelDriverMeta>(nodes[i]).type) {
                                                case WheelType.NORMAL:
                                                    rotationalJoint.WheelType =
                                                        RotationalJoint.Types.ProtoWheelType.Normal;
                                                    break;
                                                case WheelType.OMNI:
                                                    rotationalJoint.WheelType =
                                                        RotationalJoint.Types.ProtoWheelType.Omni;
                                                    break;
                                                case WheelType.MECANUM:
                                                    rotationalJoint.WheelType =
                                                        RotationalJoint.Types.ProtoWheelType.Mecanum;
                                                    break;
                                            }

                                            var bounds = protoNode.VisualMesh.Vertices.Bounds();
                                            var center = (bounds.min + bounds.max) / 2;
                                            var radius = (bounds.max - bounds.min).Magnitude / 2;
                                            protoNode.MeshColliders.Clear();
                                            protoNode.SphereColliders.Add(new Sphere() {Center = center, Radius = radius});
                                            protoNode.PhysicalProperties.StaticFriction = 2.2f;
                                            protoNode.PhysicalProperties.DynamicFriction = 0.9f;

                                            lock (wheelAxisLock) {
                                                if (wheelAxis == null) {
                                                    wheelAxis = rotationalJoint.Axis;
                                                    Logger.Log(
                                                        $"Axis Choosen: {wheelAxis.X}, {wheelAxis.Y}, {wheelAxis.Z}");
                                                } else if ((wheelAxis - rotationalJoint.Axis).Magnitude > 0.05f) {
                                                    Logger.Log(
                                                        $"Axis Replaced: {rotationalJoint.Axis.X}, {rotationalJoint.Axis.Y}, {rotationalJoint.Axis.Z}");
                                                    rotationalJoint.Axis = wheelAxis;
                                                }
                                                
                                            }
                                        }

                                        protoNode.RotationalJoint = rotationalJoint; // I think?

                                        break;
                                    default:
                                        protoNode.OtherJoint = new OtherJoint() {
                                            ConnectedBody = nodes[i].GetParent().GUID,
                                            MassScale = collectiveMasses[nodes[i].GetParent().GUID] / collectiveMasses[nodes[i].GUID]
                                        };
                                        break;
                                }
                            }
                            #endregion
                            Logger.Log($"Completed Joint for node {i + 1}");

                            protoNodes[i] = protoNode;

                        } catch (Exception e) {
                            Logger.Log($"Node {i} has a whoopsie");
                            Logger.Log(e.Message);
                            Logger.Log(e.StackTrace);
                            throw new Exception();
                        }

                    }));
                }

                nodeTasks.ForEach(x => x.Start());
                Logger.Log("All node tasks created. Waiting...");
                nodeTasks.ForEach(x => x.Wait());
                Logger.Log("Finished all node task");

                #region Serialization
                
                var dyno = new DynamicObject();
                dyno.Nodes.AddRange(protoNodes);
                dyno.Name = source.Substring(source.LastIndexOf(Path.AltDirectorySeparatorChar) + 1);

                var protoRobot = new ProtoRobot();
                protoRobot.Object = dyno;
                protoRobot.SerializerSignature = SERIALIZER_SIGNATURE;

                string o = protoRobot.Serialize(output);
                
                #endregion

                Logger.Log(" === BXDJ to Proto translation complete === ");
                return o;
            });

            Logger.Log("Task created");
            return myTask;
        }

        private static List<GamepieceDefinition> ParseFieldData(XmlDocument doc) {
            var root = doc.FirstChild;
            
            Logger.Log(doc.OuterXml);

            var gamepieceXmls = doc.SelectNodes("/FieldData/General/Gamepieces/gamepiece").ToList();

            var gamepieceDefinitions = new List<GamepieceDefinition>();
            gamepieceXmls.ForEach(x => {
                Logger.Log("Checking for Gamepiece");
                if (x.Attributes.Count != 0) { // WHY
                    var id = x.Attributes["id"].Value;
                    // TODO: Holding limit?
                    var X = float.Parse(x.Attributes["x"].Value);
                    var Y = float.Parse(x.Attributes["y"].Value);
                    var Z = float.Parse(x.Attributes["z"].Value);
                    Vec3 spawnPoint = new Vec3 { X = X, Y = Y, Z = Z };
                    gamepieceDefinitions.Add(new GamepieceDefinition { Name = id, SpawnLocation = spawnPoint });
                    Logger.Log($"Registering Gamepiece: {id}");
                }
            });
            return gamepieceDefinitions;
        }

        public static Vec3 ModVec(BXDQuaternion q, BXDVector3 original, BXDVector3 offset) {
            var uvec = new UVec3(original.x * -0.01f, original.y * 0.01f, original.z * 0.01f);
            var uquat = new UQuat(-q.X, q.Y, q.Z, -q.W).normalized;
            var v = uquat * uvec;
            return new Vec3() { X = v.x + (offset.x * -0.01f), Y = v.y + (offset.y * 0.01f), Z = v.z + (offset.z * 0.01f) };
        }

        private static float GetCollectiveMass(Guid a, Dictionary<Guid, BXDAMesh> meshes, Dictionary<Guid, List<Guid>> children) {
            if (!children.ContainsKey(a))
                return 0;
            float sum = 0.0f;
            foreach (var child in children[a]) {
                sum += meshes[child].physics.mass;
                sum += GetCollectiveMass(child, meshes, children);
            }
            return sum;
        }

        private static RigidNode_Base GetRoot(RigidNode_Base a) {
            RigidNode_Base b;
            while ((b = a.GetParent()) != null)
                a = b;
            return a;
        }

        private static RigidNode_Base ReadSkeletonSafe(string path) {
            string jsonPath = path + ".json";
            string xmlPath = path + ".bxdj";
            RigidNode_Base node = null;
            if (File.Exists(jsonPath)) {
                node = BXDJSkeletonJson.ReadSkeleton(jsonPath);
            } else {
                node = BXDJSkeleton.ReadSkeleton(xmlPath);
            }
            return node;
        }

        public static T GetDriverMeta<T>(RigidNode_Base node) where T : JointDriverMeta {
            return node != null && node.GetSkeletalJoint() != null && node.GetSkeletalJoint().cDriver != null ? node.GetSkeletalJoint().cDriver.GetInfo<T>() : null;
        }

        public static bool HasDriverMeta<T>(RigidNode_Base node) where T : JointDriverMeta {
            return GetDriverMeta<T>(node) != null;
        }
        #endregion
        
        #region Misc
        
        public static float RadiansToDegrees(float a) => (a * 180.0f) / (float)Math.PI;

        public struct TranslationType {

            public static readonly TranslationType BXDJ_TO_PROTO_ROBOT = new TranslationType("bxdj_proto_robot");
            public static readonly TranslationType BXDF_TO_PROTO_FIELD = new TranslationType("bxdf_proto_field");
            
            public string Indentifier { get; private set; }

            public TranslationType(string indentifier) {
                Indentifier = indentifier;
            }
        }

        private static HashAlgorithm sha = SHA256.Create();
        /// <summary>
        /// Hashes the contents of a file at a path
        /// </summary>
        /// <param name="path">Path to file to generate hash</param>
        /// <returns>Hash</returns>
        public static byte[] TempFileHash(string path) => sha.ComputeHash(File.ReadAllBytes(path));
        /// <summary>
        /// Hashes a byte buffer
        /// </summary>
        /// <param name="content">Content to hash</param>
        /// <returns>Hash</returns>
        public static byte[] TempFileHash(byte[] content) => sha.ComputeHash(content);
        /// <summary>
        /// Hashes the contents of a file at a path
        /// </summary>
        /// <param name="path">Path to file to generate hash</param>
        /// <returns>File name</returns>
        public static string TempFileName(string path) => TempFileHash(File.ReadAllBytes(path)).ToHexString();
        /// <summary>
        /// Hashes a byte buffer
        /// </summary>
        /// <param name="content">Content to hash</param>
        /// <returns>File name</returns>
        public static string TempFileName(byte[] content) => TempFileHash(content).ToHexString();

        #endregion

    }
}
*/