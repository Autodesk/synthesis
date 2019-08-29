using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Synthesis.BUExtensions
{
    public class BMultiShape : BCollisionShape
    {
        [SerializeField]
        protected List<CollisionShape> hullShapes = new List<CollisionShape>();

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
        /// Adds a <see cref="CollisionShape"/> to the multi hull shape.
        /// </summary>
        /// <param name="hullShape"></param>
        /// <param name="offset"></param>
        public void AddShape(CollisionShape hullShape, BulletSharp.Math.Matrix offset)
        {
            if (hullShapes.Contains(hullShape))
                return;

            hullShapes.Add(hullShape);
            compoundShape.AddChildShape(offset, hullShape);
        }

        /// <summary>
        /// Removes a <see cref="CollisionShape"/> from the multi hull shape.
        /// </summary>
        /// <param name="hullShape"></param>
        public void RemoveShape(CollisionShape hullShape)
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
}
