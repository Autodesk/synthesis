using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletUnity;
using BulletSharp;

public class UnityFieldDefinition : FieldDefinition
{
    public GameObject unityObject;
    //private const float COLLISION_MARGIN = 0.1f;
    private const float FRICTION_SCALE = 0.02f;

    public UnityFieldDefinition(Guid guid, string name)
        : base(guid, name)
    {
    }

    public void CreateTransform(Transform root)
    {
        unityObject = new GameObject();
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

                BRigidBody rb = subObject.AddComponent<BRigidBody>();
                rb.friction = currentPropertySet.Friction * FRICTION_SCALE;
                rb.mass = currentPropertySet.Mass;

                //((RigidBody)rb.GetCollisionObject()).MotionState =
                //    new DefaultMotionState(rb.GetCollisionObject().WorldTransform,
                //    new BulletSharp.Math.Matrix { Origin = new BulletSharp.Math.Vector3() });

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

                        UnityEngine.Object.Destroy(dummyBoxCollider);

                        break;
                    case PropertySet.PropertySetCollider.PropertySetCollisionType.SPHERE:
                        PropertySet.SphereCollider psSphereCollider = (PropertySet.SphereCollider)psCollider;
                        SphereCollider dummySphereCollider = meshObject.AddComponent<SphereCollider>();

                        subObject.transform.position = meshObject.transform.position;

                        BSphereShape sphereShape = subObject.AddComponent<BSphereShape>();
                        sphereShape.Radius = dummySphereCollider.radius * psSphereCollider.Scale;

                        UnityEngine.Object.Destroy(dummySphereCollider);

                        break;
                    case PropertySet.PropertySetCollider.PropertySetCollisionType.MESH:
                        if (node.CollisionMeshID != -1)
                        {
                            subObject.transform.position = meshObject.transform.position;
                            subObject.transform.rotation = meshObject.transform.rotation;

                            BulletUnity.Primitives.BConvexHull convexHull = subObject.AddComponent<BulletUnity.Primitives.BConvexHull>();
                            convexHull.meshSettings.UserMesh = AuxFunctions.GenerateCollisionMesh(meshObject.GetComponent<MeshFilter>().mesh);
                            convexHull.BuildMesh();
                            
                            // TODO: Add some sort of variant of this back?
                            //RigidBody b = (RigidBody)rb.GetCollisionObject();
                            //BulletSharp.Math.Matrix transform = b.MotionState.WorldTransform;//b.CenterOfMassTransform;
                            //transform.Origin += new BulletSharp.Math.Vector3(0f, -1f, 0f);
                            //b.MotionState.WorldTransform = transform;

                            subObject.GetComponent<MeshRenderer>().enabled = false;

                            // TODO: This statement seems to break things. If you can get to work, you could modify the center of mass.
                            ((RigidBody)rb.GetCollisionObject()).MotionState =
                                new DefaultMotionState(((RigidBody)rb.GetCollisionObject()).MotionState.WorldTransform);

                            //BConvexHullShape hullShape = subObject.AddComponent<BConvexHullShape>();
                            //hullShape.HullMesh = AuxFunctions.GenerateCollisionMesh(meshObject.GetComponent<MeshFilter>().mesh);
                            //hullShape.GetCollisionShape().Margin = Main.COLLISION_MARGIN;

                            // TODO: Find a way to move the center of gravity. This is important. See examples (or BulletUnity.Primitives?)

                            //BulletSharp.Math.Matrix transform = rb.GetCollisionObject().WorldTransform;
                            //transform.Origin += new BulletSharp.Math.Vector3(0f, 1f, 0f);
                            //rb.GetCollisionObject().WorldTransform = transform;

                            //BCompoundShape compoundShape = subObject.AddComponent<BCompoundShape>();
                            //BulletSharp.Math.Matrix otherTransform = new BulletSharp.Math.Matrix();
                            //otherTransform.Origin += new BulletSharp.Math.Vector3(0f, -1f, 0f);
                            //((CompoundShape)compoundShape.GetCollisionShape()).UpdateChildTransform(0, otherTransform); // u is here

                            //UnityEngine.Object.Destroy(subObject.GetComponent<BBoxShape>()); // TODO: Continue center of mass stuff. I think you're getting close.

                            // TODO: Find a way to implement embedded margins. See https://www.bulletphysics.org/Bullet/phpBB3/viewtopic.php?f=9&t=2358

                            //meshObject.GetComponent<MeshFilter>().mesh = hullShape.HullMesh; // Use for visualizing the collision mesh.
                        }
                        break;
                }

                // TODO: Adjust center of mass.

                if (currentPropertySet.Mass == 0)
                    rb.collisionFlags = BulletSharp.CollisionFlags.StaticObject;

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