using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BulletSharp;
using BulletUnity;
using BulletSharp.Math;
using System.Collections;
using System.IO;
using System.Text;
using BulletUnity.Debugging;
using System.Linq;
using Assets.Scripts.FSM;

/// <summary>
/// This is a class that handles everything associated with the driver practice mode.
/// It 'cheats physics' to overcome the limitations that our current simulation has to create a beter environment for drivers to practice and interact with game objects.
/// 
/// Right now, the class is designed for the user to customize interaction with up to two gamepieces. That is why most variables are in a list of two; however this number can be increased with some modification.
/// 
/// This class is only to be attached to the Robot parent GameObject
/// </summary>
public class DriverPractice : MonoBehaviour {

    public UnityEngine.Vector3[] positionOffset; //position offset vectors for gamepiece while its being held
    public List<float[]> releaseVelocity; //release velocity values for gamepiece, defined not in x,y,z coordinates, but speed, hor angle, and ver angle.
    private float[] primaryVelocity = new float[3];
    private float[] secondaryVelocity = new float[3];

    private List<UnityEngine.Vector3> releaseVelocityVector; //release velocity vector using x,y,z coordinate system, calculated from the speed, hor angle, and ver angle.

    public List<GameObject> intakeNode; //node that is identified for intaking gamepieces
    public List<GameObject> releaseNode; //node that is identified for holding/releasing gamepieces
    private List<Interactor> intakeInteractor;

    private List<int> holdingLimit; //the maximum number of game objects that this robot can hold at any given time.

    public List<List<GameObject>> objectsHeld; //list of gamepieces this robot is currently holding
    private List<GameObject> primaryHeld;
    private List<GameObject> secondaryHeld;

    public List<string> gamepieceNames; //list of the identifiers of gamepieces
    public List<List<GameObject>> spawnedGamepieces;
    private List<GameObject> spawnedPrimary;
    private List<GameObject> spawnedSecondary;
    private int gamepieceCounter = 0; //number to add at the end of each gamepiece name so that no two gamepieces have the same name

    public List<bool> displayTrajectories; //projects gamepiece trajectories if true
    private List<LineRenderer> drawnTrajectory; //a series of lines that projects a trajectory calculated from the robot's speed and the velocity vectors

    public bool modeEnabled = false; //whether driver practice mode is enabled or not

    private int configuringIndex = 0; //whether the user is configuring the primary(0) or secondary gamepiece(1)
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

    //for gamepiece spawning customizability
    private List<UnityEngine.Vector3> gamepieceSpawn;
    private GameObject spawnIndicator;
    public int settingSpawn = 0; //0 if not, 1 if editing primary, and 2 if editing secondary
    private DynamicCamera.CameraState lastCameraState;


    /// <summary>
    /// If configuration file exists, loads information and auto-configures robot.
    /// If coniguration file doesn't exist, initializes variables for users to configure.
    /// 
    /// Also loads gamepiece list from MainState.cs.
    /// 
    /// *NOTE: Because gamepiece identification in the new field format doesn't exist yet, we are using a predetermined gamepiece list. This must be changed as soon as support for gamepieces is added in the field exporter.*
    /// </summary>
    private void Start()
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
        intakeNode.Add(GameObject.Find("node_0.bxda"));
        intakeNode.Add(GameObject.Find("node_0.bxda"));

        releaseNode = new List<GameObject>();
        releaseNode.Add(GameObject.Find("node_0.bxda"));
        releaseNode.Add(GameObject.Find("node_0.bxda"));

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

        spawnedGamepieces = new List<List<GameObject>>();
        spawnedPrimary = new List<GameObject>();
        spawnedSecondary = new List<GameObject>();
        spawnedGamepieces.Add(spawnedPrimary);
        spawnedGamepieces.Add(spawnedSecondary);

        holdingLimit = new List<int>();
        holdingLimit.Add(30);
        holdingLimit.Add(30);

        SetInteractor(intakeNode[0], 0);
        SetInteractor(intakeNode[1], 1);

        gamepieceSpawn = new List<UnityEngine.Vector3>();
        gamepieceSpawn.Add(new UnityEngine.Vector3(0f,3f,0f));
        gamepieceSpawn.Add(new UnityEngine.Vector3(0f, 3f, 0f));


        //Creates two new gameobjects with line renderer components that display a calculated trajectory whem enabled
        drawnTrajectory = new List<LineRenderer>();
        GameObject lineDrawer;
        LineRenderer trajectory;

        for (int i = 0; i < 2; i++)
        {
            lineDrawer = new GameObject();
            lineDrawer.name = "DrawnTrajectory" + i;
            trajectory = lineDrawer.AddComponent<LineRenderer>();
            trajectory.startWidth = 0.2f;
            trajectory.material = Resources.Load("Materials/Projection") as Material;
            trajectory.enabled = false;
            drawnTrajectory.Add(trajectory);
        }

        drawnTrajectory[0].startColor = Color.blue;
        drawnTrajectory[0].endColor = Color.cyan;
        drawnTrajectory[1].startColor = Color.red;
        drawnTrajectory[1].endColor = Color.magenta;

        displayTrajectories = new List<bool>();
        displayTrajectories.Add(false);
        displayTrajectories.Add(false);

        Load(); //Loads pre-existing configuration if they exist
    }
	
	// Update is called once per frame
	void Update () {
        if (modeEnabled)
        {
            ProcessControls();
            
            //Allows the user to click on a dynamic object or robot node to define it as a gamepiece or mechanism
            if (Input.GetMouseButtonDown(0))
            {
                if (addingGamepiece) SetGamepiece(configuringIndex);
                else if (definingIntake || definingRelease) SetMechanism(configuringIndex);
            }

            //if currently defining a mechanism, any node the mouse hovers over will be highlighted
            if (definingIntake || definingRelease) SelectingNode();


            //highlighted nodes will automatically revert to normal colors after a set time
            if (highlightTimer > 0) highlightTimer--;
            else if (highlightTimer == 0) RevertHighlight();

            //if this variable is not 0, then user is currently changing the gamepiece spawn location
            if (settingSpawn != 0) UpdateGamepieceSpawn();
        }

        //draws and updates trajectories only if user selects that and main state is the active state
        for (int i = 0; i < 2; i++)
        {
            if (modeEnabled && displayTrajectories[i] && StateMachine.Instance.CurrentState is MainState)
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
        //making sure that 1. the robot is not carrying more than the max amount of gamepieces and 2. the intake interactor script has detected a collision with a gamepiece
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
            //retrieves the new gamepiece object and changes physics properties to 'clip' it to the node
            GameObject newObject = intakeInteractor[index].GetObject(index);
            objectsHeld[index].Add(newObject);
            newObject.GetComponent<BRigidBody>().velocity = UnityEngine.Vector3.zero;
            newObject.GetComponent<BRigidBody>().angularVelocity = UnityEngine.Vector3.zero;
            newObject.GetComponent<BRigidBody>().collisionFlags = BulletSharp.CollisionFlags.NoContactResponse;

            intakeInteractor[index].heldGamepieces.Add(newObject);

            //ignores collisions specificially between the robot and the gamepiece
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

        BRigidBody releaseRB; //rigid body of the release node
        releaseRB = releaseNode[index].GetComponent<BRigidBody>();

        if (objectsHeld[index].Count > 0)
        {
            BRigidBody gamepieceRB; //rigid body of the gamepiece

            //changes the physics values of the gamepieces to properly clip it to the robot
            //to-do: clipping works fine, but under laggy conditions they tend to bounce around unncessarily
            for (int i = 0; i < objectsHeld[index].Count; i++)
            {
                gamepieceRB = objectsHeld[index][i].GetComponent<BRigidBody>();
                gamepieceRB.velocity = releaseRB.velocity;
                gamepieceRB.SetPosition(releaseRB.transform.position + releaseRB.transform.rotation * positionOffset[index]);
                gamepieceRB.angularVelocity = UnityEngine.Vector3.zero;
                gamepieceRB.angularFactor = UnityEngine.Vector3.zero;

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
            BRigidBody gamepieceRB = objectsHeld[index][0].GetComponent<BRigidBody>();
            gamepieceRB.collisionFlags = BulletSharp.CollisionFlags.None;
            gamepieceRB.velocity += releaseNode[index].transform.rotation * releaseVelocityVector[index];
            gamepieceRB.angularFactor = UnityEngine.Vector3.one;
            StartCoroutine(UnIgnoreCollision(objectsHeld[index][0])); //starts the coroutine to ignore collisions between the robot and gamepieces

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


    /// <summary>
    /// Takes speed, horizontal angle, and vertical angle components of a set velocity and converts that into a Vector3 for Unity to use.
    /// </summary>
    /// <param name="speed">the speed magnitude</param>
    /// <param name="horAngle">horizontal angle of release in degrees</param>
    /// <param name="verAngle">vertical angle of release in degrees</param>
    /// <returns></returns>
    private UnityEngine.Vector3 VelocityToVector3(float speed, float horAngle, float verAngle)
    {
        UnityEngine.Quaternion horQuaternion;
        UnityEngine.Quaternion verQuaternion;
        UnityEngine.Vector3 finalVector = UnityEngine.Vector3.zero;
        
        //Converts each angle into a quaternion to be multiplied by
        horQuaternion = UnityEngine.Quaternion.AngleAxis(horAngle, UnityEngine.Vector3.up);
        verQuaternion = UnityEngine.Quaternion.AngleAxis(verAngle, UnityEngine.Vector3.right);

        //multiplies a reference quaternion (facing forward and standing straight) by the two quaternions made earlier and then multiplies it by a vector 
        finalVector = (UnityEngine.Quaternion.LookRotation(UnityEngine.Vector3.forward,UnityEngine.Vector3.up) * horQuaternion * verQuaternion) * UnityEngine.Vector3.forward * speed;

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
        UnityEngine.Vector3 grav = BPhysicsWorld.Get().gravity;
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

        //Retrieves the bullet physics world and does a ray test with the given coordinates and updates the callback object
        BPhysicsWorld world = BPhysicsWorld.Get();
        world.world.RayTest(start, end, rayResult);

        //If there is a collision object and it is dynamic and not a robot part, change the gamepiece to that
        if (rayResult.CollisionObject != null)
        {
            GameObject selectedObject = ((BRigidBody)rayResult.CollisionObject.UserObject).gameObject;
            if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
            {
                UserMessageManager.Dispatch("The gamepiece must be a dynamic object!", 3);
            }
            else if (selectedObject == null)
            {
                Debug.Log("DPM: Game object not found");
                    
            }
            else if (selectedObject.transform.parent != null && selectedObject.transform.parent.name == "Robot")
            {
                UserMessageManager.Dispatch("You cannot select a robot part as a gamepiece!", 3);
            }
            else
            {
                string name = selectedObject.name;
                name = name.Replace("clone_",""); //gets rid of the clone tag given to spawned gamepieces
                if (name.IndexOf(":") > 0) name = name.Substring(0, name.IndexOf(":"));

                gamepieceNames[index] = name;

                intakeInteractor[index].SetKeyword(gamepieceNames[index],index);

                UserMessageManager.Dispatch(name + " has been selected as the gamepiece", 2);
                addingGamepiece = false;
            }
        }
        else
        {
                
        }
    }

    /// <summary>
    /// Starts the process for the user to select an object as a gamepiece
    /// </summary>
    /// <param name="index">the configuration index</param>
    public void DefineGamepiece(int index)
    {
        if (modeEnabled)
        {
            if (definingIntake || definingRelease) UserMessageManager.Dispatch("You must select a robot part first!", 5);
            else if (settingSpawn != 0) UserMessageManager.Dispatch("You must set the gamepiece spawnpoint first! Press enter to save your the current position",5);
            else
            {
                UserMessageManager.Dispatch("Click on a dynamic object to add it as a gamepiece", 5);
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
        if (gamepieceNames[index] != null)
        {
            try //In case the game piece somehow doens't exist in the scene
            {
                GameObject gameobject = Instantiate(AuxFunctions.FindObject(gamepieceNames[index]).GetComponentInParent<BRigidBody>().gameObject, gamepieceSpawn[index], UnityEngine.Quaternion.identity);
                gameobject.name = "clone_" + gamepieceNames[index];
                gamepieceCounter++;
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
            spawnedGamepieces[i].Clear();
        }
        
    }

    /// <summary>
    /// Starts the process for the user to change the spawn location of a gamepiece
    /// </summary>
    /// <param name="index"></param>
    public void StartGamepieceSpawn(int index)
    {
        if (settingSpawn == 0) //checks if not currently setting new spawn location
        {
            if (GameObject.Find(gamepieceNames[index]) != null) //if gamepiece actually exists in the field
            {
                if (spawnIndicator != null) Destroy(spawnIndicator);
                
                //creates a movable gameobject that is a slightly transparent version of the defined gamepiece to represent the new spawn location
                if (spawnIndicator == null)
                {
                    spawnIndicator = Instantiate(AuxFunctions.FindObject(gamepieceNames[index]).GetComponentInParent<BRigidBody>().gameObject, new UnityEngine.Vector3(0, 3, 0), UnityEngine.Quaternion.identity);
                    spawnIndicator.name = "SpawnIndicator";
                    Destroy(spawnIndicator.GetComponent<BRigidBody>());
                    if (spawnIndicator.transform.GetChild(0) != null) spawnIndicator.transform.GetChild(0).name = "SpawnIndicatorMesh";

                    Material material = spawnIndicator.GetComponentInChildren<Renderer>().material;
                    material.shader = Shader.Find("Transparent/Diffuse");
                    Color newColor = material.color;
                    newColor.a = 0.6f;
                    material.color = newColor;
                }
                spawnIndicator.transform.position = gamepieceSpawn[index];
                settingSpawn = index + 1;

                //Switches to an overhead camera for a better view of the moving spawnpoint
                DynamicCamera dynamicCamera = Camera.main.transform.GetComponent<DynamicCamera>();
                lastCameraState = dynamicCamera.cameraState;
                dynamicCamera.SwitchCameraState(new DynamicCamera.SateliteState(dynamicCamera));

                //disables controls so that robot doesn't move while spawnpoint is being defined
                MainState.ControlsDisabled = true;
            }
            else UserMessageManager.Dispatch("You must define the gamepiece first!", 5f);
        }
        else FinishGamepieceSpawn(); //if already setting spawn, end editing process
    }

    /// <summary>
    /// Runs every step while user is changing the spawnpoint to update the new spawnpoint coordinates based on user input
    /// </summary>
    private void UpdateGamepieceSpawn()
    {
        int index = settingSpawn - 1;
        if (spawnIndicator != null)
        {
            ((DynamicCamera.SateliteState)Camera.main.transform.GetComponent<DynamicCamera>().cameraState).target = spawnIndicator;
            if (Input.GetKey(KeyCode.LeftArrow)) spawnIndicator.transform.position += UnityEngine.Vector3.forward * 0.1f;
            if (Input.GetKey(KeyCode.RightArrow)) spawnIndicator.transform.position += UnityEngine.Vector3.back * 0.1f;
            if (Input.GetKey(KeyCode.UpArrow)) spawnIndicator.transform.position += UnityEngine.Vector3.right * 0.1f;
            if (Input.GetKey(KeyCode.DownArrow)) spawnIndicator.transform.position += UnityEngine.Vector3.left * 0.1f;
            if (Input.GetKeyDown(KeyCode.Return))
            {
                UserMessageManager.Dispatch("New gamepiece spawn location has been set!", 3f);
                gamepieceSpawn[index] = spawnIndicator.transform.position;
                FinishGamepieceSpawn();
            }
        }
    }

    /// <summary>
    /// Ends the changing gamepiece spawn process
    /// </summary>
    public void FinishGamepieceSpawn()
    {
        settingSpawn = 0;
        if (spawnIndicator != null) Destroy(spawnIndicator);
        DynamicCamera dynamicCamera = Camera.main.transform.GetComponent<DynamicCamera>();
        dynamicCamera.SwitchCameraState(lastCameraState);
        MainState.ControlsDisabled = false;
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

        //If there is a collision object and it is a robot part, then define it as the mechanism
        if (rayResult.CollisionObject != null)
        {
            GameObject selectedObject = ((BRigidBody)rayResult.CollisionObject.UserObject).gameObject;
            if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
            {
                UserMessageManager.Dispatch("Please click on a robot part", 3);
            }
            else if (selectedObject == null)
            {
                Debug.Log("DPM: Game object not found");

            }
            else if (selectedObject.transform.parent != null && selectedObject.transform.parent.name == "Robot")
            {
                if (definingIntake)
                {
                    intakeNode[index] = selectedObject;
                    SetInteractor(intakeNode[index], index);

                    UserMessageManager.Dispatch(name + " has been selected as intake node", 5);
                        
                    definingIntake = false;
                }
                else
                {
                    releaseNode[index] = selectedObject;
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

    /// <summary>
    /// Highlights any robot node the mouse hovers over
    /// </summary>
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
            GameObject selectedObject = ((BRigidBody)rayResult.CollisionObject.UserObject).gameObject;

            if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
            {
                RevertNodeColors(hoveredNode, hoveredColors);
            }
            else if (selectedObject == null)
            {
                Debug.Log("DPM: Game object not found");
                RevertNodeColors(hoveredNode, hoveredColors);
            }
            else if (selectedObject.transform.parent != null && selectedObject.transform.parent.name == "Robot")
            {
                if (hoveredNode != selectedObject)
                {
                    RevertNodeColors(hoveredNode, hoveredColors);
                    hoveredNode = selectedObject;
                }           

                ChangeNodeColors(hoveredNode, hoverColor, hoveredColors);

            }
            else RevertNodeColors(hoveredNode, hoveredColors);
        }
    }

    /// <summary>
    /// Starts the process of defining a robot node as the intake mechanism
    /// </summary>
    /// <param name="index">configuration index</param>
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

    /// <summary>
    /// Starts the process of defining a robot node as the release mechanism
    /// </summary>
    /// <param name="index">configuration index</param>
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

    /// <summary>
    /// If interactor script doesn't currently exist in a gameobject, add it to that gameobject.
    /// Sets the collision keyword of that interactor to the currently defined gamepiece
    /// </summary>
    /// <param name="node">node gameobject for interactor to be set to</param>
    /// <param name="index">configuration index</param>
    private void SetInteractor(GameObject node, int index)
    {
        if (node.GetComponent<Interactor>() == null) intakeInteractor[index] = node.AddComponent<Interactor>();
        else intakeInteractor[index] = node.GetComponent<Interactor>();

        intakeInteractor[index].SetKeyword(gamepieceNames[index], index);
    }

    /// <summary>
    /// Highlights a specified robot node for a certain duration
    /// </summary>
    /// <param name="node">the robot node to be highlighted</param>
    public void HighlightNode(string node)
    {
        RevertHighlight();
        highlightedNode = GameObject.Find(node);
        ChangeNodeColors(highlightedNode, highlightColor, originalColors);
        highlightTimer = 80;
 
    }

    /// <summary>
    /// Rverts the node's colors from its highlighted colors back to its original colors
    /// </summary>
    public void RevertHighlight()
    {
        RevertNodeColors(highlightedNode, originalColors);
        highlightedNode = null;
        highlightTimer = -1;
    }

    #endregion

    #region Configuring Vector Values

    //The following functions change the position offset and release velocity values by a set amount
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
    /// <summary>
    /// Iterates through all the materials within a certain robot node and changes its color to the highlight color.
    /// It stores all the original colors into a list to be saved for reverting back later.
    /// </summary>
    /// <param name="node">robot node to be highlighted</param>
    /// <param name="color">highlight color</param>
    /// <param name="storedColors">the original colors of the node</param>
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

    /// <summary>
    /// Reverts a highlighted node back to its original state by changing the colors back to their original values
    /// </summary>
    /// <param name="node">robot node to be highlighted</param>
    /// <param name="storedColors">the original colors of the node</param>
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

    /// <summary>
    /// Saves the current configuration values to a text document
    /// </summary>
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

    /// <summary>
    /// Loads configuration information from a text document
    /// </summary>
    public void Load()
    {
        string filePath = PlayerPrefs.GetString("simSelectedRobot") + "\\dpmConfig.txt";
        if (File.Exists(filePath))
        {
            StreamReader reader = new StreamReader(filePath);
            string line = "";
            int counter = 0;
            int index = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals("#Name")) counter++;
                else if (counter == 1)
                {
                    if (line.Equals("#Spawnpoint")) counter++;
                    else
                    {
                        gamepieceNames[index] = line;
                    }
                }
                else if (counter == 2)
                {
                    if (line.Equals("#Intake Node")) counter++;
                    else gamepieceSpawn[index] = DeserializeVector3Array(line);
                }
                else if (counter == 3)
                {
                    if (line.Equals("#Release Node")) counter++;
                    else intakeNode[index] = GameObject.Find(line);
                }
                else if (counter == 4)
                {
                    if (line.Equals("#Release Position")) counter++;
                    else releaseNode[index] = GameObject.Find(line);
                }
                else if (counter == 5)
                {
                    if (line.Equals("#Release Velocity")) counter++;
                    else positionOffset[index] = DeserializeVector3Array(line);
                }
                else if (counter == 6)
                {
                    if (line.Contains("#Gamepiece"))
                    {
                        counter = 0;
                        index++;
                    }
                    else releaseVelocity[index] = DeserializeArray(line);
                }
            }
            reader.Close();

            SetInteractor(intakeNode[0], 0);
            SetInteractor(intakeNode[1], 1);
        }
    }

    /// <summary>
    /// Helper function for reading text files--converts 3 numbers separated by '|' into a Unity Vector3.
    /// </summary>
    /// <param name="aData">the string to deserialize</param>
    /// <returns>the Vector3 result</returns>
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
    /// <summary>
    /// Helper function for reading text files--converts 3 numbers separated by '|' into an 3 number array.
    /// </summary>
    /// <param name="aData">the string to deserialize</param>
    /// <returns>the array of 3 floats</returns>
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
        if (processingIndex == 0)
        {
            if (Input.GetKey(Controls.ControlKey[(int)Controls.Control.PickupPrimary]))
            {
                   
                Intake(0);
            }
            if (Input.GetKey(Controls.ControlKey[(int)Controls.Control.PickupSecondary]))
            {
                Intake(1);
            }
            if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.ReleasePrimary]))
            {
                ReleaseGamepiece(0);
            }
            else
            {
                HoldGamepiece(0);
            }
            if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.ReleaseSecondary]))
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
            if (Input.GetKey(Controls.ControlKey[(int)Controls.Control.PickupSecondary]))
            {

                Intake(1);
            }
            if (Input.GetKey(Controls.ControlKey[(int)Controls.Control.PickupPrimary]))
            {
                Intake(0);
            }
            if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.ReleaseSecondary]))
            {
                ReleaseGamepiece(1);
            }
            else
            {
                HoldGamepiece(1);
            }
            if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.ReleasePrimary]))
            {
                ReleaseGamepiece(0);
            }
            else
            {
                HoldGamepiece(0);
            }
            processingIndex = 0;
        }

        if (Input.GetKey(Controls.ControlKey[(int)Controls.Control.SpawnPrimary])) SpawnGamepiece(0);
        if (Input.GetKey(Controls.ControlKey[(int)Controls.Control.SpawnSecondary])) SpawnGamepiece(1);
    }



}
