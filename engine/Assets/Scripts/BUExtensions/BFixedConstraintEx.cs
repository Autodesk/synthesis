using BulletSharp;
using BulletUnity;
using UnityEngine;
using BM = BulletSharp.Math;

namespace Synthesis.BUExtensions
{
    public class BFixedConstraintEx : BTypedConstraint
    {
        private BM.Matrix initialTransform;

        public Quaternion localRotationOffset { get; set; }

        public new Vector3 localConstraintPoint
        {
            get
            {
                return m_localConstraintPoint;
            }
            set
            {
                m_localConstraintPoint = value;

                if (m_constraintPtr == null)
                    return;

                FixedConstraint constraint = m_constraintPtr as FixedConstraint;

                BM.Matrix updatedFrame = initialTransform;
                updatedFrame.Origin = value.ToBullet();

                constraint.SetFrames(BM.Matrix.Identity, updatedFrame);
            }
        }

        internal override bool _BuildConstraint()
        {
            BPhysicsWorld world = BPhysicsWorld.Get();

            if (m_constraintPtr != null && m_isInWorld && world != null)
            {
                m_isInWorld = false;
                world.RemoveConstraint(m_constraintPtr);
            }
            
            BRigidBody targetRigidBodyA = GetComponent<BRigidBody>();

            if (targetRigidBodyA == null)
            {
                Debug.LogError("Fixed Constraint needs to be added to a component with a BRigidBody.");
                return false;
            }

            if (!targetRigidBodyA.isInWorld)
                world.AddRigidBody(targetRigidBodyA);

            RigidBody rba = (RigidBody)targetRigidBodyA.GetCollisionObject();

            if (rba == null)
            {
                Debug.LogError("Constraint could not get bullet RigidBody from target rigid body");
                return false;
            }

            if (m_otherRigidBody == null)
            {
                Debug.LogError("Other rigid body is not set");
                return false;
            }

            if (!m_otherRigidBody.isInWorld)
                world.AddRigidBody(m_otherRigidBody);

            RigidBody rbb = (RigidBody)m_otherRigidBody.GetCollisionObject();

            if (rbb == null)
            {
                Debug.LogError("Constraint could not get bullet RigidBody from target rigid body");
                return false;
            }

            initialTransform = BM.Matrix.AffineTransformation(1f, localRotationOffset.ToBullet(), m_localConstraintPoint.ToBullet());

            m_constraintPtr = new FixedConstraint(rba, rbb, BM.Matrix.Identity, initialTransform)
            {
                Userobject = this,
                DebugDrawSize = m_debugDrawSize,
                BreakingImpulseThreshold = m_breakingImpulseThreshold,
                OverrideNumSolverIterations = m_overrideNumSolverIterations
            };

            return true;
        }
    }
}
