using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.BUExtensions
{
    public class BPhysicsTickListener
    {
        public delegate void OnTickEventHandler(DynamicsWorld world, float timeStep, int framesPassed);

        public event OnTickEventHandler OnTick;

        private static BPhysicsTickListener _instance;
        
        /// <summary>
        /// The global BPhysicsTickListener instance.
        /// </summary>
        public static BPhysicsTickListener Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BPhysicsTickListener();

                return _instance;
            }
        }

        private int lastFrameCount;

        /// <summary>
        /// Initializes a new BPhysicskTickListener instance.
        /// </summary>
        private BPhysicsTickListener()
        {
        }

        /// <summary>
        /// Called when a physics tick occurs.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="timeStep"></param>
        public void PhysicsTick(DynamicsWorld world, float timeStep)
        {
            int framesPassed = BPhysicsWorld.Get().frameCount - lastFrameCount;
            OnTick.Invoke(world, timeStep, framesPassed);
            lastFrameCount += framesPassed;
        }
    }
}
