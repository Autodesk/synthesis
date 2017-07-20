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

public class MainState : SimState
{
    const float ResetVelocity = 0.05f;
    private const int SolverIterations = 100;

    private BPhysicsWorld physicsWorld;
    private int lastFrameCount;

    private bool tracking;

    private UnityPacket unityPacket;

    private DynamicCamera dynamicCamera;
    private GameObject dynamicCameraObject;

    private RobotCamera robotCamera;
    private GameObject robotCameraObject;

    //Testing camera location, can be deleted later
    private Vector3 robotCameraPosition = new Vector3(0f, 0.5f, 0f);
    private Vector3 robotCameraRotation = new Vector3(0f, 0f, 0f);
    private Vector3 robotCameraPosition2 = new Vector3(0f, 0f, 0f);
    private Vector3 robotCameraRotation2 = new Vector3(0f, 0f, 0f);
    private Vector3 robotCameraPosition3 = new Vector3(0f, 0.5f, 0f);
    private Vector3 robotCameraRotation3 = new Vector3(0f, 90f, 0f);
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

    private Texture2D buttonTexture;
    private Texture2D buttonSelected;
    private Texture2D greyWindowTexture;
    private Texture2D darkGreyWindowTexture;
    private Texture2D lightGreyWindowTexture;
    private Texture2D transparentWindowTexture;

    private Font gravityRegular;
    private Font russoOne;

    private GUIController gui;

    private GUIStyle menuWindow;
    private GUIStyle menuButton;

    private OverlayWindow oWindow;

    private System.Random random;

    private FixedQueue<List<ContactDescriptor>> contactPoints;

    //Flags to tell different types of reset
    private bool isResettingOrientation;
    public bool IsResetting { get; private set; }

    private DriverPractice driverPractice;

    public List<Tracker> Trackers { get; private set; }

    public static bool ControlsDisabled = false;

    public override void Awake()
    {
        GImpactCollisionAlgorithm.RegisterAlgorithm((CollisionDispatcher)BPhysicsWorld.Get().world.Dispatcher);
        BPhysicsWorld.Get().DebugDrawMode = DebugDrawModes.DrawWireframe | DebugDrawModes.DrawConstraints | DebugDrawModes.DrawConstraintLimits;
        BPhysicsWorld.Get().DoDebugDraw = false;
        ((DynamicsWorld)BPhysicsWorld.Get().world).SolverInfo.NumIterations = SolverIterations;
    }

    public override void OnGUI()
    {
        if (gui == null)
        {
            //Custom style for windows
            menuWindow = new GUIStyle(GUI.skin.window);
            menuWindow.normal.background = transparentWindowTexture;
            menuWindow.onNormal.background = transparentWindowTexture;
            menuWindow.font = russoOne;

            //Custom style for buttons
            menuButton = new GUIStyle(GUI.skin.button);
            menuButton.font = russoOne;
            menuButton.normal.background = buttonTexture;
            menuButton.hover.background = buttonSelected;
            menuButton.active.background = buttonSelected;
            menuButton.onNormal.background = buttonSelected;
            menuButton.onHover.background = buttonSelected;
            menuButton.onActive.background = buttonSelected;

            gui = new GUIController();
            gui.hideGuiCallback = HideGUI;
            gui.showGuiCallback = ShowGUI;

            gui.AddWindow("Reset Robot", new DialogWindow("Reset Robot", "Quick Reset", "Reset Spawnpoint"), (object o) =>
            {
                HideGUI();
                switch ((int)o)
                {
                    case 0:
                        BeginReset();
                        EndReset();
                        break;
                    case 1:
                        IsResetting = true;
                        BeginReset();
                        break;

                }

            });

            CreateOrientWindow();

            //Added a robot view to toggle among cameras on robot
            gui.AddWindow("Switch View", new DialogWindow("Switch View", "Driver Station", "Orbit Robot", "Freeroam", "Robot view"), (object o) =>
            {
                HideGUI();

                switch ((int)o)
                {
                    case 0:
                        ToDynamicCamera();
                        dynamicCamera.SwitchCameraState(new DynamicCamera.DriverStationState(dynamicCamera));
                        break;
                    case 1:
                        ToDynamicCamera();
                        dynamicCamera.SwitchCameraState(new DynamicCamera.OrbitState(dynamicCamera));
                        DynamicCamera.MovingEnabled = true;
                        break;
                    case 2:
                        ToDynamicCamera();
                        dynamicCamera.SwitchCameraState(new DynamicCamera.FreeroamState(dynamicCamera));
                        break;
                    case 3:
                        if (robotCameraObject.GetComponent<RobotCamera>().CurrentCamera != null)
                        {
                            ToRobotCamera();
                        }
                        break;

                }
            });


            gui.AddWindow("Quit to Main Menu", new DialogWindow("Quit to Main Menu?", "Yes", "No"), (object o) =>
            {
                if ((int)o == 0)
                    SceneManager.LoadScene("MainMenu");
            });

            gui.AddWindow("Quit to Desktop", new DialogWindow("Quit to Desktop?", "Yes", "No"), (object o) =>
            {
                if ((int)o == 0)
                    Application.Quit();
            });
        }

        if (Input.GetMouseButtonUp(0) && !gui.ClickedInsideWindow())
        {
            HideGUI();
            gui.HideAllWindows();
        }

        GUI.Window(1, new Rect(0, 0, gui.GetSidebarWidth(), 25), (int windowID) =>
        {
            if (GUI.Button(new Rect(0, 0, gui.GetSidebarWidth(), 25), "Menu", menuButton))
                gui.EscPressed();
        },
            "",
            menuWindow
        );

        gui.Render();
        UserMessageManager.Render();
    }

    void CreateOrientWindow()
    {
        List<string> titles = new List<string>();
        titles.Add("Left");
        titles.Add("Right");
        titles.Add("Forward");
        titles.Add("Back");
        titles.Add("Save Orientation");
        titles.Add("Close");
        titles.Add("Default");

        List<Rect> rects = new List<Rect>();
        rects.Add(new Rect(40, 200, 105, 35));
        rects.Add(new Rect(245, 200, 105, 35));
        rects.Add(new Rect(147, 155, 105, 35));
        rects.Add(new Rect(147, 245, 105, 35));
        rects.Add(new Rect(110, 95, 190, 35));
        rects.Add(new Rect(270, 50, 90, 35));
        rects.Add(new Rect(50, 50, 90, 35));

        oWindow = new TextWindow("Orient Robot", new Rect((Screen.width / 2) - 150, (Screen.height / 2) - 125, 400, 300),
                                             new string[0], new Rect[0], titles.ToArray(), rects.ToArray());
        //The directional buttons lift the robot to avoid collison with objects, rotates it, and saves the applied rotation to a vector3
        gui.AddWindow("Orient Robot", oWindow, (object o) =>
        {
            if (!isResettingOrientation)
            {
                BeginReset();
                TransposeRobot(new Vector3(0f, 1f, 0f));
                isResettingOrientation = true;
            }

            switch ((int)o)
            {
                case 0:
                    RotateRobot(new Vector3(Mathf.PI * 0.25f, 0f, 0f));

                    break;
                case 1:
                    RotateRobot(new Vector3(-Mathf.PI * 0.25f, 0f, 0f));

                    break;
                case 2:
                    RotateRobot(new Vector3(0f, 0f, Mathf.PI * 0.25f));

                    break;
                case 3:
                    RotateRobot(new Vector3(0f, 0f, -Mathf.PI * 0.25f));

                    break;
                case 4:
                    robotStartOrientation = ((RigidNode)rootNode.ListAllNodes()[0]).MainObject.GetComponent<BRigidBody>().GetCollisionObject().WorldTransform.Basis;
                    robotStartOrientation.ToUnity();
                    EndReset();

                    break;
                case 5:
                    BeginReset();
                    oWindow.Active = false;
                    EndReset();

                    break;
                case 6:
                    robotStartOrientation = BulletSharp.Math.Matrix.Identity;
                    robotStartPosition = new Vector3(0f, 1f, 0f);
                    EndReset();
                    //BeginReset();

                    break;
            }
        });
    }

    void HideGUI()
    {
        gui.guiVisible = false;
        DynamicCamera.MovingEnabled = true;
    }

    void ShowGUI()
    {
        DynamicCamera.MovingEnabled = false;
    }

    public override void Start()
    {
        physicsWorld = BPhysicsWorld.Get();
        lastFrameCount = physicsWorld.frameCount;

        Trackers = new List<Tracker>();
        tracking = true;

        unityPacket = new UnityPacket();
        unityPacket.Start();

        Debug.Log(LoadField(PlayerPrefs.GetString("simSelectedField")) ? "Load field success!" : "Load field failed.");
        Debug.Log(LoadRobot(PlayerPrefs.GetString("simSelectedRobot")) ? "Load robot success!" : "Load robot failed.");

        dynamicCameraObject = GameObject.Find("Main Camera");
        dynamicCamera = dynamicCameraObject.AddComponent<DynamicCamera>();

        extraElements = new List<GameObject>();

        random = new System.Random();

        buttonTexture = Resources.Load("Images/greyButton") as Texture2D;
        buttonSelected = Resources.Load("Images/selectedbuttontexture") as Texture2D;
        gravityRegular = Resources.Load("Fonts/Gravity-Regular") as Font;
        russoOne = Resources.Load("Fonts/Russo_One") as Font;
        greyWindowTexture = Resources.Load("Images/greyBackground") as Texture2D;
        darkGreyWindowTexture = Resources.Load("Images/darkGreyBackground") as Texture2D;
        lightGreyWindowTexture = Resources.Load("Images/lightGreyBackground") as Texture2D;
        transparentWindowTexture = Resources.Load("Images/transparentBackground") as Texture2D;

        contactPoints = new FixedQueue<List<ContactDescriptor>>(Tracker.Length);
        isResettingOrientation = false;

        Controls.LoadControls();
    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            gui.EscPressed();
        //Debug.Log(ultraSensor.ReturnOutput());

        if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.ResetRobot]) && !IsResetting)
        {
            keyDownTime = Time.time;
        }
        if (Input.GetKeyUp(Controls.ControlKey[(int)Controls.Control.ResetRobot]) && !IsResetting)
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
        if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.CameraToggle]))
        {
            if (dynamicCameraObject.activeSelf && DynamicCamera.MovingEnabled)
            {
                //Switch to robot camera after Freeroam (make sure robot camera exists first)
                if (dynamicCamera.cameraState.GetType().Equals(typeof(DynamicCamera.OverviewState))
                    && robotCameraObject.GetComponent<RobotCamera>().CurrentCamera != null)
                {
                    ToRobotCamera();
                }

                //Toggle afterwards and will not activate dynamic camera
                dynamicCamera.ToggleCameraState(dynamicCamera.cameraState);

            }
            else if (robotCameraObject.activeSelf)
            {
                //Switch to dynamic camera after the last camera
                if (robotCamera.IsLastCamera())
                {
                    //Need to toggle before switching to dynamic because toggling will activate current camera
                    robotCamera.ToggleCamera();
                    ToDynamicCamera();
                }
                else
                {
                    robotCamera.ToggleCamera();
                }
            }
        }

        UpdateTrackers();
    }

    public override void FixedUpdate()
    {

        if (rootNode != null)
        {
            UnityPacket.OutputStatePacket packet = unityPacket.GetLastPacket();

            if (!ControlsDisabled) DriveJoints.UpdateAllMotors(rootNode, packet.dio);
        }
        
        if (IsResetting)
        {
            Resetting();
        }

        BRigidBody rigidBody = robotObject.GetComponentInChildren<BRigidBody>();


        if (!rigidBody.GetCollisionObject().IsActive)
            rigidBody.GetCollisionObject().Activate();


        if (!IsResetting && Input.GetKey(KeyCode.Space))
        {
            contactPoints.Add(null);
            StateMachine.Instance.PushState(new ReplayState(contactPoints, Trackers));
        }

        robotCameraObject.transform.position = robotObject.transform.GetChild(0).transform.position;

        UpdateTrackers();
    }

    public override void Resume()
    {
        lastFrameCount = physicsWorld.frameCount;
        tracking = true;

        contactPoints.Clear(null);
    }

    public override void Pause()
    {
        tracking = false;

        ToDynamicCamera();
    }

    bool LoadField(string directory)
    {
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
        robotCameraObject = GameObject.Find("RobotCameraList");
        robotCamera = robotCameraObject.AddComponent<RobotCamera>();

        //The camera data should be read here as a foreach loop and included in robot file
        robotCamera.AddCamera(robotObject.transform.GetChild(0).transform, robotCameraPosition, robotCameraRotation);
        robotCamera.AddCamera(robotObject.transform.GetChild(1).transform, robotCameraPosition2, robotCameraRotation2);
        robotCamera.AddCamera(robotObject.transform.GetChild(0).transform, robotCameraPosition3, robotCameraRotation3);

        robotCameraObject.SetActive(false);


        RotateRobot(robotStartOrientation);

        return true;
    }

    private void UpdateTrackers()
    {
        int numSteps = physicsWorld.frameCount - lastFrameCount;

        if (tracking)
        {
            // numSteps should only == 1 or 0, but if it's ever > 1 the system won't break.
            for (int i = 0; i < numSteps; i++)
            {
                foreach (Tracker t in Trackers)
                    t.AddState();

                List<ContactDescriptor> frameContacts = null;

                int numManifolds = physicsWorld.world.Dispatcher.NumManifolds;

                for (int j = 0; j < numManifolds; j++)
                {
                    PersistentManifold contactManifold = physicsWorld.world.Dispatcher.GetManifoldByIndexInternal(j);
                    BRigidBody obA = (BRigidBody)contactManifold.Body0.UserObject;
                    BRigidBody obB = (BRigidBody)contactManifold.Body1.UserObject;

                    if (!obA.gameObject.name.StartsWith("node") && !obB.gameObject.name.StartsWith("node"))
                        continue;

                    List<ContactDescriptor> manifoldContacts = new List<ContactDescriptor>();

                    int numContacts = contactManifold.NumContacts;

                    if (numContacts == 0)
                        continue;

                    for (int k = 0; k < numContacts; k++)
                    {
                        ManifoldPoint cp = contactManifold.GetContactPoint(k);

                        manifoldContacts.Add(new ContactDescriptor
                        {
                            AppliedImpulse = cp.AppliedImpulse,
                            Position = (cp.PositionWorldOnA + cp.PositionWorldOnB) * 0.5f
                        });
                    }

                    ContactDescriptor consolidatedContact;

                    if (obA.gameObject.name.StartsWith("node"))
                        consolidatedContact = new ContactDescriptor
                        {
                            RobotBody = obA,
                            OtherBody = obB,
                        };
                    else
                        consolidatedContact = new ContactDescriptor
                        {
                            RobotBody = obB,
                            OtherBody = obA
                        };

                    foreach (ContactDescriptor cd in manifoldContacts)
                    {
                        consolidatedContact.AppliedImpulse += cd.AppliedImpulse;
                        consolidatedContact.Position += cd.Position;
                    }

                    consolidatedContact.Position /= numContacts;

                    if (frameContacts == null)
                        frameContacts = new List<ContactDescriptor>();

                    frameContacts.Add(consolidatedContact);
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
    void BeginReset(bool resetTransform = true)
    {
        foreach (Tracker t in UnityEngine.Object.FindObjectsOfType<Tracker>())
            t.Clear();

        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            RigidBody r = (RigidBody)n.MainObject.GetComponent<BRigidBody>().GetCollisionObject();
            r.LinearVelocity = r.AngularVelocity = BulletSharp.Math.Vector3.Zero;
            r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.Zero;

            if (!resetTransform)
                continue;

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
    void EndReset()
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

    void TransposeRobot(Vector3 transposition)
    {
        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            RigidBody r = (RigidBody)n.MainObject.GetComponent<BRigidBody>().GetCollisionObject();

            BulletSharp.Math.Matrix newTransform = r.WorldTransform;
            newTransform.Origin += transposition.ToBullet();
            r.WorldTransform = newTransform;
        }
    }

    void RotateRobot(BulletSharp.Math.Matrix rotationMatrix)
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

    void RotateRobot(Vector3 rotation)
    {
        RotateRobot(BulletSharp.Math.Matrix.RotationYawPitchRoll(rotation.y, rotation.z, rotation.x));
    }


    //Helper methods to avoid conflicts between main camera and robot cameras
    void ToDynamicCamera()
    {
        dynamicCameraObject.SetActive(true);
        robotCameraObject.SetActive(false);
        if (robotCameraObject.GetComponent<RobotCamera>().CurrentCamera != null)
        {
            robotCameraObject.GetComponent<RobotCamera>().CurrentCamera.SetActive(false);
        }
    }

    void ToRobotCamera()
    {
        dynamicCameraObject.SetActive(false);
        robotCameraObject.SetActive(true);
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
}