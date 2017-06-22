using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.FEA
{
    public class Tracker : MonoBehaviour
    {
        private const float QueueTimespan = 3.0f;

        private FixedQueue<StateDescriptor> statesQueue;

        /// <summary>
        /// Returns a StateDescriptor based on the current frame.
        /// </summary>
        private StateDescriptor State
        {
            get
            {
                return new StateDescriptor
                {
                    Position = gameObject.transform.position,
                    Rotation = gameObject.transform.rotation.eulerAngles
                };
            }
        }

        /// <summary>
        /// Called when the Tracker is initialized.
        /// </summary>
        void Start()
        {
            statesQueue = new FixedQueue<StateDescriptor>((int)(QueueTimespan / Time.fixedDeltaTime), State);
            //Debug.Log(statesQueue.Length);

            // TODO: Find and store joint instance
        }

        /// <summary>
        /// Adds the current state to the states queue.
        /// </summary>
        void FixedUpdate()
        {
            statesQueue.Add(State);

            Vector3 lastPoint = Vector3.zero;
            int i = 0;

            foreach (StateDescriptor state in statesQueue)
            {
                if (lastPoint != Vector3.zero)
                {
                    float age = (float)i / statesQueue.Length;
                    Debug.DrawLine(lastPoint, state.Position, new Color(1.0f, age, 0.0f, 1.0f - age));
                }

                lastPoint = state.Position;
                i++;
            }
        }
    }
}
