using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    /// <summary>
    /// Classes that are intended to be registered with a BMultiCallback should implement this interface.
    /// </summary>
    public interface ICollisionCallback
    {
        /// <summary>
        /// Called when a collision is initiated.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="manifoldList"></param>
        void BOnCollisionEnter(CollisionObject other, BulletUnity.BCollisionCallbacksDefault.PersistentManifoldList manifoldList);

        /// <summary>
        /// Called when an existing collision continues.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="manifoldList"></param>
        void BOnCollisionStay(CollisionObject other, BulletUnity.BCollisionCallbacksDefault.PersistentManifoldList manifoldList);

        /// <summary>
        /// Called when an existing collison exits contact.
        /// </summary>
        /// <param name="other"></param>
        void BOnCollisionExit(CollisionObject other);

        /// <summary>
        /// Called when a <see cref="PersistentManifold"/> is visited (will be called once for each <see cref="PersistentManifold"/> per physics tick).
        /// </summary>
        /// <param name="pm"></param>
        void OnVisitPersistentManifold(PersistentManifold pm);

        /// <summary>
        /// Called each physics tick after all <see cref="PersistentManifold"/>s are done being visited.
        /// </summary>
        void OnFinishedVisitingManifolds();
    }
}
