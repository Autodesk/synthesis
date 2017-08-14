using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.BUExtensions
{
    public class BPhysicsWorldLateHelperEx : BPhysicsWorldLateHelper
    {
        protected override void FixedUpdate()
        {
            if (m_ddWorld != null)
            {
                float deltaTime = UnityEngine.Time.time - m_lastSimulationStepTime;
                if (deltaTime > 0f)
                {
                    ///stepSimulation proceeds the simulation over 'timeStep', units in preferably in seconds.
                    ///By default, Bullet will subdivide the timestep in constant substeps of each 'fixedTimeStep'.
                    ///in order to keep the simulation real-time, the maximum number of substeps can be clamped to 'maxSubSteps'.
                    ///You can disable subdividing the timestep/substepping by passing maxSubSteps=0 as second argument to stepSimulation, but in that case you have to keep the timeStep constant.
                    int numSteps = m_ddWorld.StepSimulation(deltaTime, m_maxSubsteps, m_fixedTimeStep);

                    if (numSteps > 0 && m_collisionEventHandler != null)
                        m_collisionEventHandler.OnPhysicsStep(m_world);

                    m__frameCount += numSteps;
                    //Debug.Log("FixedUpdate " + numSteps);
                    m_lastSimulationStepTime = UnityEngine.Time.time;
                }
            }
        }

        //This is needed for rigidBody interpolation. The motion states will update the positions of the rigidbodies
        protected override void Update()
        {
            float deltaTime = UnityEngine.Time.time - m_lastSimulationStepTime;
            if (deltaTime > 0f)
            {
                int numSteps = m_ddWorld.StepSimulation(deltaTime, m_maxSubsteps, m_fixedTimeStep);

                if (numSteps > 0 && m_collisionEventHandler != null)
                    m_collisionEventHandler.OnPhysicsStep(m_world);

                m__frameCount += numSteps;
                //Debug.Log("Update " + numSteps);
                m_lastSimulationStepTime = UnityEngine.Time.time;
            }
        }
    }
}
