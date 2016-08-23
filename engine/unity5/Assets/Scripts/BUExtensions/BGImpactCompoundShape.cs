using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using UnityEngine;

public class BGImpactCompoundShape : BCollisionShape
{
    [SerializeField]
    protected GImpactCompoundShape collisionShape = new GImpactCompoundShape();

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
                ((GImpactCompoundShape)collisionShapePtr).LocalScaling = value.ToBullet();
            }
        }
    }

    public override CollisionShape GetCollisionShape()
    {
        return collisionShape;
    }

    public override void OnDrawGizmosSelected()
    {
        // Unimplemented.
    }

    public void AddChildShape(CollisionShape shape)
    {
        collisionShape.AddChildShape(shape);
    }

    public void AddChildShape(CollisionShape shape, BulletSharp.Math.Matrix offset)
    {
        collisionShape.AddChildShape(offset, shape);
    }
}