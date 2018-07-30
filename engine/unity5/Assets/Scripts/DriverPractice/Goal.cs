using UnityEngine;
using BulletSharp;
using BulletUnity;
using UnityEngine.UI;
using System.Xml.Serialization;
using Synthesis.Utils;

namespace Synthesis.DriverPractice
{
    /// <summary>
    /// Attached to the cube-shaped trigger colliders that represent goals.
    /// This script is responsible for making gamepieces disappear when they enter a goal, and for adding points to the score.
    /// </summary>
    public class Goal : BCollisionCallbacksDefault
    {

        public string color;
        public string gamepieceKeyword;
        public string description;

        public int pointValue;

        public Vector3 position;
        
        GameObject score;

        /// <summary>
        /// Method is called whenever interactor collides with another object.
        /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and destroys it if it does.
        /// </summary>
        /// <param name="other">The object the interactor collided with</param>
        /// <param name="manifoldList">List of collision manifolds--this isn't used</param>
        public override void BOnCollisionEnter(CollisionObject other, PersistentManifoldList manifoldList)
        {
            if (other.UserObject.ToString().Contains(gamepieceKeyword))
            {
                GameObject gamepieceObject = ((BRigidBody)other.UserObject).gameObject;

                if (gamepieceObject.GetComponent<BFixedConstraint>() == null)
                {
                    gamepieceObject.SetActive(false); // Destroying the gamepiece leads to issues if the gamepiece was the original.
                    UpdateScore();
                }
            }
            
        }

        /// <summary>
        /// Set the keyword that is used for checking whether an object that enters the collider is or is a clone of this goal's gamepiece.
        /// </summary>
        /// <param name="keyword"></param>
        public void SetKeyword(string keyword)
        {
            gamepieceKeyword = keyword;
        }
        private void UpdateScore()
        {
            if (color.Equals("Red"))
            {
                score = Auxiliary.FindObject(Auxiliary.FindObject(Auxiliary.FindObject("Canvas"), "ScorePanel"), "RedScoreText");
            }
            else
            {
                score = Auxiliary.FindObject(Auxiliary.FindObject(Auxiliary.FindObject("Canvas"), "ScorePanel"), "BlueScoreText");
            }
            score.GetComponent<Text>().text = (int.Parse(score.GetComponent<Text>().text.ToString()) + pointValue).ToString();
        }
    }
}