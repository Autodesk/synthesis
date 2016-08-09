﻿using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BCylinderShapeX : BCylinderShape
{
    public new Vector3 LocalScaling
    {
        get { return m_localScaling; }
        set
        {
            m_localScaling = value;
            if (collisionShapePtr != null)
            {
                ((CylinderShapeX)collisionShapePtr).LocalScaling = value.ToBullet();
            }
        }
    }

    public override void OnDrawGizmosSelected()
    {
        UnityEngine.Vector3 position = transform.position;
        UnityEngine.Quaternion rotation = transform.rotation;
        UnityEngine.Vector3 scale = m_localScaling;
        BUtility.DebugDrawCylinder(position, rotation, scale, halfExtent.x, halfExtent.y, 2, Color.yellow);
    }

    public override CollisionShape GetCollisionShape()
    {
        if (collisionShapePtr == null)
        {
            collisionShapePtr = new CylinderShapeX(halfExtent.ToBullet());
            ((CylinderShapeX)collisionShapePtr).LocalScaling = m_localScaling.ToBullet();
        }
        return collisionShapePtr;
    }
}