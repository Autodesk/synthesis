using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Protobuf;
using Mirabuf;
using Mirabuf.Joint;
using Synthesis.Util;
using SynthesisAPI.Simulation;
using UnityEngine;
using SimObjects.MixAndMatch;
using UMaterial           = UnityEngine.Material;
using UMesh               = UnityEngine.Mesh;
using Logger              = SynthesisAPI.Utilities.Logger;
using Assembly            = Mirabuf.Assembly;
using Enum                = System.Enum;
using Joint               = Mirabuf.Joint.Joint;
using Vector3             = Mirabuf.Vector3;
using UVector3            = UnityEngine.Vector3;
using Node                = Mirabuf.Node;
using MPhysicalProperties = Mirabuf.PhysicalProperties;
using UPhysicalMaterial   = UnityEngine.PhysicMaterial;
using SynthesisAPI.Controller;

namespace Synthesis.Import {
    /// <summary>
    /// The Importer class connected functions with string parameters to import an Entity/Model into the Engine
    /// NOTE: This may be moved into the Engine instead of in it's own project.
    /// </summary>
    public static class Importer {
        private static ulong _robotTally = 0; // Just a number to add to the name of the sim object spawned


        public static (GameObject mainObject, MirabufLive[] miraLiveFiles, SimObject sim)
            MirabufAssemblyImport(string[] filePaths, MixAndMatchTransformData mixAndMatchTransformData) {

            MirabufLive[] miraLiveFiles = filePaths.Select(path => new MirabufLive(path)).ToArray();

            return MirabufAssemblyImport(miraLiveFiles, mixAndMatchTransformData);
        }

        public static (GameObject mainObject, MirabufLive[] miraLiveFiles, SimObject sim)
            MirabufAssemblyImport(MirabufLive[] miraLiveFiles, MixAndMatchTransformData mixAndMatchTransformData) {

            Assembly[] assemblies = miraLiveFiles.Select(m => m.MiraAssembly).ToArray();

            // TODO: give root object a unique name 
            GameObject rootGameObject = new GameObject($"{assemblies[0].Info.Name}_{_robotTally}");
            GameObject[] assemblyObjects = assemblies.Select(a => new GameObject(a.Info.Name)).ToArray();

            assemblyObjects.ForEach(o => o.transform.SetParent(rootGameObject.transform));

            UnityEngine.Physics.sleepThreshold = 0;

            float totalMass = 0;

            var gamepieces = new List<GamepieceSimObject>();
            var rigidDefinitions = miraLiveFiles.Select(m => m.Definitions).ToArray();

            Dictionary<string, GameObject>[] groupObjects = new Dictionary<string, GameObject>[assemblyObjects.Length];
            miraLiveFiles.ForEachIndex((i, m) =>
                groupObjects[i] = m.GenerateDefinitionObjects(assemblyObjects[i]));

            groupObjects.ForEachIndex((i, x) => x
                .Where(x => miraLiveFiles[i].Definitions.Definitions[x.Key].IsGamepiece).ForEach(x => {
                    var gpSim = new GamepieceSimObject(miraLiveFiles[i].Definitions.Definitions[x.Key].Name, x.Value);
                    try {
                        SimulationManager.RegisterSimObject(gpSim);
                    }
                    catch (Exception e) {
                        // TODO: Fix
                        throw e;
                    }

                    gamepieces.Add(gpSim);
                }));

            SimObject simObject = CreateSimObject(assemblies, miraLiveFiles, groupObjects, gamepieces);
            RegisterSimObject(simObject, assemblies, assemblyObjects);

            MakeAllJoints(assemblies, rigidDefinitions, groupObjects, simObject);

            if (mixAndMatchTransformData != null) {
                PositionMixAndMatchParts(mixAndMatchTransformData, assemblyObjects);
                ConnectMixAndMatchParts(mixAndMatchTransformData, groupObjects);
            }

            return (rootGameObject, miraLiveFiles, simObject);
        }

        /// <summary>Creates and registers the robots sim object</summary>
        private static SimObject CreateSimObject(Assembly[] assemblies, MirabufLive[] miraLiveFiles, 
            Dictionary<string,GameObject>[] groupObjects, List<GamepieceSimObject> gamepieces) {
            var state =
                assemblies[0].Data.Signals == null
                    ? new ControllableState()
                    : new ControllableState(assemblies[0].Data.Signals);
            SimObject simObject;
            if (assemblies[0].Dynamic) {
                string name = $"{assemblies[0].Info.Name}_{_robotTally}";
                _robotTally++;

                simObject = new RobotSimObject(name, state, miraLiveFiles, groupObjects[0]["grounded"]);
            }
            else {
                simObject =
                    new FieldSimObject(assemblies[0].Info.Name, state, miraLiveFiles[0], groupObjects[0]["grounded"],
                        gamepieces);
            }

            return simObject;
        }

        private static void RegisterSimObject(SimObject simObject, Assembly[] assemblies, GameObject[] assemblyObjects) {
            try {
                SimulationManager.RegisterSimObject(simObject);
            }
            catch {
                // TODO: Fix
                Logger.Log($"Field with assembly {assemblies[0].Info.Name} already exists.");
                assemblyObjects.ForEach(UnityEngine.Object.Destroy);
            }
        }

        /// <summary>Create all of a robots joints excluding those connecting mix and match parts</summary>
        private static void MakeAllJoints(Assembly[] assemblies, MirabufLive.RigidbodyDefinitions[] rigidDefinitions, 
            Dictionary<string,GameObject>[] groupObjects, SimObject simObject) {
            assemblies.ForEachIndex((partIndex, assembly) => assembly.Data.Joints.JointInstances.ForEach(jointKvp => {
                if (jointKvp.Key != "grounded") {
                    var aKey = rigidDefinitions[partIndex].PartToDefinitionMap[jointKvp.Value.ParentPart];
                    var a = groupObjects[partIndex][aKey];

                    var bKey = rigidDefinitions[partIndex].PartToDefinitionMap[jointKvp.Value.ChildPart];
                    var b = groupObjects[partIndex][bKey];

                    MakeJoint(a, b, jointKvp.Value, assemblies[partIndex], simObject, partIndex);
                }
            }));
        }

        /// <summary>Connects two robot parts with a joint. (ex: connecting a wheel to a drivetrain)</summary>
        private static void MakeJoint(GameObject gameObjectA, GameObject gameObjectB, JointInstance instance,
            Assembly assembly, SimObject simObject, int partIndex) {

            var definition = assembly.Data.Joints.JointDefinitions[instance.JointReference];

            var rigidbodyA = gameObjectA.GetComponent<Rigidbody>();
            var rigidbodyB = gameObjectB.GetComponent<Rigidbody>();

            switch (definition.JointMotionType) {
                case JointMotion.Revolute:
                    if (instance.IsWheel(assembly))
                        WheelJoint();
                    else HingeJoint();
                    break;
                case JointMotion.Slider:
                    SliderJoint();
                    break;
                default:
                    FixedJoint();
                    break;
            }
            
            void WheelJoint() {
                rigidbodyA.transform.GetComponentsInChildren<Collider>().ForEach(x => {
                    var mat = x.material;
                    mat.dynamicFriction = 0f;
                    mat.staticFriction = 0f;
                    mat.frictionCombine = PhysicMaterialCombine.Multiply;
                });

                var wheelA = gameObjectA.AddComponent<FixedJoint>();
                var originA = (UVector3)(definition.Origin ?? new UVector3());
                UVector3 jointOffset = instance.Offset ?? new Vector3();
                wheelA.anchor = originA + jointOffset;

                UVector3 axisWut;
                if (assembly.Info.Version < 5) {
                    axisWut = new UVector3(definition.Rotational.RotationalFreedom.Axis.X,
                        definition.Rotational.RotationalFreedom.Axis.Y,
                        definition.Rotational.RotationalFreedom.Axis.Z);
                }
                else {
                    axisWut = new UVector3(-definition.Rotational.RotationalFreedom.Axis.X,
                        definition.Rotational.RotationalFreedom.Axis.Y,
                        definition.Rotational.RotationalFreedom.Axis.Z);
                }

                wheelA.connectedBody = rigidbodyB;
                wheelA.connectedMassScale = rigidbodyB.mass / rigidbodyA.mass;
                var wheelB = gameObjectB.AddComponent<FixedJoint>();
                wheelB.anchor = wheelA.anchor;
                wheelB.connectedBody = rigidbodyA;
                wheelB.connectedMassScale = rigidbodyA.mass / rigidbodyB.mass;

                // TODO: Technically, a isn't guaranteed to be the wheel
                var customWheel = gameObjectA.AddComponent<CustomWheel>();

                if (instance.HasSignal()) {
                    var driver =
                        new WheelDriver(assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID + partIndex,
                            new string[] {
                                instance.SignalReference + partIndex, $"{instance.SignalReference}_mode" + partIndex
                            },
                            new string[] {
                                $"{instance.SignalReference}_encoder" + partIndex,
                                $"{instance.SignalReference}_absolute" + partIndex
                            },
                            simObject, instance, customWheel, wheelA.anchor, axisWut, float.NaN,
                            assembly.Data.Joints.MotorDefinitions.ContainsKey(definition.MotorReference)
                                ? assembly.Data.Joints.MotorDefinitions[definition.MotorReference]
                                : null);
                    SimulationManager.AddDriver(simObject.Name, driver);
                }
            }

            void HingeJoint() {
                var revoluteA = gameObjectA.AddComponent<HingeJoint>();

                var parentPartInstance = assembly.Data.Parts.PartInstances[instance.ParentPart];
                var parentPartDefinition =
                    assembly.Data.Parts.PartDefinitions[parentPartInstance.PartDefinitionReference];

                var originA = (UVector3)(definition.Origin ?? new UVector3());
                UVector3 jointOffset = instance.Offset ?? new Vector3();
                revoluteA.anchor = originA + jointOffset;

                UVector3 axisWut;
                if (assembly.Info.Version < 5) {
                    axisWut = new UVector3(definition.Rotational.RotationalFreedom.Axis.X,
                        definition.Rotational.RotationalFreedom.Axis.Y,
                        definition.Rotational.RotationalFreedom.Axis.Z);
                }
                else {
                    axisWut = new UVector3(-definition.Rotational.RotationalFreedom.Axis.X,
                        definition.Rotational.RotationalFreedom.Axis.Y,
                        definition.Rotational.RotationalFreedom.Axis.Z);
                }

                revoluteA.axis = axisWut;
                revoluteA.connectedBody = rigidbodyB;
                revoluteA.connectedMassScale = revoluteA.connectedBody.mass / rigidbodyA.mass;
                revoluteA.useMotor = true;
                // TODO: Implement and test limits
                var limits = definition.Rotational.RotationalFreedom.Limits;
                if (limits != null && limits.Lower != limits.Upper) {
                    revoluteA.useLimits = true;
                    revoluteA.limits = new JointLimits() {
                        min = -limits.Upper * Mathf.Rad2Deg,
                        max = -limits.Lower * Mathf.Rad2Deg
                    };
                }

                var revoluteB = gameObjectB.AddComponent<HingeJoint>();

                // All of the rigidbodies have the same location and orientation so these are the same for both joints
                revoluteB.anchor = revoluteA.anchor;
                revoluteB.axis = revoluteA.axis;

                revoluteB.connectedBody = rigidbodyA;
                revoluteB.connectedMassScale = revoluteB.connectedBody.mass / rigidbodyB.mass;

                // TODO: Encoder Signals
                if (instance.HasSignal()) {
                    var driver = new RotationalDriver(
                        assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
                        new string[] { instance.SignalReference, $"{instance.SignalReference}_mode" },
                        new string[] {
                            $"{instance.SignalReference}_encoder",
                            $"{instance.SignalReference}_absolute"
                        },
                        simObject, revoluteA, revoluteB, instance.IsWheel(assembly),
                        assembly.Data.Joints.MotorDefinitions.TryGetValue(definition.MotorReference,
                            out var motorDefinition)
                            ? motorDefinition
                            : null);
                    SimulationManager.AddDriver(simObject.Name, driver);
                }
            }

            void SliderJoint() {
                UVector3 anchor = definition.Origin ?? new Vector3() + instance.Offset ?? new Vector3();
                UVector3 axis = definition.Prismatic.PrismaticFreedom.Axis;
                axis = axis.normalized;
                axis.x = -axis.x;
                float midRange = ((definition.Prismatic.PrismaticFreedom.Limits.Lower +
                                   definition.Prismatic.PrismaticFreedom.Limits.Upper) /
                                  2) *
                                 0.01f;

                var sliderA = AddUnitySlider(gameObjectA, rigidbodyA, rigidbodyB);
  
                sliderA.autoConfigureConnectedAnchor = false;
                sliderA.connectedAnchor = anchor + (axis * midRange);
                sliderA.secondaryAxis = axis;
                
                if (Mathf.Abs(midRange) > 0.0001) {
                    sliderA.xMotion = ConfigurableJointMotion.Limited;

                    var ulimitA = sliderA.linearLimit;
                    ulimitA.limit = Mathf.Abs(midRange);
                    sliderA.linearLimit = ulimitA;
                }
                else {
                    sliderA.xMotion = ConfigurableJointMotion.Free;
                }

                var sliderB = AddUnitySlider(gameObjectB, rigidbodyB, rigidbodyA);

                if (instance.HasSignal()) {
                    var slideDriver =
                        new LinearDriver(assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
                            new string[] { instance.SignalReference }, Array.Empty<string>(), simObject, sliderA,
                            sliderB, 0.1f,
                            (definition.Prismatic.PrismaticFreedom.Limits.Upper * 0.01f,
                                definition.Prismatic.PrismaticFreedom.Limits.Lower * 0.01f));
                    SimulationManager.AddDriver(simObject.Name, slideDriver);
                }

                ConfigurableJoint AddUnitySlider(GameObject rootObject,Rigidbody rootRB, Rigidbody otherRB) {
                    var slider = rootObject.AddComponent<ConfigurableJoint>();
                    slider.anchor = anchor;
                    slider.axis = axis;
                    
                    slider.angularXMotion = ConfigurableJointMotion.Locked;
                    slider.angularYMotion = ConfigurableJointMotion.Locked;
                    slider.angularZMotion = ConfigurableJointMotion.Locked;
                    
                    slider.xMotion = ConfigurableJointMotion.Limited;
                    slider.yMotion = ConfigurableJointMotion.Locked;
                    slider.zMotion = ConfigurableJointMotion.Locked;
                    
                    slider.connectedBody = otherRB;
                    slider.connectedMassScale = slider.connectedBody.mass / rootRB.mass;

                    return slider;
                }
            }

            void FixedJoint() {
                AddUnityFixedJoint(rigidbodyA, rigidbodyB, definition.Origin, instance.Offset);
                AddUnityFixedJoint(rigidbodyB, rigidbodyA, definition.Origin, instance.Offset);
            }
        }

        /// <summary>Locally positions each mix and match part</summary>
        private static void PositionMixAndMatchParts(MixAndMatchTransformData transformData,
            GameObject[] assemblyObjects) {
            assemblyObjects.ForEachIndex((partIndex, gameObject) => {
                var transform = gameObject.transform;

                var partTrfData = transformData.Parts[partIndex];
                transform.localPosition = partTrfData.LocalPosition;
                transform.localRotation = partTrfData.LocalRotation;
            });
        }

        /// <summary>Connects all mix and match parts together using <see cref="CreateMixAndMatchJoint">CreateMixAndMatchJoint</see></summary>
        private static void ConnectMixAndMatchParts(MixAndMatchTransformData transformData, Dictionary<string,GameObject>[] groupObjects) {
            transformData.Parts.ForEachIndex((partIndex, part) => {
                if (part.ConnectedPart != null) {
                    var thisObject = groupObjects[partIndex]["grounded"];
                    var connectedObject = groupObjects[part.ConnectedPart.PartIndex]["grounded"];
                    CreateMixAndMatchJoint(thisObject, connectedObject,
                        new Vector3(), new Vector3());
                }
                else Debug.Log($"Part connection was null");
            });
        }

        /// <summary>Creates a fixed joint between two mix and match parts. Pass in the parts grounded objects</summary>
        private static void CreateMixAndMatchJoint(GameObject gameObjectA, GameObject gameObjectB,
            Vector3 origin, Vector3 offset) {
            var rigidbodyA = gameObjectA.GetComponent<Rigidbody>();
            var rigidbodyB = gameObjectB.GetComponent<Rigidbody>();

            AddUnityFixedJoint(rigidbodyA, rigidbodyB, origin, offset);
            AddUnityFixedJoint(rigidbodyB, rigidbodyA, origin, offset);
        }
        
        /// <summary>Adds two <see cref="FixedJoint">FixedJoint</see> components connecting unity gameobjects</summary>
        private static void AddUnityFixedJoint(Rigidbody rootObject, Rigidbody otherObject, 
                Vector3 origin, Vector3 offset) {
            var fixedJoint = rootObject.gameObject.AddComponent<FixedJoint>();
            fixedJoint.anchor = (origin ?? new Vector3()) + (offset ?? new Vector3());
            fixedJoint.axis = UVector3.forward;
            fixedJoint.connectedBody = otherObject;
            fixedJoint.connectedMassScale = fixedJoint.connectedBody.mass / rootObject.mass;
        }

#region Unused Debug Code

        /// <summary>
        /// Gets the change between 2 transformations
        /// </summary>
        /// <param name="from">Starting transformation</param>
        /// <param name="to">End transformation</param>
        /// <returns>Transformation to apply to move from start to end</returns>
        public static Matrix4x4 TransformationDelta(Matrix4x4 from, Matrix4x4 to) {
            return Matrix4x4.TRS(
                to.GetPosition() - from.GetPosition(), to.rotation * Quaternion.Inverse(from.rotation), UVector3.one);
        }

        public static void DebugAssembly(Assembly assembly) {
            Assembly debugAssembly;
            // debugAssembly.MergeFrom(assembly);
            MemoryStream ms = new MemoryStream(new byte[assembly.CalculateSize()]);
            ms.Seek(0, SeekOrigin.Begin);
            assembly.WriteTo(ms); // TODO: Causing issues all of a sudden [May 5th, 2023]
            ms.Seek(0, SeekOrigin.Begin);
            debugAssembly = Assembly.Parser.ParseFrom(ms);

            // Remove mesh data
            // debugAssembly.Data.Parts.PartDefinitions.ForEach((x, y) => { y.Bodies.Clear(); });
            debugAssembly.Data.Parts.PartDefinitions.Select(kvp => kvp.Value.Bodies)
                .ForEach(x => x.ForEach(y => { y.TriangleMesh = new TriangleMesh(); }));

            var jFormatter = new JsonFormatter(JsonFormatter.Settings.Default);
            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                              $"{Path.AltDirectorySeparatorChar}{debugAssembly.Info.Name}.json",
                jFormatter.Format(debugAssembly));
        }

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

        /// <summary>
        /// Stock source type definitions
        /// </summary>
        public struct SourceType {
            public static readonly SourceType MIRABUF_ASSEMBLY = new SourceType("mirabuf_assembly", "mira");
            // public static readonly SourceType PROTOBUF_FIELD = new SourceType("proto_field", ProtoField.FILE_ENDING);

            public string FileEnding { get; private set; }
            public string Indentifier { get; private set; }

            public SourceType(string indentifier, string fileEnding) {
                Indentifier = indentifier;
                FileEnding = fileEnding;
            }
        }

#endregion
    }
}