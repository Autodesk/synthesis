using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// This is a class that handles everything associated with the driver practice mode. 
/// It 'cheats physics' to overcome the limitations that our current simulation has to create a better
/// environment for drivers to practice and actually interact with game objects.
/// 
/// This currently only supports fields 2012, 2013, 2014, and 2016. Making it work for other games is simple. Just follow the existing templates.
/// </summary>
public partial class DriverPracticeMode : MonoBehaviour {

    private List<GameObject> interactorObjects; // This array is used to identify nodes as special interactors so that they can interact with frc game objects like balls, totes, etc.
    private List<Interactor> interactors; // This array is used to make references to the interactor classes running in each interactorObject.
    private List<Vector3> vectors;
    private List<string> gamepieces;
    private List<GameObject> ObjectsHeld = new List<GameObject>(); //This list contains all of the game objects that the robot is currently handling.
    private List<GameObject> spawnedGamepieces = new List<GameObject>();
    private int holdingLimit; //The maximum number of game objects that this robot can hold at any given time. (Ex. 1 for 2016, or 6 or 7 for 2015)

    private int gameType; //The year of the game/field.

    public bool isConfiguring = false;

    public RobotConfiguration robotConfig;

    private const float VECTOR_CONST = .1f;

    /// <summary>
    /// Checks if the robot filepath supports driver practice mode.
    /// If you want a robot aside from the default 2557 robot to support driver practice mode,
    /// then you need to add it into here. (There will be a tutorial on how to add support)
    /// </summary>
    /// <param name="robotpath"> The path of the robot folder.</param>
    /// <returns>if the robot is already configured for driver practice mode.</returns>
    public static bool CheckRobot(string robotpath)
    {
        //If you have added driver practice mode support for a robot, then add another if statement to return true for that robot.
        if (File.Exists(robotpath)) return true;
        else return false;
    }

    /// <summary>
    /// Runs the proper initialization methods based on what robot has been loaded.
    /// </summary>
    public void Initialize()
    {
        holdingLimit = 1;
        interactorObjects = new List<GameObject>();
        interactors = new List<Interactor>();
        vectors = new List<Vector3>();
        gamepieces = new List<string>();
        if (File.Exists(PlayerPrefs.GetString("dpmSelectedRobot") + "\\dpmConfiguration.txt"))
        {
            StreamReader reader = new StreamReader(PlayerPrefs.GetString("dpmSelectedRobot") + "\\dpmConfiguration.txt");
            string line = "";
            int counter = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals("#Nodes")) counter++;
                else if (counter == 1)
                {
                    if (line.Equals("#Vectors")) counter++;
                    else
                    {
                        interactorObjects.Add(GameObject.Find(line));
                    }
                }
                else if (counter == 2)
                {
                    if (line.Equals("#Gamepieces")) counter++;
                    else vectors.Add(RobotConfiguration.DeserializeVector3Array(line));
                }
                else if (counter == 3)
                {
                    gamepieces.Add(line);
                }
            }
            reader.Close();

            for (int i = 0; i < interactorObjects.Count; i++)
            {
                interactors.Add(interactorObjects[i].AddComponent<Interactor>());
                interactors[i].collisionKeyword = gamepieces[0];
            }

            line = "";
            counter = 0;
            reader = new StreamReader(PlayerPrefs.GetString("dpmSelectedField") + "\\driverpracticemode.txt");

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals("#Holdinglimit")) counter++;
                if (counter == 1) holdingLimit = Mathf.RoundToInt(float.Parse(line));
            }
            reader.Close();
        }



        if (isConfiguring)
        {
            robotConfig = GameObject.Find("RobotConfiguration").GetComponent<RobotConfiguration>();
            gamepieces.Clear();
            gamepieces.Add("gamepiece");
            UpdateConfiguration();
        }
    }
        
	/// <summary>
    /// Runs through all inputs for special controls like rolling a ball in and shooting.
    /// Also deals with all the 'cheating physics' parts of the mode.
    /// </summary>
	void Update () {
        if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.Release]))
            ReleaseGamepiece();

        if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.Spawn]))
            SpawnGamepiece();
	}
    
    void FixedUpdate ()
    {
        if (Input.GetKey(Controls.ControlKey[(int)Controls.Control.Pickup]))
            Intake();

        HoldGamepiece();
    }

    private void Intake()        
    {
        if (ObjectsHeld.Count < holdingLimit && interactors[0].getDetected())
        {
            for (int i = 0; i < ObjectsHeld.Count; i++) if (ObjectsHeld[i].Equals(interactors[0].getObject())) return;
            //The code iterates through every single collider of the robot to ignore collisions with them and the gamepiece.
            ObjectsHeld.Add(interactors[0].getObject());
            GameObject newObject = interactors[0].getObject();
            GameObject child;
            for (int i = 0; i < transform.childCount; i++)
            {
                for (int j = 0; j < transform.GetChild(i).childCount; j++)
                {
                    child = transform.GetChild(i).GetChild(j).gameObject;
                    if (child.name.Contains("ollider")) Physics.IgnoreCollision(child.collider, newObject.collider, true);
                }
            }
            for (int i = 0; i < ObjectsHeld.Count - 1; i++)
            {
                Physics.IgnoreCollision(newObject.collider, ObjectsHeld[i].collider, true);
            }
        }
    }

    private void HoldGamepiece()
    {
        if (ObjectsHeld.Count > 0)
        {
            for (int i = 0; i < ObjectsHeld.Count; i++)
            {
                ObjectsHeld[i].rigidbody.velocity = interactorObjects[1].rigidbody.velocity;
                ObjectsHeld[i].rigidbody.position = interactorObjects[1].rigidbody.position + (interactorObjects[1].rigidbody.rotation * vectors[1] * VECTOR_CONST);
                ObjectsHeld[i].rigidbody.freezeRotation = true;
            }
        }
    }

    private void ReleaseGamepiece()
    {
        if (ObjectsHeld.Count > 0)
        {
            ObjectsHeld[0].rigidbody.AddForce(interactorObjects[1].rigidbody.rotation * vectors[0] * VECTOR_CONST);
            ObjectsHeld[0].rigidbody.freezeRotation = false;
            StartCoroutine(UnIgnoreCollision(ObjectsHeld[0]));
            ObjectsHeld.RemoveAt(0);
        }
        Debug.Log(ObjectsHeld.Count);
    }

    private void SpawnGamepiece()
    {
        if (gamepieces.Count > 0)
        {
            spawnedGamepieces.Add((GameObject)Instantiate(auxFunctions.FindObject(gamepieces[0]), new Vector3(0,3,0), Quaternion.identity));
        }
    }

    public void UpdateConfiguration()
    {
        interactorObjects.Clear();
        foreach (Interactor interactor in interactors)
        {
            Destroy(interactor);
        }
        interactors.Clear();
        vectors.Clear();

        int counter = 0;
        try
        {
            foreach (string node in robotConfig.nodeSave)
            {
                interactorObjects.Add(GameObject.Find(node));
                interactors.Add(interactorObjects[counter].AddComponent<Interactor>());
                interactors[counter].collisionKeyword = gamepieces[0];
                counter++;
            }
        }
        catch
        {
            Debug.Log(robotConfig.nodeSave.Count);
        }
        counter = 0;
        foreach (Vector3 vector in robotConfig.vectorSave)
        {
            vectors.Add(vector);
        }
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
