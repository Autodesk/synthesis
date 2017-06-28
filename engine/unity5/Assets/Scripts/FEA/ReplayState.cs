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
    public class ReplayState : SimState
    {
        float pausedTime;
        float rewindTime;
        float playbackSpeed;

        List<Tracker> trackers;

        /// <summary>
        /// The normalized replay time.
        /// </summary>
        private float ReplayTime
        {
            get
            {
                return (rewindTime / Tracker.Lifetime) * (Tracker.Length - 1);
            }
        }

        /// <summary>
        /// Creates a new ReplayState instance.
        /// </summary>
        public ReplayState()
        {
            trackers = ((Tracker[])UnityEngine.Object.FindObjectsOfType(typeof(Tracker))).ToList();
        }

        /// <summary>
        /// Initializes the state.
        /// </summary>
        public override void Start()
        {
            foreach (Tracker t in trackers)
            {
                t.Tracking = false;

                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();
                r.LinearVelocity = r.AngularVelocity = BulletSharp.Math.Vector3.Zero;
                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.Zero;
            }

            pausedTime = trackers[0].States[0].Key;
            rewindTime = 0.0f;
            playbackSpeed = 1.0f;
        }

        /// <summary>
        /// Updates the positions and rotations of each tracker's parent object according to the replay time.
        /// </summary>
        public override void Update()
        {
            if (Input.GetKey(KeyCode.LeftArrow))
                rewindTime += Time.deltaTime * playbackSpeed;

            if (Input.GetKey(KeyCode.RightArrow))
                rewindTime -= Time.deltaTime * playbackSpeed;

            if (rewindTime < 0.0f)
                rewindTime = 0.0f;
            else if (rewindTime > Tracker.Lifetime)
                rewindTime = Tracker.Lifetime;

            foreach (Tracker t in trackers)
            {
                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();
                BulletSharp.Math.Matrix worldTransform = r.WorldTransform;

                if (!r.IsActive)
                    r.Activate();

                float currentTime = pausedTime - rewindTime;
                int closestIndex = FindClosestIndex(currentTime, t.States.Select((x) => x.Key).ToArray());

                var lower = t.States[closestIndex];
                StateDescriptor lowerState = lower.Value;
                float lowerTime = lower.Key;

                int upperIndex = closestIndex;

                for (; upperIndex < t.States.Length - 1 && lowerTime == t.States[upperIndex].Key; upperIndex++) ;

                if (closestIndex == upperIndex)
                {
                    worldTransform.Origin = lowerState.Position;
                    worldTransform.Basis = lowerState.Rotation;
                }
                else
                {
                    var upper = t.States[upperIndex];
                    StateDescriptor upperState = upper.Value;
                    float upperTime = upper.Key;

                    float percent = 1 - ((currentTime - upperTime) / (lowerTime - upperTime));

                    worldTransform.Origin = BulletSharp.Math.Vector3.Lerp(lowerState.Position, upperState.Position, percent);
                    worldTransform.Basis = BulletSharp.Math.Matrix.Lerp(lowerState.Rotation, upperState.Rotation, percent);
                }

                r.WorldTransform = worldTransform;
            }
        }

        /// <summary>
        /// Sets the playback speed according to the user's input.
        /// </summary>
        public override void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.UpArrow))
                playbackSpeed = 1.0f;
            else if (Input.GetKey(KeyCode.DownArrow))
                playbackSpeed = 0.1f;

            if (Input.GetKey(KeyCode.Return))
                StateMachine.Instance.PopState();
        }

        /// <summary>
        /// Resets the trackers and reenables the RigidBodies.
        /// </summary>
        public override void End()
        {
            foreach (Tracker t in trackers)
            {
                t.Tracking = true;

                StateDescriptor currentState = t.States[(int)Math.Floor(ReplayTime)].Value;

                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();
                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
                r.LinearVelocity = currentState.LinearVelocity;
                r.AngularVelocity = currentState.AngularVelocity;

                t.Clear();
            }
        }

        /// <summary>
        /// Finds the index whose value is closest to the given target.
        /// The values must be sorted in decreasing order.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private int FindClosestIndex(float target, float[] values)
        {
            if (target >= values[0])
                return 0;

            for (int i = 0; i < values.Length - 1; i++)
            {
                if (target <= values[i] && target > values[i + 1])
                    return i;
            }

            return values.Length - 1;
        }
    }
}
