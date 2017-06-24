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
        private RigidBody rigidBody;

        private FixedQueue<StateDescriptor> statesQueue;

        /// <summary>
        /// The number of seconds the tracker keeps any given state.
        /// </summary>
        public const float Lifetime = 3.0f;

        /// <summary>
        /// If true, the Tracker will actively track its parent.
        /// </summary>
        public bool Tracking { get; set; }

        /// <summary>
        /// If true, lines will be drawn showing the history of the parent's motion.
        /// </summary>
        public bool Trace { get; set; }

        /// <summary>
        /// Returns the number of states in the queue.
        /// </summary>
        public static int Length
        {
            get
            {
                return (int)(Lifetime / Time.fixedDeltaTime);
            }
        }

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
        /// Returns the StateDescriptor at the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public StateDescriptor GetState(int index)
        {
            return statesQueue[index];
        }

        /// <summary>
        /// Removes any information in the states queue until the given index.
        /// </summary>
        /// <param name="index"></param>
        public void PopTo(int index)
        {
            StateDescriptor[] oldDescriptions = new StateDescriptor[Length - index];

            for (int i = 0; i < oldDescriptions.Length; i++)
                oldDescriptions[i] = statesQueue[statesQueue.Length - 1 - i];

            statesQueue.Clear(oldDescriptions[0]);

            for (int i = 0; i < oldDescriptions.Length; i++)
                statesQueue.Add(oldDescriptions[i]);
        }

        /// <summary>
        /// Clears all saved states with the current state.
        /// </summary>
        public void Clear()
        {
            statesQueue.Clear(State);
        }

        /// <summary>
        /// Called when the Tracker is initialized.
        /// </summary>
        void Start()
        {
            Tracking = true;

            rigidBody = (RigidBody)GetComponent<BRigidBody>().GetCollisionObject();
            statesQueue = new FixedQueue<StateDescriptor>(Length, State);
        }

        /// <summary>
        /// Adds the current state to the states queue.
        /// </summary>
        void FixedUpdate()
        {
            if (Tracking)
                statesQueue.Add(State);

            if (!Trace)
                return;

            Vector3 lastPoint = Vector3.zero;
            int i = 0;

            foreach (StateDescriptor state in statesQueue)
            {
                if (lastPoint != Vector3.zero)
                {
                    float age = (float)i / statesQueue.Length;
                    Debug.DrawLine(lastPoint, state.Position.ToUnity(), new Color(1.0f, age * 0.5f, 0.0f, 1.0f - age * 0.5f));
                }

                lastPoint = state.Position.ToUnity();
                i++;
            }
        }
    }
}
