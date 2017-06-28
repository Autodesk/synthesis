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

        private float updateTime;

        /// <summary>
        /// The collection of states stored by the Tracker.
        /// </summary>
        public FixedQueue<KeyValuePair<float, StateDescriptor>> States { get; set; }

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
        private KeyValuePair<float, StateDescriptor> State
        {
            get
            {
                return new KeyValuePair<float, StateDescriptor>(updateTime, new StateDescriptor
                {
                    Position = rigidBody.WorldTransform.Origin,
                    Rotation = rigidBody.WorldTransform.Basis,
                    LinearVelocity = rigidBody.LinearVelocity,
                    AngularVelocity = rigidBody.AngularVelocity
                });
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
            rigidBody = (RigidBody)GetComponent<BRigidBody>().GetCollisionObject();
            updateTime = 0f;

            Tracking = true;
            States = new FixedQueue<KeyValuePair<float, StateDescriptor>>(Length, State);
        }

        /// <summary>
        /// Updates the total time.
        /// </summary>
        void Update()
        {
            updateTime += Time.deltaTime;
        }

        /// <summary>
        /// Adds the current state to the states queue.
        /// </summary>
        void FixedUpdate()
        {
            if (Tracking)
                States.Add(State);

            if (!Trace)
                return;

            Vector3 lastPoint = Vector3.zero;
            int i = 0;

            foreach (StateDescriptor state in States.Select((x) => x.Value))
            {
                if (lastPoint != Vector3.zero)
                {
                    float age = (float)i / States.Length;
                    Debug.DrawLine(lastPoint, state.Position.ToUnity(), new Color(age * 0.5f,
                        1.0f, age * 0.5f, 1.0f - age * 0.5f));
                }

                lastPoint = state.Position.ToUnity();
                i++;
            }
        }
    }
}
