using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// This is a class that handles everything associated with the driver practice mode. 
/// It 'cheats physics' to overcome the limitations that our current simulation has to create a better
/// environment for drivers to practice and actually interact with game objects.
/// 
/// This currently only supports 2016, but adding functionality for other fields is possible with this class; just follow the code below as a template.
/// </summary>
public partial class DriverPracticeMode : MonoBehaviour {

    private GameObject[] interactorObjects; // This array is used to identify nodes as special interactors so that they can interact with frc game objects like balls, totes, etc.
    private Interactor[] interactors; // This array is used to make references to the interactor classes running in each interactorObject.
    private List<GameObject> ObjectsHeld = new List<GameObject>(); //This list contains all of the game objects that the robot is currently handling.
    private int holdingLimit; //The maximum number of game objects that this robot can hold at any given time. (Ex. 1 for 2016, or 6 or 7 for 2015)

    private int gameType; //The year of the game/field.


    /// <summary>
    /// Checks if the robot filepath supports driver practice mode.
    /// If you want a robot aside from the default 2557 robot to support driver practice mode,
    /// then you need to add it into here. (There will be a tutorial on how to add support)
    /// </summary>
    /// <param name="robotname">the name of the robot folder</param>
    /// <returns>if the robot supports driver practice mode.</returns>
    public static bool CheckRobot(string robotname)
    {
        //If you have added driver practice mode support for a robot, then add another if statement to return true for that robot.
        if (robotname.Equals("2557-2016")) return true;
        else if (robotname.Equals("Simbotics2012")) return true;
        else return false;
    }

    /// <summary>
    /// Runs the proper initialization methods based on what robot has been loaded.
    /// </summary>
    public void Initialize(string robotname)
    {
        if (robotname.Equals("2557-2016"))
        {
            InitSota2016();
        }
        else if (robotname.Equals("Simbotics2012"))
        {
            InitSimbotics2012();
        }
    }
        
	/// <summary>
    /// Runs through all inputs for special controls like rolling a ball in and shooting.
    /// Also deals with all the 'cheating physics' parts of the mode.
    /// </summary>
	void Update () {
        if (gameType == 2016) Update2016();
	}
    
    void FixedUpdate ()
    {
        if (gameType == 2016) FixedUpdate2016();
    }

    /// <summary>
    /// Waits .5 seconds before renabling collisions between the ball and the robot.
    /// </summary>
    IEnumerator UnIgnoreCollision(GameObject unignoredObject)
    {
        yield return new WaitForSeconds(0.5f);
        GameObject child;
        for (int i = 0; i < transform.childCount; i++)
        {
            for (int j = 0; j < transform.GetChild(i).childCount; j++)
            {
                child = transform.GetChild(i).GetChild(j).gameObject;
                if (child.name.Contains("ollider")) Physics.IgnoreCollision(child.collider, unignoredObject.collider,false);
            }
        }
    }
}
