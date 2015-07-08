using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class UnityRigidNode : RigidNode_Base
{
    /// <summary>
    /// Loads the mesh from the given path into this node's object.
    /// </summary>
    /// <param name="filePath">The file to open as a BXDA mesh</param>
    public void CreateMesh(string filePath)
    {
        BXDAMesh mesh = new BXDAMesh();
        mesh.ReadFromFile(filePath);

        // Create all submesh objects
        auxFunctions.ReadMeshSet(mesh.meshes, delegate(int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
        {
            GameObject subObject = new GameObject(unityObject.name + " Subpart" + id);
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
        });

        if (!unityObject.GetComponent<Rigidbody>())
            unityObject.AddComponent<Rigidbody>();

        // Read colliders
        auxFunctions.ReadMeshSet(mesh.colliders, delegate(int id, BXDAMesh.BXDASubMesh useless, Mesh meshu)
        {
            GameObject subCollider = new GameObject(unityObject.name + " Subcollider" + id);
            subCollider.transform.parent = unityObject.transform;
            subCollider.transform.position = new Vector3(0, 0, 0);
            // Special case where it is a box
            if (meshu.triangles.Length == 0 && meshu.vertices.Length == 2)
            {
                BoxCollider box = subCollider.AddComponent<BoxCollider>();
                Vector3 center = (meshu.vertices[0] + meshu.vertices[1]) * 0.5f;
                box.center = center;
                box.size = meshu.vertices[1] - center;
            }
            else
            {
                subCollider.AddComponent<MeshCollider>().sharedMesh = meshu;
                subCollider.GetComponent<MeshCollider>().convex = true;
            }

        });

        Rigidbody rigidB = unityObject.GetComponent<Rigidbody>();
        rigidB.mass = mesh.physics.mass * Init.PHYSICS_MASS_MULTIPLIER; // Unity has magic mass units
        rigidB.centerOfMass = mesh.physics.centerOfMass.AsV3();

        bxdPhysics = mesh.physics;

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