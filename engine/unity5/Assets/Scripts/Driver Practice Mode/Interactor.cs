using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BulletSharp;
using BulletSharp.Math;
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
        private GameObject[] collisionObject = new GameObject[2];
        private bool[] collisionDetected = new bool[2];
        public string[] collisionKeyword = new string[2];
        
        public List<GameObject> heldGamepieces = new List<GameObject>();

        /// <summary>
        /// Method is called whenever interactor collides with another object.
        /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and acts if so.
        /// </summary>
        /// <param name="other">The object the interactor collided with</param>
        /// <param name="manifoldList">List of collision manifolds--this isn't used</param>
        public override void BOnCollisionEnter(CollisionObject other, PersistentManifoldList manifoldList)
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
                            Debug.Log("does this happen");
                            collisionObject[i] = tempGamepiece;
                            collisionDetected[i] = true;
                            Debug.Log(collisionObject[i].ToString() + i.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method is called whenever interactor stops colliding with another object.
        /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and acts if so.
        /// </summary>
        /// <param name="other">The object the interactor stopped colliding with</param>
        public override void BOnCollisionExit(CollisionObject other)
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
    }
}