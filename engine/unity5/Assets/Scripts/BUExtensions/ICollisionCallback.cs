using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public interface ICollisionCallback
    {
        void BOnCollisionEnter(CollisionObject other, BulletUnity.BCollisionCallbacksDefault.PersistentManifoldList manifoldList);
        void BOnCollisionStay(CollisionObject other, BulletUnity.BCollisionCallbacksDefault.PersistentManifoldList manifoldList);
        void BOnCollisionExit(CollisionObject other);
        void OnVisitPersistentManifold(PersistentManifold pm);
        void OnFinishedVisitingManifolds();
    }
}
