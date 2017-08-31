using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.BUExtensions
{
    public class BBallSocketConstraintEx : BBallSocketConstraint
    {
        private BulletSharp.Math.Vector3 _pivotInA;

        /// <summary>
        /// The local pivot point relative to body A.
        /// </summary>
        public BulletSharp.Math.Vector3 PivotInA
        {
            get
            {
                Point2PointConstraint p2p = m_constraintPtr as Point2PointConstraint;

                if (p2p == null)
                    return BulletSharp.Math.Vector3.Zero;

                return _pivotInA;
            }
            set
            {
                Point2PointConstraint p2p = m_constraintPtr as Point2PointConstraint;

                if (p2p != null)
                    p2p.PivotInA = value;

                _pivotInA = value;
            }
        }

        private BulletSharp.Math.Vector3 _pivotInB;

        /// <summary>
        /// The local pivot point relative to body B.
        /// </summary>
        public BulletSharp.Math.Vector3 PivotInB
        {
            get
            {
                Point2PointConstraint p2p = m_constraintPtr as Point2PointConstraint;

                if (p2p == null)
                    return BulletSharp.Math.Vector3.Zero;

                return _pivotInB;
            }
            set
            {
                Point2PointConstraint p2p = m_constraintPtr as Point2PointConstraint;

                if (p2p != null)
                    p2p.PivotInB = value;

                _pivotInB = value;
            }
        }

        /// <summary>
        /// Builds the constraint.
        /// </summary>
        /// <returns></returns>
        internal override bool _BuildConstraint()
        {
            BPhysicsWorld world = BPhysicsWorld.Get();
            if (m_constraintPtr != null)
            {
                if (m_isInWorld && world != null)
                {
                    m_isInWorld = false;
                    world.RemoveConstraint(m_constraintPtr);
                }
            }
            BRigidBody targetRigidBodyA = GetComponent<BRigidBody>();
            if (targetRigidBodyA == null)
            {
                Debug.LogError("BallSocketConstraint needs to be added to a component with a BRigidBody.");
                return false;
            }
            if (!targetRigidBodyA.isInWorld)
            {
                world.AddRigidBody(targetRigidBodyA);
            }
            RigidBody rba = (RigidBody)targetRigidBodyA.GetCollisionObject();
            if (rba == null)
            {
                Debug.LogError("Constraint could not get bullet RigidBody from target rigid body A");
                return false;
            }
            if (m_constraintType == ConstraintType.constrainToAnotherBody)
            {
                if (m_otherRigidBody == null)
                {
                    Debug.LogError("Other rigid body was not set");
                    return false;
                }
                if (!m_otherRigidBody.isInWorld)
                {
                    world.AddRigidBody(m_otherRigidBody);
                }
                RigidBody rbb = (RigidBody)m_otherRigidBody.GetCollisionObject();
                if (rbb == null)
                {
                    Debug.LogError("Constraint could not get bullet RigidBody from target rigid body B");
                    return false;
                }
                Vector3 pivotInOther = m_otherRigidBody.transform.InverseTransformPoint(targetRigidBodyA.transform.TransformPoint(m_localConstraintPoint));
                m_constraintPtr = new Point2PointConstraint(rbb, rba, pivotInOther.ToBullet(), m_localConstraintPoint.ToBullet());
            }
            else
            {
                m_constraintPtr = new Point2PointConstraint(rba, _pivotInA);
            }
            m_constraintPtr.Userobject = this;
            m_constraintPtr.BreakingImpulseThreshold = m_breakingImpulseThreshold;
            m_constraintPtr.DebugDrawSize = m_debugDrawSize;
            m_constraintPtr.OverrideNumSolverIterations = m_overrideNumSolverIterations;
            return true;
        }
    }
}
