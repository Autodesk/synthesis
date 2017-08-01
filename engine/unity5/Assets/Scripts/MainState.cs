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

    const float ResetVelocity = 0.05f;
    private const int SolverIterations = 100;

    private BPhysicsWorld physicsWorld;
    private int lastFrameCount;

    private bool tracking;
    private bool awaitingReplay;

    private UnityPacket unityPacket;

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

    //=================================IN PROGRESS=============================
    //private UltraSensor ultraSensor;
    //private GameObject ultraSensorObject;
    //=========================================================================

    private GameObject fieldObject;
    private UnityFieldDefinition fieldDefinition;

    private GameObject robotObject;
    private RigidNode_Base rootNode;

    private Vector3 robotStartPosition = new Vector3(0f, 1f, 0f);
    private Vector3 nodeToRobotOffset;
    private BulletSharp.Math.Matrix robotStartOrientation = BulletSharp.Math.Matrix.Identity;
    private const float HOLD_TIME = 0.8f;
    private float keyDownTime = 0f;

    private List<GameObject> extraElements;

    private OverlayWindow oWindow;

    private System.Random random;

    private FixedQueue<List<ContactDescriptor>> contactPoints;

    //Flags to tell different types of reset
    private bool isResettingOrientation;
    public bool IsResetting { get; set; }

    private DriverPractice driverPractice;

    public List<Tracker> Trackers { get; private set; }

    public static bool ControlsDisabled = false;

    private string fieldPath;
    private string robotPath;

    public RigidNode_Base activeRobot;
    public List<RigidNode_Base> dummyRootNodes = new List<RigidNode_Base>();

    public override void Awake()
    {
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
        GImpactCollisionAlgorithm.RegisterAlgorithm((CollisionDispatcher)BPhysicsWorld.Get().world.Dispatcher);
        BPhysicsWorld.Get().DebugDrawMode = DebugDrawModes.DrawWireframe | DebugDrawModes.DrawConstraints | DebugDrawModes.DrawConstraintLimits;
        BPhysicsWorld.Get().DoDebugDraw = false;
        ((DynamicsWorld)BPhysicsWorld.Get().world).SolverInfo.NumIterations = SolverIterations;
    }

    public override void OnGUI()
    {
        UserMessageManager.Render();
    }

    public override void Start()
    {
        physicsWorld = BPhysicsWorld.Get();
        lastFrameCount = physicsWorld.frameCount;

        Trackers = new List<Tracker>();

        unityPacket = new UnityPacket();
        unityPacket.Start();

        extraElements = new List<GameObject>();

        random = new System.Random();

        contactPoints = new FixedQueue<List<ContactDescriptor>>(Tracker.Length);
        isResettingOrientation = false;

        Controls.Load();

        string selectedReplay = PlayerPrefs.GetString("simSelectedReplay");

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

        dynamicCameraObject = GameObject.Find("Main Camera");
        dynamicCamera = dynamicCameraObject.AddComponent<DynamicCamera>();

        DynamicCamera.MovingEnabled = true;
    }

    public override void Update()
    {
        //Debug.Log(ultraSensor.ReturnOutput());

        //(InputControl.GetButton(Controls.buttons.pwm5Plus))
        //Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.ResetRobot]

        if ((InputControl.GetButtonDown(Controls.buttons.resetRobot)) && !IsResetting)
        {
            keyDownTime = Time.time;
        }
        if ((InputControl.GetButtonUp(Controls.buttons.resetRobot)) && !IsResetting)
        {
            if (Time.time - keyDownTime > HOLD_TIME)
            {
                IsResetting = true;
                BeginReset();
            }
            else
            {
                BeginReset();
                EndReset();
            }
        }

        // Will switch the camera state with the camera toggle button
        if ((InputControl.GetButtonDown(Controls.buttons.cameraToggle)))
        {
            if (dynamicCameraObject.activeSelf && DynamicCamera.MovingEnabled)
            {

                //Toggle afterwards and will not activate dynamic camera
                dynamicCamera.ToggleCameraState(dynamicCamera.cameraState);

            }
        }

        BRigidBody rigidBody = robotObject.GetComponentInChildren<BRigidBody>();

        if (!rigidBody.GetCollisionObject().IsActive)
            rigidBody.GetCollisionObject().Activate();

        if (!IsResetting && Input.GetKeyDown(KeyCode.Tab))
        {
            contactPoints.Add(null);
            StateMachine.Instance.PushState(new ReplayState(fieldPath, robotPath, contactPoints, Trackers));
        }

        UpdateTrackers();
    }

    public override void FixedUpdate()
    {
        if (activeRobot != null)
        {
            UnityPacket.OutputStatePacket packet = unityPacket.GetLastPacket();

            if (!ControlsDisabled) DriveJoints.UpdateAllMotors(activeRobot, packet.dio);
        }

        if (IsResetting)
        {
            Resetting();
        }

        //This line is essential for the reset to work accurately
        robotCameraObject.transform.position = robotObject.transform.GetChild(0).transform.position;

        UpdateTrackers();
    }

    public override void LateUpdate()
    {
        if (awaitingReplay)
        {
            awaitingReplay = false;
            StateMachine.Instance.PushState(new ReplayState(fieldPath, robotPath, contactPoints, Trackers));
        }
    }

    public override void Resume()
    {
        lastFrameCount = physicsWorld.frameCount;
        tracking = true;

        Resources.FindObjectsOfTypeAll<Canvas>()[0].enabled = true;

        contactPoints.Clear(null);
    }

    public override void Pause()
    {
        tracking = false;
        Resources.FindObjectsOfTypeAll<Canvas>()[0].enabled = false;

        ToDynamicCamera();
    }

    bool LoadField(string directory)
    {
        fieldPath = directory;

        fieldObject = new GameObject("Field");

        FieldDefinition.Factory = delegate (Guid guid, string name)
        {
            return new UnityFieldDefinition(guid, name);
        };

        string loadResult;
        //Change to .field file. Maybe FieldProperties? Also need to look at field definition
        fieldDefinition = (UnityFieldDefinition)BXDFProperties.ReadProperties(directory + "\\definition.bxdf", out loadResult);
        Debug.Log(loadResult);
        fieldDefinition.CreateTransform(fieldObject.transform);
        return fieldDefinition.CreateMesh(directory + "\\mesh.bxda");
    }

    bool LoadRobot(string directory)
    {
        robotPath = directory;

        robotObject = new GameObject("Robot");
        robotObject.transform.position = robotStartPosition;

        RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
        {
            return new RigidNode(guid);
        };

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        //Read .robot instead. Maybe need a RobotSkeleton class
        rootNode = BXDJSkeleton.ReadSkeleton(directory + "\\skeleton.bxdj");
        rootNode.ListAllNodes(nodes);

        foreach (RigidNode_Base n in nodes)
        {
            RigidNode node = (RigidNode)n;
            node.CreateTransform(robotObject.transform);

            if (!node.CreateMesh(directory + "\\" + node.ModelFileName))
            {
                Debug.Log("Robot not loaded!");
                UnityEngine.Object.Destroy(robotObject);
                return false;
            }

            node.CreateJoint();

            node.MainObject.AddComponent<Tracker>().Trace = true;

            Tracker t = node.MainObject.GetComponent<Tracker>();
            Debug.Log(t);
        }

        driverPractice = robotObject.AddComponent<DriverPractice>();

        //For Ultrasonic testing purposes
        //ultraSensorObject = GameObject.Find("node_0.bxda");
        //ultraSensor = ultraSensorObject.AddComponent<UltraSensor>();

        nodeToRobotOffset = robotObject.transform.GetChild(0).transform.position - robotObject.transform.position;
        //Robot camera feature
        if (robotCamera == null)
        {
            robotCameraObject = GameObject.Find("RobotCameraList");
            robotCamera = robotCameraObject.AddComponent<RobotCamera>();
        }

        robotCamera.RemoveCameras();
        //The camera data should be read here as a foreach loop and included in robot file
        //Attached to main frame and face the front
        robotCamera.AddCamera(robotObject.transform.GetChild(0).transform, robotCameraPosition, robotCameraRotation);
        //Attached to the first node and face the front
        robotCamera.AddCamera(robotObject.transform.GetChild(1).transform, robotCameraPosition2, robotCameraRotation2);
        //Attached to main frame and face the back
        robotCamera.AddCamera(robotObject.transform.GetChild(0).transform, robotCameraPosition3, robotCameraRotation3);

        robotCameraObject.SetActive(true);


        RotateRobot(robotStartOrientation);

        activeRobot = rootNode;
        return true;
    }

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

    public bool ChangeRobot(string directory)
    {
        if (GameObject.Find("Robot") != null) GameObject.Destroy(GameObject.Find("Robot"));
        return LoadRobot(directory);
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

    /// <summary>
    /// Return the robot to robotStartPosition and destroy extra game pieces
    /// </summary>
    /// <param name="resetTransform"></param>

    public void BeginReset()
    {
        foreach (Tracker t in UnityEngine.Object.FindObjectsOfType<Tracker>())
            t.Clear();

        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            RigidBody r = (RigidBody)n.MainObject.GetComponent<BRigidBody>().GetCollisionObject();
            r.LinearVelocity = r.AngularVelocity = BulletSharp.Math.Vector3.Zero;
            r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.Zero;

            BulletSharp.Math.Matrix newTransform = r.WorldTransform;
            newTransform.Origin = (robotStartPosition + n.ComOffset).ToBullet();
            newTransform.Basis = BulletSharp.Math.Matrix.Identity;
            r.WorldTransform = newTransform;
        }

        RotateRobot(robotStartOrientation);

        foreach (GameObject g in extraElements)
            UnityEngine.Object.Destroy(g);


        if (IsResetting)
        {
            Debug.Log("is resetting!");
        }
    }

    /// <summary>
    /// Can move robot around in this state, update robotStartPosition if hit enter
    /// </summary>
    void Resetting()
    {
        if (Input.GetMouseButton(1))
        {
            //Transform rotation along the horizontal plane
            Vector3 rotation = new Vector3(0f,
                Input.GetKey(KeyCode.RightArrow) ? ResetVelocity : Input.GetKey(KeyCode.LeftArrow) ? -ResetVelocity : 0f,
                0f);
            if (!rotation.Equals(Vector3.zero))
                RotateRobot(rotation);

        }
        else
        {
            //Transform position
            Vector3 transposition = new Vector3(
                Input.GetKey(KeyCode.RightArrow) ? ResetVelocity : Input.GetKey(KeyCode.LeftArrow) ? -ResetVelocity : 0f,
                0f,
                Input.GetKey(KeyCode.UpArrow) ? ResetVelocity : Input.GetKey(KeyCode.DownArrow) ? -ResetVelocity : 0f);

            if (!transposition.Equals(Vector3.zero))
                TransposeRobot(transposition);
        }

        //Update robotStartPosition when hit enter
        if (Input.GetKey(KeyCode.Return))
        {
            robotStartOrientation = ((RigidNode)rootNode.ListAllNodes()[0]).MainObject.GetComponent<BRigidBody>().GetCollisionObject().WorldTransform.Basis;
            robotStartPosition = robotObject.transform.GetChild(0).transform.position - nodeToRobotOffset;
            //Debug.Log(robotStartPosition);
            EndReset();
        }
    }

    /// <summary>
    /// Put robot back down and switch back to normal state
    /// </summary>
    public void EndReset()
    {
        IsResetting = false;
        isResettingOrientation = false;

        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            RigidBody r = (RigidBody)n.MainObject.GetComponent<BRigidBody>().GetCollisionObject();
            r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
        }

        foreach (Tracker t in UnityEngine.Object.FindObjectsOfType<Tracker>())
        {
            t.Clear();

            contactPoints.Clear(null);

        }
    }

    public void TransposeRobot(Vector3 transposition)
    {
        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            RigidBody r = (RigidBody)n.MainObject.GetComponent<BRigidBody>().GetCollisionObject();

            BulletSharp.Math.Matrix newTransform = r.WorldTransform;
            newTransform.Origin += transposition.ToBullet();
            r.WorldTransform = newTransform;
        }
    }

    public void RotateRobot(BulletSharp.Math.Matrix rotationMatrix)
    {
        BulletSharp.Math.Vector3? origin = null;

        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            RigidBody r = (RigidBody)n.MainObject.GetComponent<BRigidBody>().GetCollisionObject();

            if (origin == null)
                origin = r.CenterOfMassPosition;

            BulletSharp.Math.Matrix rotationTransform = new BulletSharp.Math.Matrix();
            rotationTransform.Basis = rotationMatrix;
            rotationTransform.Origin = BulletSharp.Math.Vector3.Zero;

            BulletSharp.Math.Matrix currentTransform = r.WorldTransform;
            BulletSharp.Math.Vector3 pos = currentTransform.Origin;
            currentTransform.Origin -= origin.Value;
            currentTransform *= rotationTransform;
            currentTransform.Origin += origin.Value;

            r.WorldTransform = currentTransform;
        }
    }

    public void RotateRobot(Vector3 rotation)
    {
        RotateRobot(BulletSharp.Math.Matrix.RotationYawPitchRoll(rotation.y, rotation.z, rotation.x));
    }


    //Helper methods to avoid conflicts between main camera and robot cameras
    void ToDynamicCamera()
    {
        dynamicCameraObject.SetActive(true);
        //robotCameraObject.SetActive(false);
        if (robotCameraObject.GetComponent<RobotCamera>().CurrentCamera != null)
        {
            robotCameraObject.GetComponent<RobotCamera>().CurrentCamera.SetActive(false);
        }
    }

    void ToRobotCamera()
    {
        dynamicCameraObject.SetActive(false);
        //robotCameraObject.SetActive(true);
        if (robotCameraObject.GetComponent<RobotCamera>().CurrentCamera != null)
        {
            robotCameraObject.GetComponent<RobotCamera>().CurrentCamera.SetActive(true);
        }
        else
        {
            UserMessageManager.Dispatch("No camera on robot", 2);
        }
    }

    public DriverPractice GetDriverPractice()
    {
        return driverPractice;
    }

    public void ResetRobotOrientation()
    {
        robotStartOrientation = BulletSharp.Math.Matrix.Identity;
        BeginReset();
        EndReset();
    }

    public void SaveRobotOrientation()
    {
        robotStartOrientation = ((RigidNode)rootNode.ListAllNodes()[0]).MainObject.GetComponent<BRigidBody>().GetCollisionObject().WorldTransform.Basis;
        robotStartOrientation.ToUnity();
        EndReset();
    }

    public bool SpawnDummyRobot(string directory)
    {

        GameObject dummyObject = new GameObject("DummyRobot");
        dummyObject.transform.position = robotStartPosition;

        RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
        {
            return new RigidNode(guid);
        };

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        //Read .robot instead. Maybe need a RobotSkeleton class
        RigidNode_Base dummyRootNode = BXDJSkeleton.ReadSkeleton(directory + "\\skeleton.bxdj");
        
        dummyRootNode.ListAllNodes(nodes);
        dummyRootNodes.Add(dummyRootNode);
        foreach (RigidNode_Base n in nodes)
        {
            RigidNode node = (RigidNode)n;
            node.CreateTransform(dummyObject.transform);

            if (!node.CreateMesh(directory + "\\" + node.ModelFileName))
            {
                Debug.Log("Robot not loaded!");
                UnityEngine.Object.Destroy(dummyObject);
                return false;
            }

            node.CreateJoint();

            node.MainObject.AddComponent<Tracker>().Trace = true;

            Tracker t = node.MainObject.GetComponent<Tracker>();
            Debug.Log(t);
        }
        activeRobot = dummyRootNode;
        return true;
    }

    public RigidNode_Base GetRootNode()
    {
        return rootNode;
    }
}