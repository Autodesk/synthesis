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


public class DriverPracticeGoal : BCollisionCallbacksDefault
{
    public DriverPracticeRobot DPRobot;

    public string gamepieceKeyword;

    /// <summary>
    /// Method is called whenever interactor collides with another object.
    /// Checks if the name of the other object contains the keyword of the gamepiece we are looking for and destroys it if it does.
    /// </summary>
    /// <param name="other">The object the interactor collided with</param>
    /// <param name="manifoldList">List of collision manifolds--this isn't used</param>
    public override void BOnCollisionEnter(CollisionObject other, PersistentManifoldList manifoldList)
    {
        if (DPRobot.modeEnabled)
        {
            string gamepiece;
            if (gamepieceKeyword != null)
            {
                gamepiece = gamepieceKeyword;

                if (other.UserObject.ToString().Contains(gamepiece))
                {
                    GameObject gamepieceObject = ((BRigidBody)other.UserObject).gameObject;

                    gamepieceObject.SetActive(false);
                }
            }
        }
    }
    
    public void SetKeyword(string keyword)
    {
        gamepieceKeyword = keyword;
    }
}
