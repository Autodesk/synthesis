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

using UMaterial = UnityEngine.Material;
using UMesh = UnityEngine.Mesh;
using Logger = SynthesisAPI.Utilities.Logger;
using Assembly = Mirabuf.Assembly;
using AssemblyData = Mirabuf.AssemblyData;
using Enum = System.Enum;
using Joint = Mirabuf.Joint.Joint;
using Vector3 = Mirabuf.Vector3;
using UVector3 = UnityEngine.Vector3;
using Node = Mirabuf.Node;
using MPhysicalProperties = Mirabuf.PhysicalProperties;
using JointMotor = UnityEngine.JointMotor;
using UPhysicalMaterial = UnityEngine.PhysicMaterial;

namespace Synthesis.Import
{
	/// <summary>
	/// The Importer class connected functions with string parameters to import an Entity/Model into the Engine
	/// NOTE: This may be moved into the Engine instead of in it's own project.
	/// </summary>
	public static class Importer {

		#region Importer Framework

		public const UInt32 CURRENT_MIRA_EXPORTER_VERSION = 5;
		public const UInt32 OLDEST_MIRA_EXPORTER_VERSION = 4;

		public const int FIELD_LAYER = 7;
		public const int DYNAMIC_1_LAYER = 8;
		private const int DYNAMIC_2_LAYER = 9; // Reserved for later
		private const int DYNAMIC_3_LAYER = 10; // Reserved for later

		#endregion

		#region Mirabuf Importer

		/// <summary>
		/// Import a mirabuf assembly
		/// </summary>
		/// <param name="miraLive">Path to mirabuf file</param>
		/// <returns>A tuple of the main gameobject that contains the imported assembly, a reference to the live file, and the simobject controlling the assembly</returns>
		public static (GameObject MainObject, MirabufLive MiraAssembly, SimObject Sim) MirabufAssemblyImport(string path) {
			return MirabufAssemblyImport(new MirabufLive(path));
		}

		/// <summary>
		/// Import a mirabuf assembly from a live file.
		/// </summary>
		/// <param name="miraLive">Live file for mirabuf data</param>
		/// <returns>A tuple of the main gameobject that contains the imported assembly, a reference to the live file, and the simobject controlling the assembly</returns>
		public static (GameObject MainObject, MirabufLive miraLive, SimObject Sim) MirabufAssemblyImport(MirabufLive miraLive) {
			// Uncommenting this will delete all bodies so the JSON file isn't huge
			// DebugAssembly(miraLive.MiraAssembly);
			// return null;

			Assembly assembly = miraLive.MiraAssembly;

			if (assembly.Info.Version < OLDEST_MIRA_EXPORTER_VERSION) {
				Logger.Log($"Out-of-date Assembly\nCurrent Version: {CURRENT_MIRA_EXPORTER_VERSION}\nVersion of Assembly: {assembly.Info.Version}", LogLevel.Warning);
			} else if (assembly.Info.Version > CURRENT_MIRA_EXPORTER_VERSION) {
				Logger.Log($"Hey Dev, the assembly you're importing is using a higher version than the current set version. Please update the CURRENT_MIRA_EXPORTER_VERSION constant", LogLevel.Debug);
			}

			UnityEngine.Physics.sleepThreshold = 0;

			GameObject assemblyObject = new GameObject(assembly.Info.Name);
			var jointToJointMap = new Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)>();
			float totalMass = 0;

			#region Rigid Definitions

			var gamepieces = new List<GamepieceSimObject>();
			var rigidDefinitions = miraLive.Definitions;

			var groupObjects = miraLive.GenerateDefinitionObjects(assemblyObject, true);

			groupObjects.Where(x => miraLive.Definitions.Definitions[x.Key].IsGamepiece).ForEach(x => {
				var gpSim = new GamepieceSimObject(miraLive.Definitions.Definitions[x.Key].Name, x.Value);
				try {
					SimulationManager.RegisterSimObject(gpSim);
				} catch (Exception e) {
					// TODO: Fix
					throw e;
				}
				gamepieces.Add(gpSim);
			});

			#endregion

			#region Joints

			var state = new ControllableState
			{
				CurrentSignalLayout = assembly.Data.Signals ?? new Signals()
			};

			SimObject simObject;
			if (assembly.Dynamic) {
				List<string> foundRobots = new List<string>();
				foreach (var kvp in SimulationManager.SimulationObjects) {
					if (kvp.Value is RobotSimObject)
						foundRobots.Add(kvp.Key);
				}
				foundRobots.ForEach(x => SimulationManager.RemoveSimObject(x));

				simObject = new RobotSimObject(assembly.Info.Name, state, miraLive, groupObjects["grounded"], jointToJointMap);
				try {
					SimulationManager.RegisterSimObject(simObject);
				} catch {
					// TODO: Fix
					Logger.Log($"Robot with assembly {assembly.Info.Name} already exists.");
					UnityEngine.Object.Destroy(assemblyObject);
				}
			} else {
				simObject = new FieldSimObject(assembly.Info.Name, state, miraLive, groupObjects["grounded"], gamepieces);
				try {
					SimulationManager.RegisterSimObject(simObject);
				} catch {
					// TODO: Fix
					Logger.Log($"Field with assembly {assembly.Info.Name} already exists.");
					UnityEngine.Object.Destroy(assemblyObject);
				}
			}

			foreach (var jointKvp in assembly.Data.Joints.JointInstances)
			{
				if (jointKvp.Key == "grounded")
					continue;

				// Logger.Log($"Joint Instance: {jointKvp.Key}", LogLevel.Debug);
				// Logger.Log($"Parent: {jointKvp.Value.ParentPart}", LogLevel.Debug);
				var aKey = rigidDefinitions.PartToDefinitionMap[jointKvp.Value.ParentPart];
				var a = groupObjects[aKey];
				// Logger.Log($"Child: {jointKvp.Value.ChildPart}", LogLevel.Debug);
				var bKey = rigidDefinitions.PartToDefinitionMap[jointKvp.Value.ChildPart];
				var b = groupObjects[bKey];

				MakeJoint(
					a,
					b,
					jointKvp.Value,
					totalMass,
					assembly,
					simObject,
					jointToJointMap
				);
			}

			#endregion

			// if (assembly.Dynamic) {
			// 	(simObject as RobotSimObject).ConfigureDefaultBehaviours();
			// 	// (simObject as RobotSimObject).ConfigureTestSimulationBehaviours();
			// }

			return (assemblyObject, miraLive, simObject);
		}

		#endregion

		#region Assistant Functions

		public static void MakeJoint(
			GameObject a, GameObject b, JointInstance instance, float totalMass,
			Assembly assembly, SimObject simObject, Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)> jointMap)
		{
			// Logger.Log($"Obj A: {a.name}, Obj B: {b.name}");
			// Stuff I'm gonna use for all joints
			var definition = assembly.Data.Joints.JointDefinitions[instance.JointReference];
			var rbA = a.GetComponent<Rigidbody>();
			var rbB = b.GetComponent<Rigidbody>();
			switch (definition.JointMotionType)
			{
				case JointMotion.Revolute: // Hinge/Revolution joint

					if (instance.IsWheel(assembly)) {
						
						rbA.transform.GetComponentsInChildren<Collider>().ForEach(x => {
							x.material.dynamicFriction = 0f;
							x.material.staticFriction = 0f;
							x.material.frictionCombine = PhysicMaterialCombine.Multiply;
						});

						var wheelA = a.AddComponent<FixedJoint>();
						var originA = (UVector3)(definition.Origin ?? new UVector3());
						UVector3 jointOffset = instance.Offset ?? new Vector3();
						wheelA.anchor = originA + jointOffset;
						
						UVector3 axisWut;
						if (assembly.Info.Version < 5) {
							axisWut = new UVector3(definition.Rotational.RotationalFreedom.Axis.X, definition.Rotational.RotationalFreedom.Axis.Y,
								definition.Rotational.RotationalFreedom.Axis.Z);
						} else {
							axisWut = new UVector3(-definition.Rotational.RotationalFreedom.Axis.X, definition.Rotational.RotationalFreedom.Axis.Y,
								definition.Rotational.RotationalFreedom.Axis.Z);
						}

						wheelA.connectedBody = rbB;
						wheelA.connectedMassScale = rbB.mass / rbA.mass;
						var wheelB = b.AddComponent<FixedJoint>();
						wheelB.anchor = wheelA.anchor;
						wheelB.connectedBody = rbA;
						wheelB.connectedMassScale = rbA.mass / rbB.mass;

						// TODO: Technically, a isn't guaranteed to be the wheel
						var customWheel = a.AddComponent<CustomWheel>();

						if (instance.HasSignal()) {
							var driver = new WheelDriver(
								assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
								new string[] { instance.SignalReference, $"{instance.SignalReference}_mode" },
								new string[] { $"{instance.SignalReference}_encoder", $"{instance.SignalReference}_absolute" },
								simObject,
								instance,
								customWheel,
								wheelA.anchor,
								axisWut,
								float.NaN,
								assembly.Data.Joints.MotorDefinitions.ContainsKey(definition.MotorReference)
									? assembly.Data.Joints.MotorDefinitions[definition.MotorReference]
									: null
							);
							SimulationManager.AddDriver(assembly.Info.Name, driver);
						}
						
						jointMap.Add(instance.Info.GUID, (wheelA, wheelB));

					} else {

						var revoluteA = a.AddComponent<HingeJoint>();

						var parentPartInstance = assembly.Data.Parts.PartInstances[instance.ParentPart];
						var parentPartDefinition = assembly.Data.Parts.PartDefinitions[parentPartInstance.PartDefinitionReference];

						var originA = (UVector3)(definition.Origin ?? new UVector3());
						UVector3 jointOffset = instance.Offset ?? new Vector3();
						revoluteA.anchor = originA + jointOffset;

						UVector3 axisWut;
						if (assembly.Info.Version < 5) {
							axisWut = new UVector3(definition.Rotational.RotationalFreedom.Axis.X, definition.Rotational.RotationalFreedom.Axis.Y,
								definition.Rotational.RotationalFreedom.Axis.Z);
						} else {
							axisWut = new UVector3(-definition.Rotational.RotationalFreedom.Axis.X, definition.Rotational.RotationalFreedom.Axis.Y,
								definition.Rotational.RotationalFreedom.Axis.Z);
						}

						revoluteA.axis = axisWut;
						revoluteA.connectedBody = rbB;
						revoluteA.connectedMassScale = revoluteA.connectedBody.mass / rbA.mass;
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
						// revoluteA.useLimits = true;
						// revoluteA.limits = new JointLimits { min = -15, max = 15 };

						var revoluteB = b.AddComponent<HingeJoint>();

						// All of the rigidbodies have the same location and orientation so these are the same for both joints
						revoluteB.anchor = revoluteA.anchor;
						revoluteB.axis = revoluteA.axis; // definition.Rotational.RotationalFreedom.Axis;

						revoluteB.connectedBody = rbA;
						revoluteB.connectedMassScale = revoluteB.connectedBody.mass / rbB.mass;

						// TODO: Encoder Signals
						if (instance.HasSignal()) {
							var driver = new RotationalDriver(
								assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
								new string[] { instance.SignalReference, $"{instance.SignalReference}_mode" },
								new string[] { $"{instance.SignalReference}_encoder", $"{instance.SignalReference}_absolute" },
								simObject,
								revoluteA,
								revoluteB,
								instance.IsWheel(assembly),
								assembly.Data.Joints.MotorDefinitions.ContainsKey(definition.MotorReference)
									? assembly.Data.Joints.MotorDefinitions[definition.MotorReference]
									: null
							);
							SimulationManager.AddDriver(assembly.Info.Name, driver);
						}

						jointMap.Add(instance.Info.GUID, (revoluteA, revoluteB));

					}

					break;
				case JointMotion.Slider:

					UVector3 anchor = definition.Origin ?? new Vector3()
						+ instance.Offset ?? new Vector3();
					UVector3 axis = definition.Prismatic.PrismaticFreedom.Axis;
					axis = axis.normalized;
					axis.x = -axis.x;
					float midRange = ((definition.Prismatic.PrismaticFreedom.Limits.Lower + definition.Prismatic.PrismaticFreedom.Limits.Upper) / 2) * 0.01f;

					var sliderA = a.AddComponent<ConfigurableJoint>();
					sliderA.anchor = anchor; // + (axis * midRange);
					sliderA.axis = axis;
					sliderA.autoConfigureConnectedAnchor = false;
					sliderA.connectedAnchor = anchor + (axis * midRange);
					sliderA.secondaryAxis = axis;
					sliderA.angularXMotion = ConfigurableJointMotion.Locked;
					sliderA.angularYMotion = ConfigurableJointMotion.Locked;
					sliderA.angularZMotion = ConfigurableJointMotion.Locked;
					// switch (definition.Prismatic.PrismaticFreedom.PivotDirection) {
					// 	case Axis.Y:
					// 		sliderA.xMotion = ConfigurableJointMotion.Locked;
					// 		sliderA.yMotion = ConfigurableJointMotion.Limited;
					// 		sliderA.zMotion = ConfigurableJointMotion.Locked;
					// 		break;
					// 	case Axis.Z:
					// 		sliderA.xMotion = ConfigurableJointMotion.Locked;
					// 		sliderA.yMotion = ConfigurableJointMotion.Locked;
					// 		sliderA.zMotion = ConfigurableJointMotion.Limited;
					// 		break;
					// 	case Axis.X:
					// 	default:
					// 		sliderA.xMotion = ConfigurableJointMotion.Limited;
					// 		sliderA.yMotion = ConfigurableJointMotion.Locked;
					// 		sliderA.zMotion = ConfigurableJointMotion.Locked;
					// 		break;
					// }

					sliderA.yMotion = ConfigurableJointMotion.Locked;
					sliderA.zMotion = ConfigurableJointMotion.Locked;
					if (Mathf.Abs(midRange) > 0.0001) {
						sliderA.xMotion = ConfigurableJointMotion.Limited;
						
						var ulimitA = sliderA.linearLimit;
						ulimitA.limit = Mathf.Abs(midRange);
						sliderA.linearLimit = ulimitA;
					} else {
						sliderA.xMotion = ConfigurableJointMotion.Free;
					}
					sliderA.connectedBody = rbB;
					sliderA.connectedMassScale = sliderA.connectedBody.mass / rbA.mass;

					var sliderB = b.AddComponent<ConfigurableJoint>();
					sliderB.anchor = sliderA.anchor;
					sliderB.axis = sliderA.axis;
					sliderB.angularXMotion = ConfigurableJointMotion.Locked;
					sliderB.angularYMotion = ConfigurableJointMotion.Locked;
					sliderB.angularZMotion = ConfigurableJointMotion.Locked;
					sliderB.xMotion = ConfigurableJointMotion.Free;
					sliderB.yMotion = ConfigurableJointMotion.Locked;
					sliderB.zMotion = ConfigurableJointMotion.Locked;
					sliderB.connectedBody = rbA;
					sliderB.connectedMassScale = sliderB.connectedBody.mass / rbB.mass;

					if (instance.HasSignal()) {
						var slideDriver = new LinearDriver(assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
							new string[] { instance.SignalReference }, Array.Empty<string>(), simObject, sliderA, sliderB, 0.1f,
							(definition.Prismatic.PrismaticFreedom.Limits.Upper * 0.01f, definition.Prismatic.PrismaticFreedom.Limits.Lower * 0.01f));
						SimulationManager.AddDriver(simObject.Name, slideDriver);
					}

					break;
				default: // Make a rigid joint
					var rigidA = a.AddComponent<FixedJoint>();
					rigidA.anchor = (definition.Origin ?? new Vector3())
					                + (instance.Offset ?? new Vector3());
					rigidA.axis = UVector3.forward;
					rigidA.connectedBody = rbB;
					rigidA.connectedMassScale = rigidA.connectedBody.mass / rbA.mass;
					// rigidA.massScale = Mathf.Pow(totalMass / rbA.mass, 1); // Not sure if this works
					var rigidB = b.AddComponent<FixedJoint>();
					rigidB.anchor = (definition.Origin ?? new Vector3())
					                + (instance.Offset ?? new Vector3());
					rigidB.axis = UVector3.forward;
					rigidB.connectedBody = rbA;
					rigidB.connectedMassScale = rigidB.connectedBody.mass / rbB.mass;
					// rigidB.connectedMassScale = Mathf.Pow(totalMass / rbA.mass, 1);
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
			return Matrix4x4.TRS(to.GetPosition() - from.GetPosition(), to.rotation * Quaternion.Inverse(from.rotation), UVector3.one);
		}

		// public static void MakePartDefinition(GameObject container, PartDefinition definition, PartInstance instance,
		// 	AssemblyData assemblyData, bool addToColliderIgnore = true, bool isConvex = true)
		// {
		// 	PhysicMaterial physMat = new PhysicMaterial
		// 	{
		// 		dynamicFriction = 0.6f, // definition.PhysicalData.,
		// 		staticFriction = 0.6f // definition.PhysicalData.Friction
		// 	};
		// 	foreach (var body in definition.Bodies)
		// 	{
		// 		var bodyObject = new GameObject(body.Info.Name);
		// 		var filter = bodyObject.AddComponent<MeshFilter>();
		// 		var renderer = bodyObject.AddComponent<MeshRenderer>();
		// 		filter.sharedMesh = body.TriangleMesh.UnityMesh;
		// 		renderer.material = assemblyData.Materials.Appearances.ContainsKey(instance.Appearance)
		// 			? assemblyData.Materials.Appearances[instance.Appearance].UnityMaterial
		// 			: assemblyData.Materials.Appearances.ContainsKey(body.AppearanceOverride)
		// 				? assemblyData.Materials.Appearances[body.AppearanceOverride].UnityMaterial
		// 				: Appearance.DefaultAppearance.UnityMaterial; // Setup the override
		// 		// renderer.material = assemblyData.Materials.Appearances.ContainsKey(instance.Appearance)
		// 		// 	? assemblyData.Materials.Appearances[instance.Appearance].UnityMaterial
		// 		// 	: Appearance.DefaultAppearance.UnityMaterial;
		// 		if (!instance.SkipCollider) {
		// 			MeshCollider collider = null;
		// 			try {
		// 				collider = bodyObject.AddComponent<MeshCollider>();
		// 				if (isConvex) {
		// 					collider.convex = true;
		// 					collider.sharedMesh = body.TriangleMesh.ColliderMesh; // Again, not sure if this actually works
		// 				} else {
		// 					collider.convex = false;
		// 					collider.sharedMesh = body.TriangleMesh.UnityMesh;
		// 				}
		// 			} catch (Exception e) {
		// 				if (collider != null) {
		// 					GameObject.Destroy(collider);
		// 					collider = null;
		// 				}
		// 			}
		//
		// 			if (collider != null)
		// 				collider.material = physMat;
		// 				// if (addToColliderIgnore)
		// 				// 	_collidersToIgnore.Add(collider);
		// 		}
		// 		bodyObject.transform.parent = container.transform;
		// 		// Ensure all transformations are zeroed after assigning parent
		// 		bodyObject.transform.localPosition = UVector3.zero;
		// 		bodyObject.transform.localRotation = Quaternion.identity;
		// 		bodyObject.transform.localScale = UVector3.one;
		// 		// bodyObject.transform.ApplyMatrix(body.);
		// 	}
		// }

		// private static MPhysicalProperties CombinePhysicalProperties(List<(UnityEngine.Transform trans, MPhysicalProperties prop)> props) {
		// 	var total = 0.0f;
		// 	var com = new UVector3();
		// 	props.ForEach(x => total += (float)x.prop.Mass);
		// 	props.ForEach(x => com += (x.trans.localToWorldMatrix.MultiplyPoint(x.prop.Com)) * (float)x.prop.Mass);
		// 	com /= total;
		// 	return new MPhysicalProperties { Mass = total, Com = com };
		// }

		#endregion

		#region Debug Functions

		public static void DebugAssembly(Assembly assembly)
		{
			Assembly debugAssembly;
			// debugAssembly.MergeFrom(assembly);
			MemoryStream ms = new MemoryStream(new byte[assembly.CalculateSize()]);
			ms.Seek(0, SeekOrigin.Begin);
			assembly.WriteTo(ms); // TODO: Causing issues all of a sudden [May 5th, 2023]
			ms.Seek(0, SeekOrigin.Begin);
			debugAssembly = Assembly.Parser.ParseFrom(ms);

			// Remove mesh data
			// debugAssembly.Data.Parts.PartDefinitions.ForEach((x, y) => { y.Bodies.Clear(); });
			debugAssembly.Data.Parts.PartDefinitions.Select(kvp => kvp.Value.Bodies).ForEach(x => x.ForEach(y => { y.TriangleMesh = new TriangleMesh(); }));

			var jFormatter = new JsonFormatter(JsonFormatter.Settings.Default);
			File.WriteAllText(
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
				+ $"{Path.AltDirectorySeparatorChar}{debugAssembly.Info.Name}.json",
				jFormatter.Format(debugAssembly));
		}

		public static void DebugGraph(GraphContainer graph)
		{
			graph.Nodes.ForEach(x => DebugNode(x, 0));
		}

		public static void DebugNode(Node node, int level)
		{
			int a = 0;
			string prefix = "";
			while (a != level)
			{
				prefix += "-";
				a++;
			}

			Debug.Log($"{prefix} {node.Value}");
			node.Children.ForEach(x => DebugNode(x, level + 1));
		}

		private static void DebugJoint(Assembly assembly, string joint)
		{
			var instance = assembly.Data.Joints.JointInstances[joint];
			var definition = assembly.Data.Joints.JointDefinitions[instance.JointReference];

			Debug.Log(Enum.GetName(typeof(Joint.JointMotionOneofCase), definition.JointMotionCase));
		}

		#endregion

		#region Assistant Types

		/// <summary>
		/// Stock source type definitions
		/// </summary>
		public struct SourceType
		{
			public static readonly SourceType MIRABUF_ASSEMBLY = new SourceType("mirabuf_assembly", "mira");
			// public static readonly SourceType PROTOBUF_FIELD = new SourceType("proto_field", ProtoField.FILE_ENDING);

			public string FileEnding  { get; private set; }
			public string Indentifier { get; private set; }

			public SourceType(string indentifier, string fileEnding)
			{
				Indentifier = indentifier;
				FileEnding = fileEnding;
			}
		}

		#endregion
	}
}
