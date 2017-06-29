using UnityEngine;
using System.Collections;
using BulletSharp;
using BulletSharp.Math;
using System.Collections;
using BulletUnity.Debugging;

namespace BulletUnity
{
    /// <summary>
    /// This is a component to be attached to any game object that is meant to interact with game pieces in driver practice mode.
    /// 
    /// Implements BCollisionCallbacksDefault because it has a callback method that triggers whenever this object collides with any other object.
    /// </summary>
    public class Interactor : BCollisionCallbacksDefault
    {
        private GameObject collisionObject;
        private bool collisionDetected = false;
        private string collisionKeyword;

        /// <summary>
        /// Method is called whenever interactor collides with another object.
        /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and acts if so.
        /// </summary>
        /// <param name="other">The object the interactor collided with</param>
        /// <param name="manifoldList">List of collision manifolds--this isn't used</param>
        public override void BOnCollisionEnter(CollisionObject other, PersistentManifoldList manifoldList)
        {
            if (other.UserObject.ToString().Contains(collisionKeyword))
            {
                collisionDetected = true;
                Debug.Log(other.UserObject.ToString());
                collisionObject = GameObject.Find(other.UserObject.ToString().Replace(" (BulletUnity.BRigidBody)", ""));
            }
        }

        /// <summary>
        /// Method is called whenever interactor stops colliding with another object.
        /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and acts if so.
        /// </summary>
        /// <param name="other">The object the interactor stopped colliding with</param>
        public override void BOnCollisionExit(CollisionObject other)
        {
            if (other.UserObject.ToString().Contains(collisionKeyword))
            {
                collisionDetected = false;
            }
        }

        public GameObject GetObject()
        {
            return collisionObject;
        }

        public bool GetDetected()
        {
            return collisionDetected;
        }

        public void SetKeyword(string keyword)
        {
            collisionKeyword = keyword;
        }
    }
}