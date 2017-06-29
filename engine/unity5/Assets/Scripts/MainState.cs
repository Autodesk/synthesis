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
    const float RESET_VELOCITY = 0.05f;

    private UnityPacket unityPacket;

    private DynamicCamera dynamicCamera;
    
    private GameObject fieldObject;
    private UnityFieldDefinition fieldDefinition;

    private GameObject robotObject;
    private RigidNode_Base rootNode;
    
    private Vector3 robotStartPosition = new Vector3(0f, 1f, 0f);
    private BulletSharp.Math.Matrix robotStartOrientation = BulletSharp.Math.Matrix.Identity;

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

    bool resetting;

    public override void Awake()
    {
        GImpactCollisionAlgorithm.RegisterAlgorithm((CollisionDispatcher)BPhysicsWorld.Get().world.Dispatcher);
        BPhysicsWorld.Get().DebugDrawMode = DebugDrawModes.DrawWireframe | DebugDrawModes.DrawConstraints | DebugDrawModes.DrawConstraintLimits;
        BPhysicsWorld.Get().DoDebugDraw = false;
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

            gui.AddAction("Reset Robot", () =>
            {
                BeginReset();
                EndReset();
            });

            CreateOrientWindow();

            gui.AddWindow("Switch View", new DialogWindow("Switch View", "Driver Station", "Orbit Robot", "Freeroam"), (object o) =>
                {
                    HideGUI();

                    switch ((int)o)
                    {
                        case 0:
                            dynamicCamera.SwitchCameraState(new DynamicCamera.DriverStationState(dynamicCamera));            
                            break;
                        case 1:
                            dynamicCamera.SwitchCameraState(new DynamicCamera.OrbitState(dynamicCamera));
                            dynamicCamera.EnableMoving();
                            break;
                        case 2:
                            dynamicCamera.SwitchCameraState(new DynamicCamera.FreeroamState(dynamicCamera));
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
        gui.AddWindow("Orient Robot", oWindow, (object o) => {
            if (!resetting)
            {
                BeginReset(false);
                TransposeRobot(new Vector3(0f, 1f, 0f));
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
                    break;
                case 5:
                    oWindow.Active = false;
                    EndReset();
                    break;
                case 6:
                    robotStartOrientation = BulletSharp.Math.Matrix.Identity;
                    EndReset();
                    BeginReset();
                    break;
            }
        });
    }

    void HideGUI()
    {
        gui.guiVisible = false;
        dynamicCamera.EnableMoving();
    }

    void ShowGUI()
    {
        dynamicCamera.DisableMoving();
    }

    public override void Start()
    {
        FixedQueue<int> queue = new FixedQueue<int>(100);

        for (int i = 0; i < 150; i++)
            queue.Add(i);

        for (int i = 0; i < queue.Length; i++)
            Debug.Log(queue[i]);

        unityPacket = new UnityPacket();
        unityPacket.Start();

        Debug.Log(LoadField(PlayerPrefs.GetString("simSelectedField")) ? "Load field success!" : "Load field failed.");
        Debug.Log(LoadRobot(PlayerPrefs.GetString("simSelectedRobot")) ? "Load robot success!" : "Load robot failed.");
        
        dynamicCamera = GameObject.Find("Main Camera").AddComponent<DynamicCamera>();

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

        resetting = false;
    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            gui.EscPressed();
	}

    public override void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.M))
            SceneManager.LoadScene("MainMenu");

        if (rootNode != null)
        {
            UnityPacket.OutputStatePacket packet = unityPacket.GetLastPacket();

            DriveJoints.UpdateAllMotors(rootNode, packet.dio);
        }

        BRigidBody rigidBody = robotObject.GetComponentInChildren<BRigidBody>();

        if (Input.GetKey(Controls.ControlKey[(int)Controls.Control.ResetRobot]))
        {
            if (!resetting)
            {
                foreach (GameObject g in extraElements)
                    UnityEngine.Object.Destroy(g);

                BeginReset();
            }

            Vector3 transposition = new Vector3(
                Input.GetKey(KeyCode.RightArrow) ? RESET_VELOCITY : Input.GetKey(KeyCode.LeftArrow) ? -RESET_VELOCITY : 0f,
                0f,
                Input.GetKey(KeyCode.UpArrow) ? RESET_VELOCITY : Input.GetKey(KeyCode.DownArrow) ? -RESET_VELOCITY : 0f);

            if (!transposition.Equals(Vector3.zero))
                TransposeRobot(transposition);
        }
        else if (oWindow != null && !oWindow.Active && resetting)
        {
            EndReset();
        }

        if (!rigidBody.GetCollisionObject().IsActive)
            rigidBody.GetCollisionObject().Activate();

        if (Input.GetKey(KeyCode.A))
            StateMachine.Instance.PushState(new ReplayState());
    }

    bool LoadField(string directory)
    {
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

    bool LoadRobot(string directory)
    {
        robotObject = new GameObject("Robot");
        robotObject.transform.position = robotStartPosition;

        RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
        {
            return new RigidNode(guid);
        };

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
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

        //Delet this
        robotObject.AddComponent<DriverPractice>();

        return true;
    }

    void BeginReset(bool resetTransform = true)
    {
        resetting = true;

        foreach (Tracker t in UnityEngine.Object.FindObjectsOfType<Tracker>())
        {
            t.Tracking = false;
            t.Clear();
        }

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
    }

    void EndReset()
    {
        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            RigidBody r = (RigidBody)n.MainObject.GetComponent<BRigidBody>().GetCollisionObject();
            r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
        }

        foreach (Tracker t in UnityEngine.Object.FindObjectsOfType<Tracker>())
        {
            t.Clear();
            t.Tracking = true;
        }

        resetting = false;
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
}
