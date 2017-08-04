using UnityEngine;
using System.Collections;
using BulletUnity;
using BulletSharp;
using System;
using System.Collections.Generic;
using BulletSharp.SoftBody;
using UnityEngine.SceneManagement;
using System.IO;
using Assets.Scripts.FEA;
using Assets.Scripts.FSM;
using System.Linq;

public class MainState : SimState
{

    public static bool draggingWindow = false;
    private const int SolverIterations = 100;

    private BPhysicsWorld physicsWorld;
    private int lastFrameCount;

    private bool tracking;
    private bool awaitingReplay;

    private UnityPacket unityPacket;

    private List<Robot> robots;
    public Robot activeRobot { get; private set; }

    private DynamicCamera dynamicCamera;
    public GameObject dynamicCameraObject;

    private RobotCamera robotCamera;
    public GameObject robotCameraObject;

    //Testing camera location, can be deleted later
    private Vector3 robotCameraPosition = new Vector3(0f, 0.5f, 0f);
    private Vector3 robotCameraRotation = new Vector3(0f, 0f, 0f);
    private Vector3 robotCameraPosition2 = new Vector3(0f, 0f, 0f);
    private Vector3 robotCameraRotation2 = new Vector3(0f, 0f, 0f);
    private Vector3 robotCameraPosition3 = new Vector3(0f, 0.5f, 0f);
    private Vector3 robotCameraRotation3 = new Vector3(0f, 180f, 0f);
    //Testing camera location, can be deleted later

    private GameObject fieldObject;
    private UnityFieldDefinition fieldDefinition;

    public bool IsResetting;
    private const float HOLD_TIME = 0.8f;
    private float keyDownTime = 0f;

    private OverlayWindow oWindow;

    private FixedQueue<List<ContactDescriptor>> contactPoints;

    public List<Tracker> Trackers { get; private set; }

    private string fieldPath;
    private string robotPath;

    public List<Robot> SpawnedRobots { get; private set; }
    private const int MAX_ROBOTS = 6;


    /// <summary>
    /// Called when the script instance is being initialized.
    /// Initializes the bullet physics environment
    /// </summary>
    public override void Awake()
    {
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
        GImpactCollisionAlgorithm.RegisterAlgorithm((CollisionDispatcher)BPhysicsWorld.Get().world.Dispatcher);
        BPhysicsWorld.Get().DebugDrawMode = DebugDrawModes.DrawWireframe | DebugDrawModes.DrawConstraints | DebugDrawModes.DrawConstraintLimits;
        BPhysicsWorld.Get().DoDebugDraw = false;
        ((DynamicsWorld)BPhysicsWorld.Get().world).SolverInfo.NumIterations = SolverIterations;
    }

    /// <summary>
    /// Called after Awake() when the script instance is enabled.
    /// Initializes variables then loads the field and robot as well as setting up replay features.
    /// </summary>
    public override void Start()
    {
        //getting bullet physics information
        physicsWorld = BPhysicsWorld.Get();
        lastFrameCount = physicsWorld.frameCount;

        //setting up replay
        Trackers = new List<Tracker>();
        contactPoints = new FixedQueue<List<ContactDescriptor>>(Tracker.Length);

        //starts a new instance of unity packet which receives packets from the driver station
        unityPacket = new UnityPacket();
        unityPacket.Start();

        //loads all the controls
        Controls.Load();

        //If a replay has been selected, load the replay. Otherwise, load the field and robot.
        string selectedReplay = PlayerPrefs.GetString("simSelectedReplay");

        SpawnedRobots = new List<Robot>();

        if (string.IsNullOrEmpty(selectedReplay))
        {
            tracking = true;
            Debug.Log(LoadField(PlayerPrefs.GetString("simSelectedField")) ? "Load field success!" : "Load field failed.");
            Debug.Log(LoadRobot(PlayerPrefs.GetString("simSelectedRobot")) ? "Load robot success!" : "Load robot failed.");
        }
        else
        {
            awaitingReplay = true;
            LoadReplay(selectedReplay);
        }

        //initializes the dynamic camera
        dynamicCameraObject = GameObject.Find("Main Camera");
        dynamicCamera = dynamicCameraObject.AddComponent<DynamicCamera>();

        DynamicCamera.MovingEnabled = true;
    }

    /// <summary>
    /// Called every step of the program to listen to input commands for various features
    /// </summary>
    public override void Update()
    {
     
        //If the reset button is held down after a certain amount of time, then go into change spawnpoint mode (reset spawnpoint feature)
        //Otherwise, reset the robot normally (quick reset feature)
        if (!activeRobot.IsResetting)
        {
            if (Input.GetKeyDown(KeyCode.U)) LoadRobot(robotPath);
            if (Input.GetKeyDown(KeyCode.Y)) SwitchActiveRobot();
        }

        // Toggles between the different camera states if the camera toggle button is pressed
        if ((InputControl.GetButtonDown(Controls.buttons[0].cameraToggle)))
        {
            if (dynamicCameraObject.activeSelf && DynamicCamera.MovingEnabled)
            {
                dynamicCamera.ToggleCameraState(dynamicCamera.cameraState);
            }
        }

        // Switches to replay mode
        if (!activeRobot.IsResetting && Input.GetKeyDown(KeyCode.Tab))
        {
            contactPoints.Add(null);
            StateMachine.Instance.PushState(new ReplayState(fieldPath, robotPath, contactPoints, Trackers));
        }

        UpdateTrackers();
    }

    public override void FixedUpdate()
    {
        //This line is essential for the reset to work accurately
        //robotCameraObject.transform.position = activeRobot.transform.GetChild(0).transform.position;

        UpdateTrackers();

        SendRobotPackets();
    }

    /// <summary>
    /// If a replay has been loaded, this is called at the end of the initialization process to switch to the replay state
    /// </summary>
    public override void LateUpdate()
    {
        if (awaitingReplay)
        {
            awaitingReplay = false;
            StateMachine.Instance.PushState(new ReplayState(fieldPath, robotPath, contactPoints, Trackers));
        }
    }

    /// <summary>
    /// Loads the field from a given directory
    /// </summary>
    /// <param name="directory">field directory</param>
    /// <returns>whether the process was successful</returns>
    bool LoadField(string directory)
    {
        fieldPath = directory;

        fieldObject = new GameObject("Field");

        FieldDefinition.Factory = delegate (Guid guid, string name)
        {
            return new UnityFieldDefinition(guid, name);
        };

        string loadResult;
        fieldDefinition = (UnityFieldDefinition)BXDFProperties.ReadProperties(directory + "\\definition.bxdf", out loadResult);
        Debug.Log(loadResult);
        fieldDefinition.CreateTransform(fieldObject.transform);
        return fieldDefinition.CreateMesh(directory + "\\mesh.bxda");
    }

    /// <summary>
    /// Loads a new robot from a given directory
    /// </summary>
    /// <param name="directory">robot directory</param>
    /// <returns>whether the process was successful</returns>
    public bool LoadRobot(string directory)
    {
        if (SpawnedRobots.Count < MAX_ROBOTS)
        {
            robotPath = directory;

            GameObject robotObject = new GameObject("Robot");
            Robot robot = robotObject.AddComponent<Robot>();

            //Initialiezs the physical robot based off of robot directory. Returns false if not sucessful
            if (!robot.InitializeRobot(directory, this)) return false;

            robotObject.AddComponent<DriverPracticeRobot>().Initialize(directory);

            //If this is the first robot spawned, then set it to be the active robot and initialize the robot camera on it
            if (activeRobot == null)
            {
                activeRobot = robot;

                ////Robot camera feature
                //if (robotCamera == null)
                //{
                //    robotCameraObject = GameObject.Find("RobotCameraList");
                //    robotCamera = robotCameraObject.GetComponent<RobotCamera>();
                //}

                //robotCamera.RemoveCameras();
                ////The camera data should be read here as a foreach loop and included in robot file
                ////Attached to main frame and face the front
                //robotCamera.AddCamera(robotObject.transform.GetChild(0).transform, robotCameraPosition, robotCameraRotation);
                ////Attached to the first node and face the front
                //robotCamera.AddCamera(robotObject.transform.GetChild(1).transform, robotCameraPosition2, robotCameraRotation2);
                ////Attached to main frame and face the back
                //robotCamera.AddCamera(robotObject.transform.GetChild(0).transform, robotCameraPosition3, robotCameraRotation3);

                //robotCameraObject.SetActive(true);
            }

            robot.controlIndex = SpawnedRobots.Count;
            SpawnedRobots.Add(robot);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Changes the active robot to a new robot with a given directory
    /// </summary>
    /// <param name="directory"></param>
    /// <returns>whether the process was successful</returns>
    public bool ChangeRobot(string directory)
    {
        return activeRobot.InitializeRobot(directory, this);
    }

    /// <summary>
    /// Changes the active robot from the current one to the next one in the list
    /// </summary>
    private void SwitchActiveRobot()
    {
        if (SpawnedRobots.Count >= 1)
        {

            if (activeRobot != null)
            {
                int index = SpawnedRobots.IndexOf(activeRobot);
                if (index < SpawnedRobots.Count - 1)
                {
                    activeRobot = SpawnedRobots[index + 1];
                }
                else
                {
                    activeRobot = SpawnedRobots[0];
                }
            }
            else activeRobot = SpawnedRobots[0];
            dynamicCamera.cameraState.robot = activeRobot.gameObject;

        }

        robotCameraObject = GameObject.Find("RobotCameraList");
        robotCamera = robotCameraObject.GetComponent<RobotCamera>();


        //GameObject sensorManager = GameObject.Find("RobotSensorManager");
        //sensorManager.GetComponent<SensorManager>().AddUltrasonicSensor(robotObject.transform.GetChild(0).gameObject, new Vector3(0, 0, 0), new Vector3(0, 0, 0));

        //robotCamera.RemoveCameras();
        ////The camera data should be read here as a foreach loop and included in robot file
        ////Attached to main frame and face the front
        //robotCamera.AddCamera(robotObject.transform.GetChild(0).transform, robotCameraPosition, robotCameraRotation);
        ////Attached to the first node and face the front
        //robotCamera.AddCamera(robotObject.transform.GetChild(1).transform, robotCameraPosition2, robotCameraRotation2);
        ////Attached to main frame and face the back
        //robotCamera.AddCamera(robotObject.transform.GetChild(0).transform, robotCameraPosition3, robotCameraRotation3);


        //robotCameraObject.SetActive(true);
    }

            


    /// <summary>
    /// Changes the active robot to a different robot based on a given index
    /// </summary>
    public void SwitchActiveRobot(int index)
    {
        if (index < SpawnedRobots.Count)
        {
            activeRobot = SpawnedRobots[index];
            dynamicCamera.cameraState.robot = activeRobot.gameObject;
        }
    }

    public void RemoveRobot(int index)
    {
        if (index < SpawnedRobots.Count && SpawnedRobots.Count > 1)
        {
            GameObject.Destroy(SpawnedRobots[index].gameObject);
            SpawnedRobots.RemoveAt(index);
            activeRobot = null;
            SwitchActiveRobot();

            int i = 0;
            foreach (Robot robot in SpawnedRobots)
            {
                robot.controlIndex = i;
                i++;
            }
        }
    }


    #region Replay Functions
    void LoadReplay(string name)
    {
        List<FixedQueue<StateDescriptor>> fieldStates;
        List<FixedQueue<StateDescriptor>> robotStates;
        Dictionary<string, List<FixedQueue<StateDescriptor>>> gamePieceStates;
        List<List<KeyValuePair<ContactDescriptor, int>>> contacts;

        string simSelectedField;
        string simSelectedRobot;

        ReplayImporter.Read(name, out simSelectedField, out simSelectedRobot, out fieldStates, out robotStates, out gamePieceStates, out contacts);

        LoadField(simSelectedField);
        LoadRobot(simSelectedRobot);

        List<Tracker> robotTrackers = Trackers.Where(x => x.transform.parent.name.Equals("Robot")).ToList();
        List<Tracker> fieldTrackers = Trackers.Except(robotTrackers).ToList();

        int i = 0;

        foreach (Tracker t in fieldTrackers)
        {
            t.States = fieldStates[i];
            i++;
        }

        i = 0;

        foreach (Tracker t in robotTrackers)
        {
            t.States = robotStates[i];
            i++;
        }

        foreach (KeyValuePair<string, List<FixedQueue<StateDescriptor>>> k in gamePieceStates)
        {
            GameObject referenceObject = GameObject.Find(k.Key);

            if (referenceObject == null)
                continue;

            foreach (FixedQueue<StateDescriptor> f in k.Value)
            {
                GameObject currentPiece = UnityEngine.Object.Instantiate(referenceObject);
                currentPiece.name = "clone_" + k.Key;
                currentPiece.GetComponent<Tracker>().States = f;
            }
        }

        foreach (var c in contacts)
        {
            if (c != null)
            {
                List<ContactDescriptor> currentContacts = new List<ContactDescriptor>();

                foreach (var d in c)
                {
                    ContactDescriptor currentContact = d.Key;
                    currentContact.RobotBody = robotTrackers[d.Value].GetComponent<BRigidBody>();
                    currentContacts.Add(currentContact);
                }

                contactPoints.Add(currentContacts);
            }
            else
            {
                contactPoints.Add(null);
            }
        }
    }

    private void UpdateTrackers()
    {
        int numSteps = physicsWorld.frameCount - lastFrameCount;

        if (tracking && numSteps > 0)
        {
            foreach (Tracker t in Trackers)
                t.AddState(numSteps);

            for (int i = numSteps; i > 0; i--)
            {
                List<ContactDescriptor> frameContacts = null;

                int numManifolds = physicsWorld.world.Dispatcher.NumManifolds;

                for (int j = 0; j < numManifolds; j++)
                {
                    PersistentManifold contactManifold = physicsWorld.world.Dispatcher.GetManifoldByIndexInternal(j);
                    BRigidBody obA = (BRigidBody)contactManifold.Body0.UserObject;
                    BRigidBody obB = (BRigidBody)contactManifold.Body1.UserObject;

                    if (!obA.gameObject.name.StartsWith("node") && !obB.gameObject.name.StartsWith("node"))
                        continue;

                    ManifoldPoint mp = null;

                    int numContacts = contactManifold.NumContacts;

                    for (int k = 0; k < numContacts; k++)
                    {
                        mp = contactManifold.GetContactPoint(k);

                        if (mp.LifeTime == i)
                            break;
                    }

                    if (mp == null)
                        continue;

                    if (frameContacts == null)
                        frameContacts = new List<ContactDescriptor>();

                    frameContacts.Add(new ContactDescriptor
                    {
                        AppliedImpulse = mp.AppliedImpulse,
                        Position = (mp.PositionWorldOnA + mp.PositionWorldOnB) * 0.5f,
                        RobotBody = obA.name.StartsWith("node") ? obA : obB
                    });
                }

                contactPoints.Add(frameContacts);
            }
        }

        lastFrameCount += numSteps;
    }

    public void StartReplay()
    {
        if (!activeRobot.IsResetting)
        {
            contactPoints.Add(null);
            StateMachine.Instance.PushState(new ReplayState(fieldPath, robotPath, contactPoints, Trackers));
        }
    }

    /// <summary>
    /// Resumes the normal simulation and exits the replay mode, showing all UI elements again
    /// </summary>
    public override void Resume()
    {
        lastFrameCount = physicsWorld.frameCount;
        tracking = true;

        Resources.FindObjectsOfTypeAll<Canvas>()[0].enabled = true;

        contactPoints.Clear(null);
    }

    /// <summary>
    /// Pauses the normal simulation for rpelay mode by disabling tracking of physics objects and disabling UI elements
    /// </summary>
    public override void Pause()
    {
        tracking = false;
        Resources.FindObjectsOfTypeAll<Canvas>()[0].enabled = false;
    }
    #endregion

   
    #region Robot Interaction Functions

    /// <summary>
    /// Starts the resetting process of the active robot
    /// </summary>
    public void BeginRobotReset()
    {
        activeRobot.BeginReset();
    }

    /// <summary>
    /// Ends the restting process of the active robot and resets the replay tracking objects
    /// </summary>
    public void EndRobotReset()
    {
        activeRobot.EndReset();
        foreach (Tracker t in UnityEngine.Object.FindObjectsOfType<Tracker>())
        {
            t.Clear();

            contactPoints.Clear(null);
        }
    }

    /// <summary>
    /// Shifts the active robot by a set transposition vector
    /// </summary>
    public void TransposeRobot(Vector3 transposition)
    {
        activeRobot.TransposeRobot(transposition);
    }

    /// <summary>
    /// Rotates the active robot about its origin by a mathematical 4x4 matrix
    /// </summary>
    public void RotateRobot(BulletSharp.Math.Matrix rotationMatrix)
    {
        activeRobot.RotateRobot(rotationMatrix);
    }

    /// <summary>
    /// Rotates the active robot about its origin by a set vector
    /// </summary>
    public void RotateRobot(Vector3 rotation)
    {
        activeRobot.RotateRobot(rotation);
    }

    /// <summary>
    /// Resets the active robot orientation to how the CAD model was originally defined (should be standing upright and facing forward if CAD was done properly)
    /// </summary>
    public void ResetRobotOrientation()
    {
        activeRobot.ResetRobotOrientation();
    }

    /// <summary>
    /// Saves the active robot's current orientation to be used whenever robot is reset
    /// </summary>
    public void SaveRobotOrientation()
    {
        activeRobot.SaveRobotOrientation();
    }

    /// <summary>
    /// Sends the received packets to the active robot
    /// </summary>
    private void SendRobotPackets()
    {
        activeRobot.Packet = unityPacket.GetLastPacket();
        foreach (Robot robot in SpawnedRobots)
        {
            if (robot != activeRobot) robot.Packet = new UnityPacket.OutputStatePacket();
        }
    }
    #endregion
}