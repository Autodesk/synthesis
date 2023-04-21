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

		private const int FIELD_LAYER = 7;
		private const int DYNAMIC_1_LAYER = 8;
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
			var parts = assembly.Data.Parts;
			MakeGlobalTransformations(assembly);
			var partObjects = new Dictionary<string, GameObject>(); // TODO: Do I need this?
			var groupObjects = new Dictionary<string, GameObject>();
			var jointToJointMap = new Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)>();
			float totalMass = 0;

			#region Rigid Definitions

			var gamepieces = new List<GamepieceSimObject>();
			var rigidDefinitions = miraLive.Definitions;

			foreach (var group in rigidDefinitions.Definitions.Values) {

				GameObject groupObject = new GameObject(group.Name);
				var collectivePhysData = new List<(UnityEngine.Transform, MPhysicalProperties)>();
				var isGamepiece = !assembly.Dynamic && group.Name.Contains("gamepiece");
				var isStatic = !assembly.Dynamic && group.Name.Contains("grounded");
				// Import Parts

				#region Parts

				foreach (var part in group.Parts) {
					if (!partObjects.ContainsKey(part.Value.Info.GUID)) {
						var partInstance = part.Value;
						var partDefinition = parts.PartDefinitions[partInstance.PartDefinitionReference];
						GameObject partObject = new GameObject(partInstance.Info.Name);

						MakePartDefinition(partObject, partDefinition, partInstance, assembly.Data, !isGamepiece, !isStatic);
						partObjects.Add(partInstance.Info.GUID, partObject);
						partObject.transform.parent = groupObject.transform;
						var gt = partInstance.GlobalTransform.CorrectUnityMatrix;
						partObject.transform.localPosition = gt.GetPosition();
						partObject.transform.localRotation = gt.rotation;
						// partObject.transform.ApplyMatrix(partInstance.GlobalTransform);
						collectivePhysData.Add((partObject.transform, partDefinition.PhysicalData));
					} else {
						Logger.Log($"Duplicate Part\nGroup name: {group.Name}\nGUID: {part.Key}", LogLevel.Warning);
					}
				}

				groupObject.transform.parent = assemblyObject.transform;

				#endregion

				if (!assembly.Dynamic && !isGamepiece)
					groupObject.transform.GetComponentsInChildren<UnityEngine.Transform>().ForEach(x => x.gameObject.layer = FIELD_LAYER);
				else if (assembly.Dynamic)
					groupObject.transform.GetComponentsInChildren<UnityEngine.Transform>().ForEach(x => x.gameObject.layer = DYNAMIC_1_LAYER);

				// Combine all physical data for grouping
				var combPhysProps = CombinePhysicalProperties(collectivePhysData);
				var rb = groupObject.AddComponent<Rigidbody>();
				if (isStatic)
					rb.isKinematic = true;
				rb.mass = (float) combPhysProps.Mass;
				totalMass += rb.mass;
				rb.centerOfMass = combPhysProps.Com; // I actually don't need to flip this
				
				if (isGamepiece) {
					var gpSim = new GamepieceSimObject(group.Name, groupObject);
					try {
						SimulationManager.RegisterSimObject(gpSim);
					} catch (Exception e) {
						// TODO: Fix
						throw e;
						Logger.Log($"Gamepiece with name {gpSim.Name} already exists.");
						UnityEngine.Object.Destroy(groupObject);
					}
					gamepieces.Add(gpSim);
				} else {
					groupObjects.Add(group.GUID, groupObject);
				}
				// DebugJointAxes.DebugPoints.Add((combPhysProps.Com, () => groupObject.transform.localToWorldMatrix));
			}

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

			if (assembly.Dynamic) {
				(simObject as RobotSimObject).ConfigureDefaultBehaviours();
				// (simObject as RobotSimObject).ConfigureTestSimulationBehaviours();
			}

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
					var revoluteA = a.AddComponent<HingeJoint>();

					var parentPartInstance = assembly.Data.Parts.PartInstances[instance.ParentPart];
					var parentPartDefinition = assembly.Data.Parts.PartDefinitions[parentPartInstance.PartDefinitionReference];

					var originA = (UVector3)(definition.Origin ?? new UVector3());
					// var firstPart = assembly.Data.Parts.PartInstances.First(x => x.Value.Info.Name.Equals(parentPartDefinition.Info.Name + ":1")).Value;
					// var firstMat = firstPart.GlobalTransform.UnityMatrix;
					// var from = Matrix4x4.TRS(
					// 	firstMat.GetPosition(),
					// 	new Quaternion(-firstMat.rotation.x, firstMat.rotation.y, firstMat.rotation.z, -firstMat.rotation.w),
					// 	UVector3.one
					// );
					// var to = Matrix4x4.TRS(
					// 	moddedMat.GetPosition(),
					// 	new Quaternion(-moddedMat.rotation.x, moddedMat.rotation.y, moddedMat.rotation.z, -moddedMat.rotation.w),
					// 	UVector3.one
					// );
					// moddedMat = DiffToTransformations(from, to);
					// var partOffset = assembly.Data.Parts.PartInstances[instance.ParentPart].GlobalTransform.UnityMatrix.GetPosition()
					// 	+ moddedMat.MultiplyPoint(originA - firstMat.GetPosition());
					UVector3 jointOffset = instance.Offset ?? new Vector3();
					revoluteA.anchor = originA + jointOffset;

					UVector3 axisWut;
					if (assembly.Info.Version < 5) {
						axisWut = new UVector3(definition.Rotational.RotationalFreedom.Axis.X, definition.Rotational.RotationalFreedom.Axis.Y, definition.Rotational.RotationalFreedom.Axis.Z);
					} else {
						axisWut = new UVector3(-definition.Rotational.RotationalFreedom.Axis.X, definition.Rotational.RotationalFreedom.Axis.Y, definition.Rotational.RotationalFreedom.Axis.Z);
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
							new string[] {instance.SignalReference}, new string[] {$"{instance.SignalReference}_encoder"}, simObject, revoluteA, revoluteB,
							assembly.Data.Joints.MotorDefinitions.ContainsKey(definition.MotorReference)
								? assembly.Data.Joints.MotorDefinitions[definition.MotorReference]
								: null
						);
						SimulationManager.AddDriver(assembly.Info.Name, driver);
					}

					jointMap.Add(instance.Info.GUID, (revoluteA, revoluteB));
					break;
				case JointMotion.Slider:

					UVector3 anchor = definition.Origin ?? new Vector3()
						+ instance.Offset ?? new Vector3();
					UVector3 axis = definition.Prismatic.PrismaticFreedom.Axis;
					axis = axis.normalized;
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
					sliderA.xMotion = ConfigurableJointMotion.Limited;
					sliderA.yMotion = ConfigurableJointMotion.Locked;
					sliderA.zMotion = ConfigurableJointMotion.Locked;
					sliderA.connectedBody = rbB;
					sliderA.connectedMassScale = sliderA.connectedBody.mass / rbA.mass;
					var ulimitA = sliderA.linearLimit;
					ulimitA.limit = Mathf.Abs(midRange);
					sliderA.linearLimit = ulimitA;

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

		public static Matrix4x4 DiffToTransformations(Matrix4x4 from, Matrix4x4 to) {
			return Matrix4x4.TRS(to.GetPosition() - from.GetPosition(), to.rotation * Quaternion.Inverse(from.rotation), UVector3.one);
		}

		public static string FindOriginalFromReferencePoint(Assembly assembly, PartDefinition def, UVector3 point) {
			string closest = string.Empty;
			float closestDist = float.MaxValue;
			assembly.Data.Parts.PartInstances.Where(y => y.Value.PartDefinitionReference.Equals(def.Info.GUID)).ForEach(p => {
				float dist = (p.Value.GlobalTransform.UnityMatrix.GetPosition() - point).magnitude;
				if (dist < closestDist) {
					closest = p.Key;
					closestDist = dist;
				}
			});
			return closest;
		}

		public static void MakePartDefinition(GameObject container, PartDefinition definition, PartInstance instance,
			AssemblyData assemblyData, bool addToColliderIgnore = true, bool isConvex = true)
		{
			PhysicMaterial physMat = new PhysicMaterial
			{
				dynamicFriction = 0.6f, // definition.PhysicalData.,
				staticFriction = 0.6f // definition.PhysicalData.Friction
			};
			foreach (var body in definition.Bodies)
			{
				var bodyObject = new GameObject(body.Info.Name);
				var filter = bodyObject.AddComponent<MeshFilter>();
				var renderer = bodyObject.AddComponent<MeshRenderer>();
				filter.sharedMesh = body.TriangleMesh.UnityMesh;
				renderer.material = assemblyData.Materials.Appearances.ContainsKey(instance.Appearance)
					? assemblyData.Materials.Appearances[instance.Appearance].UnityMaterial
					: assemblyData.Materials.Appearances.ContainsKey(body.AppearanceOverride)
						? assemblyData.Materials.Appearances[body.AppearanceOverride].UnityMaterial
						: Appearance.DefaultAppearance.UnityMaterial; // Setup the override
				// renderer.material = assemblyData.Materials.Appearances.ContainsKey(instance.Appearance)
				// 	? assemblyData.Materials.Appearances[instance.Appearance].UnityMaterial
				// 	: Appearance.DefaultAppearance.UnityMaterial;
				if (!instance.SkipCollider) {
					MeshCollider collider = null;
					try {
						collider = bodyObject.AddComponent<MeshCollider>();
						if (isConvex) {
							collider.convex = true;
							collider.sharedMesh = body.TriangleMesh.ColliderMesh; // Again, not sure if this actually works
						} else {
							collider.convex = false;
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
				bodyObject.transform.localScale = UVector3.one;
				// bodyObject.transform.ApplyMatrix(body.);
			}
		}

		private static MPhysicalProperties CombinePhysicalProperties(List<(UnityEngine.Transform trans, MPhysicalProperties prop)> props) {
			var total = 0.0f;
			var com = new UVector3();
			props.ForEach(x => total += (float)x.prop.Mass);
			props.ForEach(x => com += (x.trans.localToWorldMatrix.MultiplyPoint(x.prop.Com)) * (float)x.prop.Mass);
			com /= total;
			return new MPhysicalProperties { Mass = total, Com = com };
		}

		public static Dictionary<string, Matrix4x4> MakeGlobalTransformations(Assembly assembly) {

			var map = new Dictionary<string, Matrix4x4>();
			foreach (Node n in assembly.DesignHierarchy.Nodes) {
				Matrix4x4 trans = assembly.Data.Parts.PartInstances[n.Value].Transform == null
					? assembly.Data.Parts
						.PartDefinitions[assembly.Data.Parts.PartInstances[n.Value].PartDefinitionReference]
						.BaseTransform == null
						? Matrix4x4.identity
						: assembly.Data.Parts
							.PartDefinitions[assembly.Data.Parts.PartInstances[n.Value].PartDefinitionReference]
							.BaseTransform.UnityMatrix
					: assembly.Data.Parts.PartInstances[n.Value].Transform.UnityMatrix;
				map.Add(n.Value, trans);
				MakeGlobalTransformations(map, map[n.Value], assembly.Data.Parts, n);
			}

			foreach (var kvp in map) {
				assembly.Data.Parts.PartInstances[kvp.Key].GlobalTransform = map[kvp.Key];
			}

			return map;
		}

		public static void MakeGlobalTransformations(Dictionary<string, Matrix4x4> map, Matrix4x4 parent, Parts parts,
			Node node)
		{
			foreach (var child in node.Children)
			{
				if (!map.ContainsKey(child.Value))
				{
					map.Add(child.Value, parent * parts.PartInstances[child.Value].Transform.UnityMatrix);
					MakeGlobalTransformations(map, map[child.Value], parts, child);
				}
				else
				{
					Logger.Log($"Key \"{child.Value}\" already present in map; ignoring", LogLevel.Error);
				}
			}
		}

		#endregion

		#region Debug Functions

		public static void DebugAssembly(Assembly assembly)
		{
			Assembly debugAssembly;
			// debugAssembly.MergeFrom(assembly);
			MemoryStream ms = new MemoryStream(new byte[assembly.CalculateSize()]);
			ms.Seek(0, SeekOrigin.Begin);
			assembly.WriteTo(ms);
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
