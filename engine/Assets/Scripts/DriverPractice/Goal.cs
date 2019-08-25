﻿using UnityEngine;
using BulletSharp;
using BulletUnity;
using UnityEngine.UI;
using System.Xml.Serialization;
using Synthesis.Utils;
using Synthesis.BUExtensions;

namespace Synthesis.DriverPractice
{
    /// <summary>
    /// Attached to the cube-shaped trigger colliders that represent goals.
    /// This script is responsible for making gamepieces disappear when they enter a goal, and for adding points to the score.
    /// </summary>
    public class Goal : BCollisionCallbacksDefault
    {
        public string color; //team color that is associated with goal "Red" or "Blue"
        public string gamepieceKeyword; //gamepiece name used for collision detection
        public string description; //goal name - only for the user
        public int pointValue; //nominal point value
        public Vector3 position; //vector location of the object in world space
        public Vector3 scale; //vector that denotes the factor that the object stretches in width, height and length

        Text score;

        /// <summary>
        /// Method is called whenever interactor collides with another object.
        /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and destroys it if it does.
        /// </summary>
        /// <param name="other">The object the interactor collided with</param>
        /// <param name="manifoldList">List of collision manifolds--this isn't used</param>
        public override void BOnCollisionEnter(CollisionObject other, PersistentManifoldList manifoldList)
        {
            if (other.UserObject.ToString().Contains(gamepieceKeyword)) //.Contains() handles both gamepieces and their clones
            {
                GameObject gamepieceObject = ((BRigidBody)other.UserObject).gameObject;

                if (gamepieceObject.GetComponent<BFixedConstraintEx>() == null) //make sure gamepiece isn't held by robot
                {
                    gamepieceObject.SetActive(false); // Destroying the gamepiece leads to issues if the gamepiece was the original.
                    UpdateScore(); //change red or blue score
                }
            }
        }
        /// <summary>
        /// Increment score by point value
        /// </summary>
        private void UpdateScore()
        {
            if (score == null) score = Auxiliary.FindObject(Auxiliary.FindObject(Auxiliary.FindObject("Canvas"), "ScorePanel"), color + "ScoreText").GetComponent<Text>();
            score.text = (int.Parse(score.GetComponent<Text>().text.ToString()) + pointValue).ToString();
        }
    }
}