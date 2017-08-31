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

    /// <summary>
    /// The local scaling of the BMultiHullShape.
    /// </summary>
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

    /// <summary>
    /// Returns the BulletSharp <see cref="CollisionShape"/>.
    /// </summary>
    /// <returns></returns>
    public override CollisionShape GetCollisionShape()
    {
        return compoundShape;
    }

    /// <summary>
    /// Adds a <see cref="ConvexHullShape"/> to the multi hull shape.
    /// </summary>
    /// <param name="hullShape"></param>
    /// <param name="offset"></param>
    public void AddHullShape(ConvexHullShape hullShape, BulletSharp.Math.Matrix offset)
    {
        if (hullShapes.Contains(hullShape))
            return;

        hullShapes.Add(hullShape);
        compoundShape.AddChildShape(offset, hullShape);
    }

    /// <summary>
    /// Removes a <see cref="ConvexHullShape"/> from the multi hull shape.
    /// </summary>
    /// <param name="hullShape"></param>
    public void RemoveHullShape(ConvexHullShape hullShape)
    {
        if (!hullShapes.Contains(hullShape))
            return;

        hullShapes.Remove(hullShape);
        compoundShape.RemoveChildShape(hullShape);
    }

    /// <summary>
    /// Not implemented - do not use.
    /// </summary>
    /// <returns></returns>
    public override CollisionShape CopyCollisionShape()
    {
        return null;
    }
}
