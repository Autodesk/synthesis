using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Protobuf;
using Mirabuf;
using Mirabuf.Joint;
using Mirabuf.Signal;
using Synthesis.Util;
using SynthesisAPI.Simulation;
using UnityEngine;
using SimObjects.MixAndMatch;
using UMaterial           = UnityEngine.Material;
using UMesh               = UnityEngine.Mesh;
using Logger              = SynthesisAPI.Utilities.Logger;
using Assembly            = Mirabuf.Assembly;
using Vector3             = Mirabuf.Vector3;
using UVector3            = UnityEngine.Vector3;
using MPhysicalProperties = Mirabuf.PhysicalProperties;
using UPhysicalMaterial   = UnityEngine.PhysicMaterial;
using SynthesisAPI.Controller;
using Joint = Mirabuf.Joint.Joint;

namespace Synthesis.Import {
    public static class Importer {
#region Static Import Function

        public static (GameObject mainObject, MirabufLive[] miraLiveFiles, SimObject sim)
            ImportSimpleRobot(string filePath) {
            return ImportRobot(new ImportHelper(filePath));
        }

        public static (GameObject mainObject, MirabufLive[] miraLiveFiles, RobotSimObject sim)
            ImportMixAndMatchRobot(MixAndMatchRobotData mixAndMatchRobotData) {
            return ImportRobot(new ImportHelper(mixAndMatchRobotData));
        }

        private static (GameObject mainObject, MirabufLive[] miraLiveFiles, RobotSimObject sim)
            ImportRobot(ImportHelper importHelper) {
            importHelper.CreateSimObject();
            importHelper.MakeAllJoints();
            return (importHelper.AssemblyObject, importHelper.MiraLiveFiles, importHelper.SimObject as RobotSimObject);
        }

        public static (GameObject mainObject, MirabufLive[] miraLiveFiles, FieldSimObject sim)
            ImportField(string filePath) {
            ImportHelper importHelper = new ImportHelper(filePath);
            importHelper.ConfigureGamepieces();
            importHelper.CreateSimObject();
            importHelper.MakeAllJoints();
            return (importHelper.AssemblyObject, importHelper.MiraLiveFiles, importHelper.SimObject as FieldSimObject);
        }

#endregion

        /// <summary>Does the bulk of the work importing a mirabuf file to a robot in synthesis</summary>
        private class ImportHelper {
            private static ulong _robotTally; // Just a number to add to the name of the sim object spawned

            public GameObject AssemblyObject   => _assemblyObject;
            public MirabufLive[] MiraLiveFiles => _miraLiveFiles;
            public SimObject SimObject         => _simObject;

            private readonly MirabufLive[] _miraLiveFiles;
            private readonly GameObject _assemblyObject;
            private readonly Assembly[] _assemblies;
            private readonly(Dictionary<string, GameObject> objectDict, Matrix4x4 transformation)[] _groupObjects;
            private readonly List<GamepieceSimObject> _gamepieces = new();
            private SimObject _simObject;

            private readonly MixAndMatchRobotData _mixAndMatchRobotData;
            private readonly bool _isMixAndMatch;

#region Constructors &Setup

            public ImportHelper(MixAndMatchRobotData mixAndMatchRobotData)
                : this(mixAndMatchRobotData,
                      mixAndMatchRobotData.GlobalPartData.Select(part => new MirabufLive(part.MirabufPartFile))
                          .ToArray()) {}

            public ImportHelper(string filePath) : this(null, new[] { new MirabufLive(filePath) }) {}

            public ImportHelper(MixAndMatchRobotData mixAndMatchRobotData, MirabufLive[] miraLiveFiles) {
                _miraLiveFiles = miraLiveFiles;

                _mixAndMatchRobotData = mixAndMatchRobotData;
                _isMixAndMatch        = mixAndMatchRobotData != null;

                _assemblies = _miraLiveFiles.Select(m => m.MiraAssembly).ToArray();

                _assemblyObject = new GameObject(
                    $"{(_isMixAndMatch ? mixAndMatchRobotData!.Name :_assemblies[0].Info.Name)}_{_robotTally}");

                UnityEngine.Physics.sleepThreshold = 0;

                _groupObjects =
                    (mixAndMatchRobotData == null)
                        ? new[] { (miraLiveFiles[0].GenerateDefinitionObjects(_assemblyObject), Matrix4x4.identity) }
                        : MirabufDefinitionHelper.GenerateMixAndMatchDefinitionObjects(
                              _miraLiveFiles, _assemblyObject, mixAndMatchRobotData);
            }

            /// <summary>Populates the <see cref="_gamepieces">gamepieces</see> list with gamepieces found</summary>
            public void ConfigureGamepieces() {
                _groupObjects.ForEachIndex(
                    (i, x) => x.objectDict.Where(y => _miraLiveFiles[i].Definitions.Definitions[y.Key].IsGamepiece)
                                  .ForEach(z => {
                                      var gpSim = new GamepieceSimObject(
                                          _miraLiveFiles[i].Definitions.Definitions[z.Key].Name, z.Value);
                                      try {
                                          SimulationManager.RegisterSimObject(gpSim);
                                      } catch (Exception e) {
                                          // TODO: Fix
                                          throw e;
                                      }

                                      _gamepieces.Add(gpSim);
                                  }));
            }

            /// <summary>Creates and registers the robots sim object</summary>
            public void CreateSimObject() {
                List<Signals> allSignals = new();

                // Append part index to signal keys so two of the same part have different signals
                _assemblies.ForEachIndex((i, a) => {
                    Signals newSignals = new Signals();
                    if (a.Data.Signals != null) {
                        a.Data.Signals.SignalMap.ForEach(
                            kvp => { newSignals.SignalMap[kvp.Key + $"_{i}"] = kvp.Value; });
                        allSignals.Add(newSignals);
                    }
                });

                var state = new ControllableState(allSignals.ToArray());

                if (_assemblies[0].Dynamic) {
                    _robotTally++;
                    _simObject = new RobotSimObject(_assemblyObject.name, state, _miraLiveFiles,
                        _groupObjects[0].objectDict["grounded"], _isMixAndMatch, _mixAndMatchRobotData);
                } else {
                    _simObject = new FieldSimObject(_assemblies[0].Info.Name, state, _miraLiveFiles[0],
                        _groupObjects[0].objectDict["grounded"], _gamepieces);
                }

                void RegisterSimObject() {
                    try {
                        SimulationManager.RegisterSimObject(_simObject);
                    } catch {
                        // TODO: Fix
                        Logger.Log($"Field with assembly {_assemblies[0].Info.Name} already exists.");
                        UnityEngine.Object.Destroy(_assemblyObject);
                    }
                }

                RegisterSimObject();
            }

#endregion

#region Joints

            /// <summary>Create all of a robots joints excluding those connecting mix and match parts</summary>
            public void MakeAllJoints() {
                var rigidDefinitions = _miraLiveFiles.Select(m => m.Definitions).ToArray();

                _assemblies.ForEachIndex(
                    (partIndex, assembly) => assembly.Data.Joints.JointInstances.ForEach(jointKvp => {
                        if (jointKvp.Key != "grounded") {
                            var aKey = rigidDefinitions[partIndex].PartToDefinitionMap[jointKvp.Value.ParentPart];
                            var a    = _groupObjects[partIndex].objectDict[aKey];

                            var bKey = rigidDefinitions[partIndex].PartToDefinitionMap[jointKvp.Value.ChildPart];
                            var b    = _groupObjects[partIndex].objectDict[bKey];

                            MakeJoint(a, b, jointKvp.Value, _assemblies[partIndex],
                                _groupObjects[partIndex].transformation, partIndex);
                        }
                    }));
            }

            private static readonly float TWO_PI = 2f * Mathf.PI;

            /// <summary>Connects two robot parts with a joint. (ex: connecting a wheel to a drivetrain)</summary>
            private void MakeJoint(GameObject gameObjectA, GameObject gameObjectB, JointInstance instance,
                Assembly assembly, Matrix4x4 partTransform, int partIndex) {
                var definition = assembly.Data.Joints.JointDefinitions[instance.JointReference];

                var rigidbodyA = gameObjectA.GetComponent<Rigidbody>();
                var rigidbodyB = gameObjectB.GetComponent<Rigidbody>();

                switch (definition.JointMotionType) {
                    case JointMotion.Revolute:
                        if (instance.IsWheel(assembly))
                            WheelJoint();
                        else
                            HingeJoint();
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
                        var mat             = x.material;
                        mat.dynamicFriction = 0f;
                        mat.staticFriction  = 0f;
                        mat.frictionCombine = PhysicMaterialCombine.Multiply;
                        mat.bounceCombine   = PhysicMaterialCombine.Multiply;
                        mat.bounciness      = 0f;
                    });

                    var wheelA           = gameObjectA.AddComponent<FixedJoint>();
                    var originA          = (UVector3) (definition.Origin ?? new UVector3());
                    UVector3 jointOffset = instance.Offset ?? new Vector3();
                    wheelA.anchor        = partTransform.MultiplyPoint3x4(originA + jointOffset);

                    UVector3 axisWut;
                    if (assembly.Info.Version < 5) {
                        axisWut = new UVector3(definition.Rotational.RotationalFreedom.Axis.X,
                            definition.Rotational.RotationalFreedom.Axis.Y,
                            definition.Rotational.RotationalFreedom.Axis.Z);
                    } else {
                        axisWut = new UVector3(-definition.Rotational.RotationalFreedom.Axis.X,
                            definition.Rotational.RotationalFreedom.Axis.Y,
                            definition.Rotational.RotationalFreedom.Axis.Z);
                    }

                    wheelA.connectedBody      = rigidbodyB;
                    wheelA.connectedMassScale = rigidbodyB.mass / rigidbodyA.mass;
                    var wheelB                = gameObjectB.AddComponent<FixedJoint>();
                    wheelB.anchor             = wheelA.anchor;
                    wheelB.connectedBody      = rigidbodyA;
                    wheelB.connectedMassScale = rigidbodyA.mass / rigidbodyB.mass;

                    // TODO: Technically, a isn't guaranteed to be the wheel
                    var customWheel = gameObjectA.AddComponent<CustomWheel>();

                    if (instance.HasSignal()) {
                        var driver =
                            new WheelDriver(assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
                                new[] { instance.SignalReference, $"{instance.SignalReference}_mode" },
                                new[] { $"{instance.SignalReference}_encoder", $"{instance.SignalReference}_absolute" },
                                _simObject, instance, customWheel, wheelA.anchor, axisWut, float.NaN,
                                (assembly.Data.Joints.MotorDefinitions.ContainsKey(definition.MotorReference)
                                        ? definition.MotorReference
                                        : null)!);
                        SimulationManager.AddDriver(_simObject.Name, driver);
                    }
                }

                void HingeJoint() {
                    var revoluteA = gameObjectA.AddComponent<HingeJoint>();

                    var originA          = (UVector3) (definition.Origin ?? new UVector3());
                    UVector3 jointOffset = instance.Offset ?? new Vector3();
                    revoluteA.anchor     = partTransform.MultiplyPoint3x4(originA + jointOffset);

                    UVector3 axisWut;
                    if (assembly.Info.Version < 5) {
                        axisWut = new UVector3(definition.Rotational.RotationalFreedom.Axis.X,
                            definition.Rotational.RotationalFreedom.Axis.Y,
                            definition.Rotational.RotationalFreedom.Axis.Z);
                    } else {
                        axisWut = new UVector3(-definition.Rotational.RotationalFreedom.Axis.X,
                            definition.Rotational.RotationalFreedom.Axis.Y,
                            definition.Rotational.RotationalFreedom.Axis.Z);
                    }

                    revoluteA.axis               = partTransform.MultiplyVector(axisWut);
                    revoluteA.connectedBody      = rigidbodyB;
                    revoluteA.connectedMassScale = revoluteA.connectedBody.mass / rigidbodyA.mass;
                    revoluteA.useMotor           = true;
                    // TODO: Implement and test limits
                    var limits = definition.Rotational.RotationalFreedom.Limits;
                    if (limits != null && limits.Lower != limits.Upper) {
                        var currentPosition = definition.Rotational.RotationalFreedom.Value;
                        currentPosition =
                            (currentPosition % (TWO_PI)) - ((int) (currentPosition % (TWO_PI) / Mathf.PI) * (TWO_PI));
                        var min             = -(limits.Upper - currentPosition);
                        min                 = (min % (TWO_PI) -TWO_PI) % (TWO_PI);
                        var max             = -(limits.Lower - currentPosition);
                        max                 = (max % (TWO_PI) + TWO_PI) % (TWO_PI);
                        revoluteA.useLimits = true;
                        revoluteA.limits = new JointLimits() { min = min * Mathf.Rad2Deg, max = max * Mathf.Rad2Deg };
                        revoluteA.extendedLimits = true;
                    }

                    var revoluteB = gameObjectB.AddComponent<HingeJoint>();

                    // All of the rigidbodies have the same location and orientation so these are the same for both
                    // joints
                    revoluteB.anchor = revoluteA.anchor;
                    revoluteB.axis   = revoluteA.axis; // definition.Rotational.RotationalFreedom.Axis;

                    revoluteB.connectedBody      = rigidbodyA;
                    revoluteB.connectedMassScale = revoluteB.connectedBody.mass / rigidbodyB.mass;

                    // TODO: Encoder Signals
                    if (instance.HasSignal()) {
                        var driver =
                            new RotationalDriver(assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
                                new[] { $"{instance.SignalReference}_{partIndex}", $"{instance.SignalReference}_mode" },
                                new[] { $"{instance.SignalReference}_encoder", $"{instance.SignalReference}_absolute" },
                                _simObject, revoluteA, revoluteB, instance.IsWheel(assembly),
                                (assembly.Data.Joints.MotorDefinitions.ContainsKey(definition.MotorReference)
                                        ? definition.MotorReference
                                        : null) ??
                                    string.Empty);
                        SimulationManager.AddDriver(_simObject.Name, driver);
                    }
                }

                void SliderJoint() {
                    UVector3 anchor = definition.Origin ?? new Vector3() + instance.Offset ?? new Vector3();
                    UVector3 axis   = definition.Prismatic.PrismaticFreedom.Axis;
                    axis            = axis.normalized;
                    axis.x          = -axis.x;
                    float midRange  = ((definition.Prismatic.PrismaticFreedom.Limits.Lower +
                                          definition.Prismatic.PrismaticFreedom.Limits.Upper) /
                                         2) *
                                     0.01f;

                    var sliderA    = gameObjectA.AddComponent<ConfigurableJoint>();
                    sliderA.anchor = partTransform.MultiplyPoint3x4(anchor); // + (axis * midRange);
                    sliderA.axis   = partTransform.MultiplyVector(axis);
                    sliderA.autoConfigureConnectedAnchor = false;
                    sliderA.connectedAnchor              = partTransform.MultiplyPoint3x4(anchor + (axis * midRange));
                    sliderA.secondaryAxis                = partTransform.MultiplyVector(axis);
                    sliderA.angularXMotion               = ConfigurableJointMotion.Locked;
                    sliderA.angularYMotion               = ConfigurableJointMotion.Locked;
                    sliderA.angularZMotion               = ConfigurableJointMotion.Locked;
                    sliderA.xMotion                      = ConfigurableJointMotion.Limited;
                    sliderA.yMotion                      = ConfigurableJointMotion.Locked;
                    sliderA.zMotion                      = ConfigurableJointMotion.Locked;
                    if (Mathf.Abs(midRange) > 0.0001) {
                        sliderA.xMotion = ConfigurableJointMotion.Limited;

                        var ulimitA         = sliderA.linearLimit;
                        ulimitA.limit       = Mathf.Abs(midRange);
                        sliderA.linearLimit = ulimitA;
                    } else {
                        sliderA.xMotion = ConfigurableJointMotion.Free;
                    }
                    sliderA.connectedBody      = rigidbodyB;
                    sliderA.connectedMassScale = sliderA.connectedBody.mass / rigidbodyA.mass;

                    var sliderB                = gameObjectB.AddComponent<ConfigurableJoint>();
                    sliderB.anchor             = sliderA.anchor;
                    sliderB.axis               = sliderA.axis;
                    sliderB.angularXMotion     = ConfigurableJointMotion.Locked;
                    sliderB.angularYMotion     = ConfigurableJointMotion.Locked;
                    sliderB.angularZMotion     = ConfigurableJointMotion.Locked;
                    sliderB.xMotion            = ConfigurableJointMotion.Free;
                    sliderB.yMotion            = ConfigurableJointMotion.Locked;
                    sliderB.zMotion            = ConfigurableJointMotion.Locked;
                    sliderB.connectedBody      = rigidbodyA;
                    sliderB.connectedMassScale = sliderB.connectedBody.mass / rigidbodyB.mass;

                    if (instance.HasSignal()) {
                        assembly.Data.Joints.MotorDefinitions.TryGetValue(definition.MotorReference, out var motor);
                        var currentPosition = definition.Prismatic.PrismaticFreedom.Value;
                        var slideDriver =
                            new LinearDriver(assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
                                new[] { $"{instance.SignalReference}_{partIndex}" }, Array.Empty<string>(), _simObject,
                                sliderA, sliderB,
                                ((definition.Prismatic.PrismaticFreedom.Limits.Upper - currentPosition) * 0.01f,
                                    (definition.Prismatic.PrismaticFreedom.Limits.Lower - currentPosition) * 0.01f),
                                assembly.Data.Joints.MotorDefinitions.ContainsKey(definition.MotorReference)
                                    ? definition.MotorReference
                                    : null);
                        SimulationManager.AddDriver(_simObject.Name, slideDriver);
                    }
                }

                void FixedJoint() {
                    AddUnityFixedJoint(rigidbodyA, rigidbodyB, definition.Origin, instance.Offset);
                    AddUnityFixedJoint(rigidbodyB, rigidbodyA, definition.Origin, instance.Offset);

                    void AddUnityFixedJoint(
                        Rigidbody rootObject, Rigidbody otherObject, Vector3 origin, Vector3 offset) {
                        var fixedJoint = rootObject.gameObject.AddComponent<FixedJoint>();
                        fixedJoint.anchor =
                            partTransform.MultiplyPoint3x4((origin ?? new Vector3()) + (offset ?? new Vector3()));
                        fixedJoint.axis               = partTransform.MultiplyVector(UVector3.forward);
                        fixedJoint.connectedBody      = otherObject;
                        fixedJoint.connectedMassScale = fixedJoint.connectedBody.mass / rootObject.mass;
                    }
                }
            }

#endregion

            /// <summary>
            /// Gets the change between 2 transformations
            /// </summary>
            /// <param name="from">Starting transformation</param>
            /// <param name="to">End transformation</param>
            /// <returns>Transformation to apply to move from start to end</returns>
            public Matrix4x4 TransformationDelta(Matrix4x4 from, Matrix4x4 to) {
                return Matrix4x4.TRS(to.GetPosition() - from.GetPosition(),
                    to.rotation * Quaternion.Inverse(from.rotation), UVector3.one);
            }

            public void DebugAssembly(Assembly assembly) {
                Assembly debugAssembly;
                // debugAssembly.MergeFrom(assembly);
                MemoryStream ms = new MemoryStream(new byte[assembly.CalculateSize()]);
                ms.Seek(0, SeekOrigin.Begin);
                assembly.WriteTo(ms); // TODO: Causing issues all of a sudden [May 5th, 2023]
                ms.Seek(0, SeekOrigin.Begin);
                debugAssembly = Assembly.Parser.ParseFrom(ms);

                // Remove mesh data
                // debugAssembly.Data.Parts.PartDefinitions.ForEach((x, y) => { y.Bodies.Clear(); });
                debugAssembly.Data.Parts.PartDefinitions.Select(kvp => kvp.Value.Bodies).ForEach(x => x.ForEach(y => {
                    y.TriangleMesh = new TriangleMesh();
                }));

                var jFormatter = new JsonFormatter(JsonFormatter.Settings.Default);
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                      $"{Path.AltDirectorySeparatorChar}{debugAssembly.Info.Name}.json",
                    jFormatter.Format(debugAssembly));
            }

            public void DebugGraph(GraphContainer graph) {
                graph.Nodes.ForEach(x => DebugNode(x, 0));
            }

            public void DebugNode(Node node, int level) {
                int a         = 0;
                string prefix = "";
                while (a != level) {
                    prefix += "-";
                    a++;
                }

                Debug.Log($"{prefix} {node.Value}");
                node.Children.ForEach(x => DebugNode(x, level + 1));
            }

            private void DebugJoint(Assembly assembly, string joint) {
                var instance   = assembly.Data.Joints.JointInstances[joint];
                var definition = assembly.Data.Joints.JointDefinitions[instance.JointReference];

                Debug.Log(Enum.GetName(typeof(Joint.JointMotionOneofCase), definition.JointMotionCase));
            }

            /// <summary>
            /// Stock source type definitions
            /// </summary>
            public struct SourceType {
                public string FileEnding { get; private set; }
                public string Identifier { get; private set; }

                public SourceType(string identifier, string fileEnding) {
                    Identifier = identifier;
                    FileEnding = fileEnding;
                }
            }
        }
    }
}