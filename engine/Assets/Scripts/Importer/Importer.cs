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
	public static class Importer
	{
		#region Profiling

		public static readonly ProfilerMarker RigidgroupParsing = new ProfilerMarker("Importer.RG_Parsing");
		public static readonly ProfilerMarker BodyCreation = new ProfilerMarker("Importer.Body_Creation");
		public static readonly ProfilerMarker JointCreation = new ProfilerMarker("Importer.Joint_Creation");
		public static readonly ProfilerMarker EntireImport = new ProfilerMarker("Importer.All");
		public static readonly ProfilerMarker CollisionIgnoring = new ProfilerMarker("Importer.Collision_Ignoring");

		#endregion

		#region Importer Framework

		public const UInt32 CURRENT_MIRA_EXPORTER_VERSION = 4;

		public delegate GameObject ImportFuncString(string path);

		public delegate GameObject ImportFuncBuffer(byte[] buffer);

		/// <summary>
		/// Default Importers that come stock with Synthesis. Leaves ability to add one post release
		/// </summary>
		public static Dictionary<SourceType, (ImportFuncString strFunc, ImportFuncBuffer bufFunc)> Importers
			= new Dictionary<SourceType, (ImportFuncString strFunc, ImportFuncBuffer bufFunc)>()
			{
				// {SourceType.PROTOBUF_FIELD, (LegacyFieldImporter.ProtoFieldImport, LegacyFieldImporter.ProtoFieldImport) },
				//{SourceType.MIRABUF_ASSEMBLY, (MirabufAssemblyImport, MirabufAssemblyImport)}
			};

		/// <summary>
		/// Import a serialized DynamicObject into a Unity Environment
		/// </summary>
		/// <param name="path">Path to serialized data (this could be a directory or a file depending on your import/translate function)</param>
		/// <param name="type">Type of import to conduct</param>
		/// <param name="transType">Optional translation type to use before importing</param>
		/// <param name="forceTranslate">Force a translation of source data regardless if a temp file exists. TODO: Probably remove in the future</param>
		/// <returns>Root GameObject of whatever Entity/Model you imported</returns>
		public static GameObject Import(string path, SourceType type)
			=> Importers[type].strFunc(path);

		public static GameObject Import(byte[] contents, SourceType type)
			=> Importers[type].bufFunc(contents);

		#endregion

		#region Mirabuf Importer

		public static (GameObject MainObject, Assembly MiraAssembly, SimObject Sim) MirabufAssemblyImport(string path, bool reverseSideJoints = false) {
			byte[] buff = File.ReadAllBytes(path);

			if (buff[0] == 0x1f && buff[1] == 0x8b) {

				int originalLength = BitConverter.ToInt32(buff, buff.Length - 4);

				MemoryStream mem = new MemoryStream(buff);
				byte[] res = new byte[originalLength];
				GZipStream decompresser = new GZipStream(mem, CompressionMode.Decompress);
				decompresser.Read(res, 0, res.Length);
				decompresser.Close();
				mem.Close();
				buff = res;
			}
			return MirabufAssemblyImport(buff, reverseSideJoints);
		}

		public static (GameObject MainObject, Assembly MiraAssembly, SimObject Sim) MirabufAssemblyImport(byte[] buffer, bool reverseSideJoints = false)
			=> MirabufAssemblyImport(Assembly.Parser.ParseFrom(buffer), reverseSideJoints);

		private static List<Collider> _collidersToIgnore;

		public static (GameObject MainObject, Assembly MiraAssembly, SimObject Sim) MirabufAssemblyImport(Assembly assembly, bool reverseSideJoints = false)
		{

			EntireImport.Begin();

			// Uncommenting this will delete all bodies so the JSON file isn't huge
			DebugAssembly(assembly);
			// return null;

			if (assembly.Info.Version < CURRENT_MIRA_EXPORTER_VERSION) {
				Logger.Log($"Out-of-date Assembly\nCurrent Version: {CURRENT_MIRA_EXPORTER_VERSION}\nVersion of Assembly: {assembly.Info.Version}", LogLevel.Warning);
			} else if (assembly.Info.Version > CURRENT_MIRA_EXPORTER_VERSION) {
				Logger.Log($"Hey Dev, the assembly you're importing is using a higher version than the current set version. Please update the CURRENT_MIRA_EXPORTER_VERSION constant", LogLevel.Debug);
			}

			// Logger.Log((new System.Diagnostics.StackTrace()).ToString(), LogLevel.Debug);

			UnityEngine.Physics.sleepThreshold = 0;

			RigidgroupParsing.Begin();

			GameObject assemblyObject = new GameObject(assembly.Info.Name);
			var parts = assembly.Data.Parts;
			MakeGlobalTransformations(assembly);
			var partObjects = new Dictionary<string, GameObject>(); // TODO: Do I need this?
			var groupObjects = new Dictionary<string, GameObject>();
			var jointToJointMap = new Dictionary<string, (UnityEngine.Joint a, UnityEngine.Joint b)>();
			float totalMass = 0;
			_collidersToIgnore = new List<Collider>();

			// Import Rigid Definitions
			#region Rigid Definitions

			var gamepieces = new List<GamepieceSimObject>();
			var rigidDefinitions = FindRigidbodyDefinitions(assembly);

			RigidgroupParsing.End();
			BodyCreation.Begin();

			foreach (var group in rigidDefinitions.definitions.Values)
			{
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
						// MARK: If transform changes do work recursively, apply transformations here instead of in a separate loop
						partObject.transform.ApplyMatrix(partInstance.GlobalTransform);
						collectivePhysData.Add((partObject.transform, partDefinition.PhysicalData));
					} else {
						Logger.Log($"Duplicate Part\nGroup name: {group.Name}\nGUID: {part.Key}", LogLevel.Warning);
					}
				}

				#endregion

				// Combine all physical data for grouping
				var combPhysProps = CombinePhysicalProperties(collectivePhysData);
				var rb = groupObject.AddComponent<Rigidbody>();
				if (isStatic)
					rb.isKinematic = true;
				rb.mass = (float) combPhysProps.Mass;
				totalMass += rb.mass;
				rb.centerOfMass = combPhysProps.Com; // I actually don't need to flip this
				groupObject.transform.parent = assemblyObject.transform;
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

			BodyCreation.End();

			#endregion

			#region Joints

			JointCreation.Begin();

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

				simObject = new RobotSimObject(assembly.Info.Name, state, assembly, groupObjects["grounded"], jointToJointMap);
				try {
					SimulationManager.RegisterSimObject(simObject);
				} catch {
					// TODO: Fix
					Logger.Log($"Robot with assembly {assembly.Info.Name} already exists.");
					UnityEngine.Object.Destroy(assemblyObject);
				}
			} else {
				simObject = new FieldSimObject(assembly.Info.Name, state, assembly, groupObjects["grounded"], gamepieces);
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
				var aKey = rigidDefinitions.partToDefinitionMap[jointKvp.Value.ParentPart];
				var a = groupObjects[aKey];
				// Logger.Log($"Child: {jointKvp.Value.ChildPart}", LogLevel.Debug);
				var bKey = rigidDefinitions.partToDefinitionMap[jointKvp.Value.ChildPart];
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

			JointCreation.End();

			CollisionIgnoring.Begin();

			// TODO: This is really slow
			for (var i = 0; i < _collidersToIgnore.Count - 1; i++)
			{
				for (var j = i + 1; j < _collidersToIgnore.Count; j++)
				{
					UnityEngine.Physics.IgnoreCollision(_collidersToIgnore[i], _collidersToIgnore[j]);
				}
			}

			CollisionIgnoring.End();

			#endregion

			if (assembly.Dynamic) {
				(simObject as RobotSimObject).ConfigureArcadeDrivetrain();
				(simObject as RobotSimObject).ConfigureArmBehaviours();
				(simObject as RobotSimObject).ConfigureSliderBehaviours();
			}

			EntireImport.End();

			return (assemblyObject, assembly, simObject);
		}

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

					// var moddedMat = parentPartInstance.GlobalTransform.UnityMatrix;

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

					var axisWut = new UVector3(definition.Rotational.RotationalFreedom.Axis.X, definition.Rotational.RotationalFreedom.Axis.Y, definition.Rotational.RotationalFreedom.Axis.Z);

					revoluteA.axis = axisWut;
						// ((UVector3)definition.Rotational.RotationalFreedom.Axis).normalized;
					revoluteA.connectedBody = rbB;
					revoluteA.connectedMassScale = revoluteA.connectedBody.mass / rbA.mass;
					revoluteA.useMotor = true;
					// TODO: Implement and test limits
					var limits = definition.Rotational.RotationalFreedom.Limits;
					if (limits != null && limits.Lower != limits.Upper) {
						revoluteA.useLimits = true;
						// revoluteA.limits = new JointLimits() {
						// 	min = limits.Lower * Mathf.Rad2Deg,
						// 	max = limits.Upper * Mathf.Rad2Deg
						// };
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
							new string[] {instance.SignalReference}, Array.Empty<string>(), simObject, revoluteA, revoluteB,
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
					// switch (definition.Prismatic.PrismaticFreedom.PivotDirection) {
					// 	case Axis.Y:
					// 		sliderB.xMotion = ConfigurableJointMotion.Locked;
					// 		sliderB.yMotion = ConfigurableJointMotion.Free;
					// 		sliderB.zMotion = ConfigurableJointMotion.Locked;
					// 		break;
					// 	case Axis.Z:
					// 		sliderB.xMotion = ConfigurableJointMotion.Locked;
					// 		sliderB.yMotion = ConfigurableJointMotion.Locked;
					// 		sliderB.zMotion = ConfigurableJointMotion.Free;
					// 		break;
					// 	case Axis.X:
					// 	default:
					// 		sliderB.xMotion = ConfigurableJointMotion.Free;
					// 		sliderB.yMotion = ConfigurableJointMotion.Locked;
					// 		sliderB.zMotion = ConfigurableJointMotion.Locked;
					// 		break;
					// }
					sliderB.xMotion = ConfigurableJointMotion.Free;
					sliderB.yMotion = ConfigurableJointMotion.Locked;
					sliderB.zMotion = ConfigurableJointMotion.Locked;
					sliderB.connectedBody = rbA;
					sliderB.connectedMassScale = sliderB.connectedBody.mass / rbB.mass;
					// var ulimitB = sliderA.linearLimit;
					// ulimitB.limit = midRange;

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
						if (addToColliderIgnore)
							_collidersToIgnore.Add(collider);
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

		#region New RigidDefinition Gathering

		/// <summary>
		/// I really don't like how I made this. It gets the job done but I feel like it
		/// could use a ton of optimizations.
		/// TODO: Maybe get rid of the dictionary in the rigidbodyDefinitions and just
		/// 	store keys. I feel like that would be a bit better.
		/// </summary>
		/// <param name="definitions"></param>
		/// <param name="assembly"></param>
		/// <returns></returns>
		private static (
			Dictionary<string, RigidbodyDefinition> definitions,
			Dictionary<string, string> partToDefinitionMap
			) FindRigidbodyDefinitions(Assembly assembly)
		{
			var defs = new Dictionary<string, RigidbodyDefinition>();
			var partMap = new Dictionary<string, string>();

			// Create grounded node
			// var groundedJoint = assembly.Data.Joints.JointInstances["grounded"];

			// I'm using lambda functions so I can reuse repeated logic while taking advantage of variables in the current scope
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
			// File.WriteAllText("C:\\Users\\hunte\\Documents\\paths.json", Newtonsoft.Json.JsonConvert.SerializeObject(paths));

			var discoveredRigidGroups = new List<RigidGroup>();

			(string guid, JointInstance joint) groundJoint = default;
			int counter = 0;

			

			// Create initial definitions
			foreach (var jInst in assembly.Data.Joints.JointInstances) {
				RigidbodyDefinition mainDef;
				RigidbodyDefinition stubDef;
				bool isGrounded = jInst.Key.Equals("grounded");
				if (isGrounded) {
					groundJoint = (jInst.Key, jInst.Value);
					continue; // Skip grounded to apply it last
				}

				mainDef = new RigidbodyDefinition {
					GUID = $"{counter}",
					Name = $"{jInst.Value.Info.Name}_Rigidgroup{counter}",
					Parts = new Dictionary<string, PartInstance>()
				};
				defs[mainDef.GUID] = mainDef;

				stubDef = new RigidbodyDefinition {
						GUID = $"{counter}-2",
						Name = $"{jInst.Value.Info.Name}_Rigidgroup{counter}-2",
						Parts = new Dictionary<string, PartInstance>()
				};
				defs[stubDef.GUID] = stubDef;

				// I'm slowly turning into Nick
				// Grab all the parts listed under the joint
				if ((jInst.Value.Parts != null && jInst.Value.Parts.Nodes != null) && jInst.Value.Parts.Nodes.Count > 0) {
					// I don't know why AllTreeElements sometimes returns null elements
					var tmpParts = jInst.Value.Parts.Nodes.AllTreeElements().Where(x => x.Value != null && x.Value != string.Empty)
						.Select(x => (x.Value, assembly.Data.Parts.PartInstances[x.Value]));
					var tmpRG = new RigidGroup {
						Name = $"discovered_{counter}"
					};
					tmpRG.Occurrences.Add(tmpParts.Select(x => x.Value));
					discoveredRigidGroups.Add(tmpRG);
					// tmpParts.ForEach(x => MoveToDef(x.Value, string.Empty, mainDef.GUID));
				}

				// Grab all parts eligable via design hierarchy
				var parentSiblings = IdentifyParentSiblings(paths, jInst.Value.ParentPart, jInst.Value.ChildPart);
				// Gonna make an action so I don't have to have the same code written twice
				Action<string, string> inherit = (part, definition) => {
					List<Node> toCheck = new List<Node>();
					List<Node> tmp = new List<Node>();
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
			RigidbodyDefinition groundDef = new RigidbodyDefinition {
				Name = "grounded",
				GUID = "grounded",
				Parts = new Dictionary<string, PartInstance>()
			};
			defs.Add(groundDef.GUID, groundDef);
			groundJoint.joint.Parts?.Nodes?.AllTreeElements().Where(x => x.Value != null && x.Value != string.Empty).ForEach(x => {
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
				RigidbodyDefinition rigidDef = new RigidbodyDefinition {
					Name = $"{x.Name}",
					GUID = $"{counter}",
					Parts = new Dictionary<string, PartInstance>()
				};
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
							// Logger.Log($"MERGING DEFINITION: {defs[existingDef].Name} based on {x.Name}", LogLevel.Debug);
							rigidDef.Name += $"_{defs[existingDef].Name}";
							MergeDefinitions(rigidDef, defs[existingDef]);
						}
					}
				});

				counter++;
			});

			bool wasRemoved = false;
			RigidbodyDefinition newGrounded = default;
			// Clear excess rigidbodies
			for (int i = 0; i < defs.Keys.Count; i++) {
				if (defs[defs.Keys.ElementAt(i)].Parts.Count == 0) {
					if (defs.Keys.ElementAt(i).Equals("grounded")) {
						var groundedPartGuid = assembly.Data.Joints.JointInstances.Find(x => x.Key.Equals("grounded")).Value.Parts.Nodes[0].Value;
						newGrounded = defs[partMap[groundedPartGuid]];
						wasRemoved = true;
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
				assembly.Data.Parts.PartDefinitions.Where(x => x.Value.Dynamic).ForEach(
					x => assembly.Data.Parts.PartInstances.Where(y => y.Value.PartDefinitionReference.Equals(x.Key))
						.ForEach(y => {
							RigidbodyDefinition rigidDef = new RigidbodyDefinition {
								Name = $"gamepiece_{gamepieceCounter}",
								GUID = $"{counter}",
								Parts = new Dictionary<string, PartInstance>()
							};
							defs.Add(rigidDef.GUID, rigidDef);
							GetAllPartsInBranch(y.Key, paths, assembly.DesignHierarchy.Nodes.ElementAt(0)).ForEach(z => MoveToDef(z, partMap[z], rigidDef.GUID));

							gamepieceCounter++;
							counter++;
						})
				);
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

			return (defs, partMap);
		}

		public static (string parent, string child) IdentifyParentSiblings(Dictionary<string, List<string>> paths, string parent, string child) {
			var parentAncestors = new List<string>();
			var childAncestors = new List<string>();
			parentAncestors.AddRange(paths[parent]);
			childAncestors.AddRange(paths[child]);
			if (parentAncestors.Count == 0 || childAncestors.Count == 0) {
				return (parentAncestors.Count == 0 ? parent : parentAncestors[0], childAncestors.Count == 0 ? child : childAncestors[0]);
			}
			while (parentAncestors[0].Equals(childAncestors[0])) {
				parentAncestors.RemoveAt(0);
				childAncestors.RemoveAt(0);

				if (parentAncestors.Count == 0 || childAncestors.Count == 0) {
					return (parentAncestors.Count == 0 ? parent : parentAncestors[0], childAncestors.Count == 0 ? child : childAncestors[0]);
				}
			}
			return (parentAncestors[0], childAncestors[0]);
		}

		public static Dictionary<string, List<string>> LoadPartTreePaths(GraphContainer designHierarchy) {
			Dictionary<string, List<string>> paths = new Dictionary<string, List<string>>();
			foreach (Node n in designHierarchy.Nodes[0].Children) {
				LoadPartTreePaths(n, paths);
			}
			return paths;
		}

		public static void LoadPartTreePaths(Node n, Dictionary<string, List<string>> paths, List<string> currentPath = null) {
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

		public static List<string> GetAllPartsInBranch(string rootPart, Dictionary<string, List<string>> paths, Node rootNode) {
			var parts = new List<string>();
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

		public static Node NavigateDHPath(List<string> path, Node rootNode) {
			var current = rootNode;
			while (path.Count > 0) {
				current = current.Children.First(x => x.Value.Equals(path[0]));
				path.RemoveAt(0);
			}
			return current;
		}

		#endregion

		public static Dictionary<string, Matrix4x4> MakeGlobalTransformations(Assembly assembly)
		{
			var map = new Dictionary<string, Matrix4x4>();
			foreach (Node n in assembly.DesignHierarchy.Nodes)
			{
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

			foreach (var kvp in map)
			{
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

		// Keeping it just incase
		#region Old RigidDefinition Gathering

		static List<Node> GatherNodes(Assembly assembly, JointInstance instance)
		{
			var nodes = new List<Node>(assembly.DesignHierarchy.Nodes);
			var newRoot = new List<Node>();
			var done = false;
			while (nodes.Any() && !done)
			{
				foreach (var node in nodes)
				{
					if (node.Value == instance.ParentPart)
					{
						newRoot.Add(node);
						done = true;
					}
				}

				var newNodes = new List<Node>();
				nodes.ForEach(y => newNodes.AddRange(y.Children));
				nodes = newNodes;
			}
			if (!done)
			{
				Logger.Log($"No child parts found for node {instance.Info.Name}");
				return new List<Node>();
			}

			var res = new List<Node>();
			while (newRoot.Any())
			{
				foreach (var node in newRoot)
				{
					res.Add(node);
				}

				var newNodes = new List<Node>();
				newRoot.ForEach(y => newNodes.AddRange(y.Children));
				newRoot = newNodes;
			}
			return res;
		}

		public static List<(JointInstance instance, List<Node> nodes)> GatherNodes(Assembly assembly)
		{
			var res = assembly.Data.Joints.JointInstances.Where(p => p.Value.Info.Name != "grounded")
				.AsParallel().Select(x => (x.Value, GatherNodes(assembly, x.Value))).ToList();
			/*
			var exclusionList = res.SelectMany(ji => ji.Item2.Select(x => x.Value)).ToList();
			*/
			var groundedJoint = assembly.Data.Joints.JointInstances.First(p => p.Value.Info.Name == "grounded");
			var nodes = new List<Node>(assembly.DesignHierarchy.Nodes);
			var groundedNodes = new List<Node>();
			while (nodes.Any())
			{
				groundedNodes.AddRange(nodes);
				var newNodes = new List<Node>();
				nodes.ForEach(y => newNodes.AddRange(y.Children));
				nodes = newNodes;
			}

			res.Insert(0, (groundedJoint.Value, groundedNodes.AsParallel().Except(res.AsParallel().SelectMany(p => p.Item2)).ToList()));
			return res;
		}

		public static List<Node> GatherGrounded(Assembly assembly)
		{
			var nodes = new List<Node>(assembly.DesignHierarchy.Nodes);
			var res = new List<Node>();
			var exclusionList = assembly.Data.Joints.JointInstances.Where(p => p.Value.Info.Name != "grounded")
				.Select(p => GatherNodes(assembly, p.Value).Select(n => n.Value)).SelectMany(x => x).ToList();
			while (nodes.Any())
			{
				foreach (var node in nodes)
				{
					if (!exclusionList.Contains(node.Value))
					{
						res.Add(node);
					}
				}

				var newNodes = new List<Node>();
				nodes.ForEach(y => newNodes.AddRange(y.Children));
				nodes = newNodes;
			}

			return res;
		}

		#endregion

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
		/// Collection of parts that move together
		/// </summary>
		public struct RigidbodyDefinition
		{
			public string                           GUID;
			public string                           Name;
			public Dictionary<string, PartInstance> Parts; // Using a dictionary to make Key searches faster
		}

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
