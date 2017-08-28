using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletUnity;
using BulletSharp;
using Assets.Scripts.FEA;

public class UnityFieldDefinition : FieldDefinition
{
    public GameObject unityObject;
    private const float CollisionMargin = 0.01f;
    private const float FrictionScale = 0.02f;
    private const float RollingFrictionScale = 0.0025f;

    public UnityFieldDefinition(Guid guid, string name)
        : base(guid, name)
    {
    }

    public void CreateTransform(Transform root)
    {
        unityObject = root.gameObject;
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
        AuxFunctions.ReadMeshSet(mesh.meshes, delegate (int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
        {
            submeshes.Add(new KeyValuePair<BXDAMesh.BXDASubMesh, Mesh>(sub, meshu));
        });

        // Create all collider objects
        AuxFunctions.ReadMeshSet(mesh.colliders, delegate (int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
        {
            colliders.Add(new KeyValuePair<BXDAMesh.BXDASubMesh, Mesh>(sub, meshu));
        });

        foreach (FieldNode node in NodeGroup.EnumerateAllLeafFieldNodes())
        {
            GameObject subObject = new GameObject(node.NodeID);
            subObject.transform.parent = unityObject.transform;

            GameObject meshObject = new GameObject(node.NodeID + "-mesh");

            if (node.SubMeshID != -1)
            {
                KeyValuePair<BXDAMesh.BXDASubMesh, Mesh> currentSubMesh = submeshes[node.SubMeshID];

                BXDAMesh.BXDASubMesh sub = currentSubMesh.Key;
                Mesh meshu = currentSubMesh.Value;

                meshObject.AddComponent<MeshFilter>().mesh = meshu;
                meshObject.AddComponent<MeshRenderer>();
                Material[] matls = new Material[meshu.subMeshCount];

                for (int i = 0; i < matls.Length; i++)
                {
                    matls[i] = sub.surfaces[i].AsMaterial();
                }

                meshObject.GetComponent<MeshRenderer>().materials = matls;
            }

            // Invert the x-axis to compensate for Unity's inverted coordinate system.
            meshObject.transform.localScale = new Vector3(-1f, 1f, 1f);

            // Set the rotation of the object (the x and w properties are inverted to once again compensate for Unity's differences).
            meshObject.transform.localRotation = new Quaternion(-node.Rotation.X, node.Rotation.Y, node.Rotation.Z, -node.Rotation.W);

            // Set the position of the object (scaled by 1/100 to match Unity's scaling correctly).
            meshObject.transform.position = new Vector3(-node.Position.x * 0.01f, node.Position.y * 0.01f, node.Position.z * 0.01f);

            if (GetPropertySets().ContainsKey(node.PropertySetID))
            {
                PropertySet currentPropertySet = GetPropertySets()[node.PropertySetID];
                PropertySet.PropertySetCollider psCollider = currentPropertySet.Collider;

                switch (psCollider.CollisionType)
                {
                    case PropertySet.PropertySetCollider.PropertySetCollisionType.BOX:
                        PropertySet.BoxCollider psBoxCollider = (PropertySet.BoxCollider)psCollider;
                        BoxCollider dummyBoxCollider = meshObject.AddComponent<BoxCollider>();

                        subObject.transform.localRotation = meshObject.transform.localRotation;
                        subObject.transform.position = meshObject.transform.TransformPoint(dummyBoxCollider.center);

                        BBoxShape boxShape = subObject.AddComponent<BBoxShape>();
                        boxShape.Extents = new Vector3(
                            dummyBoxCollider.size.x * 0.5f * psBoxCollider.Scale.x,
                            dummyBoxCollider.size.y * 0.5f * psBoxCollider.Scale.y,
                            dummyBoxCollider.size.z * 0.5f * psBoxCollider.Scale.z);

                        //meshObject.AddComponent<MouseListener>();
                        UnityEngine.Object.Destroy(dummyBoxCollider);

                        break;
                    case PropertySet.PropertySetCollider.PropertySetCollisionType.SPHERE:
                        PropertySet.SphereCollider psSphereCollider = (PropertySet.SphereCollider)psCollider;
                        SphereCollider dummySphereCollider = meshObject.AddComponent<SphereCollider>();

                        subObject.transform.position = meshObject.transform.TransformPoint(dummySphereCollider.center);

                        BSphereShape sphereShape = subObject.AddComponent<BSphereShape>();
                        sphereShape.Radius = dummySphereCollider.radius * psSphereCollider.Scale;

                        //meshObject.AddComponent<MouseListener>();
                        UnityEngine.Object.Destroy(dummySphereCollider);

                        break;
                    case PropertySet.PropertySetCollider.PropertySetCollisionType.MESH:
                        PropertySet.MeshCollider psMeshCollider = (PropertySet.MeshCollider)psCollider;

                        if (psMeshCollider.Convex || currentPropertySet.Mass != 0)
                        {
                            MeshCollider dummyMeshCollider = subObject.AddComponent<MeshCollider>();
                            dummyMeshCollider.sharedMesh = meshObject.GetComponent<MeshFilter>().mesh;

                            subObject.transform.position = meshObject.transform.TransformPoint(dummyMeshCollider.bounds.center);
                            subObject.transform.rotation = meshObject.transform.rotation;

                            BConvexHullShape hullshape = subObject.AddComponent<BConvexHullShape>();
                            hullshape.HullMesh = AuxFunctions.GenerateCollisionMesh(meshObject.GetComponent<MeshFilter>().mesh, dummyMeshCollider.sharedMesh.bounds.center, 0f/*CollisionMargin*/);
                            hullshape.GetCollisionShape().Margin = CollisionMargin;

                            //subObject.AddComponent<MouseListener>();
                            UnityEngine.Object.Destroy(dummyMeshCollider);
                        }
                        else
                        {
                            subObject.transform.position = meshObject.transform.position;
                            subObject.transform.rotation = meshObject.transform.rotation;

                            BBvhTriangleMeshShape meshShape = subObject.AddComponent<BBvhTriangleMeshShape>();
                            meshShape.HullMesh = meshObject.GetComponent<MeshFilter>().mesh.GetScaledCopy(-1f, 1f, 1f);
                            meshShape.GetCollisionShape().Margin = CollisionMargin;
                        }
                        break;
                }

                BRigidBody rb = subObject.AddComponent<BRigidBody>();
                rb.friction = currentPropertySet.Friction * FrictionScale;
                rb.rollingFriction = currentPropertySet.Friction * RollingFrictionScale;
                rb.mass = currentPropertySet.Mass;

                if (currentPropertySet.Mass == 0)
                    rb.collisionFlags = BulletSharp.CollisionFlags.StaticObject;
                else
                    subObject.AddComponent<Tracker>();

                meshObject.transform.parent = subObject.transform;
            }
            else
            {
                meshObject.transform.parent = unityObject.transform;
            }
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