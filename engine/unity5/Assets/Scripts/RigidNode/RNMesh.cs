using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
public partial class RigidNode : RigidNode_Base
{
    public bool CreateMesh(string filePath)
    {
        BXDAMesh mesh = new BXDAMesh();
        mesh.ReadFromFile(filePath);

        if (!mesh.GUID.Equals(GUID))
            return false;

        AuxFunctions.ReadMeshSet(mesh.meshes, delegate (int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
        {
            GameObject meshObject = new GameObject(gameObject.name + "_mesh" + id);
            meshObject.transform.parent = gameObject.transform;
            meshObject.transform.localPosition = Vector3.zero;
            meshObject.AddComponent<MeshFilter>().mesh = meshu;
            meshObject.AddComponent<MeshRenderer>();

            Material[] materials = new Material[meshu.subMeshCount];
            for (int i = 0; i < materials.Length; i++)
                materials[i] = sub.surfaces[i].AsMaterial();

            meshObject.GetComponent<MeshRenderer>().materials = materials;
            BulletUnity.Primitives.BConvexHull convexHull = gameObject.AddComponent<BulletUnity.Primitives.BConvexHull>();
            convexHull.meshSettings.UserMesh = meshu;//AuxFunctions.GenerateCollisionMesh(meshu);
            convexHull.BuildMesh(); // Doesn't work... oh wait there's two rigid bodies.
        });

        AuxFunctions.ReadMeshSet(mesh.colliders, delegate (int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
        {
            //BulletUnity.Primitives.BConvexHull convexHull = gameObject.AddComponent<BulletUnity.Primitives.BConvexHull>();
            //convexHull.meshSettings.UserMesh = AuxFunctions.GenerateCollisionMesh(meshu);
            //convexHull.BuildMesh(); // Doesn't work... oh wait there's two rigid bodies.
            //BConvexHullShape convexHull = gameObject.AddComponent<BConvexHullShape>();
            //convexHull.HullMesh = meshu;
            //convexHull.GetCollisionShape().Margin = 0f;// Main.COLLISION_MARGIN;
            //gameObject.GetComponentInChildren<MeshFilter>().mesh = meshu; // For testing.
        });

        physicalProperties = mesh.physics;

        BRigidBody rigidBody = gameObject.GetComponent<BRigidBody>();//gameObject.AddComponent<BRigidBody>();
        rigidBody.mass = 1f;
        //rigidBody.mass = mesh.physics.mass;

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
