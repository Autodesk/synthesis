﻿using System;
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
using SynthesisAPI.Proto;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;
using System.IO.Compression;
using Synthesis;

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
		#region Importer Framework

		public delegate GameObject ImportFuncString(string path);

		public delegate GameObject ImportFuncBuffer(byte[] buffer);

		/// <summary>
		/// Default Importers that come stock with Synthesis. Leaves ability to add one post release
		/// </summary>
		public static Dictionary<SourceType, (ImportFuncString strFunc, ImportFuncBuffer bufFunc)> Importers
			= new Dictionary<SourceType, (ImportFuncString strFunc, ImportFuncBuffer bufFunc)>()
			{
				{SourceType.PROTOBUF_FIELD, (LegacyFieldImporter.ProtoFieldImport, LegacyFieldImporter.ProtoFieldImport) },
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

		public static GameObject MirabufAssemblyImport(string path, bool reverseSideJoints = false) {
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

		public static GameObject MirabufAssemblyImport(byte[] buffer, bool reverseSideJoints = false)
			=> MirabufAssemblyImport(Assembly.Parser.ParseFrom(buffer), reverseSideJoints);

		private static List<Collider> _collidersToIgnore;

		public static GameObject MirabufAssemblyImport(Assembly assembly, bool reverseSideJoints = false)
		{
			// Uncommenting this will delete all bodies so the JSON file isn't huge
			DebugAssembly(assembly);
			// return null;

			// Logger.Log((new System.Diagnostics.StackTrace()).ToString(), LogLevel.Debug);

			UnityEngine.Physics.sleepThreshold = 0;

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

			var rigidDefinitions = FindRigidbodyDefinitions(assembly);
			foreach (var group in rigidDefinitions.definitions.Values)
			{
				GameObject groupObject = new GameObject(group.Name);
				var collectivePhysData = new List<MPhysicalProperties>();
				// Import Parts

				#region Parts

				foreach (var part in group.Parts)
				{
					if (!partObjects.ContainsKey(part.Value.Info.GUID))
					{
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
					else
					{
						Logger.Log($"Duplicate key found with GUID '{part.Key}'", LogLevel.Debug);
					}
				}

				#endregion

				// Combine all physical data for grouping
				var combPhysProps = CombinePhysicalProperties(collectivePhysData);
				var rb = groupObject.AddComponent<Rigidbody>();
				rb.mass = (float) combPhysProps.Mass;
				totalMass += rb.mass;
				//rb.centerOfMass = combPhysProps.Com; // I actually don't need to flip this
				groupObject.transform.parent = assemblyObject.transform;
				groupObjects.Add(group.GUID, groupObject);
			}

			#endregion

			#region Joints


			var state = new ControllableState
			{
				CurrentSignalLayout = assembly.Data.Signals ?? new Signals()
			};
			var simObject = new RobotSimObject(assembly.Info.Name, state, assembly, groupObjects["grounded"], jointToJointMap);
			try
			{
				SimulationManager.RegisterSimObject(simObject);
			}
			catch
			{
				// TODO: Fix
				Logger.Log($"Robot with assembly {assembly.Info.Name} already exists.");
				UnityEngine.Object.Destroy(assemblyObject);
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


			for (var i = 0; i < _collidersToIgnore.Count - 1; i++)
			{
				for (var j = i + 1; j < _collidersToIgnore.Count; j++)
				{
					UnityEngine.Physics.IgnoreCollision(_collidersToIgnore[i], _collidersToIgnore[j]);
				}
			}

			#endregion

			simObject.ConfigureArcadeDrivetrain();

			if (!assembly.Dynamic)
				groupObjects["grounded"].GetComponent<Rigidbody>().isKinematic = true;

			// assemblyObject.AddComponent<RobotInstance>();
            // assemblyObject.GetComponent<RobotInstance>()
            //     .Init(assembly.Info, assembly.Data.Joints.JointInstances, assembly.Data.Joints.JointDefinitions, groupObjects["grounded"], assembly.Data.Signals, reverseSideJoints);

			return assemblyObject;
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

					var moddedMat = parentPartInstance.GlobalTransform.UnityMatrix;

					var originA = (UVector3)(definition.Origin ?? new UVector3());
					// var from = Matrix4x4.TRS(originA, Quaternion.identity, UVector3.one);
					// var firstPart = assembly.Data.Parts.PartInstances.First(
					// 	x => x.Value.PartDefinitionReference.Equals(
					// 		assembly.Data.Parts.PartInstances[instance.ParentPart].PartDefinitionReference
					// 	));
					// var firstPart = assembly.Data.Parts.PartInstances[
					// 	FindOriginalFromReferencePoint(assembly, parentPartDefinition, (UVector3)parentPartDefinition.PhysicalData.Com)
					// ];
					var firstPart = assembly.Data.Parts.PartInstances.First(x => x.Value.Info.Name.Equals(parentPartDefinition.Info.Name + ":1")).Value;
					var firstMat = firstPart.GlobalTransform.UnityMatrix;
					var from = Matrix4x4.TRS(
						firstMat.GetPosition(),
						new Quaternion(-firstMat.rotation.x, firstMat.rotation.y, firstMat.rotation.z, -firstMat.rotation.w),
						UVector3.one
					);
					var to = Matrix4x4.TRS(
						moddedMat.GetPosition(),
						new Quaternion(-moddedMat.rotation.x, moddedMat.rotation.y, moddedMat.rotation.z, -moddedMat.rotation.w),
						UVector3.one
					);
					moddedMat = DiffToTransformations(from, to);
					var partOffset = assembly.Data.Parts.PartInstances[instance.ParentPart].GlobalTransform.UnityMatrix.GetPosition()
						+ moddedMat.MultiplyPoint(originA - firstMat.GetPosition());
					// Logger.Log($"{assembly.Data.Parts.PartInstances[instance.ParentPart].Info.Name}: {partOffset.x}, {partOffset.y}, {partOffset.z}");
					UVector3 jointOffset = instance.Offset ?? new Vector3();
					// Logger.Log($"'{instance.Info.Name}' Joint Offset: {jointOffset.x}, {jointOffset.y}, {jointOffset.z}");
					// Logger.Log($"Definition Origin: {definition.Origin?.X}, {definition.Origin?.Y}, {definition.Origin?.Z}");
					revoluteA.anchor = /*partOffset*/originA + jointOffset;
					revoluteA.axis =
						definition.Rotational.RotationalFreedom.Axis;
						// moddedMat.MultiplyVector(definition.Rotational.RotationalFreedom.Axis); // CHANGE - ? -Hunter
					revoluteA.connectedBody = rbB;
					revoluteA.connectedMassScale = revoluteA.connectedBody.mass / rbA.mass;
					revoluteA.useMotor = definition.Info.Name != "grounded" &&
					                     definition.UserData != null &&
					                     definition.UserData.Data.TryGetValue("wheel", out var isWheel) &&
					                     isWheel == "true";
					// TODO: Implement and test limits
					// revoluteA.useLimits = true;
					// revoluteA.limits = new JointLimits { min = -15, max = 15 };
					
					var revoluteB = b.AddComponent<HingeJoint>();
					
					// All of the rigidbodies have the same location and orientation so these are the same for both joints
					revoluteB.anchor = revoluteA.anchor;
					revoluteB.axis = revoluteA.axis; // definition.Rotational.RotationalFreedom.Axis;
					
					revoluteB.connectedBody = rbA;
					revoluteB.connectedMassScale = revoluteB.connectedBody.mass / rbB.mass;

					// TODO: Encoder Signals
					var driver = new RotationalDriver(
						assembly.Data.Signals.SignalMap[instance.SignalReference].Info.GUID,
						new string[] {instance.SignalReference}, Array.Empty<string>(), simObject, revoluteA, revoluteB,
						new JointMotor()
						{
							force = 400.0f,
							freeSpin = false,
							targetVelocity = 900,
						});
					SimulationManager.AddDriver(assembly.Info.Name, driver);

					jointMap.Add(instance.Info.GUID, (revoluteA, revoluteB));
					break;
				case JointMotion.Slider:
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
			AssemblyData assemblyData)
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
				renderer.material = assemblyData.Materials.Appearances.ContainsKey(body.AppearanceOverride)
					? assemblyData.Materials.Appearances[body.AppearanceOverride].UnityMaterial
					: assemblyData.Materials.Appearances.ContainsKey(instance.Appearance)
						? assemblyData.Materials.Appearances[instance.Appearance].UnityMaterial
						: Appearance.DefaultAppearance.UnityMaterial; // Setup the override
				// renderer.material = assemblyData.Materials.Appearances.ContainsKey(instance.Appearance)
				// 	? assemblyData.Materials.Appearances[instance.Appearance].UnityMaterial
				// 	: Appearance.DefaultAppearance.UnityMaterial;
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

		private static MPhysicalProperties CombinePhysicalProperties(IEnumerable<MPhysicalProperties> props)
		{
			var total = 0.0;
			// var com = new Vector3();
			props.ForEach(x => total += x.Mass);
			//props.ForEach(x => com += x.Com * x.Mass);
			//com /= total;
			return new MPhysicalProperties {Mass = total};
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

			// Readd grounded if it was removed
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

			// Identify Orphaned parts and TBD
			// TODO: Gonna just expected orphaned parts are insignificant and any parts that should be included, should be rigidgrouped

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
			public static readonly SourceType PROTOBUF_FIELD = new SourceType("proto_field", ProtoField.FILE_ENDING);

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
