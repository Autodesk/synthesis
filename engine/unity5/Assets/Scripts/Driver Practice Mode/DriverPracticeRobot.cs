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

    public List<string> gamepieceNames; //list of the identifiers of gamepieces
    public List<List<GameObject>> spawnedGamepieces;
    public List<GameObject> spawnedPrimary;
    public List<GameObject> spawnedSecondary;

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

    //for gamepiece spawning customizability
    private List<UnityEngine.Vector3> gamepieceSpawn;
    private GameObject spawnIndicator;
    public int settingSpawn = 0; //0 if not, 1 if editing primary, and 2 if editing secondary
    private DynamicCamera.CameraState lastCameraState;

    public int controlIndex;

    private void Awake()
    {
        StateMachine.Instance.Link<MainState>(this);
    }

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
        intakeNode.Add(transform.GetChild(0).gameObject);
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
        gamepieceSpawn.Add(new UnityEngine.Vector3(0f, 3f, 0f));
        gamepieceSpawn.Add(new UnityEngine.Vector3(0f, 3f, 0f));


        //Setting up the trajectory renderers
        drawnTrajectory = new List<LineRenderer>();
        GameObject firstLine = GameObject.Find("DrawnTrajectory1");
        drawnTrajectory.Add(firstLine.GetComponent<LineRenderer>());
        GameObject secondLine = GameObject.Find("DrawnTrajectory2");
        drawnTrajectory.Add(secondLine.GetComponent<LineRenderer>());

        displayTrajectories = new List<bool>();
        displayTrajectories.Add(false);
        displayTrajectories.Add(false);

        //After initializing all the lists and variables, try to load from the robot directory.
        Load(robotDirectory);

        controlIndex = GetComponent<Robot>().ControlIndex;
        modeEnabled = true;
    }

    /// <summary>
    /// Update is called once per frame to process controls, tick the highlight timer, and draw trajectories
    /// </summary>
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




            if (settingSpawn != 0) UpdateGamepieceSpawn();
        }

        for (int i = 0; i < 2; i++)
        {
            if (displayTrajectories[i])
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

        if (highlightTimer > 0) highlightTimer--;
        else if (highlightTimer == 0) RevertHighlight();
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
            StartCoroutine(UnIgnoreCollision(objectsHeld[index][0]));
            intakeInteractor[index].heldGamepieces.Remove(objectsHeld[index][0]);

            BRigidBody intakeRigidBody = intakeInteractor[index].GetComponent<BRigidBody>();

            if (intakeRigidBody != null && !intakeRigidBody.GetCollisionObject().IsActive)
                intakeRigidBody.GetCollisionObject().Activate();

            BRigidBody orb = objectsHeld[index][0].GetComponent<BRigidBody>();
            orb.collisionFlags = BulletSharp.CollisionFlags.None;
            orb.velocity += releaseNode[index].transform.rotation * releaseVelocityVector[index];
            orb.angularFactor = UnityEngine.Vector3.one;

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
    /// Converts a velocity from a scalar speed and two angles into a Unity Vector3 format
    /// </summary>
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

        //Retrieves the bullet physics world and does a ray test with the given coordinates and updates the callback object
        BPhysicsWorld world = BPhysicsWorld.Get();
        world.world.RayTest(start, end, rayResult);

        //If there is a collision object and it is dynamic and not a robot part, change the gamepiece to that
        if (rayResult.CollisionObject != null)
        {
            GameObject collisionObject = (rayResult.CollisionObject.UserObject as BRigidBody).gameObject;
            if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
            {
                UserMessageManager.Dispatch("The gamepiece must be a dynamic object!", 3);
            }
            else if (collisionObject == null)
            {
                Debug.Log("DPM: Game object not found");

            }
            else if (collisionObject.transform.parent != null && collisionObject.transform.parent.name == "Robot")
            {
                UserMessageManager.Dispatch("You cannot select a robot part as a gamepiece!", 3);
            }
            else
            {
                string name = collisionObject.name.Replace("(Clone)", ""); //gets rid of the clone tag given to spawned gamepieces 
                gamepieceNames[index] = name;
                intakeInteractor[index].SetKeyword(gamepieceNames[index], index);
                GameObject gamepiece = collisionObject;

                UserMessageManager.Dispatch(name + " has been selected as the gamepiece", 2);
                addingGamepiece = false;
            }
        }
        else
        {

        }
    }

    public void DefineGamepiece(int index)
    {
        if (modeEnabled)
        {
            if (definingIntake || definingRelease) UserMessageManager.Dispatch("You must select a robot part first!", 5);
            else if (settingSpawn != 0) UserMessageManager.Dispatch("You must set the gamepiece spawnpoint first! Press enter to save your the current position", 5);
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
                gameobject.name = gamepieceNames[index] + "(Clone)";
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
        if (definingRelease || definingIntake || addingGamepiece) Debug.Log("User Error"); //Message Manager already dispatches error message to user
        else if (settingSpawn == 0)
        {
            if (GameObject.Find(gamepieceNames[index]) != null)
            {
                if (spawnIndicator != null) Destroy(spawnIndicator);
                if (spawnIndicator == null)
                {
                    spawnIndicator = Instantiate(AuxFunctions.FindObject(gamepieceNames[index]).GetComponentInParent<BRigidBody>().gameObject, new UnityEngine.Vector3(0, 3, 0), UnityEngine.Quaternion.identity);
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
                dynamicCamera.SwitchCameraState(new DynamicCamera.SateliteState(dynamicCamera));

                //MainState.ControlsDisabled = true;
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
            ((DynamicCamera.SateliteState)Camera.main.transform.GetComponent<DynamicCamera>().cameraState).target = spawnIndicator;
            if (Input.GetKey(KeyCode.A)) spawnIndicator.transform.position += UnityEngine.Vector3.forward * 0.1f;
            if (Input.GetKey(KeyCode.D)) spawnIndicator.transform.position += UnityEngine.Vector3.back * 0.1f;
            if (Input.GetKey(KeyCode.W)) spawnIndicator.transform.position += UnityEngine.Vector3.right * 0.1f;
            if (Input.GetKey(KeyCode.S)) spawnIndicator.transform.position += UnityEngine.Vector3.left * 0.1f;
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
        //MainState.ControlsDisabled = false;
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
            GameObject collisionObject = (rayResult.CollisionObject.UserObject as BRigidBody).gameObject;
            if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
            {
                UserMessageManager.Dispatch("Please click on a robot part", 3);
            }
            else if (collisionObject == null)
            {
                Debug.Log("DPM: Game object not found");

            }
            else if (collisionObject.transform.parent == transform)
            {
                if (definingIntake)
                {
                    intakeNode[index] = collisionObject;
                    SetInteractor(intakeNode[index], index);

                    UserMessageManager.Dispatch(collisionObject.name + " has been selected as intake node", 5);

                    definingIntake = false;
                }
                else
                {
                    releaseNode[index] = collisionObject;
                    SetInteractor(releaseNode[index], index);

                    UserMessageManager.Dispatch(collisionObject.name + " has been selected as release node", 5);

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
            GameObject collisionObject = (rayResult.CollisionObject.UserObject as BRigidBody).gameObject;
            if (rayResult.CollisionObject.CollisionFlags == BulletSharp.CollisionFlags.StaticObject)
            {
                RevertNodeColors(hoveredNode, hoveredColors);
            }
            else if (collisionObject == null)
            {
                Debug.Log("DPM: Game object not found");
                RevertNodeColors(hoveredNode, hoveredColors);
            }
            else if (collisionObject.transform.parent == transform)
            {
                if (hoveredNode != collisionObject)
                {
                    RevertNodeColors(hoveredNode, hoveredColors);
                }

                hoveredNode = collisionObject;

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

    public void HighlightNode(GameObject node)
    {
        RevertHighlight();
        highlightedNode = node;
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

    /// <summary>
    /// Saves all the configured values into a text file for future access
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
    /// Tries to load a text file from a set directory. If the file exists, sets the robot's configuration to match the file contents.
    /// </summary>
    /// <param name="robotDirectory"></param>
    public void Load(string robotDirectory)
    {
        string filePath = robotDirectory + "\\dpmConfig.txt";
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
                    else intakeNode[index] = AuxFunctions.FindObject(gameObject, line);
                }
                else if (counter == 4)
                {
                    if (line.Equals("#Release Position")) counter++;
                    else releaseNode[index] = AuxFunctions.FindObject(gameObject, line);
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

            for (int i = 0; i < 2; i++)
            {
                SetInteractor(intakeNode[i], i);
                releaseVelocityVector[i] = VelocityToVector3(releaseVelocity[i][0], releaseVelocity[i][1], releaseVelocity[i][2]);
            }
                        
        }
    }

    /// <summary>
    /// Converts a line reading "float1|float2|float3" into Vector3 with the 3 float values as it's x,y,z componets respectively.
    /// </summary>
    /// <param name="aData">the line to deserialize</param>
    /// <returns>the result vector 3</returns>
    public static UnityEngine.Vector3 DeserializeVector3Array(string aData)
    {
        UnityEngine.Vector3 result = new UnityEngine.Vector3(0, 0, 0);
        string[] values = aData.Split('|');
        //Debug.Log(values[0]);
        if (values.Length != 3)
            throw new System.FormatException("component count mismatch. Expected 3 components but got " + values.Length);
        result = new UnityEngine.Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        return result;
    }

    /// <summary>
    /// Converts a line reading "float1|float2|float3" into a float array with those 3 values.
    /// </summary>
    /// <param name="aData">the line to deserialize</param>
    /// <returns>the result float array</returns>
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

    /// <summary>
    /// Receives the user input and processes various functions based on input
    /// The processingIndex variable is alternated between to ensure that the primary and secondary controls do not have precedent over the other.
    /// </summary>
    private void ProcessControls()
    {
        if (processingIndex == 0)
        {
            if ((InputControl.GetButton(Controls.buttons[controlIndex].pickupPrimary)))
            {

                Intake(0);
            }
            if ((InputControl.GetButton(Controls.buttons[controlIndex].pickupSecondary)))
            {
                Intake(1);
            }
            if ((InputControl.GetButtonDown(Controls.buttons[controlIndex].releasePrimary)))
            {
                ReleaseGamepiece(0);
            }
            else
            {
                HoldGamepiece(0);
            }
            if ((InputControl.GetButtonDown(Controls.buttons[controlIndex].releaseSecondary)))
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
            if ((InputControl.GetButton(Controls.buttons[controlIndex].pickupSecondary)))
            {

                Intake(1);
            }
            if ((InputControl.GetButton(Controls.buttons[controlIndex].pickupPrimary)))
            {
                Intake(0);
            }
            if ((InputControl.GetButtonDown(Controls.buttons[controlIndex].releaseSecondary)))
            {
                ReleaseGamepiece(1);
            }
            else
            {
                HoldGamepiece(1);
            }   
            if ((InputControl.GetButtonDown(Controls.buttons[controlIndex].releasePrimary)))
            {
                ReleaseGamepiece(0);
            }
            else
            {
                HoldGamepiece(0);
            }
            processingIndex = 0;
        }

        if ((InputControl.GetButtonDown(Controls.buttons[controlIndex].spawnPrimary))) SpawnGamepiece(0);
        if ((InputControl.GetButtonDown(Controls.buttons[controlIndex].spawnSecondary))) SpawnGamepiece(1);
    }

    private void OnDestroy()
    {
        modeEnabled = false;
    }
}