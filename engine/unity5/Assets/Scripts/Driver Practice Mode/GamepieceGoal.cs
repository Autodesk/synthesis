using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using BulletSharp.Math;
using BulletUnity;
using System.IO;
using System.Text;
using BulletUnity.Debugging;
using System.Linq;

/// <summary>
/// Attached to the cube-shaped trigger colliders that represent goals.
/// This script is responsible for making gamepieces disappear when they enter a goal, and for adding points to the score.
/// </summary>
public class DriverPracticeGoal : BCollisionCallbacksDefault
{
    public DriverPracticeRobot dpmRobot;
    public Scoreboard scoreboard;

    public string gamepieceKeyword;

    public int pointValue;
    public string description;

    /// <summary>
    /// Method is called whenever interactor collides with another object.
    /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and destroys it if it does.
    /// </summary>
    /// <param name="other">The object the interactor collided with</param>
    /// <param name="manifoldList">List of collision manifolds--this isn't used</param>
    public override void BOnCollisionEnter(CollisionObject other, PersistentManifoldList manifoldList)
    {
        if (dpmRobot.modeEnabled)
        {
            string gamepiece;
            if (gamepieceKeyword != null)
            {
                gamepiece = gamepieceKeyword;

                if (other.UserObject.ToString().Contains(gamepiece))
                {
                    GameObject gamepieceObject = ((BRigidBody)other.UserObject).gameObject;

                    gamepieceObject.SetActive(false); // Destroying the gamepiece leads to issues if the gamepiece was the original.
                    
                    scoreboard.AddPoints(pointValue, description);
                }
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
}
