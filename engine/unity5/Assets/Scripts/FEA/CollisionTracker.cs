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
        private List<ContactDescriptor>[] passedContacts;
        private int framesPassed;
        private int lastFrameCount;

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
            framesPassed = -1;
            lastFrameCount = physicsWorld.frameCount;

            ContactPoints = new FixedQueue<List<ContactDescriptor>>(Tracker.Length);
        }

        /// <summary>
        /// Resets the CollisionTracker and clears all stored contact information.
        /// </summary>
        public void Reset()
        {
            ContactPoints.Clear(null);
            lastFrameCount = physicsWorld.frameCount;
            framesPassed = -1;
        }

        /// <summary>
        /// Synchronizes the CollisionTracker's frame count with the given frame count.
        /// </summary>
        /// <param name="updatedFrameCount"></param>
        public void Synchronize(int updatedFrameCount)
        {
            for (int i = 0; i <= updatedFrameCount - lastFrameCount; i++)
                ContactPoints.Add(null);
        }

        /// <summary>
        /// Finds any robot collisions and adds them to the list of collisions for the current frame.
        /// </summary>
        /// <param name="pm"></param>
        public void OnVisitPersistentManifold(PersistentManifold pm)
        {
            if (!mainState.Tracking)
                return;

            if (framesPassed == -1) // This is the first manifold visited of the frame
                framesPassed = physicsWorld.frameCount - lastFrameCount;

            if (passedContacts == null)
                passedContacts = new List<ContactDescriptor>[framesPassed];

            BRigidBody obA = pm.Body0.UserObject as BRigidBody;
            BRigidBody obB = pm.Body1.UserObject as BRigidBody;

            if ((obA == null || obB == null) || (!obA.gameObject.name.StartsWith("node") && !obB.gameObject.name.StartsWith("node")))
                return;

            int numContacts = pm.NumContacts;

            for (int i = 0; i < framesPassed; i++)
            {
                if (numContacts - 1 - i < 0)
                    break;

                ManifoldPoint mp = pm.GetContactPoint(numContacts - 1 - i);

                ContactDescriptor cd = new ContactDescriptor
                {
                    AppliedImpulse = mp.AppliedImpulse,
                    Position = (mp.PositionWorldOnA + mp.PositionWorldOnB) * 0.5f,
                    RobotBody = obA.name.StartsWith("node") ? obA : obB
                };

                if (passedContacts[framesPassed - 1 - i] == null)
                    passedContacts[framesPassed - 1 - i] = new List<ContactDescriptor>();

                passedContacts[framesPassed - 1 - i].Add(cd);
            }
        }

        /// <summary>
        /// Adds all frame collisions to the queue of total collisions.
        /// </summary>
        public void OnFinishedVisitingManifolds()
        {
            if (!mainState.Tracking)
                return;

            framesPassed = physicsWorld.frameCount - lastFrameCount;
            lastFrameCount = physicsWorld.frameCount;

            for (int i = 0; i < framesPassed; i++)
            {
                if (passedContacts != null && passedContacts.Length > i)
                    ContactPoints.Add(passedContacts[i]);
                else
                    ContactPoints.Add(null);
            }

            passedContacts = null;
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
