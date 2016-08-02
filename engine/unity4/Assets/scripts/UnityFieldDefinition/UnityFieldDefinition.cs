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

		if (!mesh.GUID.Equals(GUID))
			return false;
		
		List<FieldNode> remainingNodes = new List<FieldNode>(NodeGroup.EnumerateAllLeafFieldNodes());

		List<KeyValuePair<BXDAMesh.BXDASubMesh, Mesh>> submeshes = new List<KeyValuePair<BXDAMesh.BXDASubMesh, Mesh>>();
		List<KeyValuePair<BXDAMesh.BXDASubMesh, Mesh>> colliders = new List<KeyValuePair<BXDAMesh.BXDASubMesh, Mesh>>();

		// Create all submesh objects
		auxFunctions.ReadMeshSet(mesh.meshes, delegate(int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
		{
			submeshes.Add(new KeyValuePair<BXDAMesh.BXDASubMesh, Mesh>(sub, meshu));
		});

		// Create all collider objects
		auxFunctions.ReadMeshSet(mesh.colliders, delegate(int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
		{
			colliders.Add(new KeyValuePair<BXDAMesh.BXDASubMesh, Mesh>(sub, meshu));
		});

		foreach (FieldNode node in NodeGroup.EnumerateAllLeafFieldNodes())
		{
			GameObject subObject = new GameObject(node.NodeID);
			//subObject.transform.parent = unityObject.transform;

			if (node.SubMeshID != -1)
			{
				KeyValuePair<BXDAMesh.BXDASubMesh, Mesh> currentSubMesh = submeshes[node.SubMeshID];

				BXDAMesh.BXDASubMesh sub = currentSubMesh.Key;
				Mesh meshu = currentSubMesh.Value;

				subObject.AddComponent<MeshFilter>().mesh = meshu;
				subObject.AddComponent<MeshRenderer>();
				Material[] matls = new Material[meshu.subMeshCount];

				for (int i = 0; i < matls.Length; i++)
				{
					matls[i] = sub.surfaces[i].AsMaterial();
				}

				subObject.GetComponent<MeshRenderer>().materials = matls;
			}

			if (GetPropertySets().ContainsKey(node.PropertySetID))
			{
				PropertySet currentPropertySet = GetPropertySets()[node.PropertySetID];
				PropertySet.PropertySetCollider psCollider = currentPropertySet.Collider;
				Collider unityCollider = null;

				Debug.Log(psCollider == null);

				switch (psCollider.CollisionType)
				{
				case PropertySet.PropertySetCollider.PropertySetCollisionType.BOX:
					PropertySet.BoxCollider psBoxCollider = (PropertySet.BoxCollider)psCollider;
					BoxCollider unityBoxCollider = subObject.AddComponent<BoxCollider>();

					//unityBoxCollider.size.Scale(new Vector3(psBoxCollider.Scale.x, psBoxCollider.Scale.y, psBoxCollider.Scale.z));
					unityBoxCollider.size = new Vector3(
						unityBoxCollider.size.x * psBoxCollider.Scale.x,
						unityBoxCollider.size.y * psBoxCollider.Scale.y,
						unityBoxCollider.size.z * psBoxCollider.Scale.z);

					unityCollider = unityBoxCollider;
					break;
				case PropertySet.PropertySetCollider.PropertySetCollisionType.SPHERE:
					PropertySet.SphereCollider psSphereCollider = (PropertySet.SphereCollider)psCollider;
					SphereCollider unitySphereCollider = subObject.AddComponent<SphereCollider>();

					unitySphereCollider.radius *= psSphereCollider.Scale;

					unityCollider = unitySphereCollider;
					break;
				case PropertySet.PropertySetCollider.PropertySetCollisionType.MESH:
					if (node.CollisionMeshID != -1)
					{
                        PropertySet.MeshCollider psMeshCollider = (PropertySet.MeshCollider)psCollider;
                        KeyValuePair<BXDAMesh.BXDASubMesh, Mesh> currentSubMesh = colliders[node.CollisionMeshID];

                        BXDAMesh.BXDASubMesh sub = currentSubMesh.Key;
                        Mesh meshu = currentSubMesh.Value;

                        MeshCollider unityMeshCollider = subObject.AddComponent<MeshCollider>();
                        unityMeshCollider.sharedMesh = meshu;
                        unityMeshCollider.convex = psMeshCollider.Convex;

                        unityCollider = unityMeshCollider;
                        }
					break;
				}

				if (unityCollider != null)
				{
					unityCollider.material.dynamicFriction = unityCollider.material.staticFriction = currentPropertySet.Friction / 100f;
					unityCollider.material.frictionCombine = PhysicMaterialCombine.Minimum;

					Rigidbody rb = unityCollider.gameObject.AddComponent<Rigidbody>();

					if (currentPropertySet.Mass > 0)
					{
						rb.mass = (float)currentPropertySet.Mass * Init.PHYSICS_MASS_MULTIPLIER;
					}
					else
					{
						rb.constraints = RigidbodyConstraints.FreezeAll;
						rb.isKinematic = true;
					}
				}
			}

			// Invert the x-axis to compensate for Unity's inverted coordinate system.
			subObject.transform.localScale = new Vector3(-1f, 1f, 1f);

			// Set the position of the object (scaled by 1/100 to match Unity's scaling correctly).
			subObject.transform.position = new Vector3(-node.Position.x * 0.01f, node.Position.y * 0.01f, node.Position.z * 0.01f);

			// Set the rotation of the object (the x and w properties are inverted to once again compensate for Unity's differences).
			subObject.transform.rotation = new Quaternion(-node.Rotation.X, node.Rotation.Y, node.Rotation.Z, -node.Rotation.W);
		}

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
}
