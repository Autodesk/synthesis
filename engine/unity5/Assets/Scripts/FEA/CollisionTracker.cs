using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using BulletUnity;
using UnityEngine;

namespace Assets.Scripts.FEA
{
    public class CollisionTracker : ICollisionCallback
    {
        private MainState mainState;
        private BPhysicsWorld physicsWorld;
        private int lastFrameCount;
        private int framesPassed;

        /// <summary>
        /// The list of contact points tracked by the CollisionTracker.
        /// </summary>
        public FixedQueue<List<ContactDescriptor>> ContactPoints { get; private set; }

        /// <summary>
        /// Creates a new CollisionTracker instance.
        /// </summary>
        /// <param name="mainState"></param>
        public CollisionTracker(MainState mainState)
        {
            this.mainState = mainState;
            physicsWorld = BPhysicsWorld.Get();
            lastFrameCount = physicsWorld.frameCount;
            framesPassed = -1;

            ContactPoints = new FixedQueue<List<ContactDescriptor>>(Tracker.Length);
        }

        /// <summary>
        /// Resets the CollisionTracker and clears all stored contact information.
        /// </summary>
        public void Reset()
        {
            ContactPoints.Clear(null);
            lastFrameCount = physicsWorld.frameCount - 1;
        }

        /// <summary>
        /// Finds any robot collisions and adds them to the list of contact points.
        /// </summary>
        /// <param name="pm"></param>
        public void OnVisitPersistentManifold(PersistentManifold pm)
        {
            if (!mainState.Tracking)
                return;

            if (framesPassed == -1)
            {
                framesPassed = physicsWorld.frameCount - lastFrameCount;

                for (int i = 0; i < framesPassed; i++)
                    ContactPoints.Add(new List<ContactDescriptor>());
            }

            BRigidBody obA = pm.Body0.UserObject as BRigidBody;
            BRigidBody obB = pm.Body1.UserObject as BRigidBody;
            BRigidBody robotBody = obA != null && obA.gameObject.name.StartsWith("node") ? obA : obB != null && obB.gameObject.name.StartsWith("node") ? obB : null;

            if (robotBody == null)
                return;

            if (pm.NumContacts < 1)
                return;

            int numContacts = pm.NumContacts;

            for (int i = 0; i < Math.Min(framesPassed, numContacts); i++)
            {
                ManifoldPoint mp = pm.GetContactPoint(i);

                ContactDescriptor cd = new ContactDescriptor
                {
                    AppliedImpulse = mp.AppliedImpulse,
                    Position = (mp.PositionWorldOnA + mp.PositionWorldOnB) * 0.5f,
                    RobotBody = robotBody
                };

                if (ContactPoints[i] != null)
                    ContactPoints[i].Add(cd);
            }
        }

        /// <summary>
        /// Adds an empty list of contact points for any frames without manifolds.
        /// </summary>
        public void OnFinishedVisitingManifolds()
        {
            if (framesPassed == -1)
            {
                framesPassed = physicsWorld.frameCount - lastFrameCount;

                for (int i = 0; i < framesPassed; i++)
                    ContactPoints.Add(new List<ContactDescriptor>());
            }

            lastFrameCount += framesPassed;
            framesPassed = -1;
        }

        public void BOnCollisionEnter(CollisionObject other, BCollisionCallbacksDefault.PersistentManifoldList manifoldList)
        {
            // Not implemented
        }

        public void BOnCollisionExit(CollisionObject other)
        {
            // Not implemented
        }

        public void BOnCollisionStay(CollisionObject other, BCollisionCallbacksDefault.PersistentManifoldList manifoldList)
        {
            // Not implemented
        }
    }
}
