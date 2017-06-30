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

        private int lastFrameCount;

        /// <summary>
        /// The collection of states stored by the Tracker.
        /// </summary>
        public FixedQueue<StateDescriptor> States { get; set; }

        /// <summary>
        /// The number of seconds the tracker keeps any given state.
        /// </summary>
        public const float Lifetime = 3.0f;

        /// <summary>
        /// The number of states in the queue.
        /// </summary>
        public const int Length = (int)(Lifetime / fixedTimeStep);

        /// <summary>
        /// If true, the Tracker will actively track its parent.
        /// </summary>
        public bool Tracking { get; set; }

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
        /// Called when the Tracker is initialized.
        /// </summary>
        void Start()
        {
            physicsWorld = BPhysicsWorld.Get();
            rigidBody = (RigidBody)GetComponent<BRigidBody>().GetCollisionObject();
            lastFrameCount = physicsWorld.frameCount;

            Tracking = true;
            States = new FixedQueue<StateDescriptor>(Length, State);
        }

        /// <summary>
        /// Adds any new states to the queue.
        /// </summary>
        void Update()
        {
            if (Tracking)
                AddStates();
        }

        /// <summary>
        /// Draws lines representing stored states.
        /// </summary>
        void FixedUpdate()
        {
            if (Tracking)
                AddStates();

            if (!Trace)
                return;

            Vector3 lastPoint = Vector3.zero;
            int i = 0;

            foreach (StateDescriptor state in States)
            {
                if (lastPoint != Vector3.zero)
                {
                    float age = (float)i / States.Length;
                    //Debug.DrawLine(lastPoint, state.Position.ToUnity(), i % 2 == 0 ? Color.red : Color.blue);
                    Debug.DrawLine(lastPoint, state.Position.ToUnity(), new Color(age * 0.5f,
                        1.0f, age * 0.5f, 1.0f - age * 0.5f));
                }

                lastPoint = state.Position.ToUnity();
                i++;
            }
        }

        /// <summary>
        /// Adds states to the queue depending on how many frames have passed.
        /// </summary>
        private void AddStates()
        {
            int numSteps = physicsWorld.frameCount - lastFrameCount;

            for (int i = 0; i < numSteps; i++)
                States.Add(State);

            lastFrameCount += numSteps;
        }
    }
}
