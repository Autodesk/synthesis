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

        public FixedQueue<List<ContactDescriptor>> ContactPoints { get; private set; }
        public int LastFrameCount { get; private set; }

        public CollisionTracker(MainState mainState)
        {
            this.mainState = mainState;
            physicsWorld = BPhysicsWorld.Get();
            framesPassed = -1;

            ContactPoints = new FixedQueue<List<ContactDescriptor>>(Tracker.Length);
            LastFrameCount = physicsWorld.frameCount;
        }

        public void Reset()
        {
            ContactPoints.Clear(null);
            LastFrameCount = physicsWorld.frameCount;
            framesPassed = -1;
        }

        public void Synchronize(int updatedFrameCount)
        {
            for (int i = 0; i <= updatedFrameCount - LastFrameCount; i++)
                ContactPoints.Add(null);
        }

        public void OnVisitPersistentManifold(PersistentManifold pm)
        {
            if (!mainState.Tracking)
                return;

            if (framesPassed == -1) // This is the first manifold visited of the frame
                framesPassed = physicsWorld.frameCount - LastFrameCount;

            if (passedContacts == null)
                passedContacts = new List<ContactDescriptor>[framesPassed];

            BRigidBody obA = pm.Body0.UserObject as BRigidBody;
            BRigidBody obB = pm.Body1.UserObject as BRigidBody;

            if ((obA == null || obB == null) || (!obA.gameObject.name.StartsWith("node") && !obB.gameObject.name.StartsWith("node")))
                return;

            int numContacts = pm.NumContacts;

            for (int i = framesPassed; i > 0; i--)
            {
                ManifoldPoint mp = null;

                for (int j = 0; j < numContacts; j++)
                {
                    mp = pm.GetContactPoint(j);

                    if (mp.LifeTime == i * 2 - 1) // fsr the lifetime starts at 1 and increases by 2 every frame
                        break;
                }

                if (mp == null)
                    return;

                if (passedContacts[i - 1] == null)
                    passedContacts[i - 1] = new List<ContactDescriptor>();

                passedContacts[i - 1].Add(new ContactDescriptor
                {
                    AppliedImpulse = mp.AppliedImpulse,
                    Position = (mp.PositionWorldOnA + mp.PositionWorldOnB) * 0.5f,
                    RobotBody = obA.name.StartsWith("node") ? obA : obB
                });
            }
        }

        public void OnFinishedVisitingManifolds()
        {
            if (!mainState.Tracking)
                return;

            if (framesPassed > 0)
                for (int i = passedContacts.Length - 1; i >= 0; i--)
                    ContactPoints.Add(passedContacts[i]);

            passedContacts = null;

            LastFrameCount = physicsWorld.frameCount;
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
