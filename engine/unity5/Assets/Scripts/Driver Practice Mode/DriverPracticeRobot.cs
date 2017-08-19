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
using Assets.Scripts.FSM;

/// <summary>
/// To be added to all robots, this class 'cheats physics' to overcome the limitations that our current simulation has to create a beter environment for drivers to practice and interact with game objects.
/// 
/// </summary>
public class DriverPracticeRobot : MonoBehaviour
{
    public UnityEngine.Vector3[] positionOffset; //position offset vectors for gamepiece while its being held
    public List<float[]> releaseVelocity; //release velocity vectors for gamepiece, defined not in x,y,z coordinates, but speed, hor angle, and ver angle.
    public float[] primaryVelocity = new float[3];
    public float[] secondaryVelocity = new float[3];

    public List<UnityEngine.Vector3> releaseVelocityVector;

    public List<GameObject> intakeNode; //node that is identified for intaking gamepieces
    public List<GameObject> releaseNode; //node that is identified for holding/releasing gamepieces
    private List<Interactor> intakeInteractor;

    public List<int> holdingLimit; //the maximum number of game objects that this robot can hold at any given time.

    public List<List<GameObject>> objectsHeld; //list of gamepieces this robot is currently holding
    public List<GameObject> primaryHeld;
    public List<GameObject> secondaryHeld;

    public List<string> gamepieceNames; // list of the identifiers of gamepieces
    public List<GameObject> gamepieceOriginals; // list of the gameobjects of gamepieces
    public List<List<GameObject>> spawnedGamepieces;
    public List<GameObject> spawnedPrimary;
    public List<GameObject> spawnedSecondary;

    public List<List<GameObject>> gamepieceGoalObjects; // List of objects with trigger colliders representing gamepiece goals
    public List<GameObject> gamepieceGoalObjectsPrimary; // Goals stored in list for future possiblity of multiple goals per gamepiece
    public List<GameObject> gamepieceGoalObjectsSecondary;

    public List<bool> displayTrajectories; //projects gamepiece trajectories if true
    private List<LineRenderer> drawnTrajectory;

    public bool modeEnabled = false;

    private int configuringIndex = 0;
    private int processingIndex = 0; //we use this to alternate which index is processed first.

    public bool addingGamepiece = false; //true when user is currently selecting a gamepiece to be added.
    public bool definingIntake = false; // true when user is currently selecting a robot part for the intake mechanism
    public bool definingRelease = false; //true when user is currently selecting a robot part for the release mechanism

    //for highlight current mechanism features
    private GameObject highlightedNode;
    private List<Color> originalColors = new List<Color>();
    private Color highlightColor = new Color(1, 1, 0, 0.1f);
    private int highlightTimer = -1;

    //for defining mechanism features
    private GameObject hoveredNode;
    private List<Color> hoveredColors = new List<Color>();
    private Color hoverColor = new Color(1, 1, 0, 0.1f);

    //for gamepiece spawn and goal customizability
    private Scoreboard scoreboard; // Given to newly created goals for adding points
    private List<UnityEngine.Vector3> gamepieceSpawn;
    private List<List<UnityEngine.Vector3>> gamepieceGoals;
    private List<List<float>> gamepieceGoalSizes;
    private List<List<int>> gamepieceGoalPoints;
    private List<List<string>> gamepieceGoalDesc;
    private GameObject spawnIndicator;
    private GameObject goalIndicator;
    public int settingSpawn = 0; //0 if not, 1 if editing primary, and 2 if editing secondary
    public int settingGamepieceGoal = 0; //0 if not, 1 if editing primary, and 2 if editing secondary
    public int settingGamepieceGoalIndex = 0; // Index of goal being edited
    public bool settingGamepieceGoalVertical = false;
    private DynamicCamera.CameraState lastCameraState;

    /// <summary>
    /// If configuration file exists, loads information and auto-configures robot.
    /// If coniguration file doesn't exist, initializes variables for users to configure.
    /// 
    /// Also loads gamepiece list from MainState.cs.
    /// 
    /// *NOTE: Because gamepiece identification in the new field format doesn't exist yet, we are using a predetermined gamepiece list. This must be changed as soon as support for gamepieces is added in the field exporter.*
    /// </summary>
    public void Initialize(string robotDirectory)
    {
        //Initializes all the configurable values and assigns them a default value.
        positionOffset = new UnityEngine.Vector3[2];
        positionOffset[0] = UnityEngine.Vector3.zero;
        positionOffset[1] = UnityEngine.Vector3.zero;

        releaseVelocity = new List<float[]>();
        releaseVelocity.Add(primaryVelocity);
        releaseVelocity.Add(secondaryVelocity);

        releaseVelocityVector = new List<UnityEngine.Vector3>();
        releaseVelocityVector.Add(UnityEngine.Vector3.zero);
        releaseVelocityVector.Add(UnityEngine.Vector3.zero);

        intakeNode = new List<GameObject>();
        intakeNode.Add(transform.GetChild(0).gameObject); //We want these to be null so that the user must configure it to a node first.
        intakeNode.Add(transform.GetChild(0).gameObject);

        releaseNode = new List<GameObject>();
        releaseNode.Add(transform.GetChild(0).gameObject);
        releaseNode.Add(transform.GetChild(0).gameObject);

        intakeInteractor = new List<Interactor>();
        intakeInteractor.Add(null);
        intakeInteractor.Add(null);

        objectsHeld = new List<List<GameObject>>();
        primaryHeld = new List<GameObject>();
        secondaryHeld = new List<GameObject>();
        objectsHeld.Add(primaryHeld);
        objectsHeld.Add(secondaryHeld);
        
        gamepieceNames = new List<string>();
        gamepieceNames.Add("NOT CONFIGURED");
        gamepieceNames.Add("NOT CONFIGURED");

        gamepieceOriginals = new List<GameObject>();
        gamepieceOriginals.Add(null);
        gamepieceOriginals.Add(null);

        spawnedGamepieces = new List<List<GameObject>>();
        spawnedPrimary = new List<GameObject>();
        spawnedSecondary = new List<GameObject>();
        spawnedGamepieces.Add(spawnedPrimary);
        spawnedGamepieces.Add(spawnedSecondary);

        gamepieceGoalObjects = new List<List<GameObject>>();
        gamepieceGoalObjectsPrimary = new List<GameObject>();
        gamepieceGoalObjectsSecondary = new List<GameObject>();
        gamepieceGoalObjects.Add(gamepieceGoalObjectsPrimary);
        gamepieceGoalObjects.Add(gamepieceGoalObjectsSecondary);

        holdingLimit = new List<int>();
        holdingLimit.Add(30);
        holdingLimit.Add(30);

        SetInteractor(intakeNode[0], 0);
        SetInteractor(intakeNode[1], 1);

        gamepieceSpawn = new List<UnityEngine.Vector3>();
        gamepieceSpawn.Add(new UnityEngine.Vector3(0f, 3f, 0f));
        gamepieceSpawn.Add(new UnityEngine.Vector3(0f, 3f, 0f));

        gamepieceGoals = new List<List<UnityEngine.Vector3>>();
        gamepieceGoals.Add(new List<UnityEngine.Vector3>());
        gamepieceGoals.Add(new List<UnityEngine.Vector3>());
        
        gamepieceGoalSizes = new List<List<float>>();
        gamepieceGoalSizes.Add(new List<float>());
        gamepieceGoalSizes.Add(new List<float>());

        gamepieceGoalPoints = new List<List<int>>();
        gamepieceGoalPoints.Add(new List<int>());
        gamepieceGoalPoints.Add(new List<int>());

        gamepieceGoalDesc = new List<List<string>>();
        gamepieceGoalDesc.Add(new List<string>());
        gamepieceGoalDesc.Add(new List<string>());

        drawnTrajectory = new List<LineRenderer>();
        drawnTrajectory.Add(gameObject.AddComponent<LineRenderer>());
        GameObject secondLine = new GameObject();
        drawnTrajectory.Add(secondLine.AddComponent<LineRenderer>());
        foreach (LineRenderer line in drawnTrajectory)
        {
            line.startWidth = 0.2f;
            line.material = Resources.Load("Materials/Projection") as Material;
            line.enabled = false;
        }
        drawnTrajectory[0].startColor = Color.blue;
        drawnTrajectory[0].endColor = Color.cyan;
        drawnTrajectory[1].startColor = Color.red;
        drawnTrajectory[1].endColor = Color.magenta;

        displayTrajectories = new List<bool>();
        displayTrajectories.Add(false);
        displayTrajectories.Add(false);

        Load(robotDirectory);

        GenerateGamepieceGoalColliders(0);
        GenerateGamepieceGoalColliders(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (modeEnabled)
        {
            ProcessControls();

            if (Input.GetMouseButtonDown(0))
            {
                if (addingGamepiece) SetGamepiece(configuringIndex);
                else if (definingIntake || definingRelease) SetMechanism(configuringIndex);
            }

            if (definingIntake || definingRelease) SelectingNode();


            if (highlightTimer > 0) highlightTimer--;
            else if (highlightTimer == 0) RevertHighlight();

            if (settingSpawn != 0) UpdateGamepieceSpawn();
            if (settingGamepieceGoal != 0) UpdateGamepieceGoal();
        }

        for (int i = 0; i < 2; i++)
        {
            if (displayTrajectories[i] && StateMachine.Instance.CurrentState is MainState)
            {
                releaseVelocityVector[i] = VelocityToVector3(releaseVelocity[i][0], releaseVelocity[i][1], releaseVelocity[i][2]);
                if (!drawnTrajectory[i].enabled) drawnTrajectory[i].enabled = true;
                DrawTrajectory(releaseNode[i].transform.position + releaseNode[i].GetComponent<BRigidBody>().transform.rotation * positionOffset[i], releaseNode[i].GetComponent<BRigidBody>().velocity + releaseNode[i].transform.rotation * releaseVelocityVector[i], drawnTrajectory[i]);
            }
            else
            {
                if (drawnTrajectory[i].enabled) drawnTrajectory[i].enabled = false;
            }
        }
    }

    private void OnGUI()
    {
    }

    #region Gamepiece Manipulation Functions
    /// <summary>
    /// If the robot's intake node is touching an gamepiece, make the robot 'intake' it by adding it to the list of held objects and cheats physics by disabling collisions on the gamepiece.
    /// </summary>
    void Intake(int index)
    {
        if (objectsHeld[index].Count < holdingLimit[index] && intakeInteractor[index].GetDetected(index))
        {
            for (int i = 0; i < objectsHeld[0].Count; i++)
            {
                if (objectsHeld[0][i].Equals(intakeInteractor[0].GetObject(index))) return; //This makes sure the object the robot is touching isn't an object already being held.
            }
            for (int i = 0; i < objectsHeld[1].Count; i++)
            {
                if (objectsHeld[1][i].Equals(intakeInteractor[1].GetObject(index))) return; //This makes sure the object the robot is touching isn't an object already being held.
            }
            GameObject newObject = intakeInteractor[index].GetObject(index);
            objectsHeld[index].Add(newObject);
            newObject.GetComponent<BRigidBody>().velocity = UnityEngine.Vector3.zero;
            newObject.GetComponent<BRigidBody>().angularVelocity = UnityEngine.Vector3.zero;
            newObject.GetComponent<BRigidBody>().collisionFlags = BulletSharp.CollisionFlags.NoContactResponse;

            intakeInteractor[index].heldGamepieces.Add(newObject);


            foreach (BRigidBody rb in this.GetComponentsInChildren<BRigidBody>())
            {
                newObject.GetComponent<BRigidBody>().GetCollisionObject().SetIgnoreCollisionCheck(rb.GetCollisionObject(), true);
            }
        }
    }

    /// <summary>
    /// Binds every gamepiece the robot is holding to the proper node and its position.
    /// </summary>
    private void HoldGamepiece(int index)
    {

        BRigidBody nrb; //rigid body of the release node
        nrb = releaseNode[index].GetComponent<BRigidBody>();

        if (objectsHeld[index].Count > 0)
        {
            BRigidBody orb; //rigid body of the object


            for (int i = 0; i < objectsHeld[index].Count; i++)
            {
                orb = objectsHeld[index][i].GetComponent<BRigidBody>();
                orb.velocity = nrb.velocity;
                orb.SetPosition(nrb.transform.position + nrb.transform.rotation * positionOffset[index]);
                orb.angularVelocity = UnityEngine.Vector3.zero;
                orb.angularFactor = UnityEngine.Vector3.zero;

            }
        }
    }

    /// <summary>
    /// Releases the gamepiece from the robot at a set velocity
    /// </summary>
    private void ReleaseGamepiece(int index)
    {
        if (objectsHeld[index].Count > 0)
        {
            BRigidBody orb = objectsHeld[index][0].GetComponent<BRigidBody>();
            orb.collisionFlags = BulletSharp.CollisionFlags.None;
            orb.velocity += releaseNode[index].transform.rotation * releaseVelocityVector[index];
            orb.angularFactor = UnityEngine.Vector3.one;
            StartCoroutine(UnIgnoreCollision(objectsHeld[index][0]));
            intakeInteractor[index].heldGamepieces.Remove(objectsHeld[index][0]);
            objectsHeld[index].RemoveAt(0);
        }
    }

    /// <summary>
    /// Waits .5 seconds before renabling collisions between the release gamepiece and the robot.
    /// </summary>
    IEnumerator UnIgnoreCollision(GameObject obj)
    {
        yield return new WaitForSeconds(0.5f);

        foreach (BRigidBody rb in this.GetComponentsInChildren<BRigidBody>())
        {
            obj.GetComponent<BRigidBody>().GetCollisionObject().SetIgnoreCollisionCheck(rb.GetCollisionObject(), false);
        }
    }



    private UnityEngine.Vector3 VelocityToVector3(float speed, float horAngle, float verAngle)
    {
        UnityEngine.Quaternion horVector;
        UnityEngine.Quaternion verVector;
        UnityEngine.Vector3 finalVector = UnityEngine.Vector3.zero;

        //finalVector.x = speed * Mathf.Cos(horAngle * Mathf.Deg2Rad);
        //finalVector.y = speed * Mathf.Sin(verAngle * Mathf.Deg2Rad);
        //finalVector.z = Mathf.Sqrt(speed * speed - finalVector.y * finalVector.y - finalVector.x * finalVector.x);
        horVector = UnityEngine.Quaternion.AngleAxis(horAngle, UnityEngine.Vector3.up);
        verVector = UnityEngine.Quaternion.AngleAxis(verAngle, UnityEngine.Vector3.right);

        UnityEngine.Quaternion rotation = UnityEngine.Quaternion.Euler(verAngle, horAngle, 0);

        finalVector = (UnityEngine.Quaternion.LookRotation(UnityEngine.Vector3.forward, UnityEngine.Vector3.up) * horVector * verVector) * UnityEngine.Vector3.forward * speed;

        return (finalVector);

    }

    /// <summary>
    /// Illustrates the trajectory a released gamepiece would follow.
    /// Does this by creating rendering lines bounded to several vertices positioned based on multiplying velocity, gravity, and time.
    /// </summary>
    /// <param name="position">starting position of the gamepiece</param>
    /// <param name="velocity">starting velocity of the gamepiece</param>
    void DrawTrajectory(UnityEngine.Vector3 position, UnityEngine.Vector3 velocity, LineRenderer line)
    {
        int verts = 100; //This determines how far along time the illustration goes.
        line.positionCount = verts;

        UnityEngine.Vector3 pos = position;
        UnityEngine.Vector3 vel = velocity;
        UnityEngine.Vector3 grav = GameObject.Find("BulletPhysicsWorld").GetComponent<BPhysicsWorld>().gravity;
        for (int i = 0; i < verts; i++)
        {
            line.SetPosition(i, pos);
            vel = vel + grav * Time.fixedDeltaTime;
            pos = pos + vel * Time.fixedDeltaTime;
        }
    }
    #endregion

    #region Configuring Gamepiece

    /// <summary>
    /// Allows the user to select a dynamic object with their mouse and add it to the list of gamepieces.
    /// </summary>
    public void SetGamepiece(int index)
    {
        //Casts a ray from the camera in the direction the mouse is in and returns the closest object hit
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        BulletSharp.Math.Vector3 start = ray.origin.ToBullet();
        BulletSharp.Math.Vector3 end = ray.GetPoint(200).ToBullet();

        //Creates a callback result that will be updated if we do a ray test with it
        ClosestRayResultCallback rayResult = new ClosestRayResultCallback(ref start, ref end);

        //Remove goal colliders to prevent accidentally raycasting to them
        DestroyGamepieceGoalColliders(0);
        DestroyGamepieceGoalColliders(1);

        //Retrieves the bullet physics world and does a ray test with the given coordinates and updates the callback object
        BPhysicsWorld world = BPhysicsWorld.Get();
        world.world.RayTest(start, end, rayResult);

        //If there is a collision object and it is dynamic and not a robot part, change the gamepiece to that
        if (rayResult.CollisionObject != null)
        {
            string name = (rayResult.CollisionObject.UserObject.ToString().Replace(" (BulletUnity.BRigidBody)", ""));
            Debug.Log(name);
            if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
            {
                UserMessageManager.Dispatch("The gamepiece must be a dynamic object!", 3);
            }
            else if (GameObject.Find(name) == null)
            {
                Debug.Log("DPM: Game object not found");
            }
            else if (GameObject.Find(name).transform.parent == transform)
            {
                UserMessageManager.Dispatch("You cannot select a robot part as a gamepiece!", 3);
            }
            else if (name.Contains("Gamepiece") && name.Contains("Goal"))
            {
                Debug.Log("DPM: Raycast seems to have hit a goal collider.");
            }
            else
            {
                gamepieceNames[index] = name.Replace("(Clone)", ""); //gets rid of the clone tag given to spawned gamepieces 
                gamepieceOriginals[index] = GameObject.Find(name);
                intakeInteractor[index].SetKeyword(gamepieceNames[index], index);
                GameObject gamepiece = GameObject.Find(name);

                UserMessageManager.Dispatch(name + " has been selected as the gamepiece", 2);
                addingGamepiece = false;
            }
        }
        else
        {

        }
        
        // Regenerate goal colliders
        GenerateGamepieceGoalColliders(0);
        GenerateGamepieceGoalColliders(1);
    }

    public void DefineGamepiece(int index)
    {
        if (modeEnabled)
        {
            if (definingIntake || definingRelease) UserMessageManager.Dispatch("You must select a robot part first!", 5);
            else if (settingSpawn != 0) UserMessageManager.Dispatch("You must set the gamepiece spawnpoint first! Press enter to save your the current position", 5);
            else
            {
                configuringIndex = index;
                addingGamepiece = true;
            }
        }
    }

    /// <summary>
    /// Spawns a new gamepiece at its defined spawn location, or at the field's origin if one hasn't been defined.
    /// </summary>
    /// <param name="index">0 if primary gamepiece, 1 if secondary gamepiece</param>
    public void SpawnGamepiece(int index)
    {
        if (gamepieceOriginals[index] != null)
        {
            try //In case the game piece somehow doens't exist in the scene
            {
                // Enable original gamepiece for cloning, then return to original state
                bool originalActiveState = gamepieceOriginals[index].activeSelf;
                gamepieceOriginals[index].SetActive(true);
                GameObject gameobject = Instantiate(gamepieceOriginals[index].GetComponentInParent<BRigidBody>().gameObject, gamepieceSpawn[index], UnityEngine.Quaternion.identity);
                gamepieceOriginals[index].SetActive(originalActiveState);

                gameobject.GetComponent<BRigidBody>().collisionFlags = BulletSharp.CollisionFlags.None;
                gameobject.GetComponent<BRigidBody>().velocity = UnityEngine.Vector3.zero;
                spawnedGamepieces[index].Add(gameobject);
            }
            catch
            {
                UserMessageManager.Dispatch("Gamepiece not found!", 5);
            }
        }
        else UserMessageManager.Dispatch("You must define the gamepiece first!", 5);
    }

    /// <summary>
    /// Clears all the gamepieces sharing the same name as the ones that have been configured from the field.
    /// </summary>
    public void ClearGamepieces()
    {
        for (int i = 0; i < spawnedGamepieces.Count; i++)
        {
            foreach (GameObject g in spawnedGamepieces[i])
            {
                Destroy(g);
            }
        }
    }

    public void StartGamepieceSpawn(int index)
    {
        if (definingRelease || definingIntake || addingGamepiece || settingGamepieceGoal != 0) Debug.Log("User Error"); //Message Manager already dispatches error message to user
        else if (settingSpawn == 0)
        {
            if (GameObject.Find(gamepieceNames[index]) != null)
            {
                if (spawnIndicator != null) Destroy(spawnIndicator);
                if (spawnIndicator == null)
                {
                    spawnIndicator = Instantiate(gamepieceOriginals[index].GetComponentInParent<BRigidBody>().gameObject, new UnityEngine.Vector3(0, 3, 0), UnityEngine.Quaternion.identity);
                    spawnIndicator.name = "SpawnIndicator";
                    Destroy(spawnIndicator.GetComponent<BRigidBody>());
                    if (spawnIndicator.transform.GetChild(0) != null) spawnIndicator.transform.GetChild(0).name = "SpawnIndicatorMesh";
                    Renderer render = spawnIndicator.GetComponentInChildren<Renderer>();
                    render.material.shader = Shader.Find("Transparent/Diffuse");
                    Color newColor = render.material.color;
                    newColor.a = 0.6f;
                    render.material.color = newColor;
                }
                spawnIndicator.transform.position = gamepieceSpawn[index];
                settingSpawn = index + 1;

                DynamicCamera dynamicCamera = Camera.main.transform.GetComponent<DynamicCamera>();
                lastCameraState = dynamicCamera.cameraState;

                DynamicCamera.OrthographicSateliteState satellite = new DynamicCamera.OrthographicSateliteState(dynamicCamera);
                satellite.target = spawnIndicator;
                satellite.rotationVector = new UnityEngine.Vector3(90f, 90f, 0f);
                satellite.targetOffset = new UnityEngine.Vector3(0f, 6f, 0f);
                satellite.orthoSize = 4;

                dynamicCamera.SwitchCameraState(satellite);

                Robot.ControlsEnabled = false;
            }
            else UserMessageManager.Dispatch("You must define the gamepiece first!", 5f);
        }
        else FinishGamepieceSpawn(); //if already setting spawn, end editing process
    }

    private void UpdateGamepieceSpawn()
    {
        int index = settingSpawn - 1;
        if (spawnIndicator != null)
        {
            if (Input.GetKey(KeyCode.LeftArrow)) spawnIndicator.transform.position += UnityEngine.Vector3.forward * 0.05f;
            if (Input.GetKey(KeyCode.RightArrow)) spawnIndicator.transform.position += UnityEngine.Vector3.back * 0.05f;
            if (Input.GetKey(KeyCode.UpArrow)) spawnIndicator.transform.position += UnityEngine.Vector3.right * 0.05f;
            if (Input.GetKey(KeyCode.DownArrow)) spawnIndicator.transform.position += UnityEngine.Vector3.left * 0.05f;
            if (Input.GetKeyDown(KeyCode.Return))
            {
                UserMessageManager.Dispatch("New gamepiece spawn location has been set!", 3f);
                gamepieceSpawn[index] = spawnIndicator.transform.position;
                FinishGamepieceSpawn();
            }
        }
    }

    public void FinishGamepieceSpawn()
    {
        settingSpawn = 0;
        if (spawnIndicator != null) Destroy(spawnIndicator);
        if (lastCameraState != null)
        {
            DynamicCamera dynamicCamera = Camera.main.transform.GetComponent<DynamicCamera>();
            dynamicCamera.SwitchCameraState(lastCameraState);
            lastCameraState = null;
        }
        Robot.ControlsEnabled = true;
    }

    /// <summary>
    /// Initialize the goal display to show the descriptions and point values of the gamepiece's goals.
    /// </summary>
    /// <param name="gamepieceIndex">Gamepiece to get goal data from.</param>
    /// <param name="gm">Goal Manager to initialize the display of.</param>
    public void InitGoalManagerDisplay(int gamepieceIndex, GoalDisplayManager gm)
    {
        gm.InitializeDisplay(gamepieceGoalDesc[gamepieceIndex].ToArray(), gamepieceGoalPoints[gamepieceIndex].ToArray());
    }

    /// <summary>
    /// Add a new goal to a gamepiece.
    /// </summary>
    /// <param name="gamepieceIndex">The index of the gamepiece to add a goal to.</param>
    public void NewGoal(int gamepieceIndex)
    {
        if (GameObject.Find(gamepieceNames[gamepieceIndex]) != null)
        {
            gamepieceGoals[gamepieceIndex].Add(new UnityEngine.Vector3(0, 4, 0));
            gamepieceGoalSizes[gamepieceIndex].Add(1);
            gamepieceGoalPoints[gamepieceIndex].Add(0);
            gamepieceGoalDesc[gamepieceIndex].Add("New Goal");

            GenerateGamepieceGoalColliders(gamepieceIndex);
        }
        else UserMessageManager.Dispatch("You must define the gamepiece first!", 5f);
    }

    /// <summary>
    /// Delete a goal of a gamepiece.
    /// </summary>
    /// <param name="gamepieceIndex">The index of the gamepiece that owns the goal.</param>
    /// <param name="goalIndex">The index of the goal to be deleted.</param>
    public void DeleteGoal(int gamepieceIndex, int goalIndex)
    {
        if (GameObject.Find(gamepieceNames[gamepieceIndex]) != null)
        {
            if (goalIndex >= 0 && goalIndex < gamepieceGoals[gamepieceIndex].Count)
            {
                gamepieceGoals[gamepieceIndex].RemoveAt(goalIndex);
                gamepieceGoalSizes[gamepieceIndex].RemoveAt(goalIndex);
                gamepieceGoalPoints[gamepieceIndex].RemoveAt(goalIndex);
                gamepieceGoalDesc[gamepieceIndex].RemoveAt(goalIndex);

                GenerateGamepieceGoalColliders(gamepieceIndex);
            }
            else Debug.LogError("Cannot delete goal, does not exist!");
        }
        else UserMessageManager.Dispatch("You must define the gamepiece first!", 5f);
    }

    /// <summary>
    /// Set the description of a gamepiece's goal.
    /// </summary>
    /// <param name="gamepieceIndex">The index of the gamepiece that owns the goal.</param>
    /// <param name="goalIndex">The index of the goal to be configured.</param>
    /// <param name="description">New description of the goal.</param>
    public void SetGamepieceGoalDescription(int gamepieceIndex, int goalIndex, string description)
    {
        if (GameObject.Find(gamepieceNames[gamepieceIndex]) != null)
        {
            if (goalIndex >= 0 && goalIndex < gamepieceGoals[gamepieceIndex].Count)
            {
                gamepieceGoalDesc[gamepieceIndex][goalIndex] = description;
                GenerateGamepieceGoalColliders(gamepieceIndex);
            }
            else Debug.LogError("Cannot set goal description, goal does not exist!");
        }
        else UserMessageManager.Dispatch("You must define the gamepiece first!", 5f);
    }

    /// <summary>
    /// Set the point value of a gamepiece's goal.
    /// </summary>
    /// <param name="gamepieceIndex">The index of the gamepiece that owns the goal.</param>
    /// <param name="goalIndex">The index of the goal to be configured.</param>
    /// <param name="points">New point value of the goal.</param>
    public void SetGamepieceGoalPoints(int gamepieceIndex, int goalIndex, int points)
    {
        if (GameObject.Find(gamepieceNames[gamepieceIndex]) != null)
        {
            if (goalIndex >= 0 && goalIndex < gamepieceGoals[gamepieceIndex].Count)
            {
                gamepieceGoalPoints[gamepieceIndex][goalIndex] = points;
                GenerateGamepieceGoalColliders(gamepieceIndex);
            }
            else Debug.LogError("Cannot set goal points, goal does not exist!");
        }
        else UserMessageManager.Dispatch("You must define the gamepiece first!", 5f);
    }

    /// <summary>
    /// Begin the configuration of the size and position of a gamepiece's goal.
    /// </summary>
    /// <param name="gamepieceIndex">The index of the gamepiece that owns the goal.</param>
    /// <param name="goalIndex">The index of the goal to be configured.</param>
    public void StartGamepieceGoal(int gamepieceIndex, int goalIndex)
    {
        if (definingRelease || definingIntake || addingGamepiece || settingSpawn != 0) Debug.Log("User Error"); //Message Manager already dispatches error message to user
        else if (settingGamepieceGoal == 0)
        {
            if (GameObject.Find(gamepieceNames[gamepieceIndex]) != null)
            {
                if (goalIndex >= 0 && goalIndex < gamepieceGoals[gamepieceIndex].Count)
                {
                    if (goalIndicator != null) Destroy(goalIndicator);
                    if (goalIndicator == null)
                    {
                        goalIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube); // Create cube to show goal region
                        goalIndicator.name = "GoalIndicator";
                        Renderer render = goalIndicator.GetComponentInChildren<Renderer>();
                        render.material.shader = Shader.Find("Transparent/Diffuse");
                        Color newColor = new Color(0, 0.88f, 0, 0.6f);
                        render.material.color = newColor;
                    }
                    goalIndicator.transform.position = gamepieceGoals[gamepieceIndex][goalIndex];
                    goalIndicator.transform.localScale *= gamepieceGoalSizes[gamepieceIndex][goalIndex];
                    settingGamepieceGoal = gamepieceIndex + 1;
                    settingGamepieceGoalIndex = goalIndex;
                    settingGamepieceGoalVertical = false;

                    DynamicCamera dynamicCamera = Camera.main.transform.GetComponent<DynamicCamera>();
                    lastCameraState = dynamicCamera.cameraState;

                    DynamicCamera.OrthographicSateliteState satellite = new DynamicCamera.OrthographicSateliteState(dynamicCamera);
                    satellite.target = goalIndicator;
                    satellite.targetOffset = new UnityEngine.Vector3(0f, 6f, 0f);
                    satellite.rotationVector = new UnityEngine.Vector3(90f, 90f, 0f);
                    satellite.orthoSize = 4;
                    dynamicCamera.SwitchCameraState(satellite);

                    Robot.ControlsEnabled = false;
                }
                else Debug.LogError("Goal does not exist!");
            }
            else UserMessageManager.Dispatch("You must define the gamepiece first!", 5f);
        }
        else FinishGamepieceGoal(); //if already setting spawn, end editing process
    }

    private void UpdateGamepieceGoal()
    {
        int index = settingGamepieceGoal - 1;
        int goalIndex = settingGamepieceGoalIndex;
        if (goalIndicator != null)
        {
            if (!settingGamepieceGoalVertical)
            {
                if (Input.GetKey(KeyCode.LeftArrow)) goalIndicator.transform.position += UnityEngine.Vector3.forward * 0.04f;
                if (Input.GetKey(KeyCode.RightArrow)) goalIndicator.transform.position += UnityEngine.Vector3.back * 0.04f;
                if (Input.GetKey(KeyCode.UpArrow)) goalIndicator.transform.position += UnityEngine.Vector3.right * 0.04f;
                if (Input.GetKey(KeyCode.DownArrow)) goalIndicator.transform.position += UnityEngine.Vector3.left * 0.04f;
                if (Input.GetKey(KeyCode.Comma)) goalIndicator.transform.localScale /= 1.03f;
                if (Input.GetKey(KeyCode.Period)) goalIndicator.transform.localScale *= 1.03f;
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    DynamicCamera dynamicCamera = Camera.main.transform.GetComponent<DynamicCamera>();
                    DynamicCamera.SateliteState newSatelliteState = new DynamicCamera.SateliteState(dynamicCamera);
                    newSatelliteState.target = goalIndicator;
                    newSatelliteState.rotationVector = new UnityEngine.Vector3(15f, 0f, 0f); // Downward tilt of camera to view slightly from above

                    float offsetDist = goalIndicator.transform.localScale.magnitude + 2; // Set distance of camera to two units further than size of box
                    newSatelliteState.targetOffset = new UnityEngine.Vector3(0f, 0f, -offsetDist);// offsetDist / 32f, -offsetDist);
                    newSatelliteState.targetOffset = UnityEngine.Quaternion.Euler(newSatelliteState.rotationVector) * newSatelliteState.targetOffset; // Rotate camera offset to face block
                    newSatelliteState.targetOffset += new UnityEngine.Vector3(0f, -offsetDist / 10, 0f);

                    dynamicCamera.SwitchCameraState(newSatelliteState);

                    settingGamepieceGoalVertical = true;
                }
            }
            else
            {
                DynamicCamera.SateliteState satellite = ((DynamicCamera.SateliteState)Camera.main.transform.GetComponent<DynamicCamera>().cameraState);
                
                if (Input.GetKey(KeyCode.LeftArrow)) satellite.rotationVector += UnityEngine.Vector3.up * 1f;
                if (Input.GetKey(KeyCode.RightArrow)) satellite.rotationVector += UnityEngine.Vector3.down * 1f;
                if (Input.GetKey(KeyCode.UpArrow)) goalIndicator.transform.position += UnityEngine.Vector3.up * 0.03f;
                if (Input.GetKey(KeyCode.DownArrow)) goalIndicator.transform.position += UnityEngine.Vector3.down * 0.03f;
                if (Input.GetKey(KeyCode.Comma)) goalIndicator.transform.localScale /= 1.03f;
                if (Input.GetKey(KeyCode.Period)) goalIndicator.transform.localScale *= 1.03f;
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    UserMessageManager.Dispatch("New goal location has been set!", 3f);
                    gamepieceGoals[index][goalIndex] = goalIndicator.transform.position;
                    gamepieceGoalSizes[index][goalIndex] = goalIndicator.transform.localScale.x;

                    GenerateGamepieceGoalColliders(settingGamepieceGoal - 1);

                    FinishGamepieceGoal();
                    return;
                }

                float offsetDist = goalIndicator.transform.localScale.magnitude + 2; // Set distance of camera to two units further than size of box
                satellite.targetOffset = new UnityEngine.Vector3(0f, 0f, -offsetDist);// offsetDist / 32f, -offsetDist);
                satellite.targetOffset = UnityEngine.Quaternion.Euler(satellite.rotationVector) * satellite.targetOffset; // Rotate camera offset to face block
                satellite.targetOffset += new UnityEngine.Vector3(0f, -offsetDist / 10, 0f);
            }
        }
    }

    public void FinishGamepieceGoal()
    {
        settingGamepieceGoal = 0;
        if (goalIndicator != null) Destroy(goalIndicator);
        if (lastCameraState != null)
        {
            DynamicCamera dynamicCamera = Camera.main.transform.GetComponent<DynamicCamera>();
            dynamicCamera.SwitchCameraState(lastCameraState);
            lastCameraState = null;
        }
        Robot.ControlsEnabled = true;
    }

    /// <summary>
    /// Create colliders for all the goals of a gamepiece, deleting any existing ones.
    /// </summary>
    /// <param name="index">The gamepiece to create goal colliders for.</param>
    public void GenerateGamepieceGoalColliders(int index)
    {
        if (gamepieceNames[index] != null && GameObject.Find(gamepieceNames[index]) != null)
        {
            DestroyGamepieceGoalColliders(index);

            for (int goalIndex = 0; goalIndex < gamepieceGoals[index].Count; goalIndex++)
            {
                GameObject gameobject = new GameObject("Gamepiece" + index.ToString() + "Goal" + goalIndex.ToString());

                BBoxShape collider = gameobject.AddComponent<BBoxShape>();
                collider.Extents = new UnityEngine.Vector3(0.5f, 0.5f, 0.5f) * gamepieceGoalSizes[index][goalIndex];

                BRigidBody rigid = gameobject.AddComponent<BRigidBody>();
                rigid.collisionFlags = rigid.collisionFlags | BulletSharp.CollisionFlags.NoContactResponse | BulletSharp.CollisionFlags.StaticObject;
                rigid.transform.position = gamepieceGoals[index][goalIndex];

                DriverPracticeGoal goal = gameobject.AddComponent<DriverPracticeGoal>();
                goal.SetKeyword(gamepieceNames[index]);

                goal.description = gamepieceGoalDesc[index][goalIndex];
                goal.pointValue = gamepieceGoalPoints[index][goalIndex];

                goal.dpmRobot = this;

                if (scoreboard == null)
                    scoreboard = AuxFunctions.FindObject("StateMachine").GetComponent<Scoreboard>();

                goal.scoreboard = scoreboard;

                gamepieceGoalObjects[index].Add(gameobject);
            }
        }
        else
        {
            Debug.LogError("Cannot generate goal of undefined gamepiece!");
        }
    }

    /// <summary>
    /// Remove all goal colliders of a gamepiece from the scene.
    /// </summary>
    /// <param name="index">The gamepiece to remove all goals from.</param>
    public void DestroyGamepieceGoalColliders(int index)
    {
        try //In case the gamepiece somehow doens't exist in the scene
        {
            while (gamepieceGoalObjects[index].Count > 0) // Delete existing goal objects
            {
                Destroy(gamepieceGoalObjects[index][0]);
                gamepieceGoalObjects[index].RemoveAt(0);
            }
        }
        catch
        {
            UserMessageManager.Dispatch("Unknown error occurred when generating gamepiece goals!", 5);
        }
    }

    #endregion

    #region Configuring Mechanisms

    /// <summary>
    /// Allows the user to select a robot node with their mouse and change the intake/release node
    /// </summary>
    /// <param name="index">configuring index</param>
    public void SetMechanism(int index)
    {
        //Casts a ray from the camera in the direction the mouse is in and returns the closest object hit
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        BulletSharp.Math.Vector3 start = ray.origin.ToBullet();
        BulletSharp.Math.Vector3 end = ray.GetPoint(200).ToBullet();

        //Creates a callback result that will be updated if we do a ray test with it
        ClosestRayResultCallback rayResult = new ClosestRayResultCallback(ref start, ref end);

        //Retrieves the bullet physics world and does a ray test with the given coordinates and updates the callback object
        BPhysicsWorld world = BPhysicsWorld.Get();
        world.world.RayTest(start, end, rayResult);

        //If there is a collision object and it is dynamic and not a robot part, change the gamepiece to that
        if (rayResult.CollisionObject != null)
        {
            string name = (rayResult.CollisionObject.UserObject.ToString().Replace(" (BulletUnity.BRigidBody)", ""));
            Debug.Log(name);
            if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
            {
                UserMessageManager.Dispatch("Please click on a robot part", 3);
            }
            else if (GameObject.Find(name) == null)
            {
                Debug.Log("DPM: Game object not found");

            }
            else if (GameObject.Find(name).transform.parent == transform)
            {
                if (definingIntake)
                {
                    intakeNode[index] = GameObject.Find(name);
                    SetInteractor(intakeNode[index], index);

                    UserMessageManager.Dispatch(name + " has been selected as intake node", 5);

                    definingIntake = false;
                }
                else
                {
                    releaseNode[index] = GameObject.Find(name);
                    SetInteractor(releaseNode[index], index);

                    UserMessageManager.Dispatch(name + " has been selected as release node", 5);

                    definingRelease = false;
                }

                RevertNodeColors(hoveredNode, hoveredColors);
            }
            else
            {
                UserMessageManager.Dispatch("A gamepiece is NOT a robot part!", 3);
            }
        }
        else
        {

        }
    }

    private void SelectingNode()
    {
        //Casts a ray from the camera in the direction the mouse is in and returns the closest object hit
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        BulletSharp.Math.Vector3 start = ray.origin.ToBullet();
        BulletSharp.Math.Vector3 end = ray.GetPoint(200).ToBullet();

        //Creates a callback result that will be updated if we do a ray test with it
        ClosestRayResultCallback rayResult = new ClosestRayResultCallback(ref start, ref end);

        //Retrieves the bullet physics world and does a ray test with the given coordinates and updates the callback object
        BPhysicsWorld world = BPhysicsWorld.Get();
        world.world.RayTest(start, end, rayResult);

        //If there is a collision object and it is dynamic and not a robot part, change the gamepiece to that
        if (rayResult.CollisionObject != null)
        {
            string name = (rayResult.CollisionObject.UserObject.ToString().Replace(" (BulletUnity.BRigidBody)", ""));
            Debug.Log(name);
            if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
            {
                RevertNodeColors(hoveredNode, hoveredColors);
            }
            else if (GameObject.Find(name) == null)
            {
                Debug.Log("DPM: Game object not found");
                RevertNodeColors(hoveredNode, hoveredColors);
            }
            else if (GameObject.Find(name).transform.parent == transform)
            {
                if (hoveredNode != GameObject.Find(name))
                {
                    RevertNodeColors(hoveredNode, hoveredColors);
                }

                hoveredNode = GameObject.Find(name);

                ChangeNodeColors(hoveredNode, hoverColor, hoveredColors);

            }
            else RevertNodeColors(hoveredNode, hoveredColors);
        }
    }

    public void DefineIntake(int index)
    {
        if (modeEnabled)
        {
            if (addingGamepiece) UserMessageManager.Dispatch("You must select a gamepiece first!", 5);
            else if (definingRelease) UserMessageManager.Dispatch("You must define the release mechanism first!", 5);
            else
            {
                UserMessageManager.Dispatch("Click on a robot part to define it as the intake mechanism", 5);
                configuringIndex = index;
                definingIntake = true;
            }
        }
    }

    public void DefineRelease(int index)
    {
        if (modeEnabled)
        {
            if (addingGamepiece) UserMessageManager.Dispatch("You must select a gamepiece first!", 5);
            else if (definingIntake) UserMessageManager.Dispatch("You must define the intake mechanism first!", 5);
            else
            {
                UserMessageManager.Dispatch("Click on a robot part to define it as the release mechanism", 5);
                configuringIndex = index;
                definingRelease = true;
            }
        }
    }

    private void SetInteractor(GameObject node, int index)
    {
        if (node.GetComponent<Interactor>() == null) intakeInteractor[index] = node.AddComponent<Interactor>();
        else intakeInteractor[index] = node.GetComponent<Interactor>();

        intakeInteractor[index].SetKeyword(gamepieceNames[index], index);
    }

    public void HighlightNode(string node)
    {
        RevertHighlight();
        highlightedNode = GameObject.Find(node);
        ChangeNodeColors(highlightedNode, highlightColor, originalColors);
        highlightTimer = 80;


    }
    public void RevertHighlight()
    {
        RevertNodeColors(highlightedNode, originalColors);
        highlightedNode = null;
        highlightTimer = -1;
    }

    #endregion

    #region Configuring Vector Values


    public void ChangeOffsetX(float amount, int index)
    {
        positionOffset[index].x += amount;
    }
    public void ChangeOffsetY(float amount, int index)
    {
        positionOffset[index].y += amount;
    }
    public void ChangeOffsetZ(float amount, int index)
    {
        positionOffset[index].z += amount;
    }
    public void ChangeReleaseSpeed(float amount, int index)
    {
        releaseVelocity[index][0] += amount;
    }
    public void ChangeReleaseHorizontalAngle(float amount, int index)
    {
        releaseVelocity[index][1] += amount;
    }
    public void ChangeReleaseVerticalAngle(float amount, int index)
    {
        releaseVelocity[index][2] += amount;
    }
    #endregion

    #region Highlighting Functions
    private void ChangeNodeColors(GameObject node, Color color, List<Color> storedColors)
    {
        foreach (Renderer renderers in node.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in renderers.materials)
            {
                storedColors.Add(m.color);
                m.color = color;
            }
        }
    }

    private void RevertNodeColors(GameObject node, List<Color> storedColors)
    {
        if (node != null && storedColors.Count != 0)
        {
            int counter = 0;
            foreach (Renderer renderers in node.GetComponentsInChildren<Renderer>())
            {

                foreach (Material m in renderers.materials)
                {
                    m.color = storedColors[counter];
                    counter++;
                }
            }
            storedColors.Clear();
        }
    }
    #endregion

    public void Save()
    {
        string filePath = PlayerPrefs.GetString("simSelectedRobot");
        if (File.Exists(filePath + "\\dpmConfig.txt"))
        {
            File.Delete(filePath + "\\dpmConfig.txt");
        }
        Debug.Log("Saving to " + filePath + "\\dpmConfig.txt");
        using (StreamWriter writer = new StreamWriter(filePath + "\\dpmConfig.txt", false))
        {
            StringBuilder sb;
            for (int i = 0; i < gamepieceNames.Count; i++)
            {
                writer.WriteLine("##Gamepiece" + i);
                writer.WriteLine("#Name");
                writer.WriteLine(gamepieceNames[i]);

                writer.WriteLine("#Spawnpoint");
                sb = new StringBuilder();
                writer.WriteLine(sb.Append(gamepieceSpawn[i].x).Append("|").Append(gamepieceSpawn[i].y).Append("|").Append(gamepieceSpawn[i].z));

                for (int j = 0; j < gamepieceGoals[i].Count; j++)
                {
                    writer.WriteLine("##Goal" + j);
                    writer.WriteLine("#Position");
                    sb = new StringBuilder();
                    writer.WriteLine(sb.Append(gamepieceGoals[i][j].x).Append("|").Append(gamepieceGoals[i][j].y).Append("|").Append(gamepieceGoals[i][j].z));

                    writer.WriteLine("#Size");
                    writer.WriteLine(gamepieceGoalSizes[i][j]);

                    writer.WriteLine("#Points");
                    writer.WriteLine(gamepieceGoalPoints[i][j]);

                    writer.WriteLine("#Description");
                    writer.WriteLine(gamepieceGoalDesc[i][j]);
                }

                writer.WriteLine("#Intake Node");
                writer.WriteLine(intakeNode[i].name);

                writer.WriteLine("#Release Node");
                writer.WriteLine(releaseNode[i].name);

                writer.WriteLine("#Release Position");
                sb = new StringBuilder();
                writer.WriteLine(sb.Append(positionOffset[i].x).Append("|").Append(positionOffset[i].y).Append("|").Append(positionOffset[i].z));

                writer.WriteLine("#Release Velocity");
                sb = new StringBuilder();
                writer.WriteLine(sb.Append(releaseVelocity[i][0]).Append("|").Append(releaseVelocity[i][1]).Append("|").Append(releaseVelocity[i][2]));
            }
            writer.Close();
        }
        Debug.Log("Save successful!");
    }

    public void Load(string robotDirectory)
    {
        string filePath = robotDirectory + "\\dpmConfig.txt";
        if (File.Exists(filePath))
        {
            StreamReader reader = new StreamReader(filePath);
            string line = "";
            int counter = 0;
            int index = -1;
            int goalIndex = -1;

            while ((line = reader.ReadLine()) != null)
            {
                // Set the counter to a value representing the variable to be configured next. 
                if (line.Contains("#Gamepiece"))
                {
                    counter = 0;
                    index++;
                    goalIndex = -1;
                }
                else if (line.Equals("#Name"))
                    counter = 1;
                else if (line.Equals("#Spawnpoint"))
                    counter = 2;
                else if (line.Contains("#Goal"))
                {
                    counter = 0;
                    goalIndex++;

                    gamepieceGoals[index].Add(new UnityEngine.Vector3(0, 4, 0));
                    gamepieceGoalSizes[index].Add(1f);
                    gamepieceGoalPoints[index].Add(0);
                    gamepieceGoalDesc[index].Add("");
                }
                else if (line.Equals("#Position"))
                    counter = 3;
                else if (line.Equals("#Size"))
                    counter = 4;
                else if (line.Equals("#Points"))
                    counter = 5;
                else if (line.Equals("#Description"))
                    counter = 6;
                else if (line.Equals("#Intake Node"))
                    counter = 7;
                else if (line.Equals("#Release Node"))
                    counter = 8;
                else if (line.Equals("#Release Position"))
                    counter = 9;
                else if (line.Equals("#Release Velocity"))
                    counter = 10;

                // Apply the value of the line to the variable that was specified by the previous line.
                else if (counter == 1)
                {
                    gamepieceOriginals[index] = GameObject.Find(line);

                    if (gamepieceOriginals[index] != null)
                        gamepieceNames[index] = line;
                }
                else if (gamepieceOriginals[index] != null) // If gamepiece doesn't exist, don't import configuration of it.
                { 
                    if (counter == 2)
                        gamepieceSpawn[index] = DeserializeVector3Array(line);
                    else if (counter == 3)
                        gamepieceGoals[index][goalIndex] = DeserializeVector3Array(line);
                    else if (counter == 4)
                        gamepieceGoalSizes[index][goalIndex] = float.Parse(line);
                    else if (counter == 5)
                        gamepieceGoalPoints[index][goalIndex] = int.Parse(line);
                    else if (counter == 6)
                        gamepieceGoalDesc[index][goalIndex] = line;
                    else if (counter == 7)
                        intakeNode[index] = GameObject.Find(line);
                    else if (counter == 8)
                        releaseNode[index] = GameObject.Find(line);
                    else if (counter == 9)
                        positionOffset[index] = DeserializeVector3Array(line);
                    else if (counter == 10)
                        releaseVelocity[index] = DeserializeArray(line);
                }
            }
            reader.Close();

            SetInteractor(intakeNode[0], 0);
            SetInteractor(intakeNode[1], 1);
        }
    }

    public static UnityEngine.Vector3 DeserializeVector3Array(string aData)
    {
        UnityEngine.Vector3 result = new UnityEngine.Vector3(0, 0, 0);
        string[] values = aData.Split('|');
        Debug.Log(values[0]);
        if (values.Length != 3)
            throw new System.FormatException("component count mismatch. Expected 3 components but got " + values.Length);
        result = new UnityEngine.Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        return result;
    }
    public static float[] DeserializeArray(string aData)
    {
        float[] result = new float[3];
        string[] values = aData.Split('|');
        if (values.Length != 3)
            throw new System.FormatException("component count mismatch. Expected 3 components but got " + values.Length);
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = float.Parse(values[i]);
        }
        return result;
    }

    private void ProcessControls()
    {
        if (Robot.ControlsEnabled)
        {
            if (processingIndex == 0)
            {
                if ((InputControl.GetButton(Controls.buttons[0].pickupPrimary)))
                {

                    Intake(0);
                }
                if ((InputControl.GetButton(Controls.buttons[0].pickupSecondary)))
                {
                    Intake(1);
                }
                if ((InputControl.GetButtonDown(Controls.buttons[0].releasePrimary)))
                {
                    ReleaseGamepiece(0);
                }
                else
                {
                    HoldGamepiece(0);
                }
                if ((InputControl.GetButtonDown(Controls.buttons[0].releaseSecondary)))
                {
                    ReleaseGamepiece(1);
                }
                else
                {
                    HoldGamepiece(1);
                }
                processingIndex = 1;
            }
            else
            {
                if ((InputControl.GetButton(Controls.buttons[0].pickupSecondary)))
                {

                    Intake(1);
                }
                if ((InputControl.GetButton(Controls.buttons[0].pickupPrimary)))
                {
                    Intake(0);
                }
                if ((InputControl.GetButtonDown(Controls.buttons[0].releaseSecondary)))
                {
                    ReleaseGamepiece(1);
                }
                else
                {
                    HoldGamepiece(1);
                }
                if ((InputControl.GetButtonDown(Controls.buttons[0].releasePrimary)))
                {
                    ReleaseGamepiece(0);
                }
                else
                {
                    HoldGamepiece(0);
                }
                processingIndex = 0;
            }

            if ((InputControl.GetButtonDown(Controls.buttons[0].spawnPrimary))) SpawnGamepiece(0);
            if ((InputControl.GetButtonDown(Controls.buttons[0].spawnPrimary))) SpawnGamepiece(1);
        }
    }
}