using UnityEngine;
using System.Collections;
using BulletUnity;
using BulletSharp;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.FEA;
using Assets.Scripts.BUExtensions;
using Assets.Scripts.FSM;
using Assets.Scripts;
using Assets.Scripts.Utils;

/// <summary>
/// To be attached to all robot parent objects.
/// Handles all robot-specific interaction such as driving joints, resetting, and orienting robot.
/// </summary>
public class Robot : MonoBehaviour
{
    private bool isInitialized;

    private const float ResetVelocity = 0.05f;
    private const int SolverIterations = 100;

    private RigidNode_Base rootNode;

    private Vector3 robotStartPosition = new Vector3(0f, 1f, 0f);
    private BulletSharp.Math.Matrix robotStartOrientation = BulletSharp.Math.Matrix.Identity;

    private UnityPacket unityPacket;

    private bool isResettingOrientation;
    public bool IsResetting = false;

    private DriverPracticeRobot dpmRobot;

    public bool ControlsEnabled = true;

    private Vector3 nodeToRobotOffset;

    private MainState mainState;

    public UnityPacket.OutputStatePacket Packet;

    public int ControlIndex = 0;

    private const float HOLD_TIME = 0.8f;
    private float keyDownTime = 0f;

    public string RobotDirectory { get; private set; }
    public string RobotName;

    private RobotCameraManager robotCameraManager;

    public GameObject ManipulatorObject;
    private RigidNode_Base manipulatorNode;

    UnityPacket.OutputStatePacket.DIOModule[] emptyDIO = new UnityPacket.OutputStatePacket.DIOModule[2];

    public bool RobotHasManipulator;
    public bool RobotIsMixAndMatch;
    public bool RobotIsMecanum;

    string wheelPath;
    float wheelRadius;
    float wheelFriction;
    float wheelLateralFriction;
    float wheelMass;

    Vector3 offset;

    private DynamicCamera cam;

    //Robot statistics output
    public float Speed { get; private set; }
    private float oldSpeed;
    public float Weight { get; private set; }
    public float AngularVelocity { get; private set; }
    public float Acceleration { get; private set; }

    /// <summary>
    /// Called when robot is first initialized
    /// </summary>
    void Start()
    {

        StateMachine.Instance.Link<MainState>(this);
    }

    /// <summary>
    /// Called once per frame to ensure all rigid bodie components are activated
    /// </summary>
    void Update()
    {
        BRigidBody rigidBody = GetComponentInChildren<BRigidBody>();

        if (rigidBody == null)
        {
            AppModel.ErrorToMenu("Could not generate robot physics data.");
            return;
        }

        if (!IsResetting)
        {
            if (InputControl.GetButtonDown(Controls.buttons[ControlIndex].resetRobot) && !MixAndMatchMode.setPresetPanelOpen)
            {
                keyDownTime = Time.time;
            }

            else if (InputControl.GetButton(Controls.buttons[ControlIndex].resetRobot) &&  !MixAndMatchMode.setPresetPanelOpen &&
                !mainState.DynamicCameraObject.GetComponent<DynamicCamera>().cameraState.GetType().Equals(typeof(DynamicCamera.ConfigurationState)))
            {
                if (Time.time - keyDownTime > HOLD_TIME)
                {
                    IsResetting = true;
                    BeginReset();
                }
            }

            else if (InputControl.GetButtonUp(Controls.buttons[ControlIndex].resetRobot) && !MixAndMatchMode.setPresetPanelOpen)
            {
                BeginReset();
                EndReset();
            }
        }
        else if (!rigidBody.GetCollisionObject().IsActive)
        {
            rigidBody.GetCollisionObject().Activate();
        }
    }

    /// <summary>
    /// Called once every physics step (framerate independent) to drive motor joints as well as handle the resetting of the robot
    /// </summary>
    void FixedUpdate()
    {
        if (rootNode != null && ControlsEnabled)
        {

            if (Packet != null) DriveJoints.UpdateAllMotors(rootNode, Packet.dio, ControlIndex, RobotIsMecanum);
            else DriveJoints.UpdateAllMotors(rootNode, emptyDIO, ControlIndex, RobotIsMecanum);

            if (RobotHasManipulator)
            {
                DriveJoints.UpdateManipulatorMotors(manipulatorNode, emptyDIO, ControlIndex);
            }
        }

        if (IsResetting)
        {
            Resetting();
        }

        UpdateStats();
    }

    /// <summary>
    /// Initializes physical robot based off of robot directory.
    /// </summary>
    /// <param name="directory">folder directory of robot</param>
    /// <returns></returns>
    public bool InitializeRobot(string directory, MainState source)
    {
        RobotIsMecanum = false;

        if (RobotIsMixAndMatch)
        {
            wheelPath = RobotTypeManager.WheelPath;
            wheelFriction = RobotTypeManager.WheelFriction;
            wheelLateralFriction = RobotTypeManager.WheelLateralFriction;
            wheelRadius = RobotTypeManager.WheelRadius;
            wheelMass = RobotTypeManager.WheelMass;

            RobotIsMecanum = RobotTypeManager.IsMecanum;
        }

        #region Robot Initialization
        RobotDirectory = directory;

        //Deletes all nodes if any exist, take the old node transforms out from the robot object
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            //If this isn't done, the game object is destroyed but the parent-child transform relationship remains!
            child.parent = null;
            Destroy(child.gameObject);
        }

        //Detach and destroy all sensors on the original robot
        SensorManager sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
        sensorManager.ResetSensorLists();



        //Removes Driver Practice component if it exists
        if (dpmRobot != null)
        {
            Destroy(dpmRobot);
        }

        mainState = source; //stores the main state object

        transform.position = robotStartPosition; //Sets the position of the object to the set spawn point

        if (!File.Exists(directory + "\\skeleton.bxdj"))
            return false;

        //Loads the node and skeleton data
        RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
        {
            return new RigidNode(guid);
        };
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        rootNode = BXDJSkeleton.ReadSkeleton(directory + "\\skeleton.bxdj");
        rootNode.ListAllNodes(nodes);

        //Initializes the wheel variables
        int numWheels = nodes.Count(x => x.HasDriverMeta<WheelDriverMeta>() && x.GetDriverMeta<WheelDriverMeta>().type != WheelType.NOT_A_WHEEL);
        float collectiveMass = 0f;


        //Initializes the nodes and creates joints for the robot
        if (RobotIsMixAndMatch && !RobotIsMecanum) //If the user is in MaM and the robot they select is not mecanum, create the nodes and replace the wheel meshes to match those selected
        {
            //Load Node_0, the base of the robot
            RigidNode node = (RigidNode)nodes[0];
            node.CreateTransform(transform);

            if (!node.CreateMesh(directory + "\\" + node.ModelFileName, true, wheelMass))
            {
                Debug.Log("Robot not loaded!");
                return false;
            }

            node.CreateJoint(numWheels, RobotIsMixAndMatch);

            if (node.PhysicalProperties != null)
                collectiveMass += node.PhysicalProperties.mass;

            if (node.MainObject.GetComponent<BRigidBody>() != null)
                node.MainObject.AddComponent<Tracker>().Trace = true;

            //Get the wheel mesh data from the file they are stored in. They are stored as .bxda files. This may need to update if exporters/file types change.
            BXDAMesh mesh = new BXDAMesh();
            mesh.ReadFromFile(wheelPath + "\\node_0.bxda");

            List<Mesh> meshList = new List<Mesh>();
            List<Material[]> materialList = new List<Material[]>();

            RigidNode wheelNode = (RigidNode)BXDJSkeleton.ReadSkeleton(wheelPath + "\\skeleton.bxdj");

            Material[] materials = new Material[0];
            AuxFunctions.ReadMeshSet(mesh.meshes, delegate (int id, BXDAMesh.BXDASubMesh sub, Mesh meshu)
            {
                meshList.Add(meshu);

                materials = new Material[meshu.subMeshCount];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = sub.surfaces[i].AsMaterial(true);
                }

                materialList.Add(materials);
            }, true);


            //Loads the other nodes from the original robot
            for (int i = 1; i < nodes.Count; i++)
            {
                node = (RigidNode)nodes[i];
                node.CreateTransform(transform);

                if (!node.CreateMesh(directory + "\\" + node.ModelFileName, true, wheelMass))
                {
                    Debug.Log("Robot not loaded!");
                    return false;
                }

                //If the node is a wheel, destroy the original wheel mesh and replace it with the wheels selected in MaM
                if (node.HasDriverMeta<WheelDriverMeta>())
                {
                    int chldCount = node.MainObject.transform.childCount;
                    for (int j = 0; j < chldCount; j++)
                    {
                        Destroy(node.MainObject.transform.GetChild(j).gameObject);
                    }

                    int k = 0;

                    Vector3? offset = null; 
                    foreach (Mesh meshObject in meshList)
                    {
                        GameObject meshObj = new GameObject(node.MainObject.name + "_mesh");
                        meshObj.transform.parent = node.MainObject.transform;
                        meshObj.AddComponent<MeshFilter>().mesh = meshObject;
                        if (!offset.HasValue)
                        {
                            offset = meshObject.bounds.center;
                        }
                        meshObj.transform.localPosition = -offset.Value;

                        //Take out this line if you want some snazzy pink wheels
                        meshObj.AddComponent<MeshRenderer>().materials = materialList[k];

                        k++;
                    }
                    node.MainObject.GetComponentInChildren<MeshRenderer>().materials = materials;
                }

                //Create the joints that interact with physics
                node.CreateJoint(numWheels, RobotIsMixAndMatch, wheelFriction, wheelLateralFriction);

                if (node.HasDriverMeta<WheelDriverMeta>())
                {
                    node.MainObject.GetComponent<BRaycastWheel>().Radius = wheelRadius;
                }

                if (node.PhysicalProperties != null)
                    collectiveMass += node.PhysicalProperties.mass;

                if (node.MainObject.GetComponent<BRigidBody>() != null)
                    node.MainObject.AddComponent<Tracker>().Trace = true;
            }
        }
        else //Initialize the robot as normal
        {
            //Initializes the nodes
            foreach (RigidNode_Base n in nodes)
            {
                RigidNode node = (RigidNode)n;
                node.CreateTransform(transform);

                if (!node.CreateMesh(directory + "\\" + node.ModelFileName))
                {
                    Debug.Log("Robot not loaded!");
                    return false;
                }

                node.CreateJoint(numWheels, RobotIsMixAndMatch);

                if (node.PhysicalProperties != null)
                    collectiveMass += node.PhysicalProperties.mass;

                if (node.MainObject.GetComponent<BRigidBody>() != null)
                    node.MainObject.AddComponent<Tracker>().Trace = true;
            }
        }

        #endregion

        //Get the offset from the first node to the robot for new robot start position calculation
        //This line is CRITICAL to new reset position accuracy! DON'T DELETE IT!
        nodeToRobotOffset = gameObject.transform.GetChild(0).localPosition - robotStartPosition;

        foreach (BRaycastRobot r in GetComponentsInChildren<BRaycastRobot>())
        {
            r.RaycastRobot.OverrideMass = collectiveMass;
            r.RaycastRobot.RootRigidBody = (RigidBody)((RigidNode)nodes[0]).MainObject.GetComponent<BRigidBody>().GetCollisionObject();
        }

        RotateRobot(robotStartOrientation);

        RobotName = new DirectoryInfo(directory).Name;

        isInitialized = true;

        //Initializes Driver Practice component
        dpmRobot = gameObject.AddComponent<DriverPracticeRobot>();
        dpmRobot.Initialize(directory);

        //Initializing robot cameras
        bool hasRobotCamera = false;
        //If you are getting an error referencing this line, it is likely that the Game Object "RobotCameraList" in Scene.unity does not have the RobotCameraManager script attached to it.
        robotCameraManager = GameObject.Find("RobotCameraList").GetComponent<RobotCameraManager>();

        //Loop through robotCameraList and check if any existing camera should attach to this robot
        foreach (GameObject robotCamera in robotCameraManager.GetRobotCameraList())
        {
            if (robotCamera.GetComponent<RobotCamera>().robot.Equals(this))
            {
                //Recover the robot camera configurations
                robotCamera.GetComponent<RobotCamera>().RecoverConfiguration();
                hasRobotCamera = true;
            }

        }
        //Add new cameras to the robot if there is none robot camera belong to the current robot (which means it is a new robot)
        if (!hasRobotCamera)
        {
            //Attached to the main frame and face the front
            robotCameraManager.AddCamera(this, transform.GetChild(0).transform, new Vector3(0, 0.5f, 0), new Vector3(0, 0, 0));
            ////Attached to main frame and face the back
            robotCameraManager.AddCamera(this, transform.GetChild(0).transform, new Vector3(0, 0.5f, 0), new Vector3(0, 180, 0));
            robotCameraManager.AddCamera(this, transform.GetChild(0).transform);
        }

        //Reads the offset position for the manipulator
        if (RobotIsMixAndMatch)
        {
            offset = Vector3.zero;
            try
            {
                using (TextReader reader = File.OpenText(directory + "\\position.txt"))
                {
                    offset.x = float.Parse(reader.ReadLine());
                    offset.y = float.Parse(reader.ReadLine());
                    offset.z = float.Parse(reader.ReadLine());
                }
            } catch
            {
                offset = Vector3.zero;
            }
           
        }

        return true;
    }

    /// <summary>
    /// Loads and initializes the physical manipulator object (used in Mix and Match mode)
    /// </summary>
    /// <param name="directory">Folder directory of the manipulator</param>
    /// <param name="robotGameObject">GameObject of the robot the manipulator will be attached to</param>
    public bool InitializeManipulator(string directory, GameObject robotGameObject)
    {

        if (robotGameObject == null)
        {
            robotGameObject = GameObject.Find("Robot");
        }
        ManipulatorObject = new GameObject("Manipulator");

        RigidNode_Base.NODE_FACTORY = delegate (Guid guid)
        {
            return new RigidNode(guid);
        };

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        //TO-DO: Read .robot instead (from the new exporters if they are implemented). Maybe need a RobotSkeleton class
        manipulatorNode = BXDJSkeleton.ReadSkeleton(directory + "\\skeleton.bxdj");
        manipulatorNode.ListAllNodes(nodes);

        int numWheels = nodes.Count(x => x.HasDriverMeta<WheelDriverMeta>() && x.GetDriverMeta<WheelDriverMeta>().type != WheelType.NOT_A_WHEEL);
        float collectiveMass = 0f;

        //Load node_0 for attaching manipulator to robot
        RigidNode node = (RigidNode)nodes[0];

        node.CreateTransform(ManipulatorObject.transform);
        if (!node.CreateMesh(directory + "\\" + node.ModelFileName))
        {
            Debug.Log("Robot not loaded!");
            UnityEngine.Object.Destroy(ManipulatorObject);
            return false;
        }
        GameObject robot = robotGameObject;

        //Set the manipulator transform to match with the position of node_0 of the robot. THIS ONE ACTUALLY DOES SOMETHING: LIKE ACTUALLY



        Vector3 manipulatorTransform = robotStartPosition + offset;
        Debug.Log("Node Com Offset" + node.ComOffset);
        ManipulatorObject.transform.position = manipulatorTransform;

        node.CreateManipulatorJoint(robot);
        node.MainObject.AddComponent<Tracker>().Trace = true;
        Tracker t = node.MainObject.GetComponent<Tracker>();
        Debug.Log(t);

        //Load other nodes associated with the manipulator
        for (int i = 1; i < nodes.Count; i++)
        {
            RigidNode otherNode = (RigidNode)nodes[i];
            otherNode.CreateTransform(ManipulatorObject.transform);
            if (!otherNode.CreateMesh(directory + "\\" + otherNode.ModelFileName))
            {
                Debug.Log("Robot not loaded!");
                UnityEngine.Object.Destroy(ManipulatorObject);
                return false;
            }
            otherNode.CreateJoint(numWheels, RobotIsMixAndMatch);
            otherNode.MainObject.AddComponent<Tracker>().Trace = true;
            t = otherNode.MainObject.GetComponent<Tracker>();
            Debug.Log(t);
        }

        foreach (BRaycastRobot r in ManipulatorObject.GetComponentsInChildren<BRaycastRobot>())
            r.RaycastRobot.OverrideMass = collectiveMass;

        RotateRobot(robotStartOrientation);

        this.RobotHasManipulator = true;
        return true;
    }

    /// <summary>
    /// Deletes robot manipulator (used for Mix and Match mode)
    /// </summary>
    public void DeleteManipulatorNodes()
    {
        //Deletes all nodes if any exist, take the old node transforms out from the robot object
        int childCount = ManipulatorObject.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = ManipulatorObject.transform.GetChild(i);

            //If this isn't done, the game object is destroyed but the parent-child transform relationship remains!
            child.parent = null;
            Destroy(child.gameObject);
        }

        Destroy(ManipulatorObject);
    }

    #region Reset
    /// <summary>
    /// Return the robot to robotStartPosition and destroy extra game pieces
    /// </summary>
    /// <param name="resetTransform"></param>
    public void BeginReset()
    {
        BRigidBody rigidBody = GetComponentInChildren<BRigidBody>();

        if (rigidBody != null && !rigidBody.GetCollisionObject().IsActive)
            rigidBody.GetCollisionObject().Activate();

        if (!mainState.DynamicCameraObject.GetComponent<DynamicCamera>().cameraState.GetType().Equals(typeof(DynamicCamera.ConfigurationState)))
        {
            Debug.Log(mainState.DynamicCameraObject.GetComponent<DynamicCamera>().cameraState);
            IsResetting = true;

            foreach (RigidNode n in rootNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

                r.LinearVelocity = r.AngularVelocity = BulletSharp.Math.Vector3.Zero;
                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.Zero;

                BulletSharp.Math.Matrix newTransform = r.WorldTransform;
                newTransform.Origin = (robotStartPosition + n.ComOffset).ToBullet();
                newTransform.Basis = BulletSharp.Math.Matrix.Identity;
                r.WorldTransform = newTransform;
            }


            if (RobotHasManipulator && RobotIsMixAndMatch)
            {
                int i = 0;
                foreach (RigidNode n in manipulatorNode.ListAllNodes())
                {
                    BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                    if (br == null)
                        continue;

                    RigidBody r = (RigidBody)br.GetCollisionObject();

                    r.LinearVelocity = r.AngularVelocity = BulletSharp.Math.Vector3.Zero;
                    r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.Zero;

                    BulletSharp.Math.Matrix newTransform = r.WorldTransform;
                    newTransform.Origin = (robotStartPosition + n.ComOffset).ToBullet();
                    newTransform.Basis = BulletSharp.Math.Matrix.Identity;
                    r.WorldTransform = newTransform;
                    if (i == 0)
                    {
                        Debug.Log("Transform Origin" + newTransform.Origin);
                    }
                    i++;
                }

            }

            //Where "save orientation" works
            RotateRobot(robotStartOrientation);

            GameObject.Find("Robot").transform.GetChild(0).transform.position = new Vector3(10, 20, 5);
            if (IsResetting)
            {
                Debug.Log("is resetting!");
            }
        }
        else
        {
            UserMessageManager.Dispatch("Please don't reset robot during configuration!", 5f);
        }
    }

    /// <summary>
    /// Can move robot around in this state using WASD, update robotStartPosition if hit enter
    /// </summary>
    void Resetting()
    {
        if (Input.GetMouseButton(1))
        {
            //Transform rotation along the horizontal plane
            Vector3 rotation = new Vector3(0f,
                Input.GetKey(KeyCode.D) ? ResetVelocity : Input.GetKey(KeyCode.A) ? -ResetVelocity : 0f,
                0f);
            if (!rotation.Equals(Vector3.zero))
                RotateRobot(rotation);

        }
        else
        {
            //Transform position
            Vector3 transposition = new Vector3(
                Input.GetKey(KeyCode.D) ? ResetVelocity : Input.GetKey(KeyCode.A) ? -ResetVelocity : 0f,
                0f,
                Input.GetKey(KeyCode.W) ? ResetVelocity : Input.GetKey(KeyCode.S) ? -ResetVelocity : 0f);

            if (!transposition.Equals(Vector3.zero))
                TransposeRobot(transposition);
        }

        //Update robotStartPosition when hit enter
        if (Input.GetKey(KeyCode.Return))
        {
            robotStartOrientation = ((RigidNode)rootNode.ListAllNodes()[0]).MainObject.GetComponent<BRigidBody>().GetCollisionObject().WorldTransform.Basis;

            robotStartPosition = new Vector3(transform.GetChild(0).transform.localPosition.x - nodeToRobotOffset.x, robotStartPosition.y,
                transform.GetChild(0).transform.localPosition.z - nodeToRobotOffset.z);
            EndReset();
        }
    }

    /// <summary>
    /// End the reset process and puts the robot back down
    /// </summary>
    public void EndReset()
    {
        IsResetting = false;
        isResettingOrientation = false;

        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

            if (br == null)
                continue;

            RigidBody r = (RigidBody)br.GetCollisionObject();

            r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
        }

        if (RobotHasManipulator && RobotIsMixAndMatch)
        {
            foreach (RigidNode n in manipulatorNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

                r.LinearFactor = r.AngularFactor = BulletSharp.Math.Vector3.One;
            }
        }

        foreach (Tracker t in GetComponentsInChildren<Tracker>())
            t.Clear();
    }

    /// <summary>
    /// Shifts the robot by a set position vector
    /// </summary>
    public void TransposeRobot(Vector3 transposition)
    {
        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

            if (br == null)
                continue;

            RigidBody r = (RigidBody)br.GetCollisionObject();

            BulletSharp.Math.Matrix newTransform = r.WorldTransform;
            newTransform.Origin += transposition.ToBullet();
            r.WorldTransform = newTransform;
        }

        if (RobotHasManipulator)
        {
            foreach (RigidNode n in manipulatorNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

                BulletSharp.Math.Matrix newTransform = r.WorldTransform;
                newTransform.Origin += transposition.ToBullet();
                r.WorldTransform = newTransform;
            }
        }
    }

    /// <summary>
    /// Rotates the robot about its origin by a mathematical 4x4 matrix
    /// </summary>
    public void RotateRobot(BulletSharp.Math.Matrix rotationMatrix)
    {
        BulletSharp.Math.Vector3? origin = null;

        foreach (RigidNode n in rootNode.ListAllNodes())
        {
            BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

            if (br == null)
                continue;

            RigidBody r = (RigidBody)br.GetCollisionObject();

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

        if (RobotHasManipulator)
        {
            foreach (RigidNode n in manipulatorNode.ListAllNodes())
            {
                BRigidBody br = n.MainObject.GetComponent<BRigidBody>();

                if (br == null)
                    continue;

                RigidBody r = (RigidBody)br.GetCollisionObject();

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
    }

    /// <summary>
    /// Rotates the robot about its origin by a set vector
    /// </summary>
    public void RotateRobot(Vector3 rotation)
    {
        RotateRobot(BulletSharp.Math.Matrix.RotationYawPitchRoll(rotation.y, rotation.z, rotation.x));
    }

    /// <summary>
    /// Resets the robot orientation to how the CAD model was originally defined (should be standing upright and facing forward if CAD was done properly)
    /// </summary>
    public void ResetRobotOrientation()
    {
        robotStartOrientation = BulletSharp.Math.Matrix.Identity;
        BeginReset();
        EndReset();
    }

    /// <summary>
    /// Saves the robot's current orientation to be used whenever robot is reset
    /// </summary>
    public void SaveRobotOrientation()
    {
        robotStartOrientation = ((RigidNode)rootNode.ListAllNodes()[0]).MainObject.GetComponent<BRigidBody>().GetCollisionObject().WorldTransform.Basis;
        robotStartOrientation.ToUnity();
    }

    /// <summary>
    /// Cancel current orientation & spawnpoint changes
    /// </summary>
    public void CancelRobotOrientation()
    {
        if (IsResetting)
        {
            BeginReset();
            EndReset();
        }
    }
#endregion

    /// <summary>
    /// Returns the driver practice component of this robot
    /// </summary>
    public DriverPracticeRobot GetDriverPractice()
    {
        return GetComponent<DriverPracticeRobot>();
    }

    /// <summary>
    /// Update the stats for robot depending on whether it's metric or not
    /// </summary>
    public void UpdateStats()
    {
        GameObject mainNode = transform.GetChild(0).gameObject;
        //calculates stats of robot
        if (mainNode != null)
        {
            float currentSpeed = mainNode.GetComponent<BRigidBody>().GetCollisionObject().InterpolationLinearVelocity.Length;

            Speed = (float)Math.Round(Math.Abs(currentSpeed), 3);
            Weight = (float)Math.Round(GetWeight(), 3);
            AngularVelocity = (float)Math.Round(Math.Abs(mainNode.GetComponent<BRigidBody>().angularVelocity.magnitude), 3);
            Acceleration = (float)Math.Round((currentSpeed - oldSpeed) / Time.deltaTime, 3);
            oldSpeed = currentSpeed;
            if (!mainState.IsMetric)
            {
                Speed = (float)Math.Round(Speed * 3.28084, 3);
                Acceleration = (float)Math.Round(Acceleration * 3.28084, 3);
                Weight = (float)Math.Round(Weight * 2.20462, 3);
            }
        }
    }

    /// <summary>
    /// Get the total weight of the robot
    /// </summary>
    /// <returns></returns>
    public float GetWeight()
    {
        float weight = 0;

        foreach (Transform child in gameObject.transform)
        {
            if (child.GetComponent<BRigidBody>() != null)
            {
                weight += (float)child.GetComponent<BRigidBody>().mass;
            }
        }
        return weight;
    }

    public void SetControlIndex(int index)
    {
        ControlIndex = index;
        dpmRobot.controlIndex = index;
    }
}