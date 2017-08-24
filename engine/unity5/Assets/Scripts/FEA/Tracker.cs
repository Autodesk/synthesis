using Assets.Scripts.BUExtensions;
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

        private MainState mainState;
        private BPhysicsWorld physicsWorld;
        private RigidBody rigidBody;

        /// <summary>
        /// The collection of states stored by the Tracker.
        /// </summary>
        public FixedQueue<StateDescriptor> States { get; set; }

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
        public void AddState(DynamicsWorld world, float timeStep, int numSteps)
        {
            if (!mainState.Tracking)
                return;

            StateDescriptor nextState = State;

            if (numSteps == 1) // This will be the case the vast majority of the time
            {
                States.Add(State);
            }
            else // If there's some random lag spike
            {
                StateDescriptor lastState = States.Front;
                float lerpAmount = 1f / numSteps;

                for (int i = 0; i < numSteps; i++)
                {
                    float percent = (i + 1) * lerpAmount;

                    States.Add(new StateDescriptor
                    {
                        Position = BulletSharp.Math.Vector3.Lerp(lastState.Position, nextState.Position, percent),
                        Rotation = BulletSharp.Math.Matrix.Lerp(lastState.Rotation, nextState.Rotation, percent),
                        LinearVelocity = BulletSharp.Math.Vector3.Lerp(lastState.LinearVelocity, nextState.LinearVelocity, percent),
                        AngularVelocity = BulletSharp.Math.Vector3.Lerp(lastState.AngularVelocity, nextState.AngularVelocity, percent)
                    });
                }
            }
        }

        /// <summary>
        /// Called when the Tracker is initialized.
        /// </summary>
        void Awake()
        {
            mainState = StateMachine.Instance.FindState<MainState>();

            if (mainState == null)
                Destroy(this);

            physicsWorld = BPhysicsWorld.Get();
            rigidBody = (RigidBody)GetComponent<BRigidBody>().GetCollisionObject();

            States = new FixedQueue<StateDescriptor>(Length, State);

            BPhysicsTickListener.Instance.OnTick += AddState;
        }

        /// <summary>
        /// Called when the Tracker is destroyed.
        /// </summary>
        void OnDestroy()
        {
            BPhysicsTickListener.Instance.OnTick -= AddState;
        }
    }
}