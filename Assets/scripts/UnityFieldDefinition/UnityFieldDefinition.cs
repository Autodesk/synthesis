using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityFieldDefinition : FieldDefinition
{
	public GameObject unityObject;

	public UnityFieldDefinition(Guid guid, string name)
		: base(guid, name)
	{
	}
	
	public void CreateTransform(Transform root)
	{
		unityObject = new GameObject();
		unityObject.transform.parent = root;
		unityObject.transform.position = new Vector3(0, 0, 0);
		unityObject.name = NodeGroup.NodeGroupID;
	}

	public bool CreateMesh(string filePath)
	{
		BXDAMesh mesh = new BXDAMesh();
		mesh.ReadFromFile(filePath, null);

		if (mesh.GUID.Equals(GUID))
		{
			List<FieldNode> remainingNodes = new List<FieldNode>(NodeGroup.EnumerateAllLeafFieldNodes());

			// Create all submesh objects
			auxFunctions.ReadMeshSet(mesh.meshes, delegate(int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
			{
				foreach (FieldNode node in remainingNodes)
				{
					if (node.MeshID == id)
					{
						GameObject subObject = new GameObject(node.NodeID);
						subObject.transform.parent = unityObject.transform;
						subObject.transform.position = new Vector3(0, 0, 0);
						
						subObject.AddComponent<MeshFilter>().mesh = meshu;
						subObject.AddComponent<MeshRenderer>();
						Material[] matls = new Material[meshu.subMeshCount];
						for (int i = 0; i < matls.Length; i++)
						{
							matls[i] = sub.surfaces[i].AsMaterial();
						}
						subObject.GetComponent<MeshRenderer>().materials = matls;
						
						Collider collider = null;
						
						if (GetPhysicsGroups().ContainsKey(node.PhysicsGroupID))
						{
							switch (GetPhysicsGroups()[node.PhysicsGroupID].CollisionType)
							{
							case PhysicsGroupCollisionType.MESH:
								collider = subObject.AddComponent<MeshCollider>();
								break;
							case PhysicsGroupCollisionType.BOX:
								collider = subObject.AddComponent<BoxCollider>();
								break;
							}
							
							if (collider != null)
							{
								collider.material.dynamicFriction = collider.material.staticFriction = GetPhysicsGroups()[node.PhysicsGroupID].Friction / 10f;
								collider.material.frictionCombine = PhysicMaterialCombine.Minimum;
								
								Rigidbody r = collider.gameObject.AddComponent<Rigidbody>();
								
								if (GetPhysicsGroups()[node.PhysicsGroupID].Mass > 0)
								{
									if (collider is MeshCollider)
									{
										((MeshCollider)collider).convex = true;
									}
									r.mass = (float)GetPhysicsGroups()[node.PhysicsGroupID].Mass * Init.PHYSICS_MASS_MULTIPLIER;
								}
								else
								{
									r.constraints = RigidbodyConstraints.FreezeAll;
									r.isKinematic = true;
								}
							}
						}

						remainingNodes.Remove(node);

						break;
					}
				}
			});
			
			#region Free mesh
			foreach (var list in new List<BXDAMesh.BXDASubMesh>[] { mesh.meshes, mesh.colliders })
			{
				foreach (BXDAMesh.BXDASubMesh sub in list)
				{
					sub.verts = null;
					sub.norms = null;
					foreach (BXDAMesh.BXDASurface surf in sub.surfaces)
					{
						surf.indicies = null;
					}
				}
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = null;
				}
			}
			mesh = null;
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
			#endregion

			return true;
		}
		else
		{
			Debug.Log("The BXDF and BXDA GUIDs didn't match. Could not load mesh.");
			return false;
		}
	}
}
