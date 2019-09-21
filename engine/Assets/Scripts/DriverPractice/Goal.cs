using UnityEngine;
using BulletSharp;
using BulletUnity;
using UnityEngine.UI;
using System.Xml.Serialization;
using Synthesis.Utils;
using Synthesis.BUExtensions;
using System.Collections.Generic;

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
        public Vector3 rotation;
        public Vector3 scale; //vector that denotes the factor that the object stretches in width, height and length
        public bool KeepScored { get; private set; } = true;

        private List<GameObject> currentlyScoredObjects = new List<GameObject>();

        private Text score;

        /// <summary>
        /// Method is called whenever interactor collides with another object.
        /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and destroys it if it does.
        /// </summary>
        /// <param name="other">The object the interactor collided with</param>
        /// <param name="manifoldList">List of collision manifolds--this isn't used</param>
        public override void BOnCollisionEnter(CollisionObject other, PersistentManifoldList manifoldList)
        {
            base.BOnCollisionEnter(other, manifoldList);
            if (other.UserObject.ToString().Contains(gamepieceKeyword)) //.Contains() handles both gamepieces and their clones
            {
                GameObject gamepieceObject = ((BRigidBody)other.UserObject).gameObject;

                if (gamepieceObject.GetComponent<BFixedConstraintEx>() == null) //make sure gamepiece isn't held by robot
                {
                    if (!currentlyScoredObjects.Exists(i => i == gamepieceObject)) { // Only score if it isn't currently score
                        gamepieceObject.SetActive(KeepScored); // Destroying the gamepiece leads to issues if the gamepiece was the original.

                        //TODO: Find a way to get the point of collision between the goal and gamepiece and store it in closestPoint.
                        Vector3 closestPoint = getClosestPoint(gameObject.GetComponent<MeshFilter>().mesh.vertices, gamepieceObject.GetComponent<MeshFilter>().mesh.vertices);
                        
                        BBallSocketConstraintEx constraint = gamepieceObject.AddComponent<BBallSocketConstraintEx>();
                        constraint.PivotInA = gamepieceObject.transform.InverseTransformVector(closestPoint).ToBullet();
                        constraint.constraintType = BTypedConstraint.ConstraintType.constrainToPointInSpace;

                        currentlyScoredObjects.Add(gamepieceObject); // Track it
                        UpdateScore(); //change red or blue score
                    }
                }
            }
        }

        public override void BOnCollisionExit(CollisionObject other)
        {
            base.BOnCollisionExit(other);
            if (other.UserObject.ToString().Contains(gamepieceKeyword)){
                GameObject gamepieceObject = ((BRigidBody)other.UserObject).gameObject;
                if (gamepieceObject.activeSelf)
                {
                    currentlyScoredObjects.Remove(currentlyScoredObjects.Find(i => i == gamepieceObject)); // Remove objects that leave the scoring zone
                }
            }
        }

        public void SetKeepScored(bool value)
        {
            if(KeepScored != value)
            {
                KeepScored = value;
                foreach (var i in currentlyScoredObjects)
                {
                    i.SetActive(value);
                }
            }
        }

        private float distance3D(Vector3 pointA, Vector3 pointB)
        {
            float a = Mathf.Pow(pointA.x - pointB.x, 3);
            float b = Mathf.Pow(pointA.y - pointB.y, 3);
            float c = Mathf.Pow(pointA.z - pointB.z, 3);
            return Mathf.Pow(a + b + c, 1 / 3);
        }

        public Vector3 getClosestPoint(Vector3[] meshA, Vector3[] meshB)
        {
            Vector3 suspectA = meshA[0], suspectB = meshB[0];
            float closestDist = distance3D(suspectA, suspectB);

            float tempVar;
            for (int a = 0; a < meshA.Length; a++)
            {
                for (int b = 1; b < meshB.Length; b++)
                {
                    tempVar = distance3D(meshA[a], meshB[b]);
                    if (tempVar < closestDist)
                    {
                        closestDist = tempVar;
                        suspectA = meshA[a];
                        suspectB = meshB[b];
                    }
                }
            }

            float x = (suspectA.x + suspectB.x) / 2;
            float y = (suspectA.y + suspectB.y) / 2;
            float z = (suspectA.z + suspectB.z) / 2;
            return new Vector3(x, y, z); // Returns midpoint between the 2 closest points it could find
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