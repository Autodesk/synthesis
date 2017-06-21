using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BMultiHullShape : BCollisionShape
{
    [SerializeField]
    protected List<ConvexHullShape> hullShapes = new List<ConvexHullShape>();

    [SerializeField]
    protected CompoundShape compoundShape = new CompoundShape();

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
                ((CompoundShape)collisionShapePtr).LocalScaling = value.ToBullet();
            }
        }
    }

    public override void OnDrawGizmosSelected()
    {
        // Unimplemented.
    }

    public override CollisionShape GetCollisionShape()
    {
        return compoundShape;
    }

    public void AddHullShape(ConvexHullShape hullShape, BulletSharp.Math.Matrix offset)
    {
        if (hullShapes.Contains(hullShape))
            return;

        hullShapes.Add(hullShape);
        compoundShape.AddChildShape(offset, hullShape);
    }

    public void RemoveHullShape(ConvexHullShape hullShape)
    {
        if (!hullShapes.Contains(hullShape))
            return;

        hullShapes.Remove(hullShape);
        compoundShape.RemoveChildShape(hullShape);
    }

    //public override CollisionShape CopyCollisionShape()
    //{
    //    return null;
    //}
}
