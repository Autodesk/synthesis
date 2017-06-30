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

            rewindTime = 0.0f;
            playbackSpeed = 1.0f;
        }

        /// <summary>
        /// Updates the positions and rotations of each tracker's parent object according to the replay time.
        /// </summary>
        public override void Update()
        {
            if (Input.GetKey(KeyCode.LeftArrow))
                rewindTime += Time.smoothDeltaTime * playbackSpeed;

            if (Input.GetKey(KeyCode.RightArrow))
                rewindTime -= Time.smoothDeltaTime * playbackSpeed;

            if (rewindTime < 0.0f)
                rewindTime = 0.0f;
            else if (rewindTime > Tracker.Lifetime)
                rewindTime = Tracker.Lifetime;

            foreach (Tracker t in trackers)
            {
                float replayTime = ReplayTime;
                int currentIndex = (int)Math.Floor(replayTime);

                StateDescriptor lowerState = t.States[currentIndex];
                StateDescriptor upperState = currentIndex < t.States.Length - 1 ? t.States[currentIndex + 1] : lowerState;

                float percent = replayTime - currentIndex;

                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();

                if (!r.IsActive)
                    r.Activate();

                BulletSharp.Math.Matrix worldTransform = r.WorldTransform;

                worldTransform.Origin = BulletSharp.Math.Vector3.Lerp(lowerState.Position, upperState.Position, percent);
                worldTransform.Basis = BulletSharp.Math.Matrix.Lerp(lowerState.Rotation, upperState.Rotation, percent);

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

                StateDescriptor currentState = t.States[(int)Math.Floor(ReplayTime)];

                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();
                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
                r.LinearVelocity = currentState.LinearVelocity;
                r.AngularVelocity = currentState.AngularVelocity;

                t.Clear();
            }
        }
    }
}
