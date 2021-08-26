using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Behaviors;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mirabuf;
using Mirabuf.Joint;
using Mirabuf.Material;
using Mirabuf.Signal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Synthesis.Util;
using SynthesisAPI.Proto;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Material = SynthesisAPI.Proto.Material;
using Mesh = SynthesisAPI.Proto.Mesh;
using UMaterial = UnityEngine.Material;
using UMesh = UnityEngine.Mesh;
using Logger = SynthesisAPI.Utilities.Logger;
using Assembly = Mirabuf.Assembly;
using AssemblyData = Mirabuf.AssemblyData;
using Enum = System.Enum;
using Joint = Mirabuf.Joint.Joint;
using Transform = UnityEngine.Transform;
using Vector3 = Mirabuf.Vector3;
using UVector3 = UnityEngine.Vector3;
using Node = Mirabuf.Node;
using MPhysicalProperties = Mirabuf.PhysicalProperties;
using JointMotor = UnityEngine.JointMotor;
using Object = System.Object;

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

		public static GameObject MirabufAssemblyImport(string path, bool reverseSideJoints = false)
			=> MirabufAssemblyImport(File.ReadAllBytes(path), reverseSideJoints);

		public static GameObject MirabufAssemblyImport(byte[] buffer, bool reverseSideJoints = false)
			=> MirabufAssemblyImport(Assembly.Parser.ParseFrom(buffer), reverseSideJoints);

		private static List<Collider> _collidersToIgnore;

		public static GameObject MirabufAssemblyImport(Assembly assembly, bool reverseSideJoints = false)
		{
			// Uncommenting this will delete all bodies so the JSON file isn't huge
			//DebugAssembly(assembly);

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
			var simObject = new SimObject(assembly.Info.Name, state);
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

				Debug.Log($"Joint Key: {jointKvp.Key}");
				var a = groupObjects[jointKvp.Key];
				Debug.Log($"Child: {jointKvp.Value.ChildPart}");
				var bKey = rigidDefinitions.partToDefinitionMap[jointKvp.Value.ChildPart];
				var b = groupObjects[bKey];

				MakeJoint(
					a,
					b,
					jointKvp.Value,
					totalMass,
					assembly,
					simObject
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

			ConfigureDrivebase(assembly, assemblyObject, reverseSideJoints);

			return assemblyObject;
		}

		#region Assistant Functions

		public static void MakeJoint(
			GameObject a, GameObject b, JointInstance instance, float totalMass,
			Assembly assembly, SimObject simObject)
		{
			// Stuff I'm gonna use for all joints
			var definition = assembly.Data.Joints.JointDefinitions[instance.JointReference];
			var rbA = a.GetComponent<Rigidbody>();
			var rbB = b.GetComponent<Rigidbody>();
			switch (definition.JointMotionType)
			{
				case JointMotion.Revolute: // Hinge/Revolution joint
					var revoluteA = a.AddComponent<HingeJoint>();
					revoluteA.anchor = (definition.Origin ?? new Vector3())
					                   + (instance.Offset ?? new Vector3());
					revoluteA.axis =
						(((Matrix4x4) assembly.Data.Parts.PartInstances[instance.ParentPart].GlobalTransform).rotation)
						* definition.Rotational.RotationalFreedom.Axis; // CHANGE
					revoluteA.connectedBody = rbB;
					revoluteA.connectedMassScale = revoluteA.connectedBody.mass / rbA.mass;
					revoluteA.useMotor = definition.Info.Name != "grounded" &&
					                     definition.UserData != null &&
					                     definition.UserData.Data.TryGetValue("wheel", out var isWheel) &&
					                     isWheel == "true";
					var revoluteB = b.AddComponent<HingeJoint>();
					revoluteB.anchor = (definition.Origin ?? new Vector3())
					                   + (instance.Offset ?? new Vector3());
					revoluteB.axis = revoluteA.axis; // definition.Rotational.RotationalFreedom.Axis;
					revoluteB.connectedBody = rbA;
					revoluteB.connectedMassScale = revoluteB.connectedBody.mass / rbB.mass;

					// TODO: Encoder Signals
					var driver = new RotationalDriver(
						assembly.Data.Signals.SignalMap[instance.SignalReference].Info.Name,
						new string[] {instance.SignalReference}, Array.Empty<string>(), simObject, revoluteA, revoluteB,
						new JointMotor()
						{
							force = 400.0f,
							freeSpin = false,
							targetVelocity = 900,
						});
					SimulationManager.AddDriver(assembly.Info.Name, driver);
					break;
				case JointMotion.Slider:
				default: // Make a rigid joint
					var rigidA = a.AddComponent<FixedJoint>();
					rigidA.anchor = (definition.Origin ?? new Vector3())
					                + (instance.Offset ?? new Vector3());
					rigidA.axis = UVector3.forward;
					rigidA.connectedBody = rbB;
					rigidA.massScale = Mathf.Pow(totalMass / rbA.mass, 1); // Not sure if this works
					var rigidB = b.AddComponent<FixedJoint>();
					rigidB.anchor = (definition.Origin ?? new Vector3())
					                + (instance.Offset ?? new Vector3());
					rigidB.axis = UVector3.forward;
					rigidB.connectedBody = rbA;
					rigidB.connectedMassScale = Mathf.Pow(totalMass / rbA.mass, 1);
					break;
			}
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
			var com = new Vector3();
			props.ForEach(x => total += x.Mass);
			//props.ForEach(x => com += x.Com * x.Mass);
			//com /= total;
			return new MPhysicalProperties {Mass = total};
		}

		private static (
			Dictionary<string, RigidbodyDefinition> definitions,
			Dictionary<string, string> partToDefinitionMap
			) FindRigidbodyDefinitions(Assembly assembly)
		{
			var defs = new Dictionary<string, RigidbodyDefinition>();
			var partMap = new Dictionary<string, string>();

			int counter = 0;

			foreach (var (jointInstance, nodes) in GatherNodes(assembly))
			{
				var def = new RigidbodyDefinition
				{
					GUID = $"{jointInstance.Info.GUID}",
					Name = $"RigidGroup:{counter}",
					Parts = new Dictionary<string, PartInstance>()
				};
				foreach (var node in nodes)
				{
					Debug.Log($"Part GUID: \"{node.Value}\"");
					if(!def.Parts.ContainsKey(node.Value))
						def.Parts.Add(node.Value, assembly.Data.Parts.PartInstances[node.Value]);
					else
						Debug.Log($"Duplicate entry: {node.Value}");
				}

				defs.Add(def.GUID, def);
				foreach (var part in def.Parts)
				{
					partMap[part.Key] = def.GUID;
				}
				++counter;
			}

			/*foreach (var instance in assembly.Data.Joints.JointInstances)
			{
				var def = new RigidbodyDefinition
				{
					GUID = $"{instance.Value.Info.GUID}",
					Name = $"Rigidgroup:{counter}",
					Parts = new Dictionary<string, PartInstance>()
				};
				if (!string.IsNullOrEmpty(instance.Value.ParentPart))
					def.Parts.Add(instance.Value.ParentPart, assembly.Data.Parts.PartInstances[instance.Value.ParentPart]);
				counter++;
				var nodes = instance.Value.Parts == null ? new List<Node>() : new List<Node>(instance.Value.Parts.Nodes);

				var tmp = GatherNodes(assembly, instance.Value);

				while (nodes.Any())
				{
					foreach (var node in nodes)
					{
						Debug.Log($"Part GUID: \"{node.Value}\"");
						if (node.Value != string.Empty)
						{
							if (!def.Parts.ContainsKey(node.Value))
								def.Parts.Add(node.Value, assembly.Data.Parts.PartInstances[node.Value]);
							else
								Debug.Log($"Duplicate entry: {node.Value}");
						}
					}
					var newNodes = new List<Node>();
					nodes.ForEach(y => newNodes.AddRange(y.Children));
					nodes = newNodes;
				}

				defs.Add(def.GUID, def);
				foreach(var part in def.Parts)
				{
					partMap[part.Key] = def.GUID;
				}
			};
			*/
			return (defs, partMap);
		}

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
				map.Add(child.Value, parent * parts.PartInstances[child.Value].Transform.UnityMatrix);
				MakeGlobalTransformations(map, map[child.Value], parts, child);
			}
		}

		#endregion

		#region Robot Configuration

		public static void ConfigureDrivebase(Assembly assembly, GameObject assemblyObject, bool reverseSideJoints = false)
		{
			if (assembly.Data.Signals != null && assembly.Data.Joints.JointInstances != null)
			{
				var wheelsInstances = assembly.Data.Joints.JointInstances.Where(pair =>
					pair.Value.Info.Name != "grounded"
					&& assembly.Data.Joints.JointDefinitions[pair.Value.JointReference].UserData != null
					&& assembly.Data.Joints.JointDefinitions[pair.Value.JointReference].UserData.Data
						.TryGetValue("wheel", out var isWheel)
					&& isWheel == "true");

				var leftWheels = new List<JointInstance>();
				var rightWheels = new List<JointInstance>();

				foreach (var wheelInstance in wheelsInstances)
				{
					SimulationManager.SimulationObjects[assembly.Info.Name].State.CurrentSignals[wheelInstance.Value.SignalReference].Value = Value.ForNumber(0.0);
					var jointAnchor =
						(wheelInstance.Value.Offset ?? new Vector3()) + assembly.Data.Joints
							.JointDefinitions[wheelInstance.Value.JointReference].Origin ?? new Vector3();
					if (UnityEngine.Vector3.Dot(UnityEngine.Vector3.up, jointAnchor) > 0)
					{
						rightWheels.Add(wheelInstance.Value);
					}
					else
					{
						leftWheels.Add(wheelInstance.Value);
					}
				}

				assemblyObject.AddComponent<RobotInstance>();

				var wheels = wheelsInstances.Select(pair => pair.Value.SignalReference);
				assemblyObject.GetComponent<RobotInstance>()
					.SetLayout(assembly.Info, assembly.Data.Signals, wheels.ToList());

				SimulationManager.AddBehaviour(assembly.Info.Name, new ArcadeDrive(
					assembly.Info.Name,
					leftWheels.Select(j => j.SignalReference).ToList(),
					rightWheels.Select(j => j.SignalReference).ToList(),
					reversedSideJoints: reverseSideJoints));

			}
			else
			{
				Logger.Log($"No joints or signals found for {assembly.Info.Name}. Skipping.");
			}
		}

		public static List<Node> GatherNodes(Assembly assembly, JointInstance instance)
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
			var res = new List<(JointInstance, List<Node>)>();
			assembly.Data.Joints.JointInstances.Where(p => p.Value.Info.Name != "grounded").ForEach(x => res.Add((x.Value, GatherNodes(assembly,x.Value))));

			var exclusionList = res.SelectMany(ji => ji.Item2.Select(x => x.Value)).ToList();
			var groundedJoint = assembly.Data.Joints.JointInstances.First(p => p.Value.Info.Name == "grounded");
			var nodes = new List<Node>(assembly.DesignHierarchy.Nodes);
			var groundedNodes = new List<Node>();
			while (nodes.Any())
			{
				foreach (var node in nodes)
				{
					if (!exclusionList.Contains(node.Value))
					{
						groundedNodes.Add(node);
					}
				}

				var newNodes = new List<Node>();
				nodes.ForEach(y => newNodes.AddRange(y.Children));
				nodes = newNodes;
			}
			res.Insert(0, (groundedJoint.Value, groundedNodes));
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

		#region Debug Functions

		// Warning: This will remove all the bodies from the original assembly for some reason
		public static void DebugAssembly(Assembly assembly)
		{
			var debugAssembly = new Assembly();
			debugAssembly.MergeFrom(assembly);
			debugAssembly.Data.Parts.PartDefinitions.ForEach((x, y) => { y.Bodies.Clear(); });
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
