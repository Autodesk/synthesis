using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityFieldDefinition : FieldDefinition_Base
{
	public GameObject unityObject;
	
	public void CreateTransform(Transform root)
	{
		unityObject = new GameObject();
		unityObject.transform.parent = root;
		unityObject.transform.position = new Vector3(0, 0, 0);
		unityObject.name = base.definitionID;
	}

	public void CreateMesh(string filePath)
	{
		BXDAMesh mesh = new BXDAMesh();
		mesh.ReadFromFile(filePath, null);
		
		// Create all submesh objects
		auxFunctions.ReadMeshSet(mesh.meshes, delegate(int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
		{
			GameObject subObject = new GameObject(GetChildren()[id].nodeID);
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

			switch (GetChildren()[id].nodeCollisionType)
			{
			case FieldNodeCollisionType.MESH:
				collider = subObject.AddComponent<MeshCollider>();
				break;
			case FieldNodeCollisionType.BOX:
				collider = subObject.AddComponent<BoxCollider>();
				break;
			}

			if (collider != null)
			{
				if (collider is MeshCollider)
				{
					MeshCollider meshCollider = (MeshCollider)collider;
					meshCollider.convex = GetChildren()[id].convex;
				}
				collider.material.dynamicFriction = collider.material.staticFriction = (float)GetChildren()[id].friction / 10f;
				collider.material.frictionCombine = PhysicMaterialCombine.Minimum;
			}
		});
		
		if (!unityObject.GetComponent<Rigidbody>())
			unityObject.AddComponent<Rigidbody>();
		
		Rigidbody rigidB = unityObject.GetComponent<Rigidbody>();
		rigidB.mass = mesh.physics.mass * Init.PHYSICS_MASS_MULTIPLIER; // Unity has magic mass units
		rigidB.centerOfMass = mesh.physics.centerOfMass.AsV3();
		rigidB.isKinematic = true;
		
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
	}
}
