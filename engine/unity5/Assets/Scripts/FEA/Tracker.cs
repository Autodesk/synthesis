using Assets.Scripts.FSM;
using BulletSharp;
using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.FEA
{
    public class Tracker : MonoBehaviour
    {
        private const float fixedTimeStep = 1f / 60f;

        private BPhysicsWorld physicsWorld;
        private RigidBody rigidBody;

        /// <summary>
        /// The collection of states stored by the Tracker.
        /// </summary>
        public FixedQueue<StateDescriptor> States { get; private set; }

        /// <summary>
        /// The number of seconds the tracker keeps any given state.
        /// </summary>
        public const float Lifetime = 3f;

        /// <summary>
        /// The number of states in the queue.
        /// </summary>
        public const int Length = (int)(Lifetime / fixedTimeStep);

        /// <summary>
        /// If true, lines will be drawn showing the history of the parent's motion.
        /// </summary>
        public bool Trace { get; set; }

        /// <summary>
        /// Returns a StateDescriptor based on the current frame.
        /// </summary>
        private StateDescriptor State
        {
            get
            {
                return new StateDescriptor
                {
                    Position = rigidBody.WorldTransform.Origin,
                    Rotation = rigidBody.WorldTransform.Basis,
                    LinearVelocity = rigidBody.LinearVelocity,
                    AngularVelocity = rigidBody.AngularVelocity
                };
            }
        }

        /// <summary>
        /// Clears all saved states with the current state.
        /// </summary>
        public void Clear()
        {
            States.Clear(State);
        }

        /// <summary>
        /// Adds the current state to the states queue.
        /// </summary>
        public void AddState()
        {
            States.Add(State);
        }

        /// <summary>
        /// Called when the Tracker is initialized.
        /// </summary>
        void Start()
        {
            physicsWorld = BPhysicsWorld.Get();
            rigidBody = (RigidBody)GetComponent<BRigidBody>().GetCollisionObject();

            States = new FixedQueue<StateDescriptor>(Length, State);

            MainState mainState = StateMachine.Instance.CurrentState as MainState;

            if (mainState != null)
                mainState.Trackers.Add(this);
            else
                Destroy(this);
        }

        /// <summary>
        /// Called when the Tracker is destroyed.
        /// </summary>
        void OnDestroy()
        {
            StateMachine sm = StateMachine.Instance;

            if (sm == null)
                return;

            MainState mainState = sm.CurrentState as MainState;

            if (mainState != null)
                mainState.Trackers.Remove(this);
        }
    }
}
