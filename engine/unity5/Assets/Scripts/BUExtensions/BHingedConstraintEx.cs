using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.BUExtensions
{
    public class BHingedConstraintEx : BHingedConstraint
    {
        /// <summary>
        /// Overrides the base localConstraintAxisX to prevent usage (not implemented).
        /// </summary>
        public new Vector3 localConstraintAxisX
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Overrides the base localConstraintAxisY to prevent usage (not implemented).
        /// </summary>
        public new Vector3 localConstraintAxisY
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        protected Vector3 m_axisInA = Vector3.up;

        /// <summary>
        /// The local axis relative to body A.
        /// </summary>
        public Vector3 axisInA
        {
            get
            {
                return m_axisInA;
            }
            set
            {
                if (m_constraintPtr != null && m_axisInA != value)
                {
                    Debug.LogError("Cannot change the local constraint axis once the constraint has been created");
                }
                else
                {
                    m_axisInA = value;
                }
            }
        }

        protected Vector3 m_axisInB = Vector3.up;

        /// <summary>
        /// The local axis relative to body B.
        /// </summary>
        public Vector3 axisInB
        {
            get
            {
                return m_axisInB;
            }
            set
            {
                if (m_constraintPtr != null && m_axisInB != value)
                {
                    Debug.LogError("Cannot change the local constraint axis once the constraint has been created");
                }
                else
                {
                    m_axisInB = value;
                }
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
                Debug.LogError("BHingedConstraint needs to be added to a component with a BRigidBody.");
                return false;
            }
            if (!targetRigidBodyA.isInWorld)
            {
                world.AddRigidBody(targetRigidBodyA);
            }
            if (m_localConstraintAxisX == Vector3.zero)
            {
                Debug.LogError("Constaint axis cannot be zero vector");
                return false;
            }
            RigidBody rba = (RigidBody)targetRigidBodyA.GetCollisionObject();
            if (rba == null)
            {
                Debug.LogError("Constraint could not get bullet RigidBody from target rigid body");
                return false;
            }
            if (m_constraintType == ConstraintType.constrainToAnotherBody)
            {
                if (m_otherRigidBody == null)
                {
                    Debug.LogError("Other rigid body must not be null");
                    return false;
                }
                if (!m_otherRigidBody.isInWorld)
                {
                    world.AddRigidBody(m_otherRigidBody);
                }
                RigidBody rbb = (RigidBody)m_otherRigidBody.GetCollisionObject();
                if (rbb == null)
                {
                    Debug.LogError("Constraint could not get bullet RigidBody from target rigid body");
                    return false;
                }

                m_constraintPtr = new HingeConstraint(rba, rbb, m_localConstraintPoint.ToBullet(),
                    m_otherRigidBody.transform.InverseTransformPoint(transform.TransformPoint(m_localConstraintPoint)).ToBullet(),
                    m_axisInA.ToBullet(), m_axisInB.ToBullet());
            }
            else
            {
                m_constraintPtr = new HingeConstraint(rba, m_localConstraintPoint.ToBullet(), m_axisInA.ToBullet(), false);
            }
            if (m_enableMotor)
            {
                ((HingeConstraint)m_constraintPtr).EnableAngularMotor(true, m_targetMotorAngularVelocity, m_maxMotorImpulse);
            }
            if (m_setLimit)
            {
                ((HingeConstraint)m_constraintPtr).SetLimit(m_lowLimitAngleRadians, m_highLimitAngleRadians, m_limitSoftness, m_limitBiasFactor);
            }
            m_constraintPtr.Userobject = this;
            m_constraintPtr.DebugDrawSize = m_debugDrawSize;
            m_constraintPtr.BreakingImpulseThreshold = m_breakingImpulseThreshold;
            m_constraintPtr.OverrideNumSolverIterations = m_overrideNumSolverIterations;
            return true;
        }
    }
}
