using BulletUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using UnityEngine;

namespace Assets.Scripts.BUExtensions
{
    public class BMultiCallbacks : BCollisionCallbacksDefault
    {
        List<ICollisionCallback> callbacks;

        /// <summary>
        /// Adds the given collision callback to the list of registered callbacks.
        /// </summary>
        /// <param name="callback"></param>
        public void AddCallback(ICollisionCallback callback)
        {
            if (callbacks.Contains(callback))
            {
                Debug.LogError("Cannot add the same collision callback twice!");
                return;
            }

            callbacks.Add(callback);
        }

        /// <summary>
        /// Removes the given callback from the list of registered callbacks.
        /// </summary>
        /// <param name="callback"></param>
        public void RemoveCallback(ICollisionCallback callback)
        {
            if (callbacks.Contains(callback))
                callbacks.Remove(callback);
        }

        private void Awake()
        {
            callbacks = new List<ICollisionCallback>();
        }

        /// <summary>
        /// Called for each manifold iterated over.
        /// </summary>
        /// <param name="pm"></param>
        public override void OnVisitPersistentManifold(PersistentManifold pm)
        {
            for (int i = 0; i < callbacks.Count; i++)
                callbacks[i].OnVisitPersistentManifold(pm);

            base.OnVisitPersistentManifold(pm);
        }

        /// <summary>
        /// Called each frame when the manifolds are fininished iterating.
        /// </summary>
        public override void OnFinishedVisitingManifolds()
        {
            for (int i = 0; i < callbacks.Count; i++)
                callbacks[i].OnFinishedVisitingManifolds();

            base.OnFinishedVisitingManifolds();
        }

        /// <summary>
        /// Called when a collision is first entered.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="manifoldList"></param>
        public override void BOnCollisionEnter(CollisionObject other, PersistentManifoldList manifoldList)
        {
            for (int i = 0; i < callbacks.Count; i++)
                callbacks[i].BOnCollisionEnter(other, manifoldList);
        }

        /// <summary>
        /// Called when a preexisting collision continues.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="manifoldList"></param>
        public override void BOnCollisionStay(CollisionObject other, PersistentManifoldList manifoldList)
        {
            for (int i = 0; i < callbacks.Count; i++)
                callbacks[i].BOnCollisionStay(other, manifoldList);
        }

        /// <summary>
        /// Called when a collision is exited.
        /// </summary>
        /// <param name="other"></param>
        public override void BOnCollisionExit(CollisionObject other)
        {
            for (int i = 0; i < callbacks.Count; i++)
                callbacks[i].BOnCollisionExit(other);
        }
    }
}
