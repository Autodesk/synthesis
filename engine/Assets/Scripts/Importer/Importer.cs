using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Mirabuf;
using Mirabuf.Joint;
using Mirabuf.Material;
using Mirabuf.Signal;
using Synthesis.Util;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;
using System.IO.Compression;
using Synthesis;
using Unity.Profiling;

using UMaterial           = UnityEngine.Material;
using UMesh               = UnityEngine.Mesh;
using Logger              = SynthesisAPI.Utilities.Logger;
using Assembly            = Mirabuf.Assembly;
using AssemblyData        = Mirabuf.AssemblyData;
using Enum                = System.Enum;
using Joint               = Mirabuf.Joint.Joint;
using Vector3             = Mirabuf.Vector3;
using UVector3            = UnityEngine.Vector3;
using Node                = Mirabuf.Node;
using MPhysicalProperties = Mirabuf.PhysicalProperties;
using JointMotor          = UnityEngine.JointMotor;
using UPhysicalMaterial   = UnityEngine.PhysicMaterial;
using SynthesisAPI.Controller;

#nullable enable

namespace Synthesis.Import {
    /// <summary>
    /// The Importer class connected functions with string parameters to import an Entity/Model into the Engine
    /// NOTE: This may be moved into the Engine instead of in it's own project.
    /// </summary>
    public static class Importer {
        private static ulong _robotTally = 0; // Just a number to add to the name of the sim object spawned

#region Mirabuf Importer

        /// <summary>
        /// Import a mirabuf assembly
        /// </summary>
        /// <param name="miraLive">Path to mirabuf file</param>
        /// <returns>A tuple of the main gameobject that contains the imported assembly, a reference to the live file,
        /// and the simobject controlling the assembly</returns>
        public static (GameObject MainObject, MirabufLive MiraAssembly, SimObject Sim)
            MirabufAssemblyImport(string path) {
            return MirabufAssemblyImport(MirabufLive.OpenMirabufFile(path));
        }

        /// <summary>
        /// Import a mirabuf assembly from a live file.
        /// </summary>
        /// <param name="miraLive">Live file for mirabuf data</param>
        /// <returns>A tuple of the main gameobject that contains the imported assembly, a reference to the live file,
        /// and the simobject controlling the assembly</returns>
        public static (GameObject MainObject, MirabufLive miraLive, SimObject Sim)
            MirabufAssemblyImport(MirabufLive miraLive) {
            Assembly assembly = miraLive.MiraAssembly;

            if (assembly.Info.Version < MirabufLive.OLDEST_MIRA_EXPORTER_VERSION) {
                Logger.Log(
                    $"Out-of-date Assembly\nCurrent Version: {MirabufLive.CURRENT_MIRA_EXPORTER_VERSION}\nVersion of Assembly: {assembly.Info.Version}",
                    LogLevel.Warning);
            } else if (assembly.Info.Version > MirabufLive.CURRENT_MIRA_EXPORTER_VERSION) {
                Logger.Log(
                    $"Hey Dev, the assembly you're importing is using a higher version than the current set version. Please update the CURRENT_MIRA_EXPORTER_VERSION constant",
                    LogLevel.Debug);
            }

            UnityEngine.Physics.sleepThreshold = 0;

            GameObject assemblyObject = new GameObject(assembly.Info.Name);
            var jointToJointMap       = new Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)>();
            float totalMass           = 0;

#region Rigid Definitions

            var gamepieces       = new List<GamepieceSimObject>();
            var rigidDefinitions = miraLive.Definitions;

            var groupObjects = miraLive.GenerateDefinitionObjects(assemblyObject, true);

            groupObjects.Where(x => miraLive.Definitions.Definitions[x.Key].IsGamepiece).ForEach(x => {
                var gpSim = new GamepieceSimObject(miraLive.Definitions.Definitions[x.Key].Name, x.Value);
                try {
                    SimulationManager.RegisterSimObject(gpSim);
                } catch (Exception e) {
                    throw e;
                }
                gamepieces.Add(gpSim);
            });

#endregion

#region Joints

            var state =
                assembly.Data.Signals == null ? new ControllableState() : new ControllableState(assembly.Data.Signals);

            SimObject simObject;
            if (assembly.Dynamic) {
                string name = $"{assembly.Info.Name}_{_robotTally}";
                _robotTally++;

                simObject = new RobotSimObject(name, state, miraLive, groupObjects["grounded"], jointToJointMap);
                try {
                    SimulationManager.RegisterSimObject(simObject);
                } catch {
                    // TODO: Fix
                    Logger.Log($"Robot with assembly {name} already exists.");
                    UnityEngine.Object.Destroy(assemblyObject);
                }
            } else {
                simObject =
                    new FieldSimObject(assembly.Info.Name, state, miraLive, groupObjects["grounded"], gamepieces);
                try {
                    SimulationManager.RegisterSimObject(simObject);
                } catch {
                    // TODO: Fix
                    Logger.Log($"Field with assembly {assembly.Info.Name} already exists.");
                    UnityEngine.Object.Destroy(assemblyObject);
                }
            }

            foreach (var jointKvp in assembly.Data.Joints.JointInstances) {
                if (jointKvp.Key == "grounded")
                    continue;

                var aKey = rigidDefinitions.PartToDefinitionMap[jointKvp.Value.ParentPart];
                var a    = groupObjects[aKey];
                var bKey = rigidDefinitions.PartToDefinitionMap[jointKvp.Value.ChildPart];
                var b    = groupObjects[bKey];

                MakeJoint(a, b, jointKvp.Value, totalMass, assembly, simObject, jointToJointMap);
            }

#endregion

            return (assemblyObject, miraLive, simObject);
        }

#endregion

#region Assistant Functions

        private readonly static float TWO_PI = 2f * Mathf.PI;

        public static void MakeJoint(GameObject a, GameObject b, JointInstance instance, float totalMass,
            Assembly assembly, SimObject simObject,
            Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)> jointMap) {
            // Stuff I'm gonna use for all joints
            var definition = assembly.Data.Joints.JointDefinitions[instance.JointReference];
            var rbA        = a.GetComponent<Rigidbody>();
            var rbB        = b.GetComponent<Rigidbody>();
            switch (definition.JointMotionType) {
                case JointMotion.Revolute: // Hinge/Revolution joint

                    if (instance.IsWheel(assembly)) {
                        rbA.transform.GetComponentsInChildren<Collider>().ForEach(x => {
                            x.material.dynamicFriction = 0f;
                            x.material.staticFriction  = 0f;
                            x.material.frictionCombine = PhysicMaterialCombine.Multiply;
                        });

                        var wheelA           = a.AddComponent<FixedJoint>();
                        var originA          = (UVector3) (definition.Origin ?? new UVector3());
                        UVector3 jointOffset = instance.Offset ?? new Vector3();
                        wheelA.anchor        = originA + jointOffset;

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

                        wheelA.connectedBody      = rbB;
                        wheelA.connectedMassScale = rbB.mass / rbA.mass;
                        var wheelB                = b.AddComponent<FixedJoint>();
                        wheelB.anchor             = wheelA.anchor;
                        wheelB.connectedBody      = rbA;
                        wheelB.connectedMassScale = rbA.mass / rbB.mass;

                        // TODO: Technically, a isn't guaranteed to be the wheel
                        var customWheel = a.AddComponent<CustomWheel>();

                        if (instance.HasSignal()) {
                            var driver =
                                new WheelDriver(assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
                                    new string[] { instance.SignalReference, $"{instance.SignalReference}_mode" },
                                    new string[] { $"{instance.SignalReference}_encoder",
                                        $"{instance.SignalReference}_absolute" },
                                    simObject, instance, customWheel, wheelA.anchor, axisWut, float.NaN,
                                    assembly.Data.Joints.MotorDefinitions.ContainsKey(definition.MotorReference)
                                        ? definition.MotorReference
                                        : null);
                            SimulationManager.AddDriver(simObject.Name, driver);
                        }

                        jointMap.Add(instance.Info.GUID, (wheelA, wheelB));

                    } else {
                        var revoluteA = a.AddComponent<HingeJoint>();

                        var parentPartInstance = assembly.Data.Parts.PartInstances[instance.ParentPart];
                        var parentPartDefinition =
                            assembly.Data.Parts.PartDefinitions[parentPartInstance.PartDefinitionReference];

                        var originA          = (UVector3) (definition.Origin ?? new UVector3());
                        UVector3 jointOffset = instance.Offset ?? new Vector3();
                        revoluteA.anchor     = originA + jointOffset;

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

                        revoluteA.axis               = axisWut;
                        revoluteA.connectedBody      = rbB;
                        revoluteA.connectedMassScale = revoluteA.connectedBody.mass / rbA.mass;
                        revoluteA.useMotor           = true;
                        // TODO: Implement and test limits
                        var limits = definition.Rotational.RotationalFreedom.Limits;
                        if (limits != null && limits.Lower != limits.Upper) {
                            var currentPosition = definition.Rotational.RotationalFreedom.Value;
                            currentPosition     = (currentPosition % (TWO_PI)) -
                                              ((int) (currentPosition % (TWO_PI) / Mathf.PI) * (TWO_PI));
                            var min             = -(limits.Upper - currentPosition);
                            min                 = (min % (TWO_PI) -TWO_PI) % (TWO_PI);
                            var max             = -(limits.Lower - currentPosition);
                            max                 = (max % (TWO_PI) + TWO_PI) % (TWO_PI);
                            revoluteA.useLimits = true;
                            revoluteA.limits =
                                new JointLimits() { min = min * Mathf.Rad2Deg, max = max * Mathf.Rad2Deg };
                            revoluteA.extendedLimits = true;
                        }

                        var revoluteB = b.AddComponent<HingeJoint>();

                        // All of the rigidbodies have the same location and orientation so these are the same for both
                        // joints
                        revoluteB.anchor = revoluteA.anchor;
                        revoluteB.axis   = revoluteA.axis; // definition.Rotational.RotationalFreedom.Axis;

                        revoluteB.connectedBody      = rbA;
                        revoluteB.connectedMassScale = revoluteB.connectedBody.mass / rbB.mass;

                        // TODO: Encoder Signals
                        if (instance.HasSignal()) {
                            var driver = new RotationalDriver(
                                assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
                                new string[] { instance.SignalReference, $"{instance.SignalReference}_mode" },
                                new string[] { $"{instance.SignalReference}_encoder",
                                    $"{instance.SignalReference}_absolute" },
                                simObject, revoluteA, revoluteB, instance.IsWheel(assembly),
                                assembly.Data.Joints.MotorDefinitions.ContainsKey(definition.MotorReference)
                                    ? definition.MotorReference
                                    : null);
                            SimulationManager.AddDriver(simObject.Name, driver);
                        }

                        jointMap.Add(instance.Info.GUID, (revoluteA, revoluteB));
                    }

                    break;
                case JointMotion.Slider:

                    UVector3 anchor = definition.Origin ?? new Vector3() + instance.Offset ?? new Vector3();
                    UVector3 axis   = definition.Prismatic.PrismaticFreedom.Axis;
                    axis            = axis.normalized;
                    axis.x          = -axis.x;
                    float midRange  = ((definition.Prismatic.PrismaticFreedom.Limits.Lower +
                                          definition.Prismatic.PrismaticFreedom.Limits.Upper) /
                                         2) *
                                     0.01f;

                    var sliderA                          = a.AddComponent<ConfigurableJoint>();
                    sliderA.anchor                       = anchor; // + (axis * midRange);
                    sliderA.axis                         = axis;
                    sliderA.autoConfigureConnectedAnchor = false;
                    sliderA.connectedAnchor              = anchor + (axis * midRange);
                    sliderA.secondaryAxis                = axis;
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
                    sliderA.connectedBody      = rbB;
                    sliderA.connectedMassScale = sliderA.connectedBody.mass / rbA.mass;

                    var sliderB                = b.AddComponent<ConfigurableJoint>();
                    sliderB.anchor             = sliderA.anchor;
                    sliderB.axis               = sliderA.axis;
                    sliderB.angularXMotion     = ConfigurableJointMotion.Locked;
                    sliderB.angularYMotion     = ConfigurableJointMotion.Locked;
                    sliderB.angularZMotion     = ConfigurableJointMotion.Locked;
                    sliderB.xMotion            = ConfigurableJointMotion.Free;
                    sliderB.yMotion            = ConfigurableJointMotion.Locked;
                    sliderB.zMotion            = ConfigurableJointMotion.Locked;
                    sliderB.connectedBody      = rbA;
                    sliderB.connectedMassScale = sliderB.connectedBody.mass / rbB.mass;

                    if (instance.HasSignal()) {
                        assembly.Data.Joints.MotorDefinitions.TryGetValue(definition.MotorReference, out var motor);
                        var currentPosition = definition.Prismatic.PrismaticFreedom.Value;
                        var slideDriver =
                            new LinearDriver(assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
                                new string[] { instance.SignalReference }, Array.Empty<string>(), simObject, sliderA,
                                sliderB, (motor?.SimpleMotor.MaxVelocity ?? 30f) / 100f,
                                ((definition.Prismatic.PrismaticFreedom.Limits.Upper - currentPosition) * 0.01f,
                                    (definition.Prismatic.PrismaticFreedom.Limits.Lower - currentPosition) * 0.01f));
                        SimulationManager.AddDriver(simObject.Name, slideDriver);
                    }

                    break;
                default: // Make a rigid joint
                    var rigidA           = a.AddComponent<FixedJoint>();
                    rigidA.anchor        = (definition.Origin ?? new Vector3()) + (instance.Offset ?? new Vector3());
                    rigidA.axis          = UVector3.forward;
                    rigidA.connectedBody = rbB;
                    rigidA.connectedMassScale = rigidA.connectedBody.mass / rbA.mass;
                    var rigidB                = b.AddComponent<FixedJoint>();
                    rigidB.anchor        = (definition.Origin ?? new Vector3()) + (instance.Offset ?? new Vector3());
                    rigidB.axis          = UVector3.forward;
                    rigidB.connectedBody = rbA;
                    rigidB.connectedMassScale = rigidB.connectedBody.mass / rbB.mass;
                    break;
            }
        }

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

#endregion

#region Debug Functions

        public static void DebugGraph(GraphContainer graph) {
            graph.Nodes.ForEach(x => DebugNode(x, 0));
        }

        public static void DebugNode(Node node, int level) {
            int a         = 0;
            string prefix = "";
            while (a != level) {
                prefix += "-";
                a++;
            }

            Debug.Log($"{prefix} {node.Value}");
            node.Children.ForEach(x => DebugNode(x, level + 1));
        }

        private static void DebugJoint(Assembly assembly, string joint) {
            var instance   = assembly.Data.Joints.JointInstances[joint];
            var definition = assembly.Data.Joints.JointDefinitions[instance.JointReference];

            Debug.Log(Enum.GetName(typeof(Joint.JointMotionOneofCase), definition.JointMotionCase));
        }

#endregion

#region Assistant Types

        /// <summary>
        /// Stock source type definitions
        /// </summary>
        public struct SourceType {
            public static readonly SourceType MIRABUF_ASSEMBLY = new SourceType("mirabuf_assembly", "mira");
            public string FileEnding { get; private set; }
            public string Indentifier { get; private set; }

            public SourceType(string indentifier, string fileEnding) {
                Indentifier = indentifier;
                FileEnding  = fileEnding;
            }
        }

#endregion
    }
}