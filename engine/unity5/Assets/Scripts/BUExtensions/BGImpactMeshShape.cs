﻿using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BGImpactMeshShape : BCollisionShape
{
    [SerializeField]
    protected Mesh hullMesh;
    public Mesh HullMesh
    {
        get { return hullMesh; }
        set
        {
            if (collisionShapePtr != null && value != hullMesh)
            {
                Debug.LogError("Cannot change the Hull Mesh after the bullet shape has been created. This is only the initial value " +
                                "Use LocalScaling to change the shape of a bullet shape.");
            }
            else
            {
                hullMesh = value;
            }
        }
    }

    [SerializeField]
    protected Vector3 m_localScaling = Vector3.one;
    public Vector3 LocalScaling
    {
        get { return m_localScaling; }
        set
        {
            m_localScaling = value;
            if (collisionShapePtr != null)
            {
                ((GImpactMeshShape)collisionShapePtr).LocalScaling = value.ToBullet();
            }
        }
    }

    //todo draw the hull when not in the world
    //todo can this be used with Dynamic objects? The manual hints that it is for static only.

    public override void OnDrawGizmosSelected()
    {
        //BUtility.DebugDrawCapsule(position, rotation, scale, radius, height / 2f, 1, Gizmos.color);  
    }

    public override CollisionShape GetCollisionShape()
    {
        if (collisionShapePtr == null)
        {
            Vector3[] verts = hullMesh.vertices;
            int[] tris = hullMesh.triangles;
            //todo test for convex. Make convex if not.
            TriangleMesh tm = new TriangleMesh();
            for (int i = 0; i < tris.Length; i += 3)
            {
                tm.AddTriangle(verts[tris[i]].ToBullet(),
                               verts[tris[i + 1]].ToBullet(),
                               verts[tris[i + 2]].ToBullet(),
                               true);
            }
            collisionShapePtr = new GImpactMeshShape(tm);
            ((GImpactMeshShape)collisionShapePtr).LocalScaling = m_localScaling.ToBullet();
        }
        return collisionShapePtr;
    }

    public override CollisionShape CopyCollisionShape()
    {
        return null;
    }
}