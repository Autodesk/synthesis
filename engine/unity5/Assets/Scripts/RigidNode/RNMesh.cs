using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;
using Assets.Scripts.FSM;
using Assets.Scripts.BUExtensions;

public partial class RigidNode : RigidNode_Base
{
    private const float LinearSleepingThreshold = 0.25f;
    private const float AngularSleepingThreshold = 0.5f;
    private const float CollisionMargin = 0f;

    public bool CreateMesh(string filePath, bool isMixAndMatch = false, float wheelMass = 1.0f)
    {
        //Debug.Log(filePath);
        BXDAMesh mesh = new BXDAMesh();
        mesh.ReadFromFile(filePath);

        //if (!mesh.GUID.Equals(GUID))
        //{
        //    Debug.Log("Returning false");
        //    return false;
        //}


        List<GameObject> meshObjects = new List<GameObject>();

        AuxFunctions.ReadMeshSet(mesh.meshes, delegate (int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
        {
            GameObject meshObject = new GameObject(MainObject.name + "_mesh");
            meshObjects.Add(meshObject);

            meshObject.AddComponent<MeshFilter>().mesh = meshu;
            meshObject.AddComponent<MeshRenderer>();

            Material[] materials = new Material[meshu.subMeshCount];
            for (int i = 0; i < materials.Length; i++)
                materials[i] = sub.surfaces[i].AsMaterial(true);

            meshObject.GetComponent<MeshRenderer>().materials = materials;

            meshObject.transform.position = root.position;
            meshObject.transform.rotation = root.rotation;

        }, true);

        Vector3 com = mesh.physics.centerOfMass.AsV3();
        com.x *= -1;
        ComOffset = com;

        Mesh[] colliders = new Mesh[mesh.colliders.Count];

        AuxFunctions.ReadMeshSet(mesh.colliders, delegate (int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
        {
            colliders[id] = meshu;
        }, true);

        MainObject.transform.position = root.position + ComOffset;
        MainObject.transform.rotation = root.rotation;

        foreach (GameObject meshObject in meshObjects)
            meshObject.transform.parent = MainObject.transform;

        if (!this.HasDriverMeta<WheelDriverMeta>() || this.GetDriverMeta<WheelDriverMeta>().type == WheelType.NOT_A_WHEEL)
        {
            BMultiHullShape hullShape = MainObject.AddComponent<BMultiHullShape>();

            foreach (Mesh collider in colliders)
            {
                //Mesh m = AuxFunctions.GenerateCollisionMesh(collider, Vector3.zero, 0f/*CollisionMargin*/);
                //ConvexHullShape hull = new ConvexHullShape(Array.ConvertAll(m.vertices, x => x.ToBullet()), m.vertices.Length);
                //hull.Margin = CollisionMargin;
                //hullShape.AddHullShape(hull, BulletSharp.Math.Matrix.Translation(-ComOffset.ToBullet()));

                ConvexHullShape hull = new ConvexHullShape(Array.ConvertAll(collider.vertices, x => x.ToBullet()), collider.vertices.Length);
                hull.Margin = CollisionMargin;
                hullShape.AddHullShape(hull, BulletSharp.Math.Matrix.Translation(-ComOffset.ToBullet()));
            }

            MainObject.AddComponent<MeshRenderer>();

            PhysicalProperties = mesh.physics;
            //Debug.Log(PhysicalProperties.centerOfMass);

            BRigidBody rigidBody = MainObject.AddComponent<BRigidBody>();
            rigidBody.mass = mesh.physics.mass;
            rigidBody.friction = 0.25f;
            rigidBody.linearSleepingThreshold = LinearSleepingThreshold;
            rigidBody.angularSleepingThreshold = AngularSleepingThreshold;
            rigidBody.RemoveOnCollisionCallbackEventHandler();

            foreach (BRigidBody rb in MainObject.transform.parent.GetComponentsInChildren<BRigidBody>())
                rigidBody.GetCollisionObject().SetIgnoreCollisionCheck(rb.GetCollisionObject(), true);

            MainObject.AddComponent<BMultiCallbacks>().AddCallback((StateMachine.Instance.CurrentState as MainState).CollisionTracker);
        }

        if (this.HasDriverMeta<WheelDriverMeta>() && this.GetDriverMeta<WheelDriverMeta>().type != WheelType.NOT_A_WHEEL && GetParent() == null)
        {
            BRigidBody rigidBody = MainObject.GetComponent<BRigidBody>();
            if (isMixAndMatch)
            {
                rigidBody.mass += wheelMass;
            }
            rigidBody.GetCollisionObject().CollisionShape.CalculateLocalInertia(rigidBody.mass);
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
