using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BulletSharp;
using BulletSharp.Math;
using BulletUnity.Debugging;
using Assets.Scripts;
using System;
using Assets.Scripts.BUExtensions;

namespace BulletUnity
{
    /// <summary>
    /// This is a component to be attached to any game object that is meant to interact with game pieces in driver practice mode.
    /// 
    /// Implements BCollisionCallbacksDefault because it has a callback method that triggers whenever this object collides with any other object.
    /// </summary>
    public class Interactor : MonoBehaviour, ICollisionCallback
    {
        private GameObject[] collisionObject = new GameObject[2];
        private bool[] collisionDetected = new bool[2];
        public string[] collisionKeyword = new string[2];

        public List<GameObject> heldGamepieces = new List<GameObject>();

        private void Awake()
        {
            if (GetComponent<BMultiCallbacks>() != null)
                GetComponent<BMultiCallbacks>().AddCallback(this);
        }

        /// <summary>
        /// Method is called whenever interactor collides with another object.
        /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and acts if so.
        /// </summary>
        /// <param name="other">The object the interactor collided with</param>
        /// <param name="manifoldList">List of collision manifolds--this isn't used</param>
        public void BOnCollisionEnter(CollisionObject other, BCollisionCallbacksDefault.PersistentManifoldList manifoldList)
        {
            string gamepiece;
            for (int i = 0; i < collisionKeyword.Length; i++)
            {
                if (collisionKeyword[i] != null)
                {
                    gamepiece = collisionKeyword[i];
                    if (other.UserObject.ToString().Contains(gamepiece))
                    {
                        bool skip = false;
                        //Debug.Log(other.UserObject.ToString());
                        GameObject tempGamepiece = ((BRigidBody)other.UserObject).gameObject;
                        foreach (GameObject gp in heldGamepieces)
                        {
                            if (gp == tempGamepiece) skip = true;
                        }
                        if (!skip)
                        {
                            //Debug.Log("does this happen");
                            collisionObject[i] = tempGamepiece;
                            collisionDetected[i] = true;
                            //Debug.Log(collisionObject[i].ToString() + i.ToString());
                        }
                    }
                }
            }
        }

        public void BOnCollisionStay(CollisionObject other, BCollisionCallbacksDefault.PersistentManifoldList manifoldList)
        {
        }

        /// <summary>
        /// Method is called whenever interactor stops colliding with another object.
        /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and acts if so.
        /// </summary>
        /// <param name="other">The object the interactor stopped colliding with</param>
        public void BOnCollisionExit(CollisionObject other)
        {
            string gamepiece;
            for (int i = 0; i < collisionKeyword.Length; i++)
            {
                if (collisionKeyword[i] != null)
                {
                    gamepiece = collisionKeyword[i];
                    if (other.UserObject.ToString().Contains(gamepiece))
                    {
                        collisionDetected[i] = false;
                    }
                }
            }
        }

        public GameObject GetObject(int index)
        {
            collisionDetected[index] = false;
            return collisionObject[index];
        }

        public bool GetDetected(int index)
        {
            return collisionDetected[index];
        }

        public void SetKeyword(string keyword, int index)
        {
            collisionKeyword[index] = keyword;
        }

        public void OnVisitPersistentManifold(PersistentManifold pm)
        {
            // Not implemented
        }

        public void OnFinishedVisitingManifolds()
        {
            // Not implemented
        }
    }
}