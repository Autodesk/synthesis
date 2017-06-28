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
        float currentTime;

        List<Tracker> trackers;

        private int Index
        {
            get
            {
                return (int)((currentTime / Tracker.Lifetime) * (Tracker.Length - 1));
            }
        }

        public ReplayState()
        {
            trackers = new List<Tracker>((Tracker[])UnityEngine.Object.FindObjectsOfType(typeof(Tracker)));
        }

        public override void Start()
        {
            currentTime = 0.0f;

            foreach (Tracker t in trackers)
            {
                t.Tracking = false;

                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();
                r.LinearVelocity = r.AngularVelocity = BulletSharp.Math.Vector3.Zero;
                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.Zero;
            }
        }

        public override void Update()
        {
            foreach (Tracker t in trackers)
            {
                StateDescriptor state = t.GetState(Index);
                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();

                if (!r.IsActive)
                    r.Activate();

                BulletSharp.Math.Matrix worldTransform = r.WorldTransform;
                worldTransform.Origin = state.Position;
                worldTransform.Basis = state.Rotation;

                r.WorldTransform = worldTransform;
            }
        }

        public override void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.LeftArrow))
                currentTime += Time.fixedDeltaTime;

            if (Input.GetKey(KeyCode.RightArrow))
                currentTime -= Time.fixedDeltaTime;

            if (currentTime < 0.0f)
                currentTime = 0.0f;
            else if (currentTime > Tracker.Lifetime)
                currentTime = Tracker.Lifetime;

            if (Input.GetKey(KeyCode.D))
                StateMachine.Instance.PopState();
        }

        public override void End()
        {
            foreach (Tracker t in trackers)
            {
                t.Tracking = true;

                StateDescriptor currentState = t.GetState(Index);

                RigidBody r = (RigidBody)t.GetComponent<BRigidBody>().GetCollisionObject();
                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
                r.LinearVelocity = currentState.LinearVelocity;
                r.AngularVelocity = currentState.AngularVelocity;

                t.PopTo(Index);
            }
        }
    }
}
