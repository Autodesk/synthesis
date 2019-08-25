using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BulletSharp;
using BulletSharp.Math;
using BulletUnity.Debugging;
using System;
using Synthesis.BUExtensions;
using BulletUnity;

namespace Synthesis.DriverPractice
{
    /// <summary>
    /// This is a component to be attached to any game object that is meant to interact with game pieces in driver practice mode.
    /// 
    /// Implements BCollisionCallbacksDefault because it has a callback method that triggers whenever this object collides with any other object.
    /// </summary>
    public class Interactor : MonoBehaviour, ICollisionCallback
    {
        private List<PersistentManifold> lastManifolds;
        public List<string> gamepiece = new List<string>();
        private List<bool> collisionDetector = new List<bool>();
        private List<GameObject> collisionObject = new List<GameObject>();

        private void Awake()
        {
            if (GetComponent<BMultiCallbacks>() != null)
                GetComponent<BMultiCallbacks>().AddCallback(this);
            lastManifolds = new List<PersistentManifold>();
        }

        /// <summary>
        /// Method is called whenever interactor collides with another object.
        /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and acts if so.
        /// </summary>
        /// <param name="other">The object the interactor collided with</param>
        /// <param name="manifoldList">List of collision manifolds--this isn't used</param>
        public void BOnCollisionEnter(CollisionObject other, BCollisionCallbacksDefault.PersistentManifoldList manifoldList)
        {
            for (int i = 0; i < gamepiece.Count; i++)
                if (other.UserObject.ToString().Contains(gamepiece[i]) && ((BRigidBody)other.UserObject).gameObject.GetComponent<BFixedConstraintEx>() == null) //make sure gamepiece isn't already held - NO STEAL
                {
                    collisionDetector[i] = true;
                    collisionObject.Insert(i, ((BRigidBody)other.UserObject).gameObject); //insert at index coherent with index of gamepiece in FieldDataHandler.gamepieces
                }
        }
       
        /// <summary>
        /// Method is called whenever interactor stops colliding with another object.
        /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and acts if so.
        /// </summary>
        /// <param name="other">The object the interactor stopped colliding with</param>
        public void BOnCollisionExit(CollisionObject other)
        {
            for (int i = 0; i < gamepiece.Count; i++)
                if (other.UserObject.ToString().Contains(gamepiece[i]))
                    collisionDetector[i] = false;
        }
        /// <summary>
        /// add gamepiece to the interactor
        /// </summary>
        /// <param name="gamepiece">gamepiece name</param>
        /// <param name="id">gamepiece index</param>
        public void AddGamepiece(string gamepiece, int id)
        {
            while (this.gamepiece.Count < id) this.gamepiece.Add(""); //increase depth
            this.gamepiece.Insert(id, gamepiece);
            while (collisionDetector.Count < id) collisionDetector.Add(false); //increase depth
            collisionDetector.Insert(id, false);
            while (collisionObject.Count <= id) collisionObject.Add(new GameObject()); //increase depth
        }

        public bool GetDetected(int id)
        {
            return collisionDetector[id];
        }

        public GameObject GetObject(int id)
        {
            return collisionObject[id];
        }

        #region ICollisionCallback placeholders
        public void BOnCollisionStay(CollisionObject other, BCollisionCallbacksDefault.PersistentManifoldList manifoldList)
        {
        }
        public void OnVisitPersistentManifold(PersistentManifold pm)
        {
            lastManifolds.Add(pm);
        }

        public void OnFinishedVisitingManifolds()
        {
            foreach (PersistentManifold pm in lastManifolds)
                pm.ClearManifold();

            lastManifolds.Clear();
        }
        #endregion


    }
}